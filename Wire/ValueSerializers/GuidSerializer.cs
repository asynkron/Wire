using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class GuidSerializer : ValueSerializer
    {
        public static readonly GuidSerializer Instance = new GuidSerializer();
        public const byte Manifest = 11;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
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
            var buffer = session.GetBuffer(16);
            stream.Read(buffer, 0, 16);
            return new Guid(buffer); //TODO: cap array?
        }

        public override Type GetElementType()
        {
            return typeof (Guid);
        }
    }
}