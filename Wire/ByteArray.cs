namespace Wire
{
    public struct ByteArray
    {
        public byte[] Bytes;
        private int HashCode;

        public override bool Equals(object obj)
        {
            var other = (ByteArray) obj;
            return Utils.UnsafeCompare(Bytes, other.Bytes);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public static int GetHashCode(byte[] bytes)
        {
            var hash = 17;
            for (var i = 0; i < bytes.Length; i += 5)
            {
                hash = hash * 23 + bytes[i];
            }
            return hash;
        }

        public static ByteArray Create(byte[] bytes)
        {
            return new ByteArray()
            {
                Bytes = bytes,
                HashCode = GetHashCode(bytes),
            };
        }
    }
}