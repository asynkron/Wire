using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt64Serializer : SessionAwareValueSerializer<ulong>
    {
        public const byte Manifest = 19;
        public const int Size = sizeof(ulong);
        public static readonly UInt64Serializer Instance = new UInt64Serializer();

        public UInt64Serializer() : base(Manifest, () => WriteValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, ulong ul, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(ul, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}