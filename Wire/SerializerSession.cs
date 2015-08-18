namespace Wire
{
    public class SerializerSession
    {
        public readonly Serializer Serializer;
        private readonly byte[] _buffer;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[100];
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length)
                return _buffer;
            return new byte[length];
        }
    }
}