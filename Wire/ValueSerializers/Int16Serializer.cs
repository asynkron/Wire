using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int16Serializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new Int16Serializer();
        private readonly byte[] _manifest = {3};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((short) value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var buffer = session.GetBuffer(2);
            stream.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof (short);
        }
    }
}