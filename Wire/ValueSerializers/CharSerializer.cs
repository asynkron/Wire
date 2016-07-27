using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class CharSerializer : ValueSerializer
    {
        public const byte Manifest = 15;
        public const int Size = sizeof(char);
        public static readonly CharSerializer Instance = new CharSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var ch = (char) value;
            WriteValueImpl(stream, session, ch);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return BitConverter.ToSingle(buffer, 0);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, session, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, SerializerSession session, char ch)
        {
            var bytes = NoAllocBitConverter.GetBytes(ch, session);
            stream.Write(bytes, 0, Size);
        }

        public override Type GetElementType()
        {
            return typeof(char);
        }
    }
}