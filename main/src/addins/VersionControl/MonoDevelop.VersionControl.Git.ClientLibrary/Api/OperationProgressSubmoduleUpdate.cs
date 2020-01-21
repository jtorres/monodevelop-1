//*************************************************************************************************
// OperationProgressSubmoduleUpdate.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;

namespace Microsoft.TeamFoundation.GitApi
{
    #region Submodule Update Completed

    /// <summary>
    /// The last step in a "git submodule update" (following any necessary clone or fetch) is to
    /// checkout/merge/rebase the working tree.
    /// </summary>
    public abstract class SubmoduleUpdateCompletedProgress : OperationProgress
    {
        internal SubmoduleUpdateCompletedProgress(Internal.IOperation operation, OperationProgressType type,
            string label, string subPath, string sha)
            : base(operation, type)
        {
            if (ReferenceEquals(label, null))
                throw new ArgumentNullException(nameof(label));
            if (ReferenceEquals(subPath, null))
                throw new ArgumentNullException(nameof(subPath));
            if (ReferenceEquals(sha, null))
                throw new ArgumentNullException(nameof(sha));

            Label = label;
            SubPath = subPath;
            Sha = sha;
        }

        public readonly string Label;
        public readonly string SubPath;
        public readonly string Sha;
    }

    public sealed class SubmoduleUpdateCompletedCheckoutProgress : SubmoduleUpdateCompletedProgress
    {
        public const string LabelCheckout = "Submodule checkout completed";

        internal SubmoduleUpdateCompletedCheckoutProgress(Internal.IOperation operation, string subPath, string sha)
            : base(operation, OperationProgressType.SubmoduleUpdateCheckoutCompleted, LabelCheckout, subPath, sha)
        { }
    }

    public sealed class SubmoduleUpdateCompletedMergeProgress : SubmoduleUpdateCompletedProgress
    {
        public const string LabelMerge = "Submodule merge completed";

        internal SubmoduleUpdateCompletedMergeProgress(Internal.IOperation operation, string subPath, string sha)
            : base(operation, OperationProgressType.SubmoduleUpdateMergeCompleted, LabelMerge, subPath, sha)
        { }
    }

    public sealed class SubmoduleUpdateCompletedRebaseProgress : SubmoduleUpdateCompletedProgress
    {
        public const string LabelRebase = "Submodule rebase completed";

        internal SubmoduleUpdateCompletedRebaseProgress(Internal.IOperation operation, string subPath, string sha)
            : base(operation, OperationProgressType.SubmoduleUpdateRebaseCompleted, LabelRebase, subPath, sha)
        { }
    }

    #endregion

    #region Submodule Update Path Notification

    /// <summary>
    /// Several steps in a "git submodule update" print the absolute path to the submodule.
    /// </summary>
    public abstract class SubmoduleUpdatePathProgress : OperationProgress
    {
        internal SubmoduleUpdatePathProgress(Internal.IOperation operation, OperationProgressType type,
            string label, string absPath)
            : base(operation, type)
        {
            if (ReferenceEquals(label, null))
                throw new ArgumentNullException(nameof(label));
            if (ReferenceEquals(absPath, null))
                throw new ArgumentNullException(nameof(absPath));

            Label = label;
            AbsPath = absPath;
        }

        public readonly string Label;
        public readonly string AbsPath;
    }

    public sealed class SubmoduleUpdateCloningIntoProgress : SubmoduleUpdatePathProgress
    {
        public const string LabelClone = "Cloning submodule";

        internal SubmoduleUpdateCloningIntoProgress(Internal.IOperation operation, string absPath)
            : base(operation, OperationProgressType.SubmoduleUpdateCloningInto, LabelClone, absPath)
        { }
    }
    #endregion

    public sealed class SubmoduleUpdateStepDoneProgress : OperationProgress
    {
        public const string Label = "Done";

        internal SubmoduleUpdateStepDoneProgress(Internal.IOperation operation)
            : base(operation, OperationProgressType.SubmoduleUpdateStepDone)
        { }
    }

    public sealed class SubmoduleUpdateRegistrationCompletedProgress : OperationProgress
    {
        public const string Label = "Submodule registration completed";

        internal SubmoduleUpdateRegistrationCompletedProgress(Internal.IOperation operation, string subName, string subUrl, string subPath)
            : base(operation, OperationProgressType.SubmoduleUpdateRegistrationCompleted)
        {
            if (ReferenceEquals(subName, null))
                throw new ArgumentNullException(nameof(subName));
            if (ReferenceEquals(subUrl, null))
                throw new ArgumentNullException(nameof(subUrl));
            if (ReferenceEquals(subPath, null))
                throw new ArgumentNullException(nameof(subPath));

            SubName = subName;
            SubUrl = subUrl;
            SubPath = subPath;
        }

        public readonly string SubName;
        public readonly string SubUrl;
        public readonly string SubPath;
    }
}
