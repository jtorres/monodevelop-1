//*************************************************************************************************
// CountObjectsCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    internal class CountObjectsCommand : GitCommand
    {
        public const string Command = "count-objects";

        public CountObjectsCommand(ExecutionContext context, IRepository repository)
            : base(context, repository)
        { }

        public ObjectDatabaseDetails GetObjectCounts()
        {
            using (var command = new StringBuffer(Command))
            {
                command.Append(" --verbose");

                StringUtf8 standardError;
                StringUtf8 standardOutput;

                using (Tracer.TraceCommand(Command, Command, userData: _userData))
                {
                    try
                    {
                        int exitCode = Execute(command, out standardError, out standardOutput);

                        TestExitCode(exitCode, command, (string)standardError);
                    }
                    catch (ParseException exception) when (ParseHelper.AddContext($"{nameof(CountObjectsCommand)}.{nameof(GetObjectCounts)}", exception, command))
                    {
                        // Not reachable, but the `when` allows for additional context to be added and `throw` here makes the compiler happy
                        throw;
                    }
                }

                long duplicateCount = 0;
                long garbageCount = 0;
                long garbageSize = 0;
                long looseCount = 0;
                long looseSize = 0;
                long packedCount = 0;
                long packedSize = 0;
                int packsCount = 0;

                StringUtf8[] substrings = new StringUtf8[8];
                int i1 = 0;
                int i2 = -1;

                for (int i = 0; i < substrings.Length; i += 1)
                {
                    i2 += 1;

                    i1 = standardOutput.FirstIndexOf(':', i2 + 1);
                    if (i1 < 0)
                        throw new CountObjectsParseException(":", standardOutput, i2 + 1);

                    ValidateLabel(standardOutput, i2, i1 - i2, i);

                    i2 = standardOutput.FirstIndexOf('\n', i1 + 1);
                    if (i2 < 0)
                        throw new CountObjectsParseException("\n", standardOutput, i1 + 1);

                    substrings[i] = standardOutput.Substring(i1 + 2, i2 - i1 - 2);

                    switch (i)
                    {
                        case 0:
                            if (!substrings[i].TryParse(out looseCount))
                                throw new CountObjectsParseException(nameof(looseCount), substrings[i], 0);
                            break;

                        case 1:
                            if (!substrings[i].TryParse(out looseSize))
                                throw new CountObjectsParseException(nameof(looseSize), substrings[i], 0);
                            break;

                        case 2:
                            if (!substrings[i].TryParse(out packedCount))
                                throw new CountObjectsParseException(nameof(packedCount), substrings[i], 0);
                            break;

                        case 3:
                            if (!substrings[i].TryParse(out packsCount))
                                throw new CountObjectsParseException(nameof(packsCount), substrings[i], 0);
                            break;

                        case 4:
                            if (!substrings[i].TryParse(out packedSize))
                                throw new CountObjectsParseException(nameof(packedSize), substrings[i], 0);
                            break;

                        case 5:
                            if (!substrings[i].TryParse(out duplicateCount))
                                throw new CountObjectsParseException(nameof(duplicateCount), substrings[i], 0);
                            break;

                        case 6:
                            if (!substrings[i].TryParse(out garbageCount))
                                throw new CountObjectsParseException(nameof(garbageCount), substrings[i], 0);
                            break;

                        case 7:
                            if (!substrings[i].TryParse(out garbageSize))
                                throw new CountObjectsParseException(nameof(garbageCount), substrings[i], 0);
                            break;
                    }
                }

                return new ObjectDatabaseDetails(duplicateCount,
                                                 garbageCount,
                                                 garbageSize,
                                                 looseCount,
                                                 looseSize,
                                                 packedCount,
                                                 packedSize,
                                                 packsCount);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ValidateLabel(StringUtf8 buffer, int index, int length, int iteration)
        {
            if (ReferenceEquals(buffer, null))
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0 || index + length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            StringUtf8[] labels =
            {
                (StringUtf8)"count",
                (StringUtf8)"size",
                (StringUtf8)"in-pack",
                (StringUtf8)"packs",
                (StringUtf8)"size-pack",
                (StringUtf8)"prune-packable",
                (StringUtf8)"garbage",
                (StringUtf8)"size-garbage",
            };

            if (iteration < 0 || iteration >= labels.Length)
                throw new ArgumentOutOfRangeException(nameof(iteration));

            StringUtf8 expected = labels[iteration];
            StringUtf8 actual = buffer.Substring(index, length);

            if (actual != expected)
                throw new CountObjectsParseException("label", buffer, 0);
        }
    }
}
