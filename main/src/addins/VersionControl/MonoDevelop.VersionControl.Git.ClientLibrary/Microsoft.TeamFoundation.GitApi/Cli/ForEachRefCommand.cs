//*************************************************************************************************
// ForEachRefCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class ForEachRefCommand : GitCommand
    {
        public const string Command = "for-each-ref";

        private const char PrefixChar0 = '[';
        private const char PrefixCharZ = ']';

        public ForEachRefCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        internal static readonly Dictionary<StringUtf8, ObjectType> NameTypeLookup = new Dictionary<StringUtf8, ObjectType>(StringUtf8Comparer.Ordinal)
        {
            { (StringUtf8)"blob", ObjectType.Blob },
            { (StringUtf8)"commit", ObjectType.Commit },
            { (StringUtf8)"submodule", ObjectType.Submodule },
            { (StringUtf8)"tag", ObjectType.Tag },
            { (StringUtf8)"tree", ObjectType.Tree },
        };

        public IReferenceCollection ReadCollection(ReferenceOptions options)
        {
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, options);

                IReferenceCollection collection = ExecuteForEachRef(command, options.Flags);

                return collection;
            }
        }

        private void ApplyOptions(ArgumentList command, ReferenceOptions options)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (options.ResultLimit > 0)
            {
                command.AddOption("--count", options.ResultLimit.ToString());
            }

            command.AddOption("--format", 
                PrefixChar0 + "%(refname)" + PrefixCharZ + 
                PrefixChar0 + "%(objecttype)" + PrefixCharZ +
                PrefixChar0 + "%(objectname)" + PrefixCharZ +
                PrefixChar0 + "%(upstream)" + PrefixCharZ +
                PrefixChar0 + "%(push)" + PrefixCharZ + 
                PrefixChar0 + "%(HEAD)" + PrefixCharZ);

            if (options.Flags != ReferenceOptionFlags.None)
            {
                if ((options.Flags & ReferenceOptionFlags.RefsHeads) != 0)
                {
                    command.Add(ReferenceName.PatternRefHeads);
                }

                if ((options.Flags & ReferenceOptionFlags.RefsNotes) != 0)
                {
                    command.Add(ReferenceName.PatternRefNotes);
                }

                if ((options.Flags & ReferenceOptionFlags.RefsRemotes) != 0)
                {
                    command.Add(ReferenceName.PatternRefRemotes);
                }

                if ((options.Flags & ReferenceOptionFlags.RefsStash) != 0)
                {
                    command.Add(ReferenceName.PatternRefStash);
                }

                if ((options.Flags & ReferenceOptionFlags.RefsTags) != 0)
                {
                    command.Add(ReferenceName.PatternRefTags);
                }
            }

            if (options.Excludes != null)
            {
                if (options.Includes != null)
                    throw new InvalidOperationException($"{nameof(ReferenceOptions.Excludes)} != null && {nameof(ReferenceOptions.Includes)} != null");

                command.AddOption("--no-merge");
                command.Add(options.Excludes.RevisionText);
            }

            if (options.Includes != null)
            {
                if (options.Excludes != null)
                    throw new InvalidOperationException($"{nameof(ReferenceOptions.Excludes)} != null && {nameof(ReferenceOptions.Includes)} != null");

                command.AddOption("--merge");
                command.Add(options.Includes.RevisionText);
            }

            if (options.Contains != null)
            {
                command.AddOption("--contains");
                command.Add(options.Contains.RevisionText);
            }
        }

        private IReferenceCollection ExecuteForEachRef(string command, ReferenceOptionFlags flags)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var collection = new ReferenceCollection(flags);
            collection.SetContext(Context);
            IObjectDatabase odb = null;
            Head head = null;
            bool needHead = (flags & ReferenceOptionFlags.ReadHead) != 0;

            if ((flags & ReferenceOptionFlags.TipsAll) != 0)
            {
                odb = _repository.OpenObjectDatabase();
            }

            try
            {
                using (var buffer = new ByteBuffer())
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    int read = 0; // the count of bytes read into the buffer
                    int get = 0; // the first unparsed byte in the buffer

                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    int r;
                    while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
                    {
                        // compute total read length
                        read += r;

                        // so long as the current read point is inside the available buffer...
                        while (get < read)
                        {
                            // find the next LF character
                            int eol = buffer.FirstIndexOf(Eol, get, read - get);
                            if (eol < 0)
                            {
                                // if eol was not found, we need more buffer paged in
                                // move the read idx by the read amount, which will
                                // trigger a buffer page
                                break;
                            }

                            int i1 = get,
                                i2 = get;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            StringUtf8 canonicalName = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);

                            i1 = i2;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            StringUtf8 objectTypeName = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);
                            if (!NameTypeLookup.ContainsKey(objectTypeName))
                                throw new ReferenceParseException($"Unknown object type '{objectTypeName}'.", new StringUtf8(buffer, 0, read), get);

                            ObjectType objectType = NameTypeLookup[objectTypeName];

                            i1 = i2;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            ObjectId objectId = ObjectId.FromUtf8(buffer, i1 + 1);

                            i1 = i2;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            StringUtf8 upstreamName = (i2 > i1)
                                ? new StringUtf8(buffer, i1 + 1, i2 - i1 - 1)
                                : StringUtf8.Empty;

                            i1 = i2;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            StringUtf8 pushName = (i2 > i1)
                                ? new StringUtf8(buffer, i1 + 1, i2 - i1 - 1)
                                : StringUtf8.Empty;

                            i1 = i2;

                            i1 = buffer.FirstIndexOf(PrefixChar0, i2, eol - i2);
                            if (i1 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixChar0}'.", new StringUtf8(buffer, 0, read), get);

                            i2 = buffer.FirstIndexOf(PrefixCharZ, i1, eol - i1);
                            if (i2 < 0)
                                throw new ReferenceParseException($"Expected '{PrefixCharZ}'.", new StringUtf8(buffer, 0, read), get);

                            bool isHead = (i2 - i1 == 2 && buffer[i2 - 1] == '*');

                            canonicalName = canonicalName.Copy();

                            // for now we do not collection local or remote HEAD references
                            if (!canonicalName.EndsWith("HEAD"))
                            {
                                if (canonicalName.StartsWith(ReferenceName.PatternRefHeads))
                                {
                                    var branch = new Branch(canonicalName, objectId, objectType)
                                    {
                                        IsHead = isHead,
                                        PushTargetNameUtf8 = pushName,
                                        UpstreamNameUtf8 = upstreamName,
                                    };
                                    branch.SetContextAndCache(Context, _repository as IStringCache);
                                    collection.Add(branch);

                                    if (isHead)
                                    {
                                        head = new Head(collection, branch)
                                        {
                                            HeadType = HeadType.Normal,
                                        };
                                        head.SetContextAndCache(Context, _repository as IStringCache);

                                        collection.Head = head;
                                    }

                                    if ((flags & ReferenceOptionFlags.TipsHeads) != 0)
                                    {
                                        try
                                        {
                                            ICommit commit = odb.ReadObject<Commit>(objectId);
                                            branch.Commit = commit;
                                        }
                                        catch (ParseException exception) when (ParseHelper.AddContext("read-object", exception, objectId))
                                        {
                                            // should not be reached, but we'll re-throw just-in-case
                                            throw;
                                        }
                                    }
                                }
                                else if (canonicalName.StartsWith(ReferenceName.PatternRefRemotes))
                                {
                                    var remote = new Branch(canonicalName, objectId, objectType)
                                    {
                                        IsHead = isHead,
                                        PushTargetNameUtf8 = pushName,
                                        UpstreamNameUtf8 = upstreamName,
                                    };
                                    remote.SetContextAndCache(Context, _repository as IStringCache);
                                    collection.Add(remote);

                                    // no idea how a remote branch could be HEAD but just in case
                                    if (isHead)
                                    {
                                        head = new Head(collection, remote)
                                        {
                                            HeadType = HeadType.Normal,
                                        };
                                        head.SetContextAndCache(Context, _repository as IStringCache);

                                        collection.Head = head;
                                    }

                                    if ((flags & ReferenceOptionFlags.TipsRemotes) != 0)
                                    {
                                        try
                                        {
                                            ICommit commit = odb.ReadObject<Commit>(objectId);
                                            remote.Commit = commit;
                                        }
                                        catch (ParseException exception) when (ParseHelper.AddContext("read-object", exception, objectId))
                                        {
                                            // should not be reached, but we'll re-throw just-in-case
                                            throw;
                                        }
                                    }
                                }
                                else if (canonicalName.StartsWith(ReferenceName.PatternRefTags))
                                {
                                    var tag = new Tag(canonicalName, objectId, objectType);
                                    tag.SetContextAndCache(Context, _repository as IStringCache);
                                    collection.Add(tag);

                                    // no idea how a tag could be HEAD but just in case
                                    if (isHead)
                                    {
                                        var collectionHead = new Head(collection, tag);
                                        collectionHead.SetContextAndCache(Context, _repository as IStringCache);
                                        collection.Head = collectionHead;
                                    }

                                    // Load tag annotation if requested
                                    if (objectType == ObjectType.Tag && (flags & ReferenceOptionFlags.TagAnnotations) != 0)
                                    {
                                        // The object database can be opened on demand to load annotations
                                        if (odb == null)
                                        {
                                            odb = _repository.OpenObjectDatabase();
                                        }

                                        try
                                        {
                                            ITagAnnotation annotation = odb.ReadObject<ITagAnnotation>(objectId);
                                            tag.Annotation = annotation;
                                        }
                                        catch (ParseException exception) when (ParseHelper.AddContext("read-object", exception, objectId))
                                        {
                                            // should not be reached, but we'll re-throw just-in-case
                                            throw;
                                        }
                                    }

                                    if ((flags & ReferenceOptionFlags.TipsTags) != 0)
                                    {
                                        try
                                        {
                                            ICommit commit = odb.ReadObject<Commit>(objectId);
                                            tag.Commit = commit;
                                        }
                                        catch (ParseException exception) when (ParseHelper.AddContext("read-object", exception, objectId))
                                        {
                                            // should not be reached, but we'll re-throw just-in-case
                                            throw;
                                        }
                                    }
                                }
                                else if (canonicalName.StartsWith(ReferenceName.PatternRefNotes))
                                {
                                    // not supported yet
                                }
                                else if (canonicalName.StartsWith(ReferenceName.PatternRefStash))
                                {
                                    // not supported yet
                                }
                            }

                            get = eol + 1;
                        }

                        // when we've moved beyond what we can reliably read we need to shift the bytes
                        // left to make room in the buffer for new data
                        int copyStart = get;
                        int copyLength = read - get;

                        if (copyLength > 0)
                        {
                            // copy start->start+len to 0->len
                            // yes this leaves garbage in the buffer, we do not care
                            Buffer.BlockCopy(buffer, copyStart, buffer, 0, copyLength);
                        }

                        // reset the read counter to the copy length (start filling at the first garbage byte)
                        read = copyLength;
                        // reset the read idx to the beginning of the buffer
                        get = 0;
                    }

                    TestExitCode(process, command, stderrTask);
                }

                if (needHead)
                {
                    var initCommit = (flags & ReferenceOptionFlags.TipsHeads) != 0;

                    // If head is null here, it was not found in the ref collection.  Therefore it
                    // needs to be created without a backing reference.  One example of this is
                    // when there is an unborn head in a newly created repo.
                    if (head == null)
                    {
                        head = new Head(collection, null);
                        head.SetContextAndCache(Context, _repository as IStringCache);

                        collection.Head = head;
                    }

                    head.Initialize(_repository, true, initCommit);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(ForEachRefCommand)}.{nameof(ExecuteForEachRef)}", exception, command))
            {
                // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }
            finally
            {
                odb?.Dispose();
            }

            return collection;
        }    }
}
