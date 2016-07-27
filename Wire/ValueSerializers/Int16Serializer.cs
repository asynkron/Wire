using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class Int16Serializer : ValueSerializer
    {
        public const byte Manifest = 3;
        public const int Size = sizeof(short);
        public static readonly Int16Serializer Instance = new Int16Serializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var sh = (short) value;
            WriteValueImpl(stream, session, sh);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, short sh)
        {
            var bytes = NoAllocBitConverter.GetBytes(sh, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToInt16(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(short);
        }
    }
}