//*************************************************************************************************
// TagCommand.cs
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
    internal class TagCommand : GitCommand
    {
        public const string Command = "tag";

        public TagCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void Create(IRevision revision, string name, string message, TagOptions options)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            using (var command = new ArgumentList(Command))
            {
                ApplyOptions(command, revision, name, message, options);
                ExecuteTag(command, message);
            }
        }

        public void Delete(ITagName tag)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--delete");
                // Do not use canonical name here as the command is already scoping to tags.
                command.Add(tag.FriendlyName);

                try
                {
                    ExecuteTag(command, null);
                }
                catch (System.Exception exception)
                {
                    throw new TagDeletionException(tag.CanonicalName, exception);
                }
            }
        }

        public Task<ITagName[]> GetAllTagsAsync()
        {
            using (var command = new ArgumentList(Command))
            {
                var result = new List<ITagName>();
                using (Tracer.TraceCommand(Command, command, userData: _userData))
                using (IProcess process = CreateProcess(command, true))
                {
                    process.ProcessOutput += (sender, o) =>
                    {
                        if (o.Source == OutputSource.Out)
                        {
                            result.Add(new TagName(o.Message));
                        }
                    };

                    try
                    {
                        RunAndTestProcess(process, command);
                    }
                    catch (GitException exception) when (!string.IsNullOrEmpty(exception.ErrorText))
                    {
                        throw;
                    }
                    return Task.FromResult(result.ToArray());
                }
            }
        }

        internal static void ApplyOptions(ArgumentList command,
                                          IRevision revision,
                                          string name,
                                          string message,
                                          TagOptions options)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (revision is null)
                throw new ArgumentNullException(nameof(revision));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if ((options.Flags & TagOptionFlags.Force) != 0)
            {
                command.AddOption("--force");
            }

            // If a message is provided, send it in via stdin
            if (!string.IsNullOrEmpty(message))
            {
                command.AddOption("--file=-");
            }

            command.Add(name);
            command.Add(revision.RevisionText);
        }
        private void ExecuteTag(string command, string message)
        {
            Debug.Assert(command != null, $"The `{nameof(command)}` parameter is null");

            using (Tracer.TraceCommand(Command, command, userData: _userData))
            using (IProcess process = CreateProcess(command, true))
            {
                if (message != null)
                {
                    process.StandardInput.Write(message + "\n");
                }

                try
                {
                    RunAndTestProcess(process, command);
                }
                catch (GitException exception) when (!string.IsNullOrEmpty(exception.ErrorText))
                {
                    if (exception.ErrorText.EndsWith("is not a valid tag name.\n", StringComparison.Ordinal))
                        throw new TagNameException(exception.ErrorText, exception);

                    if (exception.ErrorText.EndsWith("already exists\n", StringComparison.Ordinal))
                        throw new TagExistsException(exception.ErrorText, exception);

                    if (exception.ErrorText.StartsWith("fatal: cannot lock ref '", StringComparison.Ordinal)
                        && exception.ErrorText.Contains("' exists; cannot create '"))
                        throw new TagCreationException(exception.ErrorText, exception);

                    throw;
                }
            }
        }
    }
}
