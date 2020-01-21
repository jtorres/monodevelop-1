//*************************************************************************************************
// CommitRange.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Abstract commit range type.
    /// <para/>
    /// Use `<see cref="ExclusiveRange"/>` or `<see cref="InclusiveRange"/>`.
    /// </summary>
    public abstract class CommitRange
    {
        protected CommitRange(IRevision since, IRevision until)
        {
            if (since == null)
                throw new ArgumentNullException(nameof(since));
            if (until == null)
                throw new ArgumentNullException(nameof(until));

            _since = since;
            _until = until;

            if (_since == null)
                throw new ReferenceTypeMismatchException(nameof(since));
            if (_until == null)
                throw new ReferenceTypeMismatchException(nameof(until));
        }

        /// <summary>
        /// Oldest revision in the range.
        /// </summary>
        internal IRevision Since
        {
            get { return _since; }
        }
        private IRevision _since;

        /// <summary>
        /// Newest revision in the range.
        /// </summary>
        internal IRevision Until
        {
            get { return _until; }
        }
        private IRevision _until;
    }
}
