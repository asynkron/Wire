using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class NullSerializer : ValueSerializer
    {
        public const byte Manifest = 0;
        public static readonly NullSerializer Instance = new NullSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return null;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }
    }
}