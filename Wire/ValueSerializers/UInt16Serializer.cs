using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt16Serializer : ValueSerializer
    {
        public const byte Manifest = 17;
        public const int Size = sizeof (ushort);
        public static readonly UInt16Serializer Instance = new UInt16Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes((ushort) value, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (ushort);
        }
    }
}