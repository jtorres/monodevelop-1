//*************************************************************************************************
// MergeConflictPatterns.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Set of regular expression patterns that match the various 'conflict' output lines from a merge.
    /// If a pattern matches, the named group 'file' contains the name/path to the conflicted file or directory.
    /// </summary>
    internal static class MergeConflictPatterns
    {
        /// <summary>
        /// All known merge conflict patterns.
        /// </summary>
        public static readonly IEnumerable<string> AllPatterns = new[]
        {
            FileContent,
            FileDelete,
            FileRename,
            DirectoryConflict,
            DirectoryRenameSplit,
            DirectoryRenameImplicit,
        };

        /// <summary>
        /// All known merge conflict patterns as compiled <see cref="Regex"/>s.
        /// </summary>
        public static readonly IEnumerable<Regex> AllRegexs = AllPatterns.Select(x => new Regex(x, RegexOptions.Compiled)).ToArray();

        /// <summary>
        /// Matches conflicts in files due to content differences.
        /// </summary>
        /// <example>CONFLICT (%s): Merge conflict in %s</example>
        public const string FileContent = @"CONFLICT.+Merge conflict in (?'file'.+)$";

        /// <summary>
        /// Matches conflicts in files where the file was deleted in one branch.
        /// </summary>
        /// <example>CONFLICT (%s/delete): %s deleted in %s and %s in %s. Version %s of %s left in tree.</example>
        /// <example>CONFLICT (%s/delete): %s deleted in %s and %s to %s in %s. Version %s of %s left in tree.</example>
        /// <example>CONFLICT (%s/delete): %s deleted in %s and %s in %s. Version %s of %s left in tree at %s.</example>
        /// <example>CONFLICT (%s/delete): %s deleted in %s and %s to %s in %s. Version %s of %s left in tree at %s.</example>
        public const string FileDelete = @"CONFLICT.+delete\): (?'file'.+) deleted";

        /// <summary>
        /// Matches conflicts in files where the file was renamed to different names in the two branches.
        /// </summary>
        /// <example>CONFLICT (rename/rename): Rename "%s"->"%s" in branch "%s" rename "%s"->"%s" in "%s"%s</example>
        /// <example>CONFLICT (rename/rename): Rename %s->%s in mybranch. Rename %s->%s in %s</example>
        /// <example>CONFLICT (rename/rename): Rename directory %s->%s in %s. Rename directory %s->%s in %s</example>
        /// <example>CONFLICT (rename/add): Rename %s->%s in %s. %s added in %s</example>
        public const string FileRename = @"CONFLICT.+\): Rename (?:directory )?\""?(?'file'.+?)\""?->";

        /// <summary>
        /// Matches conflicts in a file where a directory now exists with that name in the branch.
        /// </summary>
        /// <example>CONFLICT (%s): There is a directory with name %s in %s. Adding %s as %s</example>
        public const string DirectoryConflict = @"CONFLICT.+: There is a directory.+Adding (?'file'.+) as";

        /// <summary>
        /// Matches conflicts in a file where one of the containing directories had been renamed to different names in the two branches.
        /// </summary>
        /// <example>CONFLICT (directory rename split): Unclear where to place %s because directory %s was renamed to multiple other directories, with no destination getting a majority of the files.</example>
        public const string DirectoryRenameSplit = @"CONFLICT.+rename split\):.+where to place (?'file'.+) because";

        /// <summary>
        /// Matches conflicts in a file where the directory was implicitly renamed.
        /// </summary>
        /// <example>CONFLICT (implicit dir rename): Existing file/dir at %s in the way of implicit directory rename(s) putting the following path(s) there: %s.</example>
        public const string DirectoryRenameImplicit = @"CONFLICT.+implicit dir rename\):.+: (?'file'.+).$";
    }
}