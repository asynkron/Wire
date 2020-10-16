// -----------------------------------------------------------------------
//   <copyright file="ValueSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
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

        public abstract void WriteManifest(Stream stream, SerializerSession session);
        public abstract void WriteValue(Stream stream, object value, SerializerSession session);
        public abstract object ReadValue(Stream stream, DeserializerSession session);
        public abstract Type GetElementType();

        public virtual void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression fieldValue,
            Expression session)
        {
            var converted = c.Convert<object>(fieldValue);
            var method = typeof(ValueSerializer).GetMethod(nameof(WriteValue))!;

            //write it to the value serializer
            var vs = c.Constant(this);
            c.EmitCall(method, vs, stream, converted, session);
        }

        public virtual Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session,
            FieldInfo field)
        {
            var method = typeof(ValueSerializer).GetMethod(nameof(ReadValue))!;
            var ss = c.Constant(this);
            var read = c.Call(method, ss, stream, session);
            read = c.Convert(read, field.FieldType);
            return read;
        }

        protected static MethodInfo GetStatic(LambdaExpression expression, Type expectedReturnType)
        {
            var unaryExpression = (UnaryExpression) expression.Body;
            var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
            var methodCallObject = (ConstantExpression) methodCallExpression.Object!;
            var method = (MethodInfo) methodCallObject.Value;

            if (method.IsStatic == false) throw new ArgumentException($"Method {method.Name} should be static.");

            if (method.ReturnType != expectedReturnType)
                throw new ArgumentException($"Method {method.Name} should return {expectedReturnType.Name}.");

            return method;
        }
    }
}