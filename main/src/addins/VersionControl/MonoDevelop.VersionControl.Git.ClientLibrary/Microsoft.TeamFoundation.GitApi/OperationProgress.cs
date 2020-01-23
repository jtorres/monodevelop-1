//*************************************************************************************************
// OperationProgress.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a long-running Git operation progress updates.
    /// <para/>
    /// Base class of `<seealso cref="CheckingOutFilesProgress"/>`, `<seealso cref="ReceivingObjectsProgress"/>`, etc.
    /// </summary>
    public abstract class OperationProgress
    {
        internal OperationProgress( OperationProgressType type)
        {
            Debug.Assert(Enum.IsDefined(typeof(OperationProgressType), type), $"The `{nameof(type)}` parameter is undefined.");

            Type = type;
        }

        public string Name
        {
            get { return Type.ToString(); }
        }

        public readonly OperationProgressType Type;

    }

    /// <summary>
    /// Callback delegate to receive progress updates during an operation.
    /// </summary>
    public abstract class OperationCallback
    {
        public abstract bool OperationProgress (OperationProgress operationProgress);
        public abstract void OperationOutput (OperationOutput output);
    }

    /// <summary>
    /// Progress information related to the checking out files phase of an operation.
    /// </summary>
    public sealed class CheckingOutFilesProgress : OperationProgress
    {
        public const string Prefix = "Checking out files";

        internal CheckingOutFilesProgress(double completed, long fileCount, long fileTotal)
            : base(OperationProgressType.CheckingOutFiles)
        {
            Debug.Assert(completed >= 0 && completed <= 1, $"The `{nameof(completed)}` parameter has an unexpected value.");
            Debug.Assert(fileCount >= 0, $"The `{nameof(fileCount)}` parameter is less than zero.");
            Debug.Assert(fileTotal >= 0, $"The `{nameof(fileTotal)}` parameter is less than zero.");

            Completed = completed;
            FileCount = fileCount;
            FileTotal = fileTotal;
        }

        public readonly double Completed;
        public readonly long FileCount;
        public readonly long FileTotal;

        public override string ToString()
        {
            return $"{Prefix}: {(Completed * 100d).ToString("N0"):+3}% ({FileCount}/{FileTotal})";
        }
    }

    /// <summary>
    /// Progress information related to the compressing objects phase of an operation.
    /// </summary>
    public sealed class CompressingObjectsProgress : OperationProgress
    {
        public const string Prefix = "Compressing objects";

        internal CompressingObjectsProgress(double completed, long objectCount, long objectTotal)
            : base(OperationProgressType.CompressingObjects)
        {
            Debug.Assert(completed >= 0 && completed <= 1, $"The `{nameof(completed)}` parameter has an unexpected value.");
            Debug.Assert(objectCount >= 0, $"The `{nameof(objectCount)}` parameter is less than zero.");
            Debug.Assert(objectTotal >= 0, $"The `{nameof(objectTotal)}` parameter is less than zero.");

            Completed = completed;
            ObjectCount = objectCount;
            ObjectTotal = objectTotal;
        }

        public readonly double Completed;
        public readonly long ObjectCount;
        public readonly long ObjectTotal;

        public override string ToString()
        {
            return $"{Prefix}: {(Completed * 100d).ToString("N0"):+3}% ({ObjectCount}/{ObjectTotal})";
        }
    }

    /// <summary>
    /// Progress information related to the counting objects phase of an operation.
    /// </summary>
    public sealed class CountingObjectsProgress : OperationProgress
    {
        public const string Prefix = "Counting objects";

        internal CountingObjectsProgress(long objectsTotal, long deltasTotal, long objectsReused, long deltaReused)
            : base(OperationProgressType.CountingObjects)
        {
            Debug.Assert(objectsTotal >= 0, $"The `{nameof(objectsTotal)}` parameter is less than zero.");
            Debug.Assert(deltasTotal >= 0, $"The `{nameof(deltasTotal)}` parameter is less than zero.");
            Debug.Assert(objectsReused >= 0, $"The `{nameof(objectsReused)}` parameter is less than zero.");
            Debug.Assert(deltaReused >= 0, $"The `{nameof(deltaReused)}` parameter is less than zero.");

            ObjectsTotal = objectsTotal;
            ObjectsReused = objectsReused;
            DeltasTotal = deltasTotal;
            DeltasReused = deltaReused;
        }

        public readonly long ObjectsTotal;
        public readonly long ObjectsReused;
        public readonly long DeltasTotal;
        public readonly long DeltasReused;

        public override string ToString()
        {
            return $"{Prefix}: Total {ObjectsTotal} (delta {DeltasTotal}) reused {ObjectsReused} (delta {DeltasReused})";
        }
    }

    /// <summary>
    /// Progress information related to the receiving objects phase of an operation.
    /// </summary>
    public sealed class ReceivingObjectsProgress : OperationProgress
    {
        public const string Prefix = "Receiving objects";

        internal ReceivingObjectsProgress(double completed, long objectsRead, long objectsTotal, long readBytes, long readRate)
            : base(OperationProgressType.ReceivingObjects)
        {
            Debug.Assert(completed >= 0 && completed <= 1, $"The `{nameof(completed)}` parameter has an unexpected value.");
            Debug.Assert(objectsRead >= 0, $"The `{nameof(objectsRead)}` parameter is less than zero.");
            Debug.Assert(objectsTotal >= 0, $"The `{nameof(objectsTotal)}` parameter is less than zero.");
            Debug.Assert(readBytes >= 0, $"The `{nameof(readBytes)}` parameter is less than zero.");
            Debug.Assert(readRate >= 0, $"The `{nameof(readRate)}` parameter is less than zero.");

            Completed = completed;
            ObjectsRead = objectsRead;
            ObjectsTotal = objectsTotal;
            ReadBytes = readBytes;
            ReadRate = readRate;
        }

        public readonly double Completed;
        public readonly long ObjectsRead;
        public readonly long ObjectsTotal;
        public readonly long ReadBytes;
        public string ReadBytesText
            => Internal.StringHelper.GetMagnitudeText(ReadBytes);
        public readonly long ReadRate;
        public string ReadRateText
            => $"{Internal.StringHelper.GetMagnitudeText(ReadRate)}/s";

        public override string ToString()
        {
            return $"{Prefix}: {(Completed * 100d).ToString("N0"):+3}% ({ObjectsRead}/{ObjectsTotal}), {ReadBytesText} | {ReadRateText}";
        }
    }

    /// <summary>
    /// Progress information related to the resolving deltas phase of an operation.
    /// </summary>
    public sealed class ResolvingDeltasProgress : OperationProgress
    {
        public const string Prefix = "Resolving deltas";

        internal ResolvingDeltasProgress(double completed, long deltaCount, long deltaTotal)
            : base(OperationProgressType.ResolvingDeltas)
        {
            Debug.Assert(completed >= 0 && completed <= 1, $"The `{nameof(completed)}` parameter has an unexpected value.");
            Debug.Assert(deltaCount >= 0, $"The `{nameof(deltaCount)}` parameter is less than zero.");
            Debug.Assert(deltaTotal >= 0, $"The `{nameof(deltaTotal)}` parameter is less than zero.");

            Completed = completed;
            DeltaCount = deltaCount;
            DeltaTotal = deltaTotal;
        }

        public readonly double Completed;
        public readonly long DeltaCount;
        public readonly long DeltaTotal;

        public override string ToString()
        {
            return $"{Prefix}: {(Completed * 100d).ToString("N0"):+3}% ({DeltaCount}/{DeltaTotal})";
        }
    }

    /// <summary>
    /// Progress information related to the writing objects phase of an operation.
    /// </summary>
    public sealed class WritingObjectsProgress : OperationProgress
    {
        public const string Prefix = "Writing objects";

        internal WritingObjectsProgress(double completed, long objectCount, long objectTotal)
            : base(OperationProgressType.WritingObjects)
        {
            Debug.Assert(completed >= 0 && completed <= 1, $"The `{nameof(completed)}` parameter has an unexpected value.");
            Debug.Assert(objectCount >= 0, $"The `{nameof(objectCount)}` parameter is less than zero.");
            Debug.Assert(objectTotal >= 0, $"The `{nameof(objectTotal)}` parameter is less than zero.");

            Completed = completed;
            ObjectCount = objectCount;
            ObjectTotal = objectTotal;
        }

        public readonly double Completed;
        public readonly long ObjectCount;
        public readonly long ObjectTotal;

        public override string ToString()
        {
            return $"{Prefix}: {(Completed * 100d).ToString("N0"):+3}% ({ObjectCount}/{ObjectTotal})";
        }
    }
}
