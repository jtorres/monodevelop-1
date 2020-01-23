//*************************************************************************************************
// BlameOptions.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{

    public class Blame
    {
        public string Text { get; internal set; }
        public Annotation[] Annotations { get; internal set; }
    }

    public class Annotation
    {
        public string Revision { get; set; }

        public string Author { get; set; }
        public string AuthorMail { get; set; }
        public DateTime AuthorTime { get; set; }

        public string Summary { get; set; }

        public string PreviousRevision { get; set; }
        public string PreviousPath { get; set; }

        public override string ToString ()
        {
            return $"[Annotation: Revision={Revision}, Author={Author} {AuthorMail}, Summary={Summary}, AuthorTime={AuthorTime}]";
        }
    }

    /// <summary>
    /// Options related to `<see cref="IRepository.Blame(BlameOptions)"/>`.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct BlameOptions
    {
        public static readonly BlameOptions Default = new BlameOptions {
            IgnoreWhitespaces = false
        };

        /// <summary>
        /// When set whitespaces are ignored when comparing the parent's version and the child's to find where the lines came from.
        /// </summary>
        public bool IgnoreWhitespaces { get; set; }

        public string Path { get; set; }
    }
}
