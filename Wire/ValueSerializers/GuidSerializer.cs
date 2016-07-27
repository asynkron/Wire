using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class GuidSerializer : SessionIgnorantValueSerializer<Guid>
    {
        public const byte Manifest = 11;
        public static readonly GuidSerializer Instance = new GuidSerializer();

        public GuidSerializer() : base(() => WriteValueImpl)
        {
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public static void WriteValueImpl(Stream stream, Guid g)
        {
            var bytes = g.ToByteArray();
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            return new Guid(buffer);
        }
    }
}