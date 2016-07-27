using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public abstract class SessionAwareValueSerializer<TElementType> : ValueSerializer
    {
        private readonly MethodInfo _write;
        private readonly Action<Stream, object, SerializerSession> _writeCompiled;

        protected SessionAwareValueSerializer(
            Expression<Func<Action<Stream, TElementType, SerializerSession>>> writeStaticMethod)
        {
            _write = GetStaticVoid(writeStaticMethod);

            var stream = Expression.Parameter(typeof(Stream));
            var value = Expression.Parameter(typeof(object));
            var session = Expression.Parameter(typeof(SerializerSession));

            _writeCompiled = Expression.Lambda<Action<Stream, object, SerializerSession>>(
                Expression.Call(_write, stream, Expression.Convert(value, typeof(TElementType)), session), stream, value,
                session).Compile();
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