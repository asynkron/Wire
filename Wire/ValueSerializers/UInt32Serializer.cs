using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt32Serializer : ValueSerializer
    {
        public static readonly UInt32Serializer Instance = new UInt32Serializer();
        public const byte Manifest = 18;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((UInt32)value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof(UInt32);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(UInt32);
        }
    }
}