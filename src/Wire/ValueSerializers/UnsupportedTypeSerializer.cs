// -----------------------------------------------------------------------
//   <copyright file="UnsupportedTypeSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Compilation;

namespace Wire.ValueSerializers
{
    //https://github.com/AsynkronIT/Wire/issues/115

    public class UnsupportedTypeException : Exception
    {
        public Type Type;

        public UnsupportedTypeException(Type t, string msg) : base(msg)
        {
        }
    }

    public class UnsupportedTypeSerializer : ValueSerializer
    {
        private readonly string _errorMessage;
        private readonly Type _invalidType;

        public UnsupportedTypeSerializer(Type t, string msg)
        {
            _errorMessage = msg;
            _invalidType = t;
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session,
            FieldInfo field)
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression fieldValue,
            Expression session)
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }

        public override Type GetElementType()
        {
            throw new UnsupportedTypeException(_invalidType, _errorMessage);
        }
    }
}