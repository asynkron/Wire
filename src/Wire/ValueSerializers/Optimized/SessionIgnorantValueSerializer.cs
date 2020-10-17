// -----------------------------------------------------------------------
//   <copyright file="SessionIgnorantValueSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.Compilation;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public abstract class SessionIgnorantValueSerializer<TElementType> : ValueSerializer
    {
        private readonly byte _manifest;
        private readonly MethodInfo _read;
        private readonly Func<Stream, TElementType> _readCompiled;
        private readonly MethodInfo _write;
        private readonly Action<IBufferWriter<byte>, object> _writeCompiled;

        protected SessionIgnorantValueSerializer(byte manifest,
            Expression<Func<Action<IBufferWriter<byte>, TElementType>>> writeStaticMethod,
            Expression<Func<Func<Stream, TElementType>>> readStaticMethod)
        {
            _manifest = manifest;
            _write = GetStatic(writeStaticMethod, typeof(void));
            _read = GetStatic(readStaticMethod, typeof(TElementType));


            var c = new Compiler<Action<IBufferWriter<byte>, object>>();


            var stream = c.Parameter<IBufferWriter<byte>>("stream");
            var value = c.Parameter<object>("value");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));
            c.EmitStaticCall(_write, stream, valueTyped);

            _writeCompiled = c.Compile();


            var c2 = new Compiler<Func<Stream, TElementType>>();


            var stream2 = c2.Parameter<Stream>("stream");
            c2.EmitStaticCall(_read, stream2);

            _readCompiled = c2.Compile();
        }

        public sealed override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = _manifest;
            stream.Advance(1);
        }

        public sealed override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            _writeCompiled(stream, value);
        }

        public sealed override void EmitWriteValue(Compiler<ObjectWriter> c,
            FastExpressionCompiler.LightExpression.Expression stream,
            FastExpressionCompiler.LightExpression.Expression fieldValue,
            FastExpressionCompiler.LightExpression.Expression session)
        {
            c.EmitStaticCall(_write, stream, fieldValue);
        }

        public sealed override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _readCompiled(stream);
        }

        public sealed override FastExpressionCompiler.LightExpression.Expression EmitReadValue(Compiler<ObjectReader> c,
            FastExpressionCompiler.LightExpression.Expression stream,
            FastExpressionCompiler.LightExpression.Expression session, FieldInfo field)
        {
            return c.StaticCall(_read, stream);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}