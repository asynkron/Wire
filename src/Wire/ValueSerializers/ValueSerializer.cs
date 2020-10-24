// -----------------------------------------------------------------------
//   <copyright file="ValueSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Buffers;
using Wire.Compilation;
using ConstantExpression = System.Linq.Expressions.ConstantExpression;
using LambdaExpression = System.Linq.Expressions.LambdaExpression;
using MethodCallExpression = System.Linq.Expressions.MethodCallExpression;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace Wire.ValueSerializers
{
    public abstract class ValueSerializer
    {
        /// <summary>
        ///     Marks a given <see cref="ValueSerializer" /> as one requiring a preallocated byte buffer to perform its operations.
        ///     The byte[] value will be accessible in <see cref="ValueSerializer.EmitWriteValue" /> and
        ///     <see cref="ValueSerializer.EmitReadValue" /> in the <see cref="ICompiler{TDel}" /> with
        ///     <see cref="ICompiler{TDel}.GetVariable{T}" /> under following name
        ///     <see cref="SerializerCompiler.PreallocatedByteBuffer" />.
        /// </summary>
        public virtual int PreallocatedByteBufferSize => 0;

        public abstract void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
            where TBufferWriter : IBufferWriter<byte>;

        public abstract void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session) where TBufferWriter : IBufferWriter<byte>;

        public abstract object? ReadValue(Stream stream, DeserializerSession session);
        public abstract Type GetElementType();

        public virtual void EmitWriteValue(Compiler c, Expression writer,
            Expression value,
            Expression session)
        {
            var converted = c.Convert<object>(value);
            var method = typeof(ValueSerializer).GetMethod(nameof(WriteValue))!;

            //write it to the value serializer
            var vs = c.Constant(this);
            c.EmitCall(method, vs, writer, converted, session);
        }

        public virtual Expression EmitReadValue(Compiler c, Expression stream, Expression session,
            FieldInfo field)
        {
            var method = typeof(ValueSerializer).GetMethod(nameof(ReadValue))!;
            var ss = c.Constant(this);
            var read = c.Call(method, ss, stream, session);
            read = c.Convert(read, field.FieldType);
            return read;
        }

        
    }
}