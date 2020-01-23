//*************************************************************************************************
// DiffTreeCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class DiffTreeCommand : DiffCommand
    {
        public const string Command = "diff-tree";

        public DiffTreeCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public IDifferenceEngine OpenDifferenceEngine(DifferenceOptions options)
        {
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--stdin -m -r --full-index --no-ext-diff");

                ApplyOptions(command, options);

                IProcess process = CreateProcess(command);

                return new DifferenceEngine(this, options, process);
            }
        }

        public ITreeDifference ReadCommitDifference(ICommit commit, DifferenceOptions options)
        {
            if (commit == null)
                throw new ArgumentNullException(nameof(commit));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--no-ext-diff --full-index --root --abbrev=40 -m -r");

                ApplyOptions(command, options);

                command.AddOption("--cc");
                command.Add(commit.RevisionText);

                ObjectId objectId = ObjectId.Zero;
                ITreeDifference treeDifference = null;

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    using (IProcess process = CreateProcess(command))
                    {
                        var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                        objectId = ValidateHeadObjectId(process, commit.ObjectId);

                        treeDifference = ParseDiffOutput(process.StdOut, options);

                        TestExitCode(process, command, stderrTask);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(DiffTreeCommand)}.{nameof(ReadCommitDifference)}", exception, command, objectId))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return treeDifference;
            }
        }

        public ITreeDifference ReadStatusTreeDifference(DifferenceOptions options)
        {
            Head head = new Head();
            head.SetContextAndCache(Context, _repository as IStringCache);
            IRevision revision = head;

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--no-ext-diff --full-index --abbrev=40 -m -r");

                ApplyOptions(command, options);

                command.Add(revision.RevisionText);

                ObjectId objectId = ObjectId.Zero;
                ITreeDifference treeDifference = null;

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    using (IProcess process = CreateProcess(command))
                    {
                        var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                        objectId = ReadObjectId(process);

                        treeDifference = ParseDiffOutput(process.StdOut, options);

                        TestExitCode(process, command, stderrTask);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(DiffTreeCommand)}.{nameof(ReadStatusTreeDifference)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return treeDifference;
            }
        }

        public ITreeDifference ReadTreeDifference(ICommit source, ICommit target, DifferenceOptions options)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return ReadTreeDifference(source as IRevision, target as IRevision, options);
        }

        public ITreeDifference ReadTreeDifference(ITree source, ITree target, DifferenceOptions options)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return ReadTreeDifference(source as IRevision, target as IRevision, options);
        }

        internal ObjectId ValidateHeadObjectId(IProcess process, ObjectId expectedId)
        {
            if (ReferenceEquals(process, null))
                throw new ArgumentNullException(nameof(process));

            ObjectId objectId = ReadObjectId(process);

            // Validate the operation by comparing the returned identity with the identity of
            // the source commit. Allow ObjectId.Zero for empty commits.
            if (expectedId != objectId && objectId != ObjectId.Zero)
                throw new ObjectParseException(nameof(expectedId), (StringUtf8)objectId.ToString(), 0);

            return objectId;
        }

        private ObjectId ReadObjectId(IProcess process)
        {
            Debug.Assert(process != null, $"The `{nameof(process)}` parameter is null.");

            // create an array of bytes identity lenght plus one (for the eol character)
            byte[] oidBytes = new byte[ObjectId.Length + 1];
            int read = 0;

            // read from the git-diff-tree process until ObjectId.Length + 1 bytes have been read
            // this clears the extra garbage the git-diff-tree process puts into the stream and
            // gives us something to validate that the operation is doing what we expect
            int r;
            while (read < oidBytes.Length && (r = process.StdOut.Read(oidBytes, read, oidBytes.Length - read)) > 0)
            {
                read += r;
                // keep reading in the bytes
            }

            if (read == 0)
                return ObjectId.Zero;

            // convert the bytes into a struct
            StringUtf8 objIdStr = new StringUtf8(oidBytes);
            ObjectId objectId = objIdStr.ToObjectId();

            return objectId;
        }

        public ITreeDifference ReadTreeDifference(IRevision source, IRevision target, DifferenceOptions options)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--no-ext-diff --full-index --abbrev=40 -m -r");

                ApplyOptions(command, options);

                command.Add(source.RevisionText);
                command.Add(target.RevisionText);

                ITreeDifference treeDifference = null;

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    using (IProcess process = CreateProcess(command))
                    {
                        var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                        treeDifference = ParseDiffOutput(process.StdOut, options);

                        TestExitCode(process, command, stderrTask);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(DiffTreeCommand)}.{nameof(ReadTreeDifference)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

                return treeDifference;
            }
        }
    }
}
