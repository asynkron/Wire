using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : ValueSerializer
    {
        public const byte Manifest = 4;
        public static readonly ByteSerializer Instance = new ByteSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            stream.WriteByte((byte) value);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return (byte) stream.ReadByte();
        }

        public override Type GetElementType()
        {
            return typeof(byte);
        }
    }
}