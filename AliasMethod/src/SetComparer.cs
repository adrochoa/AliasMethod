using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AliasMethod
{
    class SetComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public bool Equals([AllowNull] IEnumerable<T> x, [AllowNull] IEnumerable<T> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] IEnumerable<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
