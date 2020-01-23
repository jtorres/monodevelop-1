//*************************************************************************************************
// CommitCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    /// <summary>
    /// Object model wrapper for "git-commit".
    /// </summary>
    internal class CommitCommand : GitCommand
    {
        public const string Command = "commit";

        /// <summary>
        /// Constructor.
        /// </summary>
        public CommitCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        /// <summary>
        /// <para>Record changes to the repository.</para>
        /// <para>https://www.kernel.org/pub/software/scm/git/docs/git-commit.html</para>
        /// </summary>
        public ObjectId Commit(string message, CommitOptions options)
        {
            if (string.IsNullOrEmpty(message) && (options.Flags & CommitOptionFlags.AllowEmptyMessage) == 0)
                throw new InvalidOptionsException($"The `{nameof(message)}' parameter must not empty, or the `{nameof(options)}' parameter must specify `{nameof(CommitOptionFlags.AllowEmptyMessage)}`.");

            // Build up the command buffer
            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, message, options);
                return ExecuteCommit(command, message);
            }
        }

        /// <summary>
        /// Apply command options.
        /// </summary>
        private void ApplyOptions(ArgumentList command, string message, CommitOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & CommitOptionFlags.AllowEmpty) != 0)
            {
                command.AddOption("--allow-empty");
            }
            if ((options.Flags & CommitOptionFlags.AllowEmptyMessage) != 0)
            {
                command.AddOption("--allow-empty-message");
            }
            if ((options.Flags & CommitOptionFlags.Amend) != 0)
            {
                command.AddOption("--amend");
            }
            if ((options.Flags & CommitOptionFlags.Only) != 0)
            {
                command.AddOption("--only");
            }

            if (!string.IsNullOrEmpty(options.AuthorName) && !string.IsNullOrEmpty(options.AuthorEmail))
            {
                command.AddOption ("--author", $"\"{options.AuthorName} <{options.AuthorEmail}>\"");
            }

            // If a message is provided, send it in via stdin
            if (!string.IsNullOrEmpty(message))
            {
                command.AddOption("--file=-");
            }
        }

        /// <summary>
        /// Execute the commit command.
        /// </summary>
        private ObjectId ExecuteCommit(string command, string message)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            ObjectId commitId = ObjectId.Zero;

            ConfigurationArguments?.Add(new ConfigurationEntry("core.abbrev", "40"));

            try
            {
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command, true))
                {
                    process.ProcessOutput += (sender, o) => {
                        if (o.Source == OutputSource.Out)
                        {
                            if (commitId == ObjectId.Zero)
                                commitId = ParseLine(o.Message);
                        }
                    };

                    // write the message to stdin
                    if (!string.IsNullOrEmpty(message))
                    {
                        var encBytes = Encoding.UTF8.GetBytes(message + "\n");

                        process.StdIn.Write(encBytes, 0, encBytes.Length);
                    }

                    RunAndTestProcess(process, command);
                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CommitCommand)}.{nameof(ExecuteCommit)}", exception))
            {
                // not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            return commitId;
        }

        ObjectId ParseLine(string buffer)
        {
            // The typical format of the commit summary is:
            //
            //      [branch commitid] subject\n
            //       1 file changed, 1 insertion(+), 1 deletion(-)\n
            //
            // There can be additional metadata for special commits like a root commit:
            //
            //      [branch (root-commit) commitid] subject\n
            //       1 file changed, 1 insertion(+), 1 deletion(-)\n
            //       create mode 100644 file.txt\n
            //
            // The branch name can contain ']', and it can even end with ']'.  So
            // it's not reliable to just search for the bracket.
            //
            //      [bran]ch] commitid] subject\n
            //       1 file changed, 1 insertion(+), 1 deletion(-)\n

            // Skip the branch name by finding the first space.  This is done so
            // the next search can be for ']'.
            int space = buffer.IndexOf(' ');
            if (space < 0)
                throw new CommitParseException("space", new StringUtf8(buffer), 0);

            // Find the first ']' character after the branch name
            int closeBracket = buffer.IndexOf(']', space);
            if (closeBracket < 0)
                throw new CommitParseException("]", new StringUtf8(buffer), space);

            // By specifying "-c core.abbrev=40", the commit ID will be the full
            // sha instead of the typical short sha.
            if (closeBracket < 40)
                throw new CommitParseException("ID too short", new StringUtf8(buffer), closeBracket);

            return ObjectId.FromString(buffer, closeBracket - ObjectId.Length);
        }
    }
}
