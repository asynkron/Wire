using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class UInt32Serializer : ValueSerializer
    {
        public const byte Manifest = 18;
        public const int Size = sizeof(uint);
        public static readonly UInt32Serializer Instance = new UInt32Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var u = (uint) value;
            WriteValueImpl(stream, session, u);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, uint u)
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

        public override Type GetElementType()
        {
            return typeof(uint);
        }
    }
}