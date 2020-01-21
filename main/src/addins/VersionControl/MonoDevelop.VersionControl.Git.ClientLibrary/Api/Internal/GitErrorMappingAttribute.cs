//*************************************************************************************************
// GitErrorMappingAttribute.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    /// <summary>
    /// Class of attributes used for mapping from textual errors from git.exe commands to <see cref="Exception"/>Exceptions</see>/>
    /// </summary>
    internal abstract class GitErrorMappingAttributeBase : Attribute
    {
        /// <summary>
        /// Whether or not the supplied error message string matches the criteria described by this attribute
        /// </summary>
        /// <param name="errorMessage">The error message to match</param>
        /// <returns>True if the error matches that for this mapping, else false</returns>
        public abstract bool IsMatchingError(string errorMessage);

        /// <summary>
        /// Create a new instance of the exception type associated with this attribute
        /// </summary>
        /// <param name="exitCode">The git.exe exit code</param>
        /// <param name="errorMessage">The error message from git.exe</param>
        /// <returns>A newly constructed <see cref="System.Exception"> of the correct type</see>/></returns>
        public abstract System.Exception CreateException(int exitCode, string errorMessage);

        static IReadOnlyDictionary<Type, GitErrorMappingAttributeBase[]> errorToExceptionMappings;
        public static IEnumerable<GitErrorMappingAttributeBase> GetMappings(Type type)
        {
            if (errorToExceptionMappings == null)
            {
                // There is a potential race here, but it doesn't matter if multiple threads attempt to initialize the mappings collection. The last will win.
                errorToExceptionMappings = BuildErrorToExceptionMappings();
            }
            GitErrorMappingAttributeBase[] mappingsForType;
            return errorToExceptionMappings.TryGetValue(type, out mappingsForType) ? mappingsForType : Enumerable.Empty<GitErrorMappingAttributeBase>();
        }

        private static IReadOnlyDictionary<Type, GitErrorMappingAttributeBase[]> BuildErrorToExceptionMappings()
        {
            var mappings = new Dictionary<Type, GitErrorMappingAttributeBase[]>();

            foreach (Type type in typeof(GitErrorMappingAttributeBase).GetTypeInfo().Assembly.GetTypes())
            {
                IEnumerable<GitErrorMappingAttributeBase> attrs = type.GetCustomAttributes<GitErrorMappingAttributeBase>(true);
                Debug.Assert(attrs != null);
                if (attrs.Any())
                {
                    mappings[type] = attrs.ToArray();
                }
            }

            return mappings;
        }
    }

    /// <summary>
    /// General purpose git.exe error message to exception mapping attribute
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ExceptionType = {ExceptionType}")]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    internal class GitErrorMappingAttribute : GitErrorMappingAttributeBase
    {
        /// <summary>
        /// The type of <see cref="System.Exception"/> to be raised for matching error messages
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// The constant string prefix to match in error messages, or null to match any prefix (the default)
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The constant string suffix to match in error messages, or null to match any suffix (the default)
        /// </summary>
        public string Suffix { get; set; }

        private readonly Func<string, int, System.Exception> newFunc;

        /// <summary>
        /// Construct an instance of the attribute
        /// </summary>
        /// <param name="exceptionType">The subtype of <see cref="Exception"/> to raise (required)</param>
        public GitErrorMappingAttribute(Type exceptionType)
        {
            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));
            if (!exceptionType.GetTypeInfo().IsSubclassOf(typeof(System.Exception)) || exceptionType.GetTypeInfo().IsAbstract)
                throw new ArgumentException($"{exceptionType.Name} is not a concrete subclass of {typeof(System.Exception).Name}", nameof(exceptionType));

            this.ExceptionType = exceptionType;

            this.newFunc = GenerateInstantiator(exceptionType);
        }

        private static readonly ParameterExpression ExitCodeParam = Expression.Parameter(typeof(int), "exitCode");
        private static readonly ParameterExpression MessageParam = Expression.Parameter(typeof(string), "message");
        private static readonly ParameterExpression TypeParam = Expression.Parameter(typeof(Type), "exceptionType");
        private static readonly ParameterExpression[] ConstructorParams = new ParameterExpression[] { MessageParam, ExitCodeParam };

        private static Func<string, int, System.Exception> GenerateInstantiator(Type exceptionType)
        {
            const BindingFlags BindConstructor = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance;

            // .Net standard compatibility: The GetConstructor overloads that accept both BindingFlags and parameter types are not supported in 1.6, 
            // so we have to get all the constructors (a short list) and search them. This allows us to match specific parameter names as well though.
            ConstructorInfo[] constructors = exceptionType.GetTypeInfo().GetConstructors(BindConstructor);
            var constructorInfo = constructors.FirstOrDefault(ci =>
            {
                ParameterInfo[] parms = ci.GetParameters();
                // Is it a (string errorText, int exitCode) constructor?
                return parms.Length == 2
                    && parms[0].ParameterType == typeof(string) && string.Equals(parms[0].Name, "errorText", StringComparison.Ordinal)
                    && parms[1].ParameterType == typeof(int) && string.Equals(parms[1].Name, "exitCode", StringComparison.Ordinal);
            });
            if (constructorInfo == null)
            {
                // No (string errorText, int exitCode) constructor, look for standard string message constructor
                constructorInfo = constructors.FirstOrDefault(ci =>
                {
                    ParameterInfo[] parms = ci.GetParameters();
                    return parms.Length == 1 && parms[0].ParameterType == typeof(string);
                });
                if (constructorInfo == null)
                {
                    // No string message constructor, look for default constructor
                    constructorInfo = constructors.FirstOrDefault(ci => ci.GetParameters().Length == 0);
                    if (constructorInfo == null)
                    {
                        // This exception will be thrown when the exception map is constructed on the first error.
                        // It indicates a coding error, but will be caught by any test that generates a git error.
                        throw new ArgumentException($"{exceptionType.Name} does not implement any of the required constructors");
                    }
                }
            }

            // Create a compiled expression that constructs an instance of the exception type invoking the most
            // specific constructor available with the correct number of arguments.
            return Expression.Lambda<Func<string, int, Exception>>(
                Expression.New(constructorInfo, ConstructorParams.Take(constructorInfo.GetParameters().Length)),
                MessageParam, ExitCodeParam
            ).Compile();
        }

        /// <summary>
        /// Whether or not the supplied error message string matches the criteria described by this attribute
        /// </summary>
        /// <param name="errorMessage">The error message to match</param>
        /// <returns>True if the error matches that for this mapping, else false</returns>
        public override bool IsMatchingError(string errorMessage)
        {
            string toMatch = errorMessage ?? "";
            return (string.IsNullOrEmpty(this.Prefix) || toMatch.StartsWith(this.Prefix, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(this.Suffix) || toMatch.EndsWith(this.Suffix, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Create a new instance of the exception type associated with this attribute
        /// </summary>
        /// <param name="exitCode">The git.exe exit code</param>
        /// <param name="errorMessage">The error message from git.exe</param>
        /// <returns>A newly constructed <see cref="Exception"> of the correct type</see>/></returns>
        public override System.Exception CreateException(int exitCode, string errorMessage)
        {
            return this.newFunc(errorMessage, exitCode);
        }
    }
}
