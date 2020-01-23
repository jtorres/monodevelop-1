//*************************************************************************************************
// Environment.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.TeamFoundation.GitApi.Internal;
using Microsoft.TeamFoundation.GitApi.Internal.Win32;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of operating system environmental settings.
    /// </summary>
    public partial class Environment : IEnumerable<Environment.Variable>
    {
        /// <summary>
        /// The character used to separate path values contained within the `<see cref="Key.Path"/>` environment variable value.
        /// </summary>
        public const char PathSegmentDelimiter = ';';

        internal static readonly EnvironmentVariableComparer ValueComparer = Variable.Comparer;

        internal Environment(IEnumerable<Variable> variables, string workingDirectory)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));
            if (workingDirectory == null)
                throw new ArgumentNullException(nameof(workingDirectory));

            // Capture the working directory
            _workingDirectory = workingDirectory;

            // Allocate the array to contain the final set
            _variables = DeduplicateVariables(variables);
        }

        private static bool? _isProcessInJob;
        private static bool? _isWow64;
        private static readonly object _syncpoint = new object();
        private static Version _version;
        protected Variable[] _variables;
        protected string _workingDirectory;

        /// <summary>
        /// Gets the number of environment variables contained in this instance of `<see cref="Environment"/>`.
        /// </summary>
        public int Count
        {
            get { return _variables.Length; }
        }

        /// <summary>
        /// Gets `<see langword="true"/>` if the operating system is a 64-bit operating system;
        /// otherwise `<see langword="false"/>`.
        /// </summary>
        public static bool Is64BitSystem { get { return Is64BitProcess || IsWow64; } }

        /// <summary>
        /// Gets `<see langword="true"/>` if the current process is a 64-bit process; otherwise `<see langword="false"/>`.
        /// </summary>
        public static bool Is64BitProcess { get { return System.Environment.Is64BitProcess; } }

        /// <summary>
        /// Gets `<see langword="true"/>` if the current process is a job object; otherwise `<see langword="false"/>`.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static bool IsProcessInJob
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_isProcessInJob == null)
                    {
                        bool isProcessInJob;
                        if (!Kernel32.IsProcessInJob(processHandle: SafeProcessHandle.CurrentProcessHandle,
                                                     jobHandle: SafeJobObjectHandle.Null,
                                                     isProcessInJob: out isProcessInJob))
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw new Win32Exception(error, $"Failed `{Kernel32.Name}!{nameof(Kernel32.IsProcessInJob)}`");
                        }

                        _isProcessInJob = isProcessInJob;
                    }

                    return _isProcessInJob.Value;
                }
            }
        }

        /// <summary>
        /// Gets `<see langword="true"/>` if the current process is Windows-on-Windows (aka 32-bit
        /// application running on a 64-bit operating system); otherwise `<see langword="false"/>`.
        /// </summary>
        /// <exception cref="Win32Exception">When the call to the operating system</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static bool IsWow64
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_isWow64 == null)
                    {
                        bool isWow64Process;
                        if (!Kernel32.IsWow64Process(processHandle: SafeProcessHandle.CurrentProcessHandle,
                                                     isWow64Process: out isWow64Process))
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw new Win32Exception(error, $"Failed `{Kernel32.Name}!{nameof(Kernel32.IsWow64Process)}`");
                        }

                        _isWow64 = isWow64Process;
                    }
                }

                return _isWow64.Value;
            }
        }

        /// <summary>
        /// The longest possible length "path" environment variable.
        /// </summary>
        public static int MaximumPathVariableLength
        {
            get
            {
                const int Windows7 = 7;

                // The value for PATH in theory (starting with Win7 SP1) be unlimited, however the environment block
                // is limited to 32 KiB (larger in later versions of Windows). These limits are somewhat artificial
                // however, to maintain the best possible compatibility across various generations of software these
                // limitation seems reasonable.
                // see: https://blogs.msdn.microsoft.com/oldnewthing/20100203-00/?p=15083/ for more details.
                return (System.Environment.OSVersion.Version.Major > Windows7)
                    ? 8191  // After Windows 7, environmental variable PATH values could be as long as 8191 UTF-16 characters.
                    : 2047; // Prior to Windows 8, environmental variable PATH values were limited to 2047 UTF-16 characters.
            }
        }

        /// <summary>
        /// Gets a read-only list of the <see cref="Variable"/> contained in this instance of <see cref="Environment"/>.
        /// </summary>
        public IReadOnlyList<Variable> Variables
        {
            get { lock (_syncpoint) return _variables; }
        }

        /// <summary>
        /// Gets the version of this library.
        /// </summary>
        public static Version Version
        {
            get
            {
                lock (_syncpoint)
                {
                    if (_version == null)
                    {
                        _version = System.Reflection.Assembly.GetAssembly(typeof(Environment))?.GetName()?.Version ?? new Version();
                    }
                    return _version;
                }
            }
        }

        /// <summary>
        /// Gets the working directory.
        /// </summary>
        public string WorkingDirectory
        {
            get { return _workingDirectory; }
        }

        /// <summary>
        /// Searches this instance of `<see cref="Environment"/>` for a `<see cref="Variable"/>` with
        /// a `<see cref="Variable.Name"/>` equivalent to `<paramref name="name"/>`.
        /// <para/>
        /// Returns `<see langword="true"/>` if a match is found; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="name">The `<see cref="Variable.Name"/>` to search for.</param>
        public bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                return false;

            var variable = new Variable(name, string.Empty);

            return Array.BinarySearch(_variables, variable, ValueComparer) >= 0;
        }

        /// <summary>
        /// Returns a new instance of `<see cref="Environment"/>` by combining the variables from this instance and `<paramref name="variable"/>`.
        /// </summary>
        /// <param name="variable">
        /// The list of variables to append to the `<see cref="Environment"/>`.
        /// <para/>
        /// `<see cref="ConfigurationEntry"/>` with a <see lang="null"/> `<see cref="ConfigurationEntry.Value"/>` will be removed.
        /// </param>
        public Environment CreateWith(Variable variable)
        {
            var list = new List<Variable>(_variables)
            {
                variable
            };
            return new Environment(list, _workingDirectory);
        }

        /// <summary>
        /// Returns a new instance of `<see cref="Environment"/>` by combining the variables from this instance and `<paramref name="variables"/>`.
        /// </summary>
        /// <param name="variables">
        /// The variable to append to the `<see cref="Environment"/>`.
        /// <para/>
        /// `<see cref="ConfigurationEntry"/>` with a <see lang="null"/> `<see cref="ConfigurationEntry.Value"/>` will be removed.
        /// </param>
        public Environment CreateWith(IEnumerable<Variable> variables)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            var list = new List<Variable>(_variables);
            list.AddRange(variables);

            return new Environment(list, _workingDirectory);
        }

        public static Environment CreateWith (List<Variable> variables, string workingDirectory)
        {
            if (variables == null)
                throw new ArgumentNullException (nameof (variables));
            return new Environment (variables, workingDirectory);
        }

        public IEnumerator<Variable> GetEnumerator()
        {
            for (int i = 0; i < _variables.Length; i += 1)
            {
                yield return _variables[i];
            }

            yield break;
        }

        /// <summary>
        /// Returns The value of the environment variable specified by `<paramref name="name"/>`, or `<see langword="null"/>` if the environmen variable is not found.
        /// </summary>
        /// <param name="name">The name of the environment variable</param>
        public string GetEnvironmentVariable(string name)
        {
            Variable variable;
            if (TryGet(name, out variable))
                return variable.Value;

            return null;
        }

        /// <summary>
        /// Retrieves the value of an environment variable by name.
        /// <para/>
        /// Returns `<see langword="true"/>` if successful; otherwise `<see langword="false"/>`.
        /// </summary>
        /// <param name="name">The name of the environment variable</param>
        /// <param name="value">The environment variable specified by `<paramref name="name"/>`, or `<see langword="null"/>` if the environmen variable is not found.</param>
        public bool TryGet(string name, out Variable value)
        {
            value = Variable.Empty;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            var variable = new Variable(name, string.Empty);

            int index = -1;
            if ((index = Array.BinarySearch(_variables, variable, ValueComparer)) >= 0)
            {
                value = _variables[index];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whites `<see cref="Variables"/>` to `<paramref name="buffer"/>` in a format the operating system can utilize.
        /// </summary>
        /// <param name="buffer">The buffer to serialize `<see cref="Variables"/>` to.</param>
        public void GetEnvironmentBlock(ref char[] buffer)
        {
            var enumerator = GetEnumerator();
            int idx = 0;

            try
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    const int VariablePadding = 3; // "{name}={value}\0" plus an extra '\0' for the final variable
                    // Since there is no real upper-bounds on the size of an environment block
                    // that Windows will accept, just grow the array to fit.
                    while (idx + item.Name.Length + item.Value.Length + VariablePadding > buffer.Length)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }

                    item.Name.CopyTo(0, buffer, idx, item.Name.Length);
                    idx += item.Name.Length;

                    buffer[idx] = '=';
                    idx += 1;

                    item.Value.CopyTo(0, buffer, idx, item.Value.Length);
                    idx += item.Value.Length;

                    buffer[idx] = '\0';
                    idx += 1;
                }

                buffer[idx] = '\0';
            }
            finally
            {
                // Dispose the enumerator
                enumerator?.Dispose();
            }
        }

        protected static Variable[] DeduplicateVariables(IEnumerable<Variable> variables)
        {
            // Since each variable can only appear once, we need to allocate block of memory to
            // to sort and de-duplicate them.
            Variable[] src = new List<Variable>(variables).ToArray();
            Variable[] dst = new Variable[src.Length];
            int length = 0;

            // Add all of the variables to the set
            for (int i = 0; i < src.Length; i += 1)
            {
                Variable variable = src[i];

                // Drop any variable with an empty name (illegal)
                if (string.IsNullOrEmpty(variable.Name))
                    continue;

                // Linear search (because we cannot safely sort) the array for duplicates
                int idx = -1;
                for (int j = 0; j < length; j += 1)
                {
                    if (Variable.NameStringComparer.Equals(variable.Name, dst[j].Name))
                    {
                        idx = j;
                        break;
                    }
                }

                if (idx < 0)
                {
                    // Drop any variable with an empty value (signals removal)
                    if (string.IsNullOrEmpty(variable.Value))
                        continue;

                    dst[length] = variable;
                    length += 1;
                }
                else
                {
                    // Drop any variable with an empty value (signals removal)
                    if (string.IsNullOrEmpty(variable.Value))
                    {
                        // Shift the array left
                        Array.ConstrainedCopy(dst, idx + 1, dst, idx, length - idx - 1);
                        length -= 1;
                    }
                    else
                    {
                        dst[idx] = variable;
                    }
                }
            }

            // Since we rely on binary search, we need to sort the list
            Array.Sort(dst, 0, length, Variable.Comparer);

            // Resize the array because it is assumed there are no empty/invalid variables.
            Array.Resize(ref dst, length);

            return dst;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
