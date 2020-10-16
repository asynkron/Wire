// -----------------------------------------------------------------------
//   <copyright file="SessionAwareByteArrayRequiringValueSerializer.cs" company="Asynkron HB">
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
    public abstract class SessionAwareValueSerializer<TElementType> : ValueSerializer
    {
        private readonly byte _manifest;
        private readonly MethodInfo _read;
        private readonly Func<Stream, byte[], TElementType> _readCompiled;
        private readonly MethodInfo _write;
        private readonly Action<IBufferWriter<byte>, object, int> _writeCompiled;

        protected SessionAwareValueSerializer(byte manifest,
            Expression<Func<Action<IBufferWriter<byte>, TElementType, int>>> writeStaticMethod,
            Expression<Func<Func<Stream, byte[], TElementType>>> readStaticMethod)
        {
            _manifest = manifest;
            _write = GetStatic(writeStaticMethod, typeof(void));
            _read = GetStatic(readStaticMethod, typeof(TElementType));
            
            var c = new Compiler<Action<IBufferWriter<byte>, object, int>>();
            
            var stream = c.Parameter<IBufferWriter<byte>>("stream");
            var value = c.Parameter<object>("value");
            var size = c.Parameter<int>("size");
            var valueTyped = c.CastOrUnbox(value, typeof(TElementType));

            c.EmitStaticCall(_write, stream, valueTyped, size);

            _writeCompiled = c.Compile();
            
            
            var c2 = new Compiler<Func<Stream, byte[], TElementType>>();

            var stream2 = c2.Parameter<Stream>("stream");
            var buffer2 = c2.Parameter<byte[]>("buffer");
            c2.EmitStaticCall(_read, stream2, buffer2);

            _readCompiled = c2.Compile();
        }

        public sealed override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            stream.WriteByte(_manifest);
        }

        public sealed override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            _writeCompiled(stream, value, PreallocatedByteBufferSize);
        }

        public sealed override void EmitWriteValue(Compiler<ObjectWriter> c,
            FastExpressionCompiler.LightExpression.Expression stream,
            FastExpressionCompiler.LightExpression.Expression fieldValue,
            FastExpressionCompiler.LightExpression.Expression session)
        {
            var size = c.GetVariable<int>(SerializerCompiler.PreallocatedByteBuffer);
            c.EmitStaticCall(_write, stream, fieldValue, size);
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