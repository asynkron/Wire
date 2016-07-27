using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int32Serializer : SessionAwareValueSerializer<int>
    {
        public const byte Manifest = 8;
        public const int Size = sizeof(int);
        public static readonly Int32Serializer Instance = new Int32Serializer();

        public Int32Serializer()
            : base(() => WriteValueImpl)
        {
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public static void WriteValueImpl(Stream stream, int i, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(i, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}