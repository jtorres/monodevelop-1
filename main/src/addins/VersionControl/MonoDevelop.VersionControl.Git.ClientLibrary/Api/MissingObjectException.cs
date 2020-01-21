//*************************************************************************************************
// RevList.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Microsoft.TeamFoundation.GitApi
{
    public sealed class MissingObjectException : ExceptionBase
    {
        const string MessageFormat = "Object Not Found: {0}";

        internal MissingObjectException(ObjectId objectId)
            : base(MissingMessage(objectId))
        {
            ObjectId = objectId;
        }

        internal MissingObjectException(ObjectId objectId, Exception innerException)
            : base(MissingMessage(objectId), innerException)
        {
            ObjectId = objectId;
        }

        internal MissingObjectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        internal MissingObjectException(string errorMessage)
            : base(errorMessage)
        {
            ObjectId = ExtractObjectIdFromGitError(errorMessage);
        }

        private static ObjectId ExtractObjectIdFromGitError(string errorMessage)
        {
            Match match = ObjectIdExpression.Match(errorMessage);
            return match.Success ? ObjectId.FromString(match.Captures[0].Value) : ObjectId.Zero;
        }
        
        private static readonly Regex ObjectIdExpression = new Regex(@"\b([a-f0-9]{40})\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public readonly ObjectId ObjectId;

        private static string MissingMessage(ObjectId objectId)
        {
            return String.Format(MessageFormat, objectId);
        }
    }
}
