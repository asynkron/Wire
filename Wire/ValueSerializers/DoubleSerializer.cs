using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DoubleSerializer : ValueSerializer
    {
        public const byte Manifest = 13;
        public static readonly DoubleSerializer Instance = new DoubleSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((double) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (double);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToDouble(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (double);
        }
    }
}