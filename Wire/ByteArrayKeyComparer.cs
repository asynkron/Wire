using System.Collections.Generic;

namespace Wire
{
    /// <summary>
    /// By default ByteArrayKey overrides "public bool Equals(object obj)" to do comparisions.
    /// But this causes boxing/allocations, so by having a custom comparer we can prevent that.
    /// </summary>
    public class ByteArrayKeyComparer : IEqualityComparer<ByteArrayKey>
    {
        public static readonly ByteArrayKeyComparer Instance = new ByteArrayKeyComparer();

        public bool Equals(ByteArrayKey x, ByteArrayKey y)
        {
            return ByteArrayKey.Compare(x.Bytes, y.Bytes);
        }

        public int GetHashCode(ByteArrayKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
