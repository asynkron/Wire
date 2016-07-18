using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class CharSerializer : ValueSerializer
    {
        public const byte Manifest = 15;
        public static readonly CharSerializer Instance = new CharSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((char) value);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof (char);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToSingle(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (char);
        }
    }
}