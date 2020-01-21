using System;
using System.Collections.Generic;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal class ReferenceCanonicalNameComparer : IComparer<IReference>, IEqualityComparer<IReference>
    {
        public ReferenceCanonicalNameComparer()
        { }

        public int Compare(IReference left, IReference right)
        {
            return StringComparer.Ordinal.Compare(left?.CanonicalName, right?.CanonicalName);
        }

        public bool Equals(IReference left, IReference right)
        {
            return StringComparer.Ordinal.Equals(left?.CanonicalName, right?.CanonicalName);
        }

        public int GetHashCode(IReference value)
        {
            return StringComparer.Ordinal.GetHashCode(value?.CanonicalName);
        }
    }
}
