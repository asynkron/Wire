using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class GuidSerializer : ValueSerializer
    {
        public const byte Manifest = 11;
        public static readonly GuidSerializer Instance = new GuidSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = ((Guid) value).ToByteArray();
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            return new Guid(buffer);
        }

        public override Type GetElementType()
        {
            return typeof (Guid);
        }
    }
}