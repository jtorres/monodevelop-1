//*************************************************************************************************
// Tag.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representation of a Git tag name, reference, or object.
    /// </summary>
    public interface ITagName : IReferenceName
    { }

    public sealed class TagName : ReferenceName, ITagName
    {
        public TagName(string name)
            : this((StringUtf8)name)
        { }

        internal TagName(StringUtf8 name)
            : base(name)
        { }

        /// <summary>
        /// Determine if tagName is a syntactically valid full tag name (beginning with "refs/tags/").
        /// </summary>
        public static bool IsLegalFullyQualifiedName(string name)
        {
            if (name == null)
                return false;
            if (!name.StartsWith(PatternRefTags))
                return false;

            return Reference.IsLegalName(name);
        }

        protected override void ParseName(StringUtf8 name, out StringUtf8 canonicalName, out StringUtf8 friendlyName)
        {
            var local = (string)name;

            if (IsLegalFullyQualifiedName(local))
            {
                canonicalName = name;
                friendlyName = name.Substring(PatternRefTags.Length);
            }
            else if (IsLegalName(local))
            {
                friendlyName = name;
                canonicalName = PatternRefTagsUtf8 + _friendlyName;
            }
            else
            {
                throw TagNameException.FromName(local);
            }
        }
    }
}
