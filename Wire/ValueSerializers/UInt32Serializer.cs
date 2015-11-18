using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt32Serializer : ValueSerializer
    {
        public const byte Manifest = 18;
        public static readonly UInt32Serializer Instance = new UInt32Serializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((uint) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var size = sizeof (uint);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (uint);
        }
    }
}