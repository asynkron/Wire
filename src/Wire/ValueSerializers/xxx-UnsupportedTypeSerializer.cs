// -----------------------------------------------------------------------
//   <copyright file="UnsupportedTypeSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Buffers;
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

        public override Expression EmitReadValue(Compiler c, Expression stream, Expression session,
            FieldInfo field)
        {
            throw _exception;
        }

        public override void EmitWriteValue(Compiler c, Expression writer,
            Expression value,
            Expression session)
        {
            throw _exception;
        }

        public override object? ReadValue(Stream stream, DeserializerSession session)
        {
            throw _exception;
        }

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            throw _exception;
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            throw _exception;
        }

        public override Type GetElementType()
        {
            throw _exception;
        }
    }
}