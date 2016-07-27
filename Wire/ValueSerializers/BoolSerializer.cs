using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class BoolSerializer : ValueSerializer
    {
        public const byte Manifest = 6;
        public static readonly BoolSerializer Instance = new BoolSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var b = (bool) value;
            WriteValueImpl(stream, b);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var b = stream.ReadByte();
            return b != 0;
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, bool b)
        {
            stream.WriteByte((byte) (b ? 1 : 0));
        }

        public override Type GetElementType()
        {
            return typeof(bool);
        }
    }
}