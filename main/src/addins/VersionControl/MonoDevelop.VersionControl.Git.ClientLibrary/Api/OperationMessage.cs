//*************************************************************************************************
// OperationMessage.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    public abstract class OperationMessage : OperationProgress
    {
        internal OperationMessage(Internal.IOperation operation, OperationProgressType type, string message)
            : base(operation, type)
        {
            if (ReferenceEquals(message, null))
                throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public readonly string Message;

        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// Message from, usually checkout, that a reference name is ambiguous but the operation was not aborted or prevented.
    /// </summary>
    public class AmbiguousReferenceWarningMessage : OperationMessage
    {
        internal AmbiguousReferenceWarningMessage(Internal.IOperation operation, string referenceName)
            : base(operation, OperationProgressType.AmbiguousReferenceWarning, CreateMessage(referenceName))
        {
            _referenceName = referenceName;
        }

        private readonly string _referenceName;

        public string ReferenceName
        {
            get { return _referenceName; }
        }

        internal static string CreateMessage(string referenceName)
        {
            return Invariant($"Warning: reference name '{referenceName}' is ambiguous.");
        }
    }

    /// <summary>
    /// Message from the applying changes phase of the operation.
    /// </summary>
    public sealed class ApplyingPatchMessage : OperationMessage
    {
        internal ApplyingPatchMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.ApplyingPatch, message)
        { }
    }

    /// <summary>
    /// A notification message from the current phase of an operation.
    /// </summary>
    public sealed class GenericOperationMessage : OperationMessage
    {
        internal GenericOperationMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.GenericOperation, message)
        { }
    }

    /// <summary>
    /// Hint
    /// </summary>
    public sealed class HintOperationMessage : OperationMessage
    {
        internal HintOperationMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.HintMessage, message)
        { }
    }

    /// <summary>
    /// Message from the merging files phase of an operation.
    /// </summary>
    public sealed class MergeOperationMessage : OperationMessage
    {
        internal MergeOperationMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.MergeOperation, message)
        { }
    }

    /// <summary>
    /// Message from a rebase operation that it is rewinding the HEAD commit.
    /// </summary>
    public sealed class RewindingHeadMessage : OperationMessage
    {
        internal RewindingHeadMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.RewindingHead, message)
        { }
    }

    /// <summary>
    /// Message from the waiting for remote phase of an operation.
    /// </summary>
    public sealed class WaitingForRemoteMessage : OperationMessage
    {
        internal WaitingForRemoteMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.WaitingForRemote, message)
        { }
    }

    /// <summary>
    /// A warning from the current phase of an operation.
    /// </summary>
    public sealed class WarningMessage : OperationMessage
    {
        internal WarningMessage(Internal.IOperation operation, string message, OperationErrorType type)
            : base(operation, OperationProgressType.Warning, message)
        {
            Error = new OperationError(message, type);
        }

        public readonly OperationError Error;
    }

    /// <summary>
    /// The working directory was updated by the current operation.
    /// </summary>
    public sealed class WorkingDirectoryUpdatedMessage: OperationMessage
    {
        internal WorkingDirectoryUpdatedMessage(Internal.IOperation operation, string message)
            : base(operation, OperationProgressType.WorkingDirectoryUpdated, message)
        { }
    }
}
