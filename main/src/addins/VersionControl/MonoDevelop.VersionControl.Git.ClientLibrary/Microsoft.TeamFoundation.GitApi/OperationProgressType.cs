//*************************************************************************************************
// OperationProgressType.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public enum OperationProgressType
    {
        Initializing = 0,

        /// <summary>
        /// `<see cref="AmbiguousReferenceWarningMessage"/>`.
        /// </summary>
        AmbiguousReferenceWarning,

        /// <summary>
        /// `<see cref="ApplyingPatchMessage"/>`.
        /// </summary>
        ApplyingPatch,

        /// <summary>
        /// `<see cref="CheckingOutFilesProgress"/>`.
        /// </summary>
        CheckingOutFiles,

        /// <summary>
        /// `<see cref="CompressingObjectsProgress"/>`.
        /// </summary>
        CompressingObjects,

        /// <summary>
        /// `<see cref="CountingObjectsProgress"/>`.
        /// </summary>
        CountingObjects,

        /// <summary>
        /// `<see cref="GenericOperationMessage"/>`.
        /// </summary>
        GenericOperation,

        /// <summary>
        /// `<see cref="HintOperationMessage"/>`.
        /// </summary>
        HintMessage,

        /// <summary>
        /// `<see cref="MergeOperationMessage"/>`.
        /// </summary>
        MergeOperation,

        /// <summary>
        /// `<see cref="ReceivingObjectsProgress"/>`.
        /// </summary>
        ReceivingObjects,

        /// <summary>
        /// `<see cref="ResolvingDeltasProgress"/>`.
        /// </summary>
        ResolvingDeltas,

        /// <summary>
        /// `<see cref="RewindingHeadMessage"/>`
        /// </summary>
        RewindingHead,

        /// <summary>
        /// `<see cref="SubmoduleUpdateCheckoutCompleted"/>`
        /// </summary>
        SubmoduleUpdateCheckoutCompleted,

        /// <summary>
        /// `<see cref="SubmoduleUpdateCloningIntoProgress"/>`.
        /// </summary>
        SubmoduleUpdateCloningInto,

        /// <summary>
        /// `<see cref="SubmoduleUpdateCompletedMergeProgress"/>`.
        /// </summary>
        SubmoduleUpdateMergeCompleted,

        /// <summary>
        /// `<see cref="SubmoduleUpdateCompletedRebaseProgress"/>`.
        /// </summary>
        SubmoduleUpdateRebaseCompleted,

        /// <summary>
        /// `<see cref="SubmoduleUpdateStepDoneProgress"/>`.
        /// </summary>
        SubmoduleUpdateStepDone,

        /// <summary>
        /// `<see cref="SubmoduleUpdateRegistrationCompletedProgress"/>`.
        /// </summary>
        SubmoduleUpdateRegistrationCompleted,

        /// <summary>
        /// `<see cref="WaitingForRemoteMessage"/>`.
        /// </summary>
        WaitingForRemote,

        /// <summary>
        /// `<see cref="WarningMessage"/>`.
        /// </summary>
        Warning,

        /// <summary>
        /// `<see cref="WorkingDirectoryUpdatedMessage"/>`.
        /// </summary>
        WorkingDirectoryUpdated,

        /// <summary>
        /// `<see cref="WritingObjectsProgress"/>`.
        /// </summary>
        WritingObjects
    }
}
