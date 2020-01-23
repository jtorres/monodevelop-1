//*************************************************************************************************
// RevertOperation.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class RevertOperation : Operation
    {
        public RevertOperation(ExecutionContext context, OperationProgressDelegate progressCallback)
            : base(context, progressCallback)
        { }

        protected override void ParseStdErr(Stream readableStream)
            => ParseInput(readableStream);

        protected override void ParseStdOut(Stream readableStream)
            => ParseInput(readableStream);

        private void ParseInput(Stream readableStream)
        {
            const string PrefixSkipped = "Skipped ";

            if (ReferenceEquals(readableStream, null))
                throw new ArgumentNullException(nameof(readableStream));
            if (!readableStream.CanRead)
                throw new ArgumentException(nameof(readableStream));

            // SAMPLE OUTPUT
            //
            // > revert <commit>
            //
            //      Success output
            //
            //          [master e4ffdbe] Revert "Edited file"
            //          1 file changed, 1 insertion(+), 1 deletion(-)
            //
            //      Conflict output
            //
            //          error: could not revert a56fb1d... Edited file
            //          hint: after resolving the conflicts, mark the corrected paths
            //          hint: with 'git add <paths>' or 'git rm <paths>'
            //          hint: and commit the result with 'git commit'
            //
            //      Conflict alternate output
            //
            //          CONFLICT (modify/delete): file.txt deleted in (empty tree) and modified in HEAD. Version HEAD of file.txt left in tree.
            //
            //      Skipped output
            //
            //          Skipped file.txt (merged same as existing)
            //
            // > revert --continue
            //
            //      Success output
            //
            //          [master e4ffdbe] Revert "Edited file"
            //          1 file changed, 1 insertion(+), 1 deletion(-)
            //
            //      Conflict output
            //
            //          U       file.txt
            //          error: commit is not possible because you have unmerged files.
            //          hint: Fix them up in the work tree, and then use 'git add/rm <file>'
            //          hint: as appropriate to mark resolution and make a commit.
            //          fatal: Exiting because of an unresolved conflict.
            //

            var parser = new OperationParser[] { new WarningAndErrorParser(this), };

            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    OperationProgress progress;

                    if (line.StartsWith(PrefixSkipped, StringComparison.Ordinal))
                    {
                        string message = line.Substring(PrefixSkipped.Length);

                        progress = new GenericOperationMessage(this, message);

                        Update(progress);
                    }
                    else if (TryParse(line, parser, out progress))
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
    }
}
