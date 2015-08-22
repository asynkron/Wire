using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : ValueSerializer
    {
        public static readonly ByteSerializer Instance = new ByteSerializer();
        private readonly byte _manifest = 4;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((byte) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return (byte) stream.ReadByte();
        }

        public override Type GetElementType()
        {
            return typeof (byte);
        }
    }
}