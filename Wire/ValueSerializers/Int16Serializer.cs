using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class Int16Serializer : SessionAwareValueSerializer<short>
    {
        public const byte Manifest = 3;
        public const int Size = sizeof(short);
        public static readonly Int16Serializer Instance = new Int16Serializer();

        public Int16Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, short sh, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(sh, session);
            stream.Write(bytes, 0, Size);
        }

        public static short ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToInt16(buffer, 0);
        }
    }
}