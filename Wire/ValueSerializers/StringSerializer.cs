using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class StringSerializer : SessionAwareValueSerializer<string>
    {
        public const byte Manifest = 7;
        public static readonly StringSerializer Instance = new StringSerializer();

        public StringSerializer()
            : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, string s, SerializerSession session)
        {
            int byteCount;
            var bytes = NoAllocBitConverter.GetBytes(s, session, out byteCount);
            stream.Write(bytes, 0, byteCount);
        }

        public static string ReadValueImpl(Stream stream, DeserializerSession session)
        {
            return stream.ReadString(session);
        }
    }
}