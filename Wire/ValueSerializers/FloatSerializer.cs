using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class FloatSerializer : ValueSerializer
    {
        public static readonly FloatSerializer Instance = new FloatSerializer();
        private readonly byte _manifest = 12;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((float) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof (float);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToSingle(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (float);
        }
    }
}