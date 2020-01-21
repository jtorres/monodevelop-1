//
//*************************************************************************************************
// ArgumentList.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Text;
using System.Threading;

namespace Microsoft.TeamFoundation.GitApi.Cli
{
    public sealed class ArgumentList : IDisposable
    {
        private static readonly System.Collections.Concurrent.ConcurrentBag<StringBuilder> _cache;

        static ArgumentList()
        {
            _cache = new System.Collections.Concurrent.ConcurrentBag<StringBuilder>();
        }

        StringBuilder sb = new StringBuilder();

        public ArgumentList(string command)
        {
            if (!_cache.TryTake(out sb))
            {
                sb = new StringBuilder();
            }

            sb.Append(command);
        }

        public void Prepend(string argument)
        {
            var arg = ShouldQuote(argument) ? Quote(argument) : argument;
            sb.Insert(0, arg + " ");
        }

        public void Add(string argument)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(ShouldQuote(argument) ? Quote(argument) : argument);
        }

        public void Add(string argument, string concat1)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(ShouldQuote(argument) ? Quote(argument) : argument);
            sb.Append(ShouldQuote(concat1) ? Quote(concat1) : concat1);
        }

        public void Add(string argument, string concat1, string concat2)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(ShouldQuote(argument) ? Quote(argument) : argument);
            sb.Append(ShouldQuote(concat1) ? Quote(concat1) : concat1);
            sb.Append(ShouldQuote(concat2) ? Quote(concat2) : concat2);
        }

        public void Add(params string[] argumentsToConcat)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            foreach (var argument in argumentsToConcat)
                sb.Append(ShouldQuote(argument) ? Quote(argument) : argument);
        }

        /// <summary>
        /// Inserts an option - no quotation check. Space is added beforehand if needed.
        /// </summary>
        internal void AddOption(string option)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(option);
        }

        /// <summary>
        /// Inserts a command line option : {option}={value}. Value is quoted if needed.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="value">The value.</param>
        internal void AddOption(string option, string value)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(option);
            sb.Append('=');
            sb.Append(ShouldQuote(value) ? Quote(value) : value);
        }

        private string Quote(string argument)
        {
            if (!_cache.TryTake(out var resultBuilder))
            {
                resultBuilder = new StringBuilder();
            }

            resultBuilder.Append('"');
            foreach (var ch in argument)
            {
                if (ch == '"' || ch == '\\')
                    resultBuilder.Append('\\');
                resultBuilder.Append(ch);
            }
            resultBuilder.Append('"');
            var result = resultBuilder.ToString();
            resultBuilder.Clear();
            _cache?.Add(resultBuilder);

            return result;
        }

        private bool ShouldQuote(string argument)
        {
            return argument.IndexOfAny(new[] { ' ', '\t', '"', '\\' }) >= 0;
        }

        public override string ToString()
        {
            return sb.ToString ();
        }

        public static implicit operator string(ArgumentList argumentList)
        {
            if (ReferenceEquals(argumentList, null))
                return null;

            return argumentList.ToString();
        }

        public void Dispose()
        {
            StringBuilder builder;
            if ((builder = Interlocked.Exchange(ref sb, null)) == null)
                return;
            builder.Clear();
            _cache?.Add(builder);
        }

        public void EndOptions()
        {
            Add("--");
        }

    }
}