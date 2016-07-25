using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int64Serializer : ValueSerializer
    {
        public const byte Manifest = 2;
        public static readonly Int64Serializer Instance = new Int64Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((long) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (long);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToInt64(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (long);
        }
    }
}