using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class BoolSerializer : ValueSerializer
    {
        public const byte Manifest = 6;
        public static readonly BoolSerializer Instance = new BoolSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var b = (bool) value;
            stream.WriteByte((byte) (b ? 1 : 0));
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var b = stream.ReadByte();
            return b != 0;
        }

        public override Type GetElementType()
        {
            return typeof (bool);
        }
    }
}