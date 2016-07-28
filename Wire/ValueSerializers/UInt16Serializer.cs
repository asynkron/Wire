using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt16Serializer : SessionAwareValueSerializer<ushort>
    {
        public const byte Manifest = 17;
        public const int Size = sizeof(ushort);
        public static readonly UInt16Serializer Instance = new UInt16Serializer();

        public UInt16Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, ushort u, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(u, session);
            stream.Write(bytes, 0, Size);
        }

        public static ushort ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToUInt16(buffer, 0);
        }
    }
}