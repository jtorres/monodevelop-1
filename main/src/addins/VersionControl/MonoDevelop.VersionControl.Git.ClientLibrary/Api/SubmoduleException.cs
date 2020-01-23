//*************************************************************************************************
// SubmoduleUpdateException.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public abstract class SubmoduleUpdateException : ExceptionBase
    {
        internal SubmoduleUpdateException(string message)
            : base(message)
        { }
    }

    /// <summary>
    /// Cannot update an uninitialized submodule.
    /// See "git submodule update --init".
    /// </summary>
    public sealed class SubmoduleUpdateNotInitializedException : SubmoduleUpdateException
    {
        internal SubmoduleUpdateNotInitializedException(string path)
            : base(path)
        { }
    }

    /// <summary>
    /// Requested commit not present in submodule.
    /// </summary>
    public sealed class SubmoduleUpdateRevisionNotFoundException : SubmoduleUpdateException
    {
        public const string Label = "Submodule revision not found";

        internal SubmoduleUpdateRevisionNotFoundException(string subPath)
            : base($"{Label}: {subPath}")
        {
            SubPath = subPath;
        }

        internal SubmoduleUpdateRevisionNotFoundException(string remoteBranch, string subPath)
            : base($"{Label}: {remoteBranch} {subPath}")
        {
            RemoteBranch = remoteBranch;
            SubPath = subPath;
        }

        public readonly string RemoteBranch;
        public readonly string SubPath;
    }

    /// <summary>
    /// Unable to complete the update because checkout, merge, or rebase failed.
    /// </summary>
    public abstract class SubmoduleUpdateUnableToCompleteException : SubmoduleUpdateException
    {
        internal SubmoduleUpdateUnableToCompleteException(string label, string sha, string subPath)
            : base($"{label}: {sha} {subPath}")
        {
            Label = label;
            Sha = sha;
            SubPath = subPath;
        }

        public readonly string Label;
        public readonly string Sha;
        public readonly string SubPath;
    }

    public sealed class SubmoduleUpdateUnableToCompleteCheckoutException : SubmoduleUpdateUnableToCompleteException
    {
        public const string LabelCheckout = "Submodule update unable to complete checkout";

        internal SubmoduleUpdateUnableToCompleteCheckoutException(string sha, string subPath)
            : base(LabelCheckout, sha, subPath)
        { }
    }

    public sealed class SubmoduleUpdateUnableToCompleteMergeException : SubmoduleUpdateUnableToCompleteException
    {
        public const string LabelMerge = "Submodule update unable to complete merge";

        internal SubmoduleUpdateUnableToCompleteMergeException(string sha, string subPath)
            : base(LabelMerge, sha, subPath)
        { }
    }
    public sealed class SubmoduleUpdateUnableToCompleteRebaseException : SubmoduleUpdateUnableToCompleteException
    {
        public const string LabelRebase = "Submodule update unable to complete rebase";

        internal SubmoduleUpdateUnableToCompleteRebaseException(string sha, string subPath)
            : base(LabelRebase, sha, subPath)
        { }
    }

    /// <summary>
    /// Unable to fetch submodule.
    /// </summary>
    public sealed class SubmoduleUpdateUnableToFetchException : SubmoduleUpdateException
    {
        internal SubmoduleUpdateUnableToFetchException(string path)
            : base(path)
        { }
    }

    /// <summary>
    /// Unable to recurse into a submodule within this submodule.
    /// </summary>
    public sealed class SubmoduleUpdateUnableToRecurseException : SubmoduleUpdateException
    {
        internal SubmoduleUpdateUnableToRecurseException(string path)
            : base(path)
        { }
    }

    /// <summary>
    /// Cannot update a submodule that is a part of a merge conflict in the super.
    /// </summary>
    public sealed class SubmoduleUpdateUnmergedException : SubmoduleUpdateException
    {
        internal SubmoduleUpdateUnmergedException(string path)
            : base(path)
        { }
    }
}
