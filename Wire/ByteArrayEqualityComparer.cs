using System.Collections.Generic;

namespace Wire
{
    public class ByteArrayEqualityComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] x, byte[] y)
        {
            return Utils.UnsafeCompare(x, y);
        }

        public override int GetHashCode(byte[] obj)
        {
            int hash = 17;
            for (int i = 0; i < obj.Length; i += 5)
            {
                hash = hash * 23 + obj[i];
            }
            return hash;
        }
    }
}