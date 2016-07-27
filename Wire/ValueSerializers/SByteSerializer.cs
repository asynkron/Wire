using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : ValueSerializer
    {
        public const byte Manifest = 20;
        public static readonly SByteSerializer Instance = new SByteSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override unsafe void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var @sbyte = (sbyte) value;
            stream.WriteByte(*(byte*) &@sbyte);
        }

        public override unsafe object ReadValue(Stream stream, DeserializerSession session)
        {
            var @byte = (byte) stream.ReadByte();
            return *(sbyte*) &@byte;
        }

        public override Type GetElementType()
        {
            return typeof(sbyte);
        }
    }
}