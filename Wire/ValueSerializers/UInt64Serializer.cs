using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt64Serializer : ValueSerializer
    {
        public const byte Manifest = 19;
        public static readonly UInt64Serializer Instance = new UInt64Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((ulong) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (ulong);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (ulong);
        }
    }
}