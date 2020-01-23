//*************************************************************************************************
// MergeBaseCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Object model wrapper for "git-merge-base".
    /// </summary>
    internal class MergeBaseCommand : GitCommand
    {
        public const string Command = "merge-base";

        /// <summary>
        /// Constructor.
        /// </summary>
        public MergeBaseCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Find as good common ancestors as possible for a merge.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-merge-base.html</para>
        /// </summary>
        public ObjectId FindMergeBase(IRevision revisionA, IRevision revisionB)
        {
            if (revisionA?.RevisionText == null)
                throw new ArgumentNullException(nameof(revisionA));
            if (revisionB?.RevisionText == null)
                throw new ArgumentNullException(nameof(revisionB));

            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                command.Add(revisionA.RevisionText);
                command.Add(revisionB.RevisionText);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out StringUtf8 standardOut);

                        TestExitCode(executeResult, command);

                        return standardOut.ToObjectId();
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(MergeBaseCommand)}.{nameof(FindMergeBase)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        /// <summary>
        /// <para>Check if revisionA is an ancestor of revisionB.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-merge-base.html</para>
        /// </summary>
        public bool IsAncestor(IRevision revisionA, IRevision revisionB)
        {
            if (revisionA?.RevisionText == null)
                throw new ArgumentNullException(nameof(revisionA));
            if (revisionB?.RevisionText == null)
                throw new ArgumentNullException(nameof(revisionB));

            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--is-ancestor");
                command.Add(revisionA.RevisionText);
                command.Add(revisionB.RevisionText);

                using (Tracer.TraceCommand(Command, command, userData: _userData))
                {
                    var executeResult = Execute(command, out string standardOut);

                    TestExitCode(executeResult, command, 1);

                    return (executeResult.ExitCode == 0);
                }
            }
        }
    }
}
