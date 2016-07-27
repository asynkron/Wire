using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class FloatSerializer : ValueSerializer
    {
        public const byte Manifest = 12;
        public const int Size = sizeof(float);
        public static readonly FloatSerializer Instance = new FloatSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var f = (float) value;
            WriteValueImpl(stream, session, f);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, float f)
        {
            var bytes = NoAllocBitConverter.GetBytes(f, session);
            stream.Write(bytes, 0, Size);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToSingle(buffer, 0);
        }

        public override Type GetElementType()
        {
            return typeof(float);
        }
    }
}