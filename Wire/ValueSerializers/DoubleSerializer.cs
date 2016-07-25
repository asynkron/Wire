using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DoubleSerializer : ValueSerializer
    {
        public const byte Manifest = 13;
        const int Size = sizeof(double);
        public static readonly DoubleSerializer Instance = new DoubleSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes((double) value, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToDouble(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(double);
        }
    }
}