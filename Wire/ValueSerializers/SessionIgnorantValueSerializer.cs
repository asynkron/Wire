using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.ExpressionDSL;

namespace Wire.ValueSerializers
{
    public abstract class SessionIgnorantValueSerializer<TElementType> : ValueSerializer
    {
        private readonly MethodInfo _write;
        private readonly Action<Stream, object> _writeCompiled;

        protected SessionIgnorantValueSerializer(
            Expression<Func<Action<Stream, TElementType>>> writeStaticMethod)
        {
            _write = GetStaticVoid(writeStaticMethod);

            var stream = Expression.Parameter(typeof(Stream));
            var value = Expression.Parameter(typeof(object));

            _writeCompiled = Expression.Lambda<Action<Stream, object>>(
                Expression.Call(_write, stream, Expression.Convert(value, typeof(TElementType))), stream, value)
                .Compile();
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