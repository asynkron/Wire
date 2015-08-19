using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Wire.ValueSerializers
{
    public class Int32Serializer : ValueSerializer
    {
        public static readonly Int32Serializer Instance = new Int32Serializer();
        private readonly byte[] _manifest = {8};

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((int) value);
            stream.Write(bytes, 0, bytes.Length);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void WriteValue(Stream stream, int value, SerializerSession session)
        //{
        //    var bytes = BitConverter.GetBytes(value);
        //    stream.Write(bytes, 0, bytes.Length);
        //}

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var size = sizeof(int);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            return BitConverter.ToInt32(buffer, 0);
        }

        //public static int ReadValue(Stream stream, SerializerSession session)
        //{
        //    var size = sizeof(int);
        //    var buffer = session.GetBuffer(size);
        //    stream.Read(buffer, 0, size);
        //    return BitConverter.ToInt32(buffer, 0);
        //}

        public override Type GetElementType()
        {
            return typeof (int);
        }
    }
}