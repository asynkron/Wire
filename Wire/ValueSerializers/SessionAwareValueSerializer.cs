using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.Compilation;

namespace Wire.ValueSerializers
{
    public abstract class SessionAwareValueSerializer<TElementType> : ValueSerializer
    {
        private readonly byte _manifest;
        private readonly MethodInfo _write;
        private readonly ObjectWriter _writeCompiled;

        protected SessionAwareValueSerializer(byte manifest,
            Expression<Func<Action<Stream, TElementType, SerializerSession>>> writeStaticMethod)
        {
            _manifest = manifest;
            _write = GetStaticVoid(writeStaticMethod);

            var c = new Compiler<ObjectWriter>();

            var stream = c.Parameter<Stream>("stream");
            var value = c.Parameter<object>("value");
            var session = c.Parameter<SerializerSession>("session");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));
            c.EmitStaticCall(_write,stream,valueTyped,session);

            _writeCompiled = c.Compile();
        }

        public sealed override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public sealed override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            _writeCompiled(stream, value, session);
        }

        public sealed override void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            c.EmitStaticCall(_write, stream, fieldValue, session);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}