using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public static readonly ByteArraySerializer Instance = new ByteArraySerializer();
        public const byte Manifest = 9;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = (byte[]) value;
            stream.WriteLengthEncodedByteArray(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return stream.ReadLengthEncodedByteArray(session);
        }

        public override Type GetElementType()
        {
            return typeof (byte[]);
        }
    }
}