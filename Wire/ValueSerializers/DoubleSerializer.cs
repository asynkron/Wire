using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DoubleSerializer : SessionAwareValueSerializer<double>
    {
        public const byte Manifest = 13;
        const int Size = sizeof(double);
        public static readonly DoubleSerializer Instance = new DoubleSerializer();

        public DoubleSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static void WriteValueImpl(Stream stream, double d, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(d, session);
            stream.Write(bytes, 0, Size);
        }

        public static double ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToDouble(buffer, 0);
        }
    }
}