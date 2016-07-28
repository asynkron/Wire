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

            var stream = Expression.Parameter(typeof(Stream));
            var value = Expression.Parameter(typeof(object));
            var session = Expression.Parameter(typeof(SerializerSession));

            _writeCompiled = Expression.Lambda<Action<Stream, object, SerializerSession>>(
                Expression.Call(_write, stream, Expression.Convert(value, typeof(TElementType)), session), stream, value,
                session).Compile();

            session = Expression.Parameter(typeof(DeserializerSession));
            _readCompiled = Expression.Lambda<Func<Stream, DeserializerSession, TElementType>>(
                Expression.Call(_read, stream, session), stream,
                session).Compile();
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

        public sealed override int EmitReadValue(Compiler<ObjectReader> c, int stream, int session, FieldInfo field)
        {
            return c.StaticCall(_read, stream, session);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}