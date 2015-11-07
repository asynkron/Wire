using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : ValueSerializer
    {
        public static readonly ByteSerializer Instance = new ByteSerializer();
        public const byte Manifest = 4;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((byte) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return (byte) stream.ReadByte();
        }

        public override Type GetElementType()
        {
            return typeof (byte);
        }
    }
}