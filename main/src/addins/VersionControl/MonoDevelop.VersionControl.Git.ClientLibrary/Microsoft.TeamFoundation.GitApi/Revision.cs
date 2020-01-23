//*************************************************************************************************
// LiteralRevision.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model abstraction of an object identity, reference name, or other object-database identifier.
    /// </summary>
    public interface IRevision : IEquatable<IRevision>, IEquatable<string>
    {
        /// <summary>
        /// Gets the textual representation of the revision.
        /// <para/>
        /// This value, when passed to Git, will be interpreted depending on context and will ultimately resolve to a tree object (a revision in history).
        /// </summary>
        string RevisionText { get; }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Revision : Base, IEquatable<string>, IRevision, ILoggable
    {
        /// <summary>
        /// The value used by Git when referencing the head of current workspace.
        /// </summary>
        public const string CanonicalHeadLabel = Head.CanonicalLabel;

        /// <summary>
        /// Name of branch when HEAD is detached.
        /// <para/>
        /// The value of the label is derived from 'git status --porcelain=v2'.
        /// </summary>
        public const string DetachedHeadLabel = Head.DetachedLabel;

        /// <summary>
        /// Substitute branch name when in a bogus state, such as when HEAD points to a broken ref.
        /// <para/>
        /// The value of the label is derived from 'git status --porcelain=v2'.
        /// </summary>
        public const string MalformedHeadLabel = Head.MalformedLabel;

        /// <summary>
        /// Substitute commit name when prior to the initial commit.
        /// <para/>
        /// The value of the label is derived from 'git status --porcelain=v2'.
        /// </summary>
        public const string UnbornHeadLabel = Head.UnbornLabel;

        /// <summary>
        /// Static `<see cref="Revision"/>` instance of the value used by Git when referencing the head of current workspace.
        /// </summary>
        public static readonly Revision HeadRevision = new Revision(CanonicalHeadLabel);

        public static readonly StringComparer StringComparer = StringComparer.Ordinal;
        public static readonly StringComparison StringComparison = StringComparison.Ordinal;

        internal static readonly RevisionComparer Comparer = new RevisionComparer();
        internal static readonly StringUtf8Comparer StringUtf8Comparer = StringUtf8Comparer.Ordinal;

        public Revision(string revisionText)
            : this((StringUtf8)revisionText)
        { }

        internal Revision(StringUtf8 revisionText)
            : base()
        {
            _revisionText = revisionText;
        }

        private StringUtf8 _revisionText;

        /// <summary>
        /// Gets the string necessary to pass to git.exe as a command line parameter.
        /// </summary>
        [JsonProperty]
        public string RevisionText
        {
            get { return (string)_revisionText; }
        }

        internal StringUtf8 RevisionTextUtf8
        {
            get { return _revisionText; }
        }

        private string DebuggerDisplay
        {
            get { return FormattableString.Invariant($"{nameof(Revision)}: \"{_revisionText}\""); }
        }

        public bool Equals(Revision other)
            => Comparer.Equals(this, other);

        public bool Equals(IRevision other)
            => Comparer.Equals(this, other);

        public bool Equals(string other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            var a = obj as Revision;
            if (!ReferenceEquals(a, null))
                return Equals(a);

            var b = obj as IRevision;
            if (!ReferenceEquals(b, null))
                return Equals(b);

            var c = obj as string;
            if (!ReferenceEquals(c, null))
                return Equals(c);

            return base.Equals(obj);
        }

        public override int GetHashCode()
            => StringComparer.GetHashCode(_revisionText);

        public override string ToString()
        {
            return (string)_revisionText;
        }

        internal void SetRevisionText(StringUtf8 revisionText)
        {
            _revisionText = revisionText;
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine($"{nameof(Revision)} {{ Rev: {_revisionText} }}");
        }
    }
}
