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
            using (var command = new StringBuffer(Command))
            {
                ApplyOptions(command, message, options);
                return ExecuteCommit(command, message);
            }
        }

        /// <summary>
        /// Apply command options.
        /// </summary>
        private void ApplyOptions(StringBuffer command, string message, CommitOptions options)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            if ((options.Flags & CommitOptionFlags.AllowEmpty) != 0)
            {
                command.Append(" --allow-empty");
            }
            if ((options.Flags & CommitOptionFlags.AllowEmptyMessage) != 0)
            {
                command.Append(" --allow-empty-message");
            }
            if ((options.Flags & CommitOptionFlags.Amend) != 0)
            {
                command.Append(" --amend");
            }
            if ((options.Flags & CommitOptionFlags.Only) != 0)
            {
                command.Append(" --only");
            }

            if (!string.IsNullOrEmpty(options.AuthorName) && !string.IsNullOrEmpty(options.AuthorEmail))
            {
                command.Append(" --author=").Append($"\"{options.AuthorName} <{options.AuthorEmail}>\"");
            }

            // If a message is provided, send it in via stdin
            if (!string.IsNullOrEmpty(message))
            {
                command.Append(" --file=-");
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
                using (var buffer = new ByteBuffer())
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command))
                {
                    var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                    // write the message to stdin
                    if (!string.IsNullOrEmpty(message))
                    {
                        var encBytes = Encoding.UTF8.GetBytes(message + "\n");

                        process.StdIn.Write(encBytes, 0, encBytes.Length);
                    }

                    // close stdin to signal the end of the message
                    process.StdIn.Close();

                    int read = 0; // The count of bytes read into the buffer
                    int idx = 0; // The first unparsed byte in the buffer

                    int r;
                    while (read < buffer.Length && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) > 0)
                    {
                        read += r;

                        while (idx < read)
                        {
                            // Find the LF character
                            int eol = buffer.FirstIndexOf(Eol, idx, read - idx);
                            if (eol < 0)
                            {
                                // If no LF was found, we need more buffer paged in so move the
                                // read idx by the read amount, which will trigger a buffer page
                                break;
                            }

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
                            int space = buffer.FirstIndexOf(' ', idx, read - idx);
                            if (space < 0)
                                throw new CommitParseException("space", new StringUtf8(buffer, 0, read), idx);

                            // Find the first ']' character after the branch name
                            int closeBracket = buffer.FirstIndexOf(']', space, read - space);
                            if (closeBracket < 0)
                                throw new CommitParseException("]", new StringUtf8(buffer, 0, read), idx);

                            // By specifying "-c core.abbrev=40", the commit ID will be the full
                            // sha instead of the typical short sha.
                            if (closeBracket - idx < 40)
                                throw new CommitParseException("ID too short", new StringUtf8(buffer, 0, read), idx);

                            commitId = ObjectId.FromUtf8(buffer, closeBracket - ObjectId.Length);

                            // The commit ID has been successfully read, so break the loop
                            // and just let the outer loop drain stdout
                            break;
                        }

                        if (commitId != ObjectId.Zero)
                        {
                            // Drain stdout since we don't need any of the rest of the commit summary
                            process.StdOut.CopyTo(System.IO.Stream.Null);
                            break;
                        }

                        // when we've moved beyond what we can reliably read we need to shift the bytes
                        // left to make room in the buffer for new data
                        MakeSpace(buffer, ref idx, ref read);
                    }

                    TestExitCode(process, command, stderrTask);

                }
            }
            catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CommitCommand)}.{nameof(ExecuteCommit)}", exception))
            {
                // not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                throw;
            }

            return commitId;
        }
    }
}
