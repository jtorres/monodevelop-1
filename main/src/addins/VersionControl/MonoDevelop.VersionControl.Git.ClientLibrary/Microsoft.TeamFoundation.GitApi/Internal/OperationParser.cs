using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal abstract class OperationParser
    {
        public abstract bool TryParse(string line, out OperationProgress progress);
    }

    /// <remarks>
    /// This parser must be placed before any parser or check for strings starting with "warning:",
    /// otherwise this parser might never be checked.
    /// </remarks>
    internal class AmbiguousReferenceWarningParser : OperationParser
    {
        private const string pattern = @"[Ww]arning:\s+refname\s+'([^']+)'\s+is\s+ambiguous";
        private static readonly Regex Regex = new Regex(pattern, RegexOptions.CultureInvariant);

        public override bool TryParse(string line, out OperationProgress progress)
        {
            Match match;
            if ((match = Regex.Match(line)).Success)
            {
                string referenceName = match.Groups[1].Value;

                progress = new AmbiguousReferenceWarningMessage(referenceName);
                return true;
            }

            progress = null;
            return false;
        }
    }

    internal sealed class ApplyingMessageParser : OperationParser
    {
        private const string Prefix = "Applying: ";

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                var message = line.Substring(Prefix.Length);
                progress = new ApplyingPatchMessage(message);
            }

            return progress != null;
        }
    }

    internal sealed class RewindingHeadMessageParser : OperationParser
    {
        private const string RewindMessage = "First, rewinding head to replay your work on top of it...";

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(RewindMessage, StringComparison.Ordinal))
            {
                progress = new RewindingHeadMessage(line);
            }

            return progress != null;
        }
    }

    internal sealed class CheckingOutFilesParser : OperationParser
    {
        // matches strings like: Checking out files:   2% (2493/100850)
        private const string Pattern = @"^\s*Checking\s+out\s+files:\s+(\d+)%\s+\((\d+)\/(\d+)\)";
        private const string Prefix = CheckingOutFilesProgress.Prefix + ":";

        public CheckingOutFilesParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _regex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _regex.Match(line)).Success)
                {
                    var completedStr = match.Groups[1].Value;
                    var fileCountStr = match.Groups[2].Value;
                    var fileTotalStr = match.Groups[3].Value;

                    double completed;
                    if (!Double.TryParse(completedStr, out completed))
                        return false;

                    completed /= 100d;

                    long fileCount;
                    if (!Int64.TryParse(fileCountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out fileCount))
                        return false;

                    long fileTotal;
                    if (!Int64.TryParse(fileTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out fileTotal))
                        return false;

                    progress = new CheckingOutFilesProgress(completed, fileCount, fileTotal);
                }
            }

            return progress != null;
        }
    }

    internal sealed class CompressingObjectsParser : OperationParser
    {
        // matches strings like: Compressing objects:   2% (2493/100850)
        private const string Pattern = @"^\s*Compressing\s+objects:\s+(\d+)%\s+\((\d+)\/(\d+)\)";
        private const string Prefix = CompressingObjectsProgress.Prefix + ":";

        public CompressingObjectsParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _regex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _regex.Match(line)).Success)
                {
                    var completedStr = match.Groups[1].Value;
                    var objectCountStr = match.Groups[2].Value;
                    var objectTotalStr = match.Groups[3].Value;

                    double completed;
                    if (!Double.TryParse(completedStr, NumberStyles.Number, CultureInfo.InvariantCulture, out completed))
                        return false;

                    completed = completed / 100d;

                    long objectCount;
                    if (!Int64.TryParse(objectCountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectCount))
                        return false;

                    long objectTotal;
                    if (!Int64.TryParse(objectTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectTotal))
                        return false;

                    progress = new CompressingObjectsProgress(completed, objectCount, objectTotal);
                }
            }

            return progress != null;
        }
    }

    internal sealed class CountingObjectsParser : OperationParser
    {
        // matches strings like: Compressing objects:   2% (2493/100850)
        private const string Pattern = @"^\s*Counting\s+objects:\s+Total\s+(\d+)\s+\(delta\s+(\d+)\)\s+reused\s+(\d+)\s+\(delta\s+(\d+)\)";
        private const string Prefix = CountingObjectsProgress.Prefix + ":";

        public CountingObjectsParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _regex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _regex.Match(line)).Success)
                {
                    var objectsTotalStr = match.Groups[1].Value;
                    var deltasTotalStr = match.Groups[2].Value;
                    var objectsReusedStr = match.Groups[3].Value;
                    var deltasReusedStr = match.Groups[4].Value;

                    long objectsTotal;
                    if (!Int64.TryParse(objectsTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsTotal))
                        return false;

                    long deltasTotal;
                    if (!Int64.TryParse(deltasTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out deltasTotal))
                        return false;

                    long objectsReused;
                    if (!Int64.TryParse(objectsReusedStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsReused))
                        return false;

                    long deltasReused;
                    if (!Int64.TryParse(deltasReusedStr, NumberStyles.Number, CultureInfo.InvariantCulture, out deltasReused))
                        return false;

                    progress = new CountingObjectsProgress(objectsTotal, deltasTotal, objectsReused, deltasReused);
                }
            }

            return progress != null;
        }
    }

    internal sealed class HintMessageParser : OperationParser
    {
        public override bool TryParse(string line, out OperationProgress progress)
        {
            const string Prefix = "hint: ";

            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal) )
            {
                // strip off the prefix and trailing whitespace off
                string message = line.Substring(Prefix.Length, line.Length - Prefix.Length)
                                     .TrimEnd();
                progress = new HintOperationMessage(message);
            }

            return progress != null;
        }
    }

    internal sealed class ReceivingObjectsParser : OperationParser
    {
        // matches strings like: Receiving objects:  23% (920758/3974313), 5.68 MiB | 2.53 MiB/s
        private const string PatternLong = @"^\s*Receiving\s+objects:\s+(\d+)%\s+\((\d+)\/(\d+)\),\s+([\d\.]+)\s+([\w]+)\s\|\s+([\d\.]+)\s+([\w]+)\/s";
        // matches strings like: Receiving objects:  23% (920758/3974313)
        private const string PatternShort = @"^\s*Receiving\s+objects:\s+(\d+)%\s+\((\d+)\/(\d+)\)";
        private const string Prefix = ReceivingObjectsProgress.Prefix + ":";

        public ReceivingObjectsParser()
        {
            _longRegex = new Regex(PatternLong, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _shortRegex = new Regex(PatternShort, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _longRegex;
        private readonly Regex _shortRegex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _longRegex.Match(line)).Success)
                {
                    var completeStr = match.Groups[1].Value;
                    var objectsReadStr = match.Groups[2].Value;
                    var objectsTotalStr = match.Groups[3].Value;
                    var readBytesStr = match.Groups[4].Value;
                    var readMagnitude = match.Groups[5].Value;
                    var readRateStr = match.Groups[6].Value;
                    var rateMagnitude = match.Groups[7].Value;

                    double completed = Double.NaN;
                    if (!Double.TryParse(completeStr, NumberStyles.Number, CultureInfo.InvariantCulture, out completed))
                        return false;

                    completed /= 100d;

                    long objectsRead;
                    if (!Int64.TryParse(objectsReadStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsRead))
                        return false;

                    long objectsTotal;
                    if (!Int64.TryParse(objectsTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsTotal))
                        return false;

                    double readBytesDbl;
                    if (!Double.TryParse(readBytesStr, NumberStyles.Number, CultureInfo.InvariantCulture, out readBytesDbl))
                        return false;

                    long readBytes = StringHelper.GetBytesFromMagnitude(readBytesDbl, readMagnitude);

                    double readRateDbl;
                    if (!Double.TryParse(readRateStr, NumberStyles.Number, CultureInfo.InvariantCulture, out readRateDbl))
                        return false;

                    long readRate = StringHelper.GetBytesFromMagnitude(readRateDbl, rateMagnitude);

                    progress = new ReceivingObjectsProgress(completed, objectsRead, objectsTotal, readBytes, readRate);
                }
                else if ((match = _shortRegex.Match(line)).Success)
                {
                    var completeStr = match.Groups[1].Value;
                    var objectsReadStr = match.Groups[2].Value;
                    var objectsTotalStr = match.Groups[3].Value;
                    var readBytesStr = match.Groups[4].Value;
                    var readMagnitude = match.Groups[5].Value;
                    var readRateStr = match.Groups[6].Value;
                    var rateMagnitude = match.Groups[7].Value;

                    double completed = Double.NaN;
                    if (!Double.TryParse(completeStr, NumberStyles.Number, CultureInfo.InvariantCulture, out completed))
                        return false;

                    completed /= 100d;

                    long objectsRead;
                    if (!Int64.TryParse(objectsReadStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsRead))
                        return false;

                    long objectsTotal;
                    if (!Int64.TryParse(objectsTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectsTotal))
                        return false;

                    progress = new ReceivingObjectsProgress(completed, objectsRead, objectsTotal, 0, 0);
                }
            }

            return progress != null;
        }
    }

    internal sealed class RemoteMessageParser : OperationParser
    {
        private const string Prefix = "remote: ";

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                var message = line.Substring(Prefix.Length);
                progress = new WaitingForRemoteMessage(message);
            }

            return progress != null;
        }
    }

    internal sealed class ResolvingDeltasParser : OperationParser
    {
        // matches strings like: Resolving deltas:   0% (0/1998315)
        private const string Pattern = @"^\s*Resolving\s+deltas:\s+(\d+)%\s+\((\d+)\/(\d+)\)";
        private const string Prefix = ResolvingDeltasProgress.Prefix + ":";

        public ResolvingDeltasParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _regex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _regex.Match(line)).Success)
                {
                    var completedStr = match.Groups[1].Value;
                    var deltaCountStr = match.Groups[2].Value;
                    var deltaTotalStr = match.Groups[3].Value;

                    double completed;
                    if (!Double.TryParse(completedStr, NumberStyles.Number, CultureInfo.InvariantCulture, out completed))
                        return false;

                    completed /= 100d;

                    long deltaCount;
                    if (!Int64.TryParse(deltaCountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out deltaCount))
                        return false;

                    long deltaTotal;
                    if (!Int64.TryParse(deltaTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out deltaTotal))
                        return false;

                    progress = new ResolvingDeltasProgress(completed, deltaCount, deltaTotal);
                }
            }

            return progress != null;
        }
    }

    #region Submodule Update responses

    internal sealed class SubmoduleUpdateModeCompletedParser : OperationParser
    {
        // Submodule path 'sub_a': checked out '445c908626f0261e390a6df6062b068eb178101f'
        private const string PatternCheckout = @"^\s*Submodule path '([^']+)': checked out '([^']+)'";
        // Submodule path 'sub_a': merged in '445c908626f0261e390a6df6062b068eb178101f'
        private const string PatternMerge = @"^\s*Submodule path '([^']+)': merged in '([^']+)'";
        // Submodule path 'sub_a': rebased into '445c908626f0261e390a6df6062b068eb178101f'
        private const string PatternRebase = @"^\s*Submodule path '([^']+)': rebased into '([^']+)'";
        private readonly Regex _regexCheckout;
        private readonly Regex _regexMerge;
        private readonly Regex _regexRebase;

        public SubmoduleUpdateModeCompletedParser()
        {
            _regexCheckout = new Regex(PatternCheckout, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _regexMerge = new Regex(PatternMerge, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _regexRebase = new Regex(PatternRebase, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match matchCheckout = _regexCheckout.Match(line);
            if (matchCheckout.Success)
            {
                progress = new SubmoduleUpdateCompletedCheckoutProgress(matchCheckout.Groups[1].Value, matchCheckout.Groups[2].Value);
                return true;
            }

            Match matchMerge = _regexMerge.Match(line);
            if (matchMerge.Success)
            {
                progress = new SubmoduleUpdateCompletedMergeProgress(matchMerge.Groups[1].Value, matchMerge.Groups[2].Value);
                return true;
            }

            Match matchRebase = _regexRebase.Match(line);
            if (matchRebase.Success)
            {
                progress = new SubmoduleUpdateCompletedRebaseProgress(matchRebase.Groups[1].Value, matchRebase.Groups[2].Value);
                return true;
            }

            // Consider handling messages from custom update mode (where
            // an arbitrary command is run rather than checkout/merge/rebase).

            return false;
        }
    }

    internal sealed class SubmoduleUpdateCloningIntoParser : OperationParser
    {
        // Cloning into 'path'...
        private const string Pattern = @"^\s*Cloning into '([^']+)'";
        private readonly Regex _regex;

        public SubmoduleUpdateCloningIntoParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match;
            if ((match = _regex.Match(line)).Success)
            {
                progress = new SubmoduleUpdateCloningIntoProgress(match.Groups[1].Value);
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateStepDoneParser : OperationParser
    {
        // We usually get 2 lines:
        //     Cloning into 'path'...
        //     done.
        // Here we handle the second line.
        // Since we might get this from other commands (such as fetch),
        // we just report the "done" here.  If the caller cares, they
        // can keep state on the previously-reported line 1.
        private const string Prefix = "done.";

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                progress = new SubmoduleUpdateStepDoneProgress();
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateNotInitializedParser : OperationParser
    {
        // $ git submodule update sub_a
        // Submodule path 'sub_a' not initialized
        // Maybe you want to use 'update --init'?
        //
        // $ echo $?
        // 0
        //
        // Note how "git submodule update" does not return an error code,
        // so we have to throw an error.
        private const string Pattern = @"^\s*Submodule path '([^']+)' not initialized";
        private readonly Regex _regex;

        public SubmoduleUpdateNotInitializedParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match = _regex.Match(line);
            if (match.Success)
            {
                var exception = new SubmoduleUpdateNotInitializedException(match.Groups[1].Value);
                throw exception;
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateRegistrationCompletedParser : OperationParser
    {
        // Submodule 'sub_a' (url) registered for path 'sub_a'
        // TODO.GitApi: confirm that the first term is the "name"
        // TODO.GitApi: rather than another copy of the path.
        private const string Pattern = @"^\s*Submodule '([^']+)' \(([^)]+)\) registered for path '([^']+)'";
        private readonly Regex _regex;

        public SubmoduleUpdateRegistrationCompletedParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match = _regex.Match(line);
            if (match.Success)
            {
                progress = new SubmoduleUpdateRegistrationCompletedProgress(match.Groups[1].Value,
                                                                            match.Groups[2].Value,
                                                                            match.Groups[3].Value);
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateRevisionNotFound : OperationParser
    {
        // "git submodule update" should exit with an error code,
        // but if we see these we can give a tighter error message.

        private const string Prefix = "Unable to find current ";
        // Unable to find current revision in submodule path '<path>'
        private const string Pattern1 = @"\s*Unable to find current revision in submodule path '([^']+)'";
        // Unable to find current <remote>/<branch> revision in submodule path '<path>'
        private const string Pattern2 = @"\s*Unable to find current ([^ ]+) revision in submodule path '([^']+)'";
        private readonly Regex _regex1;
        private readonly Regex _regex2;

        public SubmoduleUpdateRevisionNotFound()
        {
            _regex1 = new Regex(Pattern1, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _regex2 = new Regex(Pattern2, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match1 = _regex1.Match(line);
            if (match1.Success)
            {
                var exception = new SubmoduleUpdateRevisionNotFoundException(match1.Groups[1].Value);
                throw exception;
            }

            Match match2 = _regex2.Match(line);
            if (match2.Success)
            {
                var exception = new SubmoduleUpdateRevisionNotFoundException(match1.Groups[1].Value, match1.Groups[2].Value);
                throw exception;
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateUnmergedParser : OperationParser
    {
        // $ git merge b
        // warning: Failed to merge submodule foo/bar/mysub (merge following commits not found)
        // Auto-merging foo/bar/mysub
        // CONFLICT (submodule): Merge conflict in foo/bar/mysub
        // Automatic merge failed; fix conflicts and then commit the result.
        //
        // $ git submodule status
        // U0000000000000000000000000000000000000000 foo/bar/mysub
        //
        // $ git submodule--helper list --prefix .
        // 160000 0000000000000000000000000000000000000000 U       foo/bar/mysub
        //
        // $ git submodule--helper list foo/bar/mysub
        // 160000 0000000000000000000000000000000000000000 U       foo/bar/mysub
        //
        // $ git submodule update foo/bar/mysub
        // Skipping unmerged submodule foo/bar/mysub
        //
        // $ echo $?
        // 0
        //
        // Note how "git submodule update" does not return an error for this,
        // so we have to throw an error.
        private const string Pattern = @"^\s*Skipping unmerged submodule (.*)";
        private readonly Regex _regex;

        public SubmoduleUpdateUnmergedParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match = _regex.Match(line);
            if (match.Success)
            {
                var exception = new SubmoduleUpdateUnmergedException(match.Groups[1].Value);
                throw exception;
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateUnableToCompleteParser : OperationParser
    {
        // "git submodule update" should exit with an error code,
        // but if we see these we can give a tighter error message.

        // Unable to checkout 'sha' in submodule path 'path'
        private const string PatternCheckout = @"^\s*Unable to checkout '([^']+)' in submodule path '([^']+)'";
        // Unable to merge 'sha' in submodule path 'path'
        private const string PatternMerge = @"^\s*Unable to merge '([^']+)' in submodule path '([^']+)'";
        // Unable to rebase 'sha' in submodule path 'path'
        private const string PatternRebase = @"^\s*Unable to rebase '([^']+)' in submodule path '([^']+)'";
        private readonly Regex _regexCheckout;
        private readonly Regex _regexMerge;
        private readonly Regex _regexRebase;

        public SubmoduleUpdateUnableToCompleteParser()
        {
            _regexCheckout = new Regex(PatternCheckout, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _regexMerge = new Regex(PatternMerge, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
            _regexRebase = new Regex(PatternRebase, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match matchCheckout = _regexCheckout.Match(line);
            if (matchCheckout.Success)
            {
                var exception = new SubmoduleUpdateUnableToCompleteCheckoutException(matchCheckout.Groups[1].Value, matchCheckout.Groups[2].Value);
                throw exception;
            }

            Match matchMerge = _regexMerge.Match(line);
            if (matchMerge.Success)
            {
                var exception = new SubmoduleUpdateUnableToCompleteMergeException(matchMerge.Groups[1].Value, matchMerge.Groups[2].Value);
                throw exception;
            }

            Match matchRebase = _regexRebase.Match(line);
            if (matchRebase.Success)
            {
                var exception = new SubmoduleUpdateUnableToCompleteRebaseException(matchRebase.Groups[1].Value, matchRebase.Groups[2].Value);
                throw exception;
            }

            // Consider handling messages from custom update mode (where
            // an arbitrary command is run rather than checkout/merge/rebase).

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateUnableToFetchParser : OperationParser
    {
        // "git submodule update" should exit with an error code,
        // but if we see these we can give a tighter error message.

        private const string Pattern = @"^\s*Unable to fetch in submodule path '([^']+)'";
        private readonly Regex _regex;

        public SubmoduleUpdateUnableToFetchParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match = _regex.Match(line);
            if (match.Success)
            {
                var exception = new SubmoduleUpdateUnableToFetchException(match.Groups[1].Value);
                throw exception;
            }

            return progress != null;
        }
    }

    internal sealed class SubmoduleUpdateUnableToRecurseParser : OperationParser
    {
        // "git submodule update" should exit with an error code,
        // but if we see these we can give a tighter error message.

        private const string Pattern = @"^\s*Failed to recurse into submodule path '([^']+)'";
        private readonly Regex _regex;

        public SubmoduleUpdateUnableToRecurseParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            Match match = _regex.Match(line);
            if (match.Success)
            {
                var exception = new SubmoduleUpdateUnableToRecurseException(match.Groups[1].Value);
                throw exception;
            }

            return progress != null;
        }
    }

    #endregion

    internal sealed class WarningAndErrorParser : OperationParser
    {
        private const string PrefixError = Operation.PrefixError;
        private const string PrefixWarning = Operation.PrefixWarning;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(PrefixError, StringComparison.Ordinal))
            {
                var message = line.Substring(PrefixError.Length);
                progress = new WarningMessage(message, OperationErrorType.Error);
            }
            else if (line.StartsWith(PrefixWarning, StringComparison.Ordinal))
            {
                var message = line.Substring(PrefixWarning.Length);
                progress = new WarningMessage(message, OperationErrorType.Warning);
            }

            return progress != null;
        }
    }

    internal sealed class WarningParser : OperationParser
    {

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Operation.PrefixWarning, StringComparison.Ordinal))
            {
                var message = line.Substring(Operation.PrefixWarning.Length);
                progress = new WarningMessage( message, OperationErrorType.Warning);
            }

            return progress != null;
        }
    }

    internal sealed class WritingObjectsParser : OperationParser
    {
        // matches strings like: Writing objects:   0% (0/1998315)
        private const string Pattern = @"^\s*Writing\sobjects\:\s+(\d+)%\s+\((\d+)\/(\d+)\)";
        private const string Prefix = WritingObjectsProgress.Prefix + ":";

        public WritingObjectsParser()
        {
            _regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        private readonly Regex _regex;

        public override bool TryParse(string line, out OperationProgress progress)
        {
            progress = null;

            if (line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                Match match;
                if ((match = _regex.Match(line)).Success)
                {
                    var completedStr = match.Groups[1].Value;
                    var objectCountStr = match.Groups[2].Value;
                    var objectTotalStr = match.Groups[3].Value;

                    double completed;
                    if (!Double.TryParse(completedStr, NumberStyles.Number, CultureInfo.InvariantCulture, out completed))
                        return false;

                    completed /= 100d;

                    long objectCount;
                    if (!Int64.TryParse(objectCountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectCount))
                        return false;

                    long objectTotal;
                    if (!Int64.TryParse(objectTotalStr, NumberStyles.Number, CultureInfo.InvariantCulture, out objectTotal))
                        return false;

                    progress = new WritingObjectsProgress(completed, objectCount, objectTotal);
                }
            }

            return progress != null;
        }
    }
}
