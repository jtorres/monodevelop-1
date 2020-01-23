//*************************************************************************************************
// CheckoutIndexCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            using (var command = new ArgumentList(Command))
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
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, paths, options);
                ExecuteCheckoutIndex(command, paths);
            }
        }

        /// <summary>
        /// Apply the options to the command.
        /// </summary>
        private void ApplyOptions(ArgumentList command, IEnumerable<string> paths, CheckoutIndexOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & CheckoutIndexOptionFlags.Force) != 0)
            {
                command.AddOption("--force");
            }

            if (paths != null)
            {
                command.AddOption("--stdin");
            }

            switch (options.Stage)
            {
                default:
                case CheckoutIndexOptionStage.Default:
                    break;

                case CheckoutIndexOptionStage.Stage1:
                    command.AddOption("--stage=1");
                    break;

                case CheckoutIndexOptionStage.Stage2:
                    command.AddOption("--stage=2");
                    break;

                case CheckoutIndexOptionStage.Stage3:
                    command.AddOption("--stage=3");
                    break;

                case CheckoutIndexOptionStage.All:
                    command.AddOption("--stage=all");
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
            using (IProcess process = CreateProcess(command, true))
            {
                try
                {
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

                    RunAndTestProcess(process, command);
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
