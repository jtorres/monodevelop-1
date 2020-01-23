//*************************************************************************************************
// ExclusiveRange.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git commit-ish range.
    /// </summary>
    public sealed class ExclusiveRange : CommitRange
    {
        public const string RangeOperator = "..";

        /// <summary>
        /// Creates a new instance of `<see cref="ExclusiveRange"/>`.
        /// </summary>
        /// <param name="since">Only include revisions newer in the range.</param>
        /// <param name="until">Only include revisions older in the range.</param>
        public ExclusiveRange(IRevision since, IRevision until)
            : base(since, until)
        { }

        public override string ToString()
        {
            return $"{Since.RevisionText}{RangeOperator}{Until.RevisionText}";
        }
    }
}
