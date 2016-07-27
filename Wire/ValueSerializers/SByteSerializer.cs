using System.IO;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : SessionIgnorantValueSerializer<sbyte>
    {
        public const byte Manifest = 20;
        public static readonly SByteSerializer Instance = new SByteSerializer();

        public SByteSerializer() : base(Manifest, () => WriteValueImpl)
        {
        }

        public static unsafe void WriteValueImpl(Stream stream, sbyte @sbyte)
        {
            stream.WriteByte(*(byte*) &@sbyte);
        }

        public override unsafe object ReadValue(Stream stream, DeserializerSession session)
        {
            var @byte = (byte) stream.ReadByte();
            return *(sbyte*) &@byte;
        }
    }
}