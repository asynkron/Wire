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
        private readonly Action<Stream, object, SerializerSession> _writeCompiled;
        private readonly MethodInfo _read;
        private readonly Func<Stream, DeserializerSession, TElementType> _readCompiled;

        protected SessionAwareValueSerializer(byte manifest,
            Expression<Func<Action<Stream, TElementType, SerializerSession>>> writeStaticMethod,
            Expression<Func<Func<Stream, DeserializerSession, TElementType>>> readStaticMethod)
        {
            _manifest = manifest;
            _write = GetStatic(writeStaticMethod, typeof(void));
            _read = GetStatic(readStaticMethod, typeof(TElementType));

            var c = new Compiler<Action<Stream, object, SerializerSession>>();

            var stream = c.Parameter<Stream>("stream");
            var value = c.Parameter<object>("value");
            var session = c.Parameter<SerializerSession>("session");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));
            c.EmitStaticCall(_write, stream, valueTyped, session);

            _writeCompiled = c.Compile();

            var c2 = new Compiler<Func<Stream, DeserializerSession, TElementType>>();
            var stream2 = c2.Parameter<Stream>("stream");
            var session2 = c2.Parameter<DeserializerSession>("session");
            c2.EmitStaticCall(_read,stream2,session2);

            _readCompiled = c2.Compile();
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

        public sealed override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _readCompiled(stream, session);
        }

        public sealed override int EmitReadValue(ICompiler<ObjectReader> c, int stream, int session, FieldInfo field)
        {
            return c.StaticCall(_read, stream, session);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}