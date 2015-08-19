using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class CharSerializer : ValueSerializer
    {
        public static readonly CharSerializer Instance = new CharSerializer();
        private readonly byte[] _manifest = {15};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((char) value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof (char);
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