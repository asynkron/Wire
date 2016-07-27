using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : ValueSerializer
    {
        public const byte Manifest = 4;
        public static readonly ByteSerializer Instance = new ByteSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var b = (byte) value;
            WriteValueImpl(stream, b);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, fieldValue);
        }

        public static void WriteValueImpl(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return (byte) stream.ReadByte();
        }

        public override Type GetElementType()
        {
            return typeof(byte);
        }
    }
}