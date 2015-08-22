using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        private readonly byte[] _buffer;
        public readonly Dictionary<int, object> ObjectById;
        public readonly Dictionary<object, int> Objects;
        public readonly Serializer Serializer;
        public int NextObjectId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[8];
            if (serializer.Options.PreserveObjectReferences)
            {
                Objects = new Dictionary<object, int>();
                ObjectById = new Dictionary<int, object>();
            }
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length)
                return _buffer;
            return new byte[length];
        }
    }
}