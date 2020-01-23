//*************************************************************************************************
// CheckoutOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class CheckoutOperation : Operation
    {
        private const string ErrorWouldOverwrite = PrefixError + "Your local changes to the following files would be overwritten by";
        private const string ErrorUntrackedDirectory = PrefixError + "Updating the following directories would lose untracked files";
        private const string ErrorUntrackedRemoved = PrefixError + "The following untracked working tree files would be removed by";
        private const string ErrorUntrackedOverwrite = PrefixError + "The following untracked working tree files would be overwritten";
        private const string ErrorSparseOutOfDate = PrefixError + "Cannot update sparse checkout: the following entries are not up-to-date";
        private const string ErrorSparseOverwrite = PrefixError + "The following Working tree files would be overwritten by";
        private const string ErrorSparseRemoved = PrefixError + "The following Working tree files would be removed by";

        internal CheckoutOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        { }

        protected override void ParseStdErr(Stream readableStream)
            => ParseInput(readableStream);

        protected override void ParseStdOut(Stream readableStream)
            => ParseInput(readableStream);

        private void ParseInput(Stream readableStream)
        {
            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            var parsers1 = new OperationParser[] { new CheckingOutFilesParser(this) };
            var parsers2 = new OperationParser[] 
            {
                new AmbiguousReferenceWarningParser(this),
                new WarningAndErrorParser(this)
            };

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    OperationProgress progress;

                    if (TryParse(line, parsers1, out progress))
                    {
                        Update(progress);
                    }
                    else if (line.StartsWith(PrefixError, StringComparison.Ordinal))
                    {
                        ICollection<CheckoutConflict> conflicts = null;

                        // handle special case errors
                        if (line.StartsWith(ErrorWouldOverwrite, StringComparison.Ordinal))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.TrackedFileOverwrite);
                        }
                        else if (line.StartsWith(ErrorUntrackedRemoved))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.UntrackedFileRemove);
                        }
                        else if (line.StartsWith(ErrorUntrackedOverwrite))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.UntrackedFileOverwrite);
                        }
                        else if (line.StartsWith(ErrorUntrackedDirectory))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.UntrackedFilesDirectoryUpdate);
                        }
                        else if (line.StartsWith(ErrorSparseOutOfDate))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.SparseFileOutOfDate);
                        }
                        else if (line.StartsWith(ErrorSparseOverwrite))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.SparseFilesOverwrite);
                        }
                        else if (line.StartsWith(ErrorSparseRemoved))
                        {
                            conflicts = ReadConflicts(reader, CheckoutConfictType.SparseFileRemove);
                        }

                        string message = line.Substring(PrefixError.Length);

                        if (conflicts?.Count > 0)
                        {
                            var exception = new CheckoutConflictException(message);
                            exception.AddConflictingPaths(conflicts);
                            throw exception;
                        }
                        else
                        {
                            Update(new WarningMessage(this, message, OperationErrorType.Error));
                        }
                    }
                    else if (TryParse(line, parsers2, out progress))
                    {
                        Update(progress);
                    }
                    else if (IsMessageFatal(line, reader))
                    {
                        break;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        progress = new GenericOperationMessage(this, line);

                        Update(progress);
                    }
                }
            }
        }

        private ICollection<CheckoutConflict> ReadConflicts(StreamReader reader, CheckoutConfictType type)
        {
            // git formats errors with "error: <message>\n" then repeats "\t<path>\n" for each
            // conflicted file, then it terminates - so were safe just reading to the end and
            // assuming that all lines which start with '\t' are paths.

            List<CheckoutConflict> conflicts = new List<CheckoutConflict>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("\t", StringComparison.Ordinal))
                {
                    string path = line.Substring(1);

                    conflicts.Add(new CheckoutConflict(path, type));
                }
            }

            return conflicts;
        }
    }
}
