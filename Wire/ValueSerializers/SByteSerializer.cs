using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : ValueSerializer
    {
        public const byte Manifest = 20;
        public static readonly SByteSerializer Instance = new SByteSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var @sbyte = (sbyte) value;
            WriteValueImpl(stream, @sbyte);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl));
            c.EmitStaticCall(method, stream, fieldValue);
        }

        public static unsafe void WriteValueImpl(Stream stream, sbyte @sbyte)
        {
            stream.WriteByte(*(byte*) &@sbyte);
        }

        public override unsafe object ReadValue(Stream stream, DeserializerSession session)
        {
            var @byte = (byte) stream.ReadByte();
            return *(sbyte*) &@byte;
        }

        public override Type GetElementType()
        {
            return typeof(sbyte);
        }
    }
}