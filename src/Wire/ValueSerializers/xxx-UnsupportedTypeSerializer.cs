// -----------------------------------------------------------------------
//   <copyright file="UnsupportedTypeSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Compilation;

namespace Wire.ValueSerializers
{
    //https://github.com/AsynkronIT/Wire/issues/115

    public class UnsupportedTypeSerializer : ValueSerializer
    {
        private readonly Exception _exception;
        private readonly Type _invalidType;

        public UnsupportedTypeSerializer(Type t, Exception exception)
        {
            _exception = exception;
            _invalidType = t;
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session,
            FieldInfo field)
        {
            throw _exception;
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression value,
            Expression session)
        {
            throw _exception;
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            throw _exception;
        }

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            throw _exception;
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            throw _exception;
        }

        public override Type GetElementType()
        {
            throw _exception;
        }
    }
}