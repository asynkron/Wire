using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt16Serializer : ValueSerializer
    {
        public const byte Manifest = 17;
        public static readonly UInt16Serializer Instance = new UInt16Serializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((ushort) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (ushort);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (ushort);
        }
    }
}