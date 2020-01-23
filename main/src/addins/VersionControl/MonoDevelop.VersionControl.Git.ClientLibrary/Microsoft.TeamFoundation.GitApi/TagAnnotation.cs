//*************************************************************************************************
// TagAnnotation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    public interface ITagAnnotation : IEquatable<ITagAnnotation>, IObject, ITagName
    {
        string FirstLine { get; }
		
        string Message { get; }
		
        IIdentity Tagger { get; }
		
        ObjectId TargetId { get; }
		
        ObjectType TargetType { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class TagAnnotation : ObjectBase, ITagAnnotation, IEquatable<TagAnnotation>, ILoggable
    {
        const string ObjectLinePrefix = "object ";
        const string TagLinePrefix = "tag ";
        const string TaggerLinePrefix = "tagger ";
        const string TypeLinePrefix = "type ";

        public TagAnnotation(ObjectHeader header)
            : base(header, ObjectType.Tag)
        { }

        private StringUtf8 _firstLine;
        private StringUtf8 _message;
        private StringUtf8 _name;
        private readonly object _syncpoint = new object();
        private IIdentity _tagger;
        private ObjectId _targetId;
        private ObjectType _targetType;

        protected internal override void SetContextAndCache(IExecutionContext context, IStringCache cache)
        {
            base.SetContextAndCache(context, cache);

            (_tagger as Identity)?.SetContextAndCache(context, cache);
        }

        [JsonProperty]
        public string CanonicalName
        {
            get { return FormattableString.Invariant($"{ReferenceName.PatternRefTags}/{FriendlyName}"); }
        }

        [JsonProperty]
        public string FirstLine
        {
            get { lock (_syncpoint) return (_firstLine ?? _message)?.ToString(); }
        }

        [JsonProperty]
        public string Message
        {
            get { lock (_syncpoint) return _message?.ToString(); }
        }

        [JsonProperty]
        public string FriendlyName
        {
            get { lock (_syncpoint) return _name?.ToString(); }
        }

        [JsonProperty]
        public override string RevisionText
        {
            get { return FriendlyName; }
        }

        [JsonProperty]
        public IIdentity Tagger
        {
            get { lock (_syncpoint) return _tagger; }
        }

        [JsonProperty]
        public ObjectId TargetId
        {
            get { lock (_syncpoint) return _targetId; }
        }

        [JsonProperty]
        public ObjectType TargetType
        {
            get { lock (_syncpoint) return _targetType; }
        }

        public static bool Equals(ITagAnnotation left, ITagAnnotation right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;

            return ObjectId.Equals(left.ObjectId, right.ObjectId);
        }

        public bool Equals(TagAnnotation other)
            => Commit.Equals(this as ITagAnnotation, other as ITagAnnotation);

        public bool Equals(ITagAnnotation other)
            => Commit.Equals(this as ITagAnnotation, other);

        public override bool Equals(object obj)
            => Commit.Equals(this as ITagAnnotation, obj as ITagAnnotation);

        public override int GetHashCode()
            => ObjectId.GetHashCode();

        public override string ToString()
            => base.ToString();

        internal override unsafe void ParseData(ByteBuffer buffer, ref int index, int count, int skipPrefix, INamedObjectFilter filter)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (skipPrefix < 0 || skipPrefix > 4)
                throw new ArgumentOutOfRangeException(nameof(skipPrefix));

            // Sample tag annotation
            //
            //      object 6fd87c48d7771826aed76ec93e73cc9cb6320d42\n
            //      type commit\n
            //      tag TagName\n
            //      tagger Tagger Name <tagger@email> 1471583955 -0400\n
            //      \n
            //      Message for sample annotated tag\n

            IIdentity tagger = null;
            ObjectId? targetId = null;
            StringUtf8 name = null;
            StringUtf8 targetTypeName = null;
            ObjectType targetType = ObjectType.Unknown;
            StringUtf8 message = null;
            StringUtf8 firstLine = null;

            int get = index;
            int end = index + count;

            // ignore stray new line characters
            while (get < end && buffer[get] == '\n')
            {
                get += 1;
            }

            while (get < end)
            {
                int eol = buffer.FirstIndexOf('\n', get, end - get);
                if (eol < 0)
                {
                    eol = buffer.FirstIndexOf('\0', get, end - get);
                    if (eol < 0)
                        break;
                }

                if (buffer.StartsWith(ObjectLinePrefix, get, ObjectLinePrefix.Length))
                {
                    int start = get + ObjectLinePrefix.Length;
                    targetId = ObjectId.FromUtf8(buffer, start);
                }
                else if (buffer.StartsWith(TaggerLinePrefix, get, TaggerLinePrefix.Length))
                {
                    int start = get + TaggerLinePrefix.Length;
                    tagger = Identity.FromUtf8(buffer, start, eol - start);
                    ((Identity)tagger).SetContextAndCache(Context, _cache);
                }
                else if (buffer.StartsWith(TagLinePrefix, get, TagLinePrefix.Length))
                {
                    int start = get + TagLinePrefix.Length;
                    name = new StringUtf8(buffer, start, eol - start);
                }
                else if (buffer.StartsWith(TypeLinePrefix, get, TypeLinePrefix.Length))
                {
                    int start = get + TypeLinePrefix.Length;

                    targetTypeName = new StringUtf8(buffer, start, eol - start);

                    if (!Cli.ForEachRefCommand.NameTypeLookup.ContainsKey(targetTypeName))
                    {
                        var exception = new ReferenceParseException($"unknown-type", targetTypeName, get);
                        ParseHelper.AddContext("type", exception, targetTypeName);
                        throw exception;
                    }

                    targetType = Cli.ForEachRefCommand.NameTypeLookup[targetTypeName];
                }
                else
                {
                    // ignore stray new line characters
                    while (get < end && buffer[get] == '\n')
                    {
                        get += 1;
                    }

                    message = new StringUtf8(buffer, get, end - get);
                    message = message.TrimLeftTab(skipPrefix);

                    int i1 = message.FirstIndexOf('\n');

                    firstLine = (i1 > 0)
                        ? message.Substring(0, i1)
                        : message;

                    break;
                }

                get = eol + 1;
            }

            lock (_syncpoint)
            {
                if (targetId == null)
                    throw new ObjectParseException("target-id", new StringUtf8(buffer, index, count), 0);

                _name = name ?? StringUtf8.Empty;
                _targetId = targetId.Value;
                _targetType = targetType;
                _tagger = tagger;
                _message = message ?? StringUtf8.Empty;
                _firstLine = firstLine ?? _message;

                index += count + 1;
            }
        }

        public void SetData(StringUtf8 name, ObjectId targetId, ObjectType targetType, IIdentity tagger, StringUtf8 firstLine, StringUtf8 message)
        {
            lock (_syncpoint)
            {
                _name = name ?? StringUtf8.Empty;
                _targetId = targetId;
                _targetType = targetType;
                _tagger = tagger;
                _message = message ?? StringUtf8.Empty;
                _firstLine = firstLine ?? _message;
            }
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            string prefix2 = context.ParseHelper.GetParseErrorIndent(indent + 1);
            string prefix3 = context.ParseHelper.GetParseErrorIndent(indent + 2);

            log.Append(prefix).AppendLine(nameof(TagAnnotation));
            log.Append(prefix).AppendLine("{");
            {
                log.Append(prefix2).Append("name: ").AppendLine(FriendlyName);
                log.Append(prefix2).Append("target-id: ").AppendLine(TargetId.ToString());
                log.Append(prefix2).Append("target-type: ").AppendLine(TargetType.ToString());

                log.Append(prefix2).AppendLine("tagger:");
                log.Append(prefix2).AppendLine("{");
                {
                    (Tagger as ILoggable)?.Log(context, log, indent + 2);
                }
                log.Append(prefix2).AppendLine("}");

                log.Append(prefix2).Append("first-line: \"").Append(FirstLine).AppendLine("\"");
                log.Append(prefix2).Append("message: \"").Append(Message).AppendLine("\"");
            }
            log.Append(prefix).AppendLine("}");
        }
    }
}
