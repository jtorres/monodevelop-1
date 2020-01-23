//*************************************************************************************************
// CheckoutIndexCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Object model wrapper for "git-checkout-index.exe".
    /// </summary>
    internal class CheckoutIndexCommand : GitCommand
    {
        public const string Command = "checkout-index";

        public CheckoutIndexCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Copy all files from the index to the working tree.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-checkout-index.html</para>
        /// </summary>
        /// <param name="options">Checkout index options</param>
        public void CheckoutAll(CheckoutIndexOptions options)
        {
            // Build up the command buffer
            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, null, options);
                ExecuteCheckoutIndex(command, null);
            }
        }

        /// <summary>
        /// <para>Copy files from the index to the working tree.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-checkout-index.html</para>
        /// </summary>
        /// <param name="paths">File list</param>
        /// <param name="options">Checkout index options</param>
        public void CheckoutPaths(IEnumerable<string> paths, CheckoutIndexOptions options)
        {
            // Build up the command buffer
            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, paths, options);
                ExecuteCheckoutIndex(command, paths);
            }
        }

        /// <summary>
        /// Apply the options to the command.
        /// </summary>
        private void ApplyOptions(StringBuffer command, IEnumerable<string> paths, CheckoutIndexOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & CheckoutIndexOptionFlags.Force) != 0)
            {
                command.Append(" --force");
            }

            if (paths != null)
            {
                command.Append(" --stdin");
            }

            switch (options.Stage)
            {
                default:
                case CheckoutIndexOptionStage.Default:
                    break;

                case CheckoutIndexOptionStage.Stage1:
                    command.Append(" --stage=1");
                    break;

                case CheckoutIndexOptionStage.Stage2:
                    command.Append(" --stage=2");
                    break;

                case CheckoutIndexOptionStage.Stage3:
                    command.Append(" --stage=3");
                    break;

                case CheckoutIndexOptionStage.All:
                    command.Append(" --stage=all");
                    break;
            }
        }

        /// <summary>
        /// Execute the checkout-index command.
        /// </summary>
        private void ExecuteCheckoutIndex(string command, IEnumerable<string> paths)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command))
            {
                try
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });
                    var stdoutTask = Task.Run(() => { process.StdOut.CopyTo(System.IO.Stream.Null); });

                    // If a collection of paths is provided, send them to stdin one per line
                    // with a blank line at the end
                    if (paths != null)
                    {
                        foreach (string path in paths)
                        {
                            if (!string.IsNullOrEmpty(path))
                            {
                                process.StandardInput.Write(path + '\n');
                            }
                        }
                    }

                    TestExitCode(process, command, stderrTask);
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CheckoutIndexCommand)}.{nameof(ExecuteCheckoutIndex)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }
    }
}
