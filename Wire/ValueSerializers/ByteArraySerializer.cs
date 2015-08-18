using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new ByteArraySerializer();
        private readonly byte[] _manifest = {9};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = (byte[]) value;
            Int32Serializer.Instance.WriteValue(stream, bytes.Length, session);
            stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteValue(Stream stream, byte[] bytes, SerializerSession session)
        {
            Int32Serializer.Instance.WriteValue(stream, bytes.Length, session);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var length = (int) Int32Serializer.Instance.ReadValue(stream, session);
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        public override Type GetElementType()
        {
            return typeof (byte[]);
        }
    }
}