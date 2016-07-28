using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : SessionIgnorantValueSerializer<byte>
    {
        public const byte Manifest = 4;
        public static readonly ByteSerializer Instance = new ByteSerializer();

        public ByteSerializer() : base(Manifest, () => WriteValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return (byte) stream.ReadByte();
        }
    }
}