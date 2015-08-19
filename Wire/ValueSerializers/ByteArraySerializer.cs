using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public static readonly ByteArraySerializer Instance = new ByteArraySerializer();
        private readonly byte[] _manifest = {9};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = (byte[]) value;
            stream.WriteInt32(bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(Stream stream, byte[] bytes, SerializerSession session)
        {
            stream.WriteInt32(bytes.Length);
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