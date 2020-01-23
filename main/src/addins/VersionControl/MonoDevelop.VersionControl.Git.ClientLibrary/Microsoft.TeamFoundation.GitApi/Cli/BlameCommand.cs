//*************************************************************************************************
// BlameCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{

    internal class BlameCommand : GitCommand
    {
        public const string Command = "blame";

        public BlameCommand (ExecutionContext context, IRepository repository) : base (context, repository)
        {
        }

        public BlameCommand (ExecutionContext context, Environment environment, object userData) : base (context, environment, userData)
        {
        }

        public Blame ReadBlame (BlameOptions options)
        {
            var lines = new List<Annotation> ();
            var sb = new StringBuilder ();

            using (var command = new ArgumentList(Command)) {
                command.AddOption("-p");
                command.AddOption("--encoding=UTF-8");

                if (options.IgnoreWhitespaces)
                    command.AddOption("-w");

                if (options.Path != null) {
                    command.EndOptions();
                    command.Add(options.Path);
                }

                try {
                    using (Tracer.TraceCommand (Command, command, userData: _userData))
                    using (var process = CreateProcess (command, true)) {
                        var annotationDictionary = new Dictionary<string, Annotation> ();
                        var hashSet = new HashSet<string> ();
                        Annotation currentAnnotation = null;
                        process.ProcessOutput += (sender, o) => {
                            string output = o.Message;
                            if (currentAnnotation == null) {
                                var idx = output.IndexOf (' ');
                                if (idx < 0)
                                    return;
                                var revision = output.Substring (0, idx);
                                if (!annotationDictionary.TryGetValue (revision, out currentAnnotation)) {
                                    currentAnnotation = new Annotation () {
                                        Revision = revision
                                    };
                                    annotationDictionary[revision] = currentAnnotation;
                                }
                            } else if (output[0] == '\t') {
                                if (sb.Length == 0 && output[1] == 0xFEFF) { // filter UTF-8 BOM
                                    sb.Append (output, 2, output.Length - 2);
                                } else {
                                    sb.Append (output,  1, output.Length - 1);
                                }
                                sb.AppendLine ();
                                lines.Add (currentAnnotation);
                                currentAnnotation = null;
                            } else if (output.StartsWith ("author ", StringComparison.Ordinal)) {
                                currentAnnotation.Author = output.Substring ("author ".Length);
                            } else if (output.StartsWith ("author-mail ")) {
                                currentAnnotation.AuthorMail = output.Substring ("author-mail ".Length);
                            } else if (output.StartsWith ("summary ")) {
                                currentAnnotation.Summary = output.Substring ("summary ".Length);
                            } else if (output.StartsWith ("previous ")) {
                                var startIdx = "previous ".Length;
                                var splitIdx = output.IndexOf (' ', startIdx, output.Length - startIdx - 1);
                                currentAnnotation.PreviousRevision = output.Substring (startIdx, splitIdx - startIdx);
                                currentAnnotation.PreviousPath = output.Substring (splitIdx + 1, output.Length - splitIdx - 1);
                            } else if (output.StartsWith ("author-time ")) {
                                var startIdx = "author-time ".Length;
                                currentAnnotation.AuthorTime = ParseTimeStamp (output, startIdx);
                            }
                        };

                        TestExitCode (process, command);
                    }
                } catch (ParseException exception) when (ParseHelper.AddContext ($"{nameof (BlameCommand)}.{nameof (ReadBlame)}", exception, command)) {
                    // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                    throw;
                }

            }
            return new Blame {
                Text = sb.ToString (),
                Annotations = lines.ToArray ()
            };
        }

        private void Process_ProcessOutput (object sender, OperationOutput e)
        {
            throw new NotImplementedException ();
        }

        static DateTime ParseTimeStamp (string txt, int startIndex)
        {
            int i = startIndex;
            long time = 0;
            while (i  < txt.Length) {
                char ch = txt[i];
                if (ch < '0' || ch > '9')
                    break;
                time = ch - '0' + time * 10;
                i++;
            }
            return DateTime.FromBinary (time);
        }
    }
}
