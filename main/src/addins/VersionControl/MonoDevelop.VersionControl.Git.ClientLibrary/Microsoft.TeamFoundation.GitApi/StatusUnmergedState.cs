//*************************************************************************************************
// StatusSnapshotUnmergedState.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Unmerged, or conflict, state of a status entry.
    /// <para/>
    /// Details of why a status entry failed to merge, and was placed into a conflict state.
    /// </summary>
    public enum StatusUnmergedState
    {
        None = 0,

        /// <summary>
        /// 'DD' Both deleted.
        /// </summary>
        DeleteDelete = 1,

        /// <summary>
        /// 'AU' Added by us (somehow unmerged/conflict).
        /// </summary>
        AddedUnmerged = 2,

        /// <summary>
        /// 'UD' Deleted by them.
        /// </summary>
        UnmergedDeleted = 3,

        /// <summary>
        /// 'UA' Added by them (somehow unmerged/conflict).
        /// </summary>
        UnmergedAdded = 4,

        /// <summary>
        /// 'DU' Deleted by us.
        /// </summary>
        DeletedUnmerged = 5,

        /// <summary>
        /// 'AA' Both added.
        /// </summary>
        AddedAdded = 6,

        /// <summary>
        /// 'UU' Both modified.
        /// </summary>
        UnmergedUnumerged = 7
    }
}
