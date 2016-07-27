using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt32Serializer : SessionAwareValueSerializer<uint>
    {
        public const byte Manifest = 18;
        public const int Size = sizeof(uint);
        public static readonly UInt32Serializer Instance = new UInt32Serializer();

        public UInt32Serializer() : base(Manifest, () => WriteValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, uint u, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(u, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToUInt32(buffer, 0);
        }
    }
}