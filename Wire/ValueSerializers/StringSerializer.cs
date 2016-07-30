using System;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class StringSerializer : ValueSerializer
    {
        public const byte Manifest = 7;
        public static readonly StringSerializer Instance = new StringSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            stream.WriteString(value);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return stream.ReadString(session);
        }

        public override Type GetElementType()
        {
            return TypeEx.StringType;
        }
    }
}