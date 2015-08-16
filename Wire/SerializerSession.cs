namespace Wire
{
    public class SerializerSession
    {
        public Serializer Serializer { get; set; }
        public byte[] Buffer { get; set; }

        public byte[] GetBuffer(int length)
        {
            if (length <= Buffer.Length)
                return Buffer;
            return new byte[length];
        }
    }
}