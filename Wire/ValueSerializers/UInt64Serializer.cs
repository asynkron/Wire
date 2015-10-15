using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt64Serializer : ValueSerializer
    {
        public static readonly UInt64Serializer Instance = new UInt64Serializer();
        public const byte Manifest = 19;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((UInt64)value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof(UInt64);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(UInt64);
        }
    }
}