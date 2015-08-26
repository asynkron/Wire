using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Wire
{
    public class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }

        public bool Equals(T left, T right)
        {
            return left == right; // Reference identity comparison
        }
    }
}