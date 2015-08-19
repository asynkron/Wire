using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int64Serializer : ValueSerializer
    {
        public static readonly Int64Serializer Instance = new Int64Serializer();
        private readonly byte[] _manifest = {2};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((long) value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof (long);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToInt64(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (long);
        }
    }
}