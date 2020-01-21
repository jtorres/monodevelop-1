//*************************************************************************************************
// InclusiveRange.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representing a Git inclusive commit-ish range.
    /// </summary>
    public class InclusiveRange : CommitRange
    {
        public const string RangeOperator = "...";

        /// <summary>
        /// Creates a new instance of <see cref="ExclusiveRange"/>.
        /// </summary>
        /// <param name="since">The oldest revision in the range.</param>
        /// <param name="until">The newest revision in the range.</param>
        public InclusiveRange(IRevision since, IRevision until)
            : base(since, until)
        { }

        public override string ToString()
        {
            return $"{Since.RevisionText}{RangeOperator}{Until.RevisionText}";
        }
    }
}
