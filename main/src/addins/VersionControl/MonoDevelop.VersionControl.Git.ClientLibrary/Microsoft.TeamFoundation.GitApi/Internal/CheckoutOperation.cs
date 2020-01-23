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
        static readonly OperationParser[] parsers1 = { new CheckingOutFilesParser() };
        static readonly OperationParser[] parsers2 = {
            new AmbiguousReferenceWarningParser(),
            new WarningAndErrorParser()
        };

        internal CheckoutOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        {
        }

        int parseState = 0;
        List<CheckoutConflict> conflicts = new List<CheckoutConflict> ();
        CheckoutConfictType conflictType;

        protected override bool ParseOutput(OperationOutput output)
        {
            string line = output.Message;

            switch (parseState)
            {
                case 0:
                    if (output == OperationOutput.OutputClosed)
                        break;
                    if (TryParse(line, parsers1, out var progress))
                    {
                        Update(progress);
                        return true;
                    }
                    else if (line.StartsWith(PrefixError, StringComparison.Ordinal))
                    {
                        conflicts.Clear();

                        // handle special case errors
                        if (line.StartsWith(ErrorWouldOverwrite, StringComparison.Ordinal))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.TrackedFileOverwrite;
                        }
                        else if (line.StartsWith(ErrorUntrackedRemoved))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.UntrackedFileRemove;
                        }
                        else if (line.StartsWith(ErrorUntrackedOverwrite))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.UntrackedFileOverwrite;
                        }
                        else if (line.StartsWith(ErrorUntrackedDirectory))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.UntrackedFilesDirectoryUpdate;
                        }
                        else if (line.StartsWith(ErrorSparseOutOfDate))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.SparseFileOutOfDate;
                        }
                        else if (line.StartsWith(ErrorSparseOverwrite))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.SparseFilesOverwrite;
                        }
                        else if (line.StartsWith(ErrorSparseRemoved))
                        {
                            parseState = 1;
                            conflictType = CheckoutConfictType.SparseFileRemove;
                        }
                        return false;
                    }
                    else if (TryParse(line, parsers2, out var progress2))
                    {
                        Update(progress2);
                        return true;
                    }
                    else if (IsMessageFatal(line))
                    {
                        parseState = 3;
                    }
                    else if ((line = CleanLine(line)) != null)
                    {
                        Update(new GenericOperationMessage(line));
                        return true;
                    }
                    break;
                case 1:
                    string message = "";
                    if (output != OperationOutput.OutputClosed)
                    {
                        if (line.StartsWith("\t", StringComparison.Ordinal))
                        {
                            string path = line.Substring(1);
                            conflicts.Add(new CheckoutConflict(path, conflictType));
                            break;
                        }
                        message = line.Substring(PrefixError.Length);
                    }

                    if (conflicts?.Count > 0)
                    {
                        var exception = new CheckoutConflictException(message);
                        exception.AddConflictingPaths(conflicts);
                        throw exception;
                    }
                    else
                    {
                        Update(new WarningMessage(message, OperationErrorType.Error));
                        return true;
                    }
                case 3:
                    ReadFault(output);
                    break;
            }

            return false;
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
