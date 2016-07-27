using System;
using System.IO;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public abstract class ValueSerializer
    {
        public abstract void WriteManifest(Stream stream, SerializerSession session);
        public abstract void WriteValue(Stream stream, object value, SerializerSession session);
        public abstract object ReadValue(Stream stream, DeserializerSession session);
        public abstract Type GetElementType();

        public virtual void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var converted = c.ConvertTo<object>(fieldValue);
            var method = typeof(ValueSerializer).GetMethod(nameof(WriteValue));

            //write it to the value serializer
            var vs = c.Constant(this);
            c.EmitCall(method, vs, stream, converted, session);
        }
    }
}