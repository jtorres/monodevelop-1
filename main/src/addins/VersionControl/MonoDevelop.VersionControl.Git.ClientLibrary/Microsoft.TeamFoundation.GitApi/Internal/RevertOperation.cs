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
        const string PrefixSkipped = "Skipped ";

        static readonly OperationParser[] parser = { new WarningAndErrorParser() };

        public RevertOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        { }

        protected override bool ParseOutput(OperationOutput output)
        {
            if (output == OperationOutput.OutputClosed)
                return false;
            string line = output.Message;
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

            if (line.StartsWith(PrefixSkipped, StringComparison.Ordinal))
            {
                string message = line.Substring(PrefixSkipped.Length);
                Update(new GenericOperationMessage(message));
            }
            else if (TryParse(line, parser, out var progress))
            {
                Update(progress);
                return true;
            }
            else if (IsMessageFatal(line))
            {
                return false;
            }
            else if ((line = CleanLine(line)) != null)
            {
                Update(new GenericOperationMessage(line));
            }
            return false;
        }
    }
}
