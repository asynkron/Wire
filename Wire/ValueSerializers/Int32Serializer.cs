using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int32Serializer : ValueSerializer
    {
        public const byte Manifest = 8;
        public static readonly Int32Serializer Instance = new Int32Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((int) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (int);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToInt32(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (int);
        }
    }
}