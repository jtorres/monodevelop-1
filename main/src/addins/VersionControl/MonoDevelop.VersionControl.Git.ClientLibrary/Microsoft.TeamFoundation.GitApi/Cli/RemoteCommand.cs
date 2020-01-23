//*************************************************************************************************
// RemoteCommand.cs
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
    internal class RemoteCommand : GitCommand
    {
        public const string Command = "remote";

        public RemoteCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public void Add(string url, string name, RemoteTagOptions options)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!Remote.IsLegalName(name))
                throw new ArgumentException(nameof(name), RemoteNameException.FromName(name));

            Debug.Assert(Enum.IsDefined(typeof(RemoteTagOptions), options), $"The `{nameof(options)}` is undefined.");

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("add");

                switch (options)
                {
                    case RemoteTagOptions.AllTags:
                        command.AddOption("--tags");
                        break;

                    case RemoteTagOptions.NoTags:
                        command.AddOption("--no-tags");
                        break;
                }

                command.Add(name);
                command.Add(url);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(Add)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public void Prune(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!Remote.IsLegalName(name))
                throw new ArgumentException(nameof(name), RemoteNameException.FromName(name));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("-v prune");

                command.Add(name);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(Prune)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public IRemoteCollection ReadCollection()
        {
            StringUtf8 Fetch = (StringUtf8)"fetch";
            StringUtf8 Push = (StringUtf8)"push";

            Dictionary<StringUtf8, StringPair> table = new Dictionary<StringUtf8, StringPair>(StringUtf8Comparer.Ordinal);

            // build the command
            using (var command = new ArgumentList(Command))
            {
                command.AddOption("--verbose");

                try
                {
                    // invoke the command and process its output
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    using (IProcess process = CreateProcess(command))
                    {
                        var stderrTask = Task.Run(() => { return process.StandardError.ReadToEnd(); });

                        using (var buffer = new ByteBuffer())
                        {
                            int eol = -1;
                            int get = 0;
                            int read = 0;

                            int r;
                            while ((read < buffer.Length) && (r = process.StdOut.Read(buffer, read, buffer.Length - read)) != 0)
                            {
                                read += r;

                                while (get < read)
                                {
                                    eol = buffer.FirstIndexOf('\n', get, read - get);
                                    if (eol < 0)
                                        break;

                                    // lines are in the format of "<name>\t<url> (<type>)<eol>"
                                    // split each line into parts and store the result data into the table
                                    // this will correlate name, fetch, and push information. This is needed
                                    // because name is reported twice; fetch and push are reported on
                                    // separate lines.

                                    int i1 = buffer.FirstIndexOf('\t', get, eol - get);
                                    if (i1 < 0)
                                        throw new RemoteParseException("find\\t", new StringUtf8(buffer, 0, read), get);

                                    // When there is a stray configuration option for a remote, but the remote
                                    // does not contain URL information only the name will be presented by this
                                    // command. To detect this look for "<name>\t<eol>", and skip it.

                                    if (i1 + 1 == eol)
                                    {
                                        // Move `get` past the newline character, and repeat the loop.
                                        get = eol + 1;
                                        continue;
                                    }

                                    // seek the last parenthesis set because URL can contain them...
                                    int i4 = buffer.LastIndexOf(')', i1 + 1, eol - i1 - 1);
                                    if (i4 < 0)
                                        throw new RemoteParseException("find)", new StringUtf8(buffer, 0, read), get);

                                    int i3 = buffer.LastIndexOf('(', i1 + 1, i4 - i1 - 1);
                                    if (i3 < 0)
                                        throw new RemoteParseException("find(", new StringUtf8(buffer, 0, read), get);

                                    // Remove trailing whitespace from the url
                                    int i2 = i3 - 1;
                                    while (i2 > i1 && buffer[i2] == ' ')
                                    {
                                        i2--;
                                    }

                                    // capture the output line as a string
                                    StringUtf8 remote = new StringUtf8(buffer, get, eol - get);

                                    // since substrings are near free with this string type, substring the various parts
                                    StringUtf8 name = remote.Substring(0, i1 - get);
                                    StringUtf8 url = remote.Substring(i1 - get + 1, i2 - i1);
                                    StringUtf8 type = remote.Substring(i3 - get + 1, i4 - i3 - 1);

                                    if (!table.ContainsKey(name))
                                    {
                                        table.Add(name, new StringPair { });
                                    }

                                    if (type == Fetch)
                                    {
                                        table[name] = new StringPair
                                        {
                                            FetchUrl = url,
                                            PushUrl = table[name].PushUrl,
                                        };
                                    }
                                    else if (type == Push)
                                    {
                                        table[name] = new StringPair
                                        {
                                            FetchUrl = table[name].FetchUrl,
                                            PushUrl = url,
                                        };
                                    }
                                    else
                                    {
                                        throw new RemoteParseException("type==??", new StringUtf8(buffer, 0, read), get);
                                    }

                                    get = eol + 1;
                                }

                                MakeSpace(buffer, ref get, ref read);
                            }
                        }

                        TestExitCode(process, command, stderrTask);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(ReadCollection)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }

            // alloc a remote collection and populate it with data from the table
            RemoteCollection collection = new RemoteCollection();
            collection.SetContext(Context);

            foreach (var item in table)
            {
                Remote remote = new Remote(item.Key, item.Value.FetchUrl, item.Value.PushUrl);
                remote.SetContext(Context);
                collection.Add(remote);
            }

            return collection;
        }

        public void Rename(string oldName, string newName)
        {
            if (oldName == null)
                throw new ArgumentNullException(nameof(oldName));
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));
            if (!Remote.IsLegalName(oldName))
                throw new ArgumentException(nameof(oldName), RemoteNameException.FromName(oldName));
            if (!Remote.IsLegalName(newName))
                throw new ArgumentException(nameof(newName), RemoteNameException.FromName(newName));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("rename");

                command.Add(oldName);
                command.Add(newName);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(Rename)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public void Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (!Remote.IsLegalName(name))
                throw new ArgumentException(nameof(name), RemoteNameException.FromName(name));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("remove");

                command.Add(name);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(Remove)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public void SetFetchUrl(string name, string url)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (!Remote.IsLegalName(name))
                throw new ArgumentException(nameof(name), RemoteNameException.FromName(name));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("set-url");
                command.Add(name);
                command.Add(url);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(SetFetchUrl)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        public void SetPushUrl(string name, string url)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (!Remote.IsLegalName(name))
                throw new ArgumentException(nameof(name), RemoteNameException.FromName(name));

            using (var command = new ArgumentList(Command))
            {
                command.AddOption("set-url --push");
                command.Add(name);
                command.Add(url);

                try
                {
                    using (Tracer.TraceCommand(Command, command, userData: _userData))
                    {
                        var executeResult = Execute(command, out string standardOutput);

                        TestExitCode(executeResult, command);
                    }
                }
                catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(RemoteCommand)}.{nameof(SetPushUrl)}", exception, command))
                {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }
            }
        }

        private struct StringPair
        {
            public StringUtf8 FetchUrl;
            public StringUtf8 PushUrl;
        }
    }
}
