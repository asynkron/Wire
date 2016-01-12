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
            var hash = 17;
            if (obj == null)
                return hash;
            for (var i = 0; i < obj.Length; i += 5)
            {
                hash = hash*23 + obj[i];
            }
            return hash;
        }
    }
}