using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.Compilation;

namespace Wire.ValueSerializers
{
    public abstract class SessionIgnorantValueSerializer<TElementType> : ValueSerializer
    {
        private readonly byte _manifest;
        private readonly MethodInfo _write;
        private readonly Action<Stream, object> _writeCompiled;

        protected SessionIgnorantValueSerializer(byte manifest,
            Expression<Func<Action<Stream, TElementType>>> writeStaticMethod)
        {
            _manifest = manifest;
            _write = GetStaticVoid(writeStaticMethod);

            var c = new Compiler<Action<Stream, object>>();

            var stream = c.Parameter<Stream>("stream");
            var value = c.Parameter<object>("value");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));
            c.EmitStaticCall(_write,stream, valueTyped);

            _writeCompiled = c.Compile();
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public sealed override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            _writeCompiled(stream, value);
        }

        public sealed override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            c.EmitStaticCall(_write, stream, fieldValue);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}