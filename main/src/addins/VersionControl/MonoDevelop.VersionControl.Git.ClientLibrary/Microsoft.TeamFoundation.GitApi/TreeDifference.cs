//*************************************************************************************************
// TreeDifference.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git tree-difference.
    /// <para/>
    /// Represents the differences between one or more trees in a history.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public interface ITreeDifference : IEnumerable<ITreeDifferenceEntry>
    {
        /// <summary>
        /// Gets a read-only list of the entries contained in the tree-difference.
        /// </summary>
        IReadOnlyList<ITreeDifferenceEntry> Entries { get; }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal class TreeDifference : Base, ILoggable, ITreeDifference
    {
        internal TreeDifference()
            : base()
        {
            _entries = new List<ITreeDifferenceEntry>();
        }

        private List<ITreeDifferenceEntry> _entries;

        [JsonProperty]
        public IReadOnlyList<ITreeDifferenceEntry> Entries
        {
            get { return _entries; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private string DebuggerDisplay
        {
            get { return Invariant($"{nameof(TreeDifference)}: [{_entries.Count}]"); }
        }

        public IEnumerator<ITreeDifferenceEntry> GetEnumerator()
        {
            foreach (var entry in _entries)
            {
                yield return entry;
            }
        }

        internal void AddEntry(ITreeDifferenceEntry entry)
        {
            _entries.Add(entry);
        }

        /// <summary>
        /// Parse diff --raw output.
        /// </summary>
        internal ParseResult ParseRaw(ByteBuffer buffer, ref int index, int count, int maxEntries)
        {
            const char Eol = '\n';
            const char SourceCountToken = ':';
            const char Break = '*';
            const char Nul = '\0';

            // The format of the output will look like this:
            //
            //              srcMod tgtMod srcId      tgtId   type srcFile tgtFile
            //             ---------------------------------------------------------
            // edit        :100644 100644 bcd1234... 0123456... M file0
            // copy edit   :100644 100644 abcd123... 1234567... C68 file1 file2
            // rename edit :100644 100644 abcd123... 1234567... R86 file1 file3
            // create      :000000 100644 0000000... 1234567... A file4
            // delete      :100644 000000 1234567... 0000000... D file5
            // unmerged    :000000 000000 0000000... 0000000... U file6
            //
            // The format for a merge commit will look like this:
            //
            //               srcMod srcMod tgtMod srcId      srcId      tgtId    type srcFile tgtFile
            //             ----------------------------------------------------------------------------
            // merge       ::100644 100644 100644 fabadb8... cc95eb0... 4866510... MM         describe.c
            while (index < count)
            {
                // find the next LF character
                int eol = buffer.FirstIndexOf(Eol, index, count - index);
                if (eol < 0)
                {
                    // If eol was not found, we need more buffer paged in
                    // move the read idx by the read amount, which will
                    // trigger a buffer page
                    return ParseResult.MatchIncomplete;
                }

                int i1 = index,
                    i2 = index;

                if (buffer[index] != SourceCountToken)
                {
                    // Assuming that we've reached this because of the break token
                    if (buffer[index] != Break)
                        throw new DifferenceParseException("src-count", new StringUtf8(buffer, 0, count), index);

                    return ParseResult.NoMatch;
                }

                // Count the number of ':' characters at the start of the output line,
                // the number of ':' character indicates the number of parent commits.
                while (i1 < count && buffer[i1] == SourceCountToken)
                {
                    i1 += 1;
                }

                // Calculate the number of source (aka parent) commits.
                int srcCount = i1 - index;
                // Commits with more than a single parent are a merges.
                bool isMerge = srcCount > 1;

                ObjectId[] srcOids = new ObjectId[srcCount];
                TreeEntryDetailMode[] srcModes = new TreeEntryDetailMode[srcCount];
                TreeDifferenceType[] srcTypes = new TreeDifferenceType[srcCount];

                i1 -= 1;
                i2 = i1;

                // Parse the mode of each src (parent) commit for this record.
                for (int i = 0; i < srcCount; i += 1)
                {
                    i1 = i2;

                    // The modes are separated by a single ' ' character.
                    i2 = buffer.FirstIndexOf(' ', i1 + 1, eol - i1 - 1);
                    if (i2 < 0)
                        throw new DifferenceParseException("src-mode-space", new StringUtf8(buffer, 0, count), index);

                    // Capture the UTF-8 encoded string representation of the octal mode value.
                    string parsedSrcMode = Encoding.ASCII.GetString(buffer, i1 + 1, i2 - i1 - 1);

                    // Attempt to parse a decimal number from the octal mode value.
                    uint srcModeValue;
                    if (!uint.TryParse(parsedSrcMode, NumberStyles.Number, CultureInfo.InvariantCulture, out srcModeValue))
                        throw new DifferenceParseException("src-mode", new StringUtf8(buffer, 0, count), index);

                    // Cast the decimal value of the octal mode value to the typed enumeration
                    // (yes - the enum is in base10 where the string representation matches the base8 values)
                    srcModes[i] = (TreeEntryDetailMode)srcModeValue;
                }

                i1 = i2;

                // Parse the target (current) commit's mode value for this record.
                i2 = buffer.FirstIndexOf(' ', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                    throw new DifferenceParseException("tgt-mode-space", new StringUtf8(buffer, 0, count), index);

                // Capture the UTF-8 encoded string representation of the octal mode value.
                string parsedTgtMode = Encoding.ASCII.GetString(buffer, i1 + 1, i2 - i1 - 1);

                // Attempt to parse a decimal number from the octal mode value.
                uint tgtModeValue;
                if (!uint.TryParse(parsedTgtMode, NumberStyles.Number, CultureInfo.InvariantCulture, out tgtModeValue))
                    throw new DifferenceParseException("tgt-mode", new StringUtf8(buffer, 0, count), index);

                // Cast the decimal value of the octal mode value to the typed enumeration.
                // (yes - the enum is in base10 where the string representation matches the base8 values)
                TreeEntryDetailMode tgtMode = (TreeEntryDetailMode)tgtModeValue;

                i1 = i2;

                // Parse the object id of each src (parent) commit
                for (int i = 0; i < srcCount; i += 1)
                {
                    i1 = i2;

                    // The object id are separated by ' ' characters
                    i2 = buffer.FirstIndexOf(' ', i1 + 1, eol - i1 - 1);
                    if (i2 < 0)
                        throw new DifferenceParseException("src-oid-space", new StringUtf8(buffer, 0, count), index);

                    // Copy the UTF-8 encoded SHA-1 object id value
                    StringUtf8 parsedSrcOid = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);
                    if (parsedSrcOid.Length != 40)
                        throw new DifferenceParseException("src-oid", new StringUtf8(buffer, 0, count), index);

                    // Convert the UTF-8 encoded value into an object id
                    srcOids[i] = parsedSrcOid.ToObjectId();
                }

                i1 = i2;

                // Parse the object if of the target  (current) commit.
                // The object id are separated by ' ' characters
                i2 = buffer.FirstIndexOf(' ', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                    throw new DifferenceParseException("tgt-oid-space", new StringUtf8(buffer, 0, count), index);

                // Copy the UTF-8 encoded SHA-1 object id value
                StringUtf8 parsedTgtOid = new StringUtf8(buffer, i1 + 1, i2 - i1 - 1);
                if (parsedTgtOid.Length != 40)
                    throw new DifferenceParseException("tgt-oid", new StringUtf8(buffer, 0, count), index);

                // Convert the UTF-8 encoded value into an object id
                ObjectId tgtOid = parsedTgtOid.ToObjectId();

                i1 = i2;

                // The initial data is separated from the path data by a tab character
                i2 = buffer.FirstIndexOf('\t', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                    throw new DifferenceParseException("type-tab", new StringUtf8(buffer, 0, count), index);

                // The last of the initial data ends with a set of change type values
                // Read the whole list of change types
                string parsedModes = Encoding.ASCII.GetString(buffer, i1 + 1, i2 - i1 - 1);
                int confidence = 0;

                // Compute the details source (parent) commit's data
                for (int i = 0; i < srcCount; i += 1)
                {
                    // Convert the difference type from the parse mode
                    TreeDifferenceType type = Cli.DiffCommand.GetDifferenceType(parsedModes[i]);
                    if (type == TreeDifferenceType.Invalid)
                        throw new DifferenceParseException("type", new StringUtf8(buffer, 0, count), index);

                    srcTypes[i] = type;

                    // Parse the rename confidence from the record, unless it's a merge commit.
                    // Git does not calculate the rename confidence for merge commits.
                    if (!isMerge
                        && (type == TreeDifferenceType.Renamed || type == TreeDifferenceType.Copied))
                    {
                        int x = i + 1;

                        while (x < parsedModes.Length && char.IsNumber(parsedModes[x]))
                        {
                            x += 1;
                        }

                        if (x != i)
                        {
                            string conStr = parsedModes.Substring(i + 1, x - i - 1);
                            int conVal;
                            if (ParseHelper.TryParseNumber(conStr, out conVal))
                            {
                                confidence = conVal;
                                i = x + 1;
                            }
                            else
                            {
                                // Git can return renamed entries with no confidence information, which means
                                // even though the type is renamed, there is no rename data - so we have to
                                // treat it as a generic edit.
                                confidence = 0;
                            }
                        }
                    }
                }

                // Create a difference detail for the target (current) commit.
                TreeDifferenceDetail[] srcDetails = new TreeDifferenceDetail[srcCount];
                TreeDifferenceDetail tgtDetail = new TreeDifferenceDetail(tgtOid, tgtMode, TreeDifferenceType.Unmodified);

                // Allocate a difference detail for each source (parent) commit
                for (int i = 0; i < srcCount; i += 1)
                {
                    TreeDifferenceType type = srcTypes[i];

                    // Merge commits do not contain detailed rename information, therefore we
                    // change the type to "in parent" version where the event really happened.
                    if (isMerge && type == TreeDifferenceType.Renamed)
                    {
                        type = TreeDifferenceType.RenamedInParent;
                    }
                    else if (isMerge && type == TreeDifferenceType.Copied)
                    {
                        type = TreeDifferenceType.CopiedInParent;
                    }

                    TreeDifferenceDetail srcDetail = new TreeDifferenceDetail(srcOids[i], srcModes[i], type);
                    srcDetails[i] = srcDetail;
                }

                // Move the read cursors ahead
                i1 = i2;

                // The second cursor is either at EOL now (merge or no renames) or at the start of the old path
                i2 = buffer.FirstIndexOf('\t', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                    i2 = eol;

                // Copy the path
                StringUtf8 parsedSrc = PathHelper.ParsePathValue(buffer, i1 + 1, i2 - i1 - 1);
                if (parsedSrc is null || parsedSrc.Length == 0)
                    throw new DifferenceParseException("src-file", new StringUtf8(buffer, 0, count), index);

                // Only copy the original path if this is not a merge commit and a rename entry
                StringUtf8 parsedTgt = (!isMerge && i2 != eol)
                    ? PathHelper.ParsePathValue(buffer, i2 + 1, eol - i2 - 1)
                    : null;

                if (!isMerge
                    && srcTypes.Length == 1
                    && (srcTypes[0] == TreeDifferenceType.Renamed || srcTypes[0] == TreeDifferenceType.Copied)
                    && (parsedTgt is null || parsedTgt.Length == 0))
                    throw new DifferenceParseException("tgt-file", new StringUtf8(buffer, 0, count), index);

                StringUtf8 srcPath = null;
                StringUtf8 tgtPath = null;

                TreeDifferenceEntry entry = null;

                // If the entry is a merge or there's no parsed target, then it is a normal entry;
                if (isMerge || parsedTgt == null)
                {
                    parsedTgt = parsedSrc;
                    tgtPath = parsedTgt;

                    entry = new TreeDifferenceEntry(tgtPath, tgtDetail, srcDetails);
                }
                // Otherwise, it is either a copied or renamed file entry.
                else
                {
                    Debug.Assert(srcTypes != null && srcTypes.Length > 0);

                    srcPath = parsedSrc;
                    tgtPath = parsedTgt;

                    // Since renames and copies only have a single parent, we can safely rely on the
                    // the first entry in source types array.
                    if (srcTypes[0] == TreeDifferenceType.Renamed)
                    {
                        entry = new TreeDifferenceRenamedEntry(srcPath, tgtPath, confidence, tgtDetail, srcDetails);
                    }
                    else if (srcTypes[0] == TreeDifferenceType.Copied)
                    {
                        entry = new TreeDifferenceCopiedEntry(srcPath, tgtPath, confidence, tgtDetail, srcDetails);
                    }
                }

                if (entry != null)
                {
                    entry.SetContext(Context);
                }
                _entries.Add(entry);

                index = eol + 1;

                if (maxEntries > 0 && _entries.Count == maxEntries)
                {
                    return ParseResult.MatchComplete;
                }
                else if (index == count)
                {
                    return ParseResult.MatchMaybeComplete;
                }
                else if (index < buffer.Length && buffer[index] == Nul)
                {
                    return ParseResult.MatchComplete;
                }
            }

            return ParseResult.MatchIncomplete;
        }

        /// <summary>
        /// Parse diff --name-status output.
        /// </summary>
        internal ParseResult ParseNameStatus(ByteBuffer buffer, ref int index, int count, int maxEntries)
        {
            const char Eol = '\n';
            const char Nul = '\0';

            // The format of the output will look like this:
            //
            //          type srcFile tgtFile
            //------------------------------
            // edit        M file0
            // copy edit   C68 file1 file2
            // rename edit R86 file1 file3
            // create      A file4
            // delete      D file5
            // unmerged    U file6
            while (index < count)
            {
                // find the next LF character
                int eol = buffer.FirstIndexOf(Eol, index, count - index);
                if (eol < 0)
                {
                    // If eol was not found, we need more buffer paged in
                    // move the read idx by the read amount, which will
                    // trigger a buffer page
                    return ParseResult.MatchIncomplete;
                }

                int i1 = index,
                    i2 = index;

                // The initial data is separated from the path data by a tab character
                i2 = buffer.FirstIndexOf('\t', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                    throw new DifferenceParseException("type-tab", new StringUtf8(buffer, 0, count), index);

                // The last of the initial data ends with a set of change type values
                // Read the whole list of change types
                string parsedMode = Encoding.ASCII.GetString(buffer, i1, i2 - i1);
                int confidence = 0;

                // Convert the difference type from the parse mode
                TreeDifferenceType type = Cli.DiffCommand.GetDifferenceType(parsedMode[0]);
                if (type == TreeDifferenceType.Invalid)
                {
                    throw new DifferenceParseException("type", new StringUtf8(buffer, 0, count), index);
                }

                // Parse the rename confidence from the record, unless it's a merge commit.
                // Git does not calculate the rename confidence for merge commits.
                if (type == TreeDifferenceType.Renamed || type == TreeDifferenceType.Copied)
                {
                    int x = 1;

                    while (x < parsedMode.Length && char.IsNumber(parsedMode[x]))
                    {
                        x += 1;
                    }

                    if (x != 1)
                    {
                        string conStr = parsedMode.Substring(1, x - 1);
                        int conVal;
                        if (ParseHelper.TryParseNumber(conStr, out conVal))
                        {
                            confidence = conVal;
                        }
                        else
                        {
                            // Git can return renamed entries with no confidence information, which means
                            // even though the type is renamed, there is no rename data - so we have to
                            // treat it as a generic edit.
                            confidence = 0;
                        }
                    }
                }

                // Use an unmodified difference detail for the target (current) commit.
                TreeDifferenceNameStatusDetail tgtDetail = TreeDifferenceNameStatusDetail.Unmodified;

                // Allocate a difference detail for each source (parent) commit
                TreeDifferenceNameStatusDetail[] srcDetails = new TreeDifferenceNameStatusDetail[1];
                srcDetails[0] = new TreeDifferenceNameStatusDetail(type);

                // Move the read cursors ahead
                i1 = i2;

                // The second cursor is either at EOL now (merge or no renames) or at the start of the old path
                i2 = buffer.FirstIndexOf('\t', i1 + 1, eol - i1 - 1);
                if (i2 < 0)
                {
                    i2 = eol;
                }

                // Copy the path
                StringUtf8 parsedSrc = PathHelper.ParsePathValue(buffer, i1 + 1, i2 - i1 - 1);
                if (parsedSrc is null || parsedSrc.Length == 0)
                {
                    throw new DifferenceParseException("src-file", new StringUtf8(buffer, 0, count), index);
                }

                // Only copy the original path if this is not a merge commit and a rename entry
                StringUtf8 parsedTgt = (i2 != eol)
                    ? PathHelper.ParsePathValue(buffer, i2 + 1, eol - i2 - 1)
                    : null;

                if ((type == TreeDifferenceType.Renamed || type == TreeDifferenceType.Copied) &&
                    (parsedTgt is null || parsedTgt.Length == 0))
                {
                    throw new DifferenceParseException("tgt-file", new StringUtf8(buffer, 0, count), index);
                }

                StringUtf8 srcPath = null;
                StringUtf8 tgtPath = null;

                TreeDifferenceEntry entry = null;

                // If there's no parsed target, then it is a normal entry;
                if (parsedTgt == null)
                {
                    parsedTgt = parsedSrc;
                    tgtPath = parsedTgt;

                    entry = new TreeDifferenceEntry(tgtPath, tgtDetail, srcDetails);
                }
                // Otherwise, it is either a copied or renamed file entry.
                else
                {
                    srcPath = parsedSrc;
                    tgtPath = parsedTgt;

                    // Since renames and copies only have a single parent, we can safely rely on the
                    // the first entry in source types array.
                    if (type == TreeDifferenceType.Renamed)
                    {
                        entry = new TreeDifferenceRenamedEntry(srcPath, tgtPath, confidence, tgtDetail, srcDetails);
                    }
                    else if (type == TreeDifferenceType.Copied)
                    {
                        entry = new TreeDifferenceCopiedEntry(srcPath, tgtPath, confidence, tgtDetail, srcDetails);
                    }
                }

                Debug.Assert(entry != null);
                entry.SetContext(Context);
                _entries.Add(entry);

                index = eol + 1;

                if (maxEntries > 0 && _entries.Count == maxEntries)
                {
                    return ParseResult.MatchComplete;
                }
                else if (index == count)
                {
                    return ParseResult.MatchMaybeComplete;
                }
                else if (index < buffer.Length && buffer[index] == Nul)
                {
                    return ParseResult.MatchComplete;
                }
            }

            return ParseResult.MatchIncomplete;
        }

        void ILoggable.Log(ExecutionContext context, StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);

            log.Append(prefix).AppendLine(nameof(TreeDifference));
            log.Append(prefix).AppendLine("{");
            {
                foreach (ITreeDifferenceEntry entry in Entries)
                {
                    (entry as ILoggable)?.Log(context, log, indent + 1);
                }
            }
            log.Append(prefix).AppendLine("}");
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
