// -----------------------------------------------------------------------
//   <copyright file="SessionAwareByteArrayRequiringValueSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

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
        private readonly MethodInfo _read;
        private readonly Func<Stream, byte[], TElementType> _readCompiled;
        private readonly MethodInfo _write;
        private readonly Action<Stream, object, byte[]> _writeCompiled;

        protected SessionAwareValueSerializer(byte manifest,
            Expression<Func<Action<Stream, TElementType, byte[]>>> writeStaticMethod,
            Expression<Func<Func<Stream, byte[], TElementType>>> readStaticMethod)
        {
            _manifest = manifest;
            _write = GetStatic(writeStaticMethod, typeof(void));
            _read = GetStatic(readStaticMethod, typeof(TElementType));


            var c = new Compiler<Action<Stream, object, byte[]>>();


            var stream = c.Parameter<Stream>("stream");
            var value = c.Parameter<object>("value");
            var buffer = c.Parameter<byte[]>("buffer");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));

            c.EmitStaticCall(_write, stream, valueTyped, buffer);

            _writeCompiled = c.Compile();
            var c2 = new Compiler<Func<Stream, byte[], TElementType>>();

            var stream2 = c2.Parameter<Stream>("stream");
            var buffer2 = c2.Parameter<byte[]>("buffer");
            c2.EmitStaticCall(_read, stream2, buffer2);

            _readCompiled = c2.Compile();
        }

        public sealed override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public sealed override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            _writeCompiled(stream, value, session.GetBuffer(PreallocatedByteBufferSize));
        }

        public sealed override void EmitWriteValue(Compiler<ObjectWriter> c,
            FastExpressionCompiler.LightExpression.Expression stream,
            FastExpressionCompiler.LightExpression.Expression fieldValue,
            FastExpressionCompiler.LightExpression.Expression session)
        {
            var byteArray = c.GetVariable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            c.EmitStaticCall(_write, stream, fieldValue, byteArray);
        }

        public sealed override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _readCompiled(stream, session.GetBuffer(PreallocatedByteBufferSize));
        }

        public sealed override FastExpressionCompiler.LightExpression.Expression EmitReadValue(Compiler<ObjectReader> c,
            FastExpressionCompiler.LightExpression.Expression stream,
            FastExpressionCompiler.LightExpression.Expression session, FieldInfo field)
        {
            var byteArray = c.GetVariable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            return c.StaticCall(_read, stream, byteArray);
        }

        public sealed override Type GetElementType()
        {
            return typeof(TElementType);
        }
    }
}