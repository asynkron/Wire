using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int64Serializer : SessionAwareValueSerializer<long>
    {
        public const byte Manifest = 2;
        public const int Size = sizeof(long);
        public static readonly Int64Serializer Instance = new Int64Serializer();

        public Int64Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, long l, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(l, session);
            stream.Write(bytes, 0, Size);
        }

        public static long ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}