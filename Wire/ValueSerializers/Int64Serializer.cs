using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class Int64Serializer : ValueSerializer
    {
        public const byte Manifest = 2;
        public const int Size = sizeof(long);
        public static readonly Int64Serializer Instance = new Int64Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var l = (long) value;
            WriteValueImpl(stream, session, l);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, long l)
        {
            var bytes = NoAllocBitConverter.GetBytes(l, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToInt64(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(long);
        }
    }
}