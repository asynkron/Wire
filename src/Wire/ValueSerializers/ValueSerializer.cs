// -----------------------------------------------------------------------
//   <copyright file="ValueSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

using System.Reflection;
using Wire.Compilation;
using Wire.Internal;
using FastExpressionCompiler.LightExpression;

namespace Wire.ValueSerializers
{
    public abstract class ValueSerializer
    {
        /// <summary>
        ///     Marks a given <see cref="ValueSerializer" /> as one requiring a preallocated byte buffer to perform its operations.
        ///     The byte[] value will be accessible in <see cref="ValueSerializer.EmitWriteValue" /> and
        ///     <see cref="ValueSerializer.EmitReadValue" /> in the <see cref="ICompiler{TDel}" /> with
        ///     <see cref="ICompiler{TDel}.GetVariable{T}" /> under following name
        ///     <see cref="DefaultCodeGenerator.PreallocatedByteBuffer" />.
        /// </summary>
        public virtual int PreallocatedByteBufferSize => 0;

        public abstract void WriteManifest( Stream stream,  SerializerSession session);
        public abstract void WriteValue( Stream stream, object value,  SerializerSession session);
        public abstract object ReadValue( Stream stream,  DeserializerSession session);
        public abstract Type GetElementType();

        public virtual void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression fieldValue, Expression session)
        {
            var converted = c.Convert<object>(fieldValue);
            var method = typeof(ValueSerializer).GetMethod(nameof(WriteValue))!;

            //write it to the value serializer
            var vs = c.Constant(this);
            c.EmitCall(method, vs, stream, converted, session);
        }

        public virtual Expression EmitReadValue( Compiler<ObjectReader> c, Expression stream, Expression session,
             FieldInfo field)
        {
            var method = typeof(ValueSerializer).GetMethod(nameof(ReadValue))!;
            var ss = c.Constant(this);
            var read = c.Call(method, ss, stream, session);
            read = c.Convert(read, field.FieldType);
            return read;
        }

        protected static MethodInfo GetStatic( System.Linq.Expressions.LambdaExpression expression,  Type expectedReturnType)
        {
            var unaryExpression = (System.Linq.Expressions.UnaryExpression) expression.Body;
            var methodCallExpression = (System.Linq.Expressions.MethodCallExpression) unaryExpression.Operand;
            var methodCallObject = (System.Linq.Expressions.ConstantExpression) methodCallExpression.Object!;
            var method = (MethodInfo) methodCallObject.Value;

            if (method.IsStatic == false) throw new ArgumentException($"Method {method.Name} should be static.");

            if (method.ReturnType != expectedReturnType)
                throw new ArgumentException($"Method {method.Name} should return {expectedReturnType.Name}.");

            return method;
        }
    }
}