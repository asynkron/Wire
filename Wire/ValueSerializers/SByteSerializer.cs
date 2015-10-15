using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : ValueSerializer
    {
        public static readonly SByteSerializer Instance = new SByteSerializer();
        public const byte Manifest = 20;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((sbyte)value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return (sbyte)stream.ReadByte();
        }

        public override Type GetElementType()
        {
            return typeof(sbyte);
        }
    }
}