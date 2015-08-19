namespace Wire
{
    public class SerializerSession
    {
        private readonly byte[] _buffer;
        public readonly Serializer Serializer;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[8];
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length)
                return _buffer;
            return new byte[length];
        }
    }
}