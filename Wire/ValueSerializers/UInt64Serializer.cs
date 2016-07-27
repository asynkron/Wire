using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class UInt64Serializer : ValueSerializer
    {
        public const byte Manifest = 19;
        public const int Size = sizeof(ulong);
        public static readonly UInt64Serializer Instance = new UInt64Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var ul = (ulong) value;
            WriteValueImpl(stream, session, ul);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, ulong ul)
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

        public override Type GetElementType()
        {
            return typeof(ulong);
        }
    }
}