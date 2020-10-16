// -----------------------------------------------------------------------
//   <copyright file="StringSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Compilation;
using Wire.Extensions;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class StringSerializer : ValueSerializer
    {
        public const byte Manifest = 7;
        public static readonly StringSerializer Instance = new StringSerializer();

        public static void WriteValueImpl(Stream stream, string s, SerializerSession session)
        {
            var bytes = BitConverterEx.GetLengthEncodedBytes(s, session, out var byteCount);
            stream.Write(bytes, 0, byteCount);
        }

        private static string ReadValueImpl(Stream stream, DeserializerSession session)
        {
            return stream.ReadString(session!)!;
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            WriteValueImpl(stream, (string) value, session);
        }

        public override object? ReadValue(Stream stream, DeserializerSession session)
        {
            return ReadValueImpl(stream, session);
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session, FieldInfo field)
        {
            var readMethod =  typeof(StringSerializer).GetMethod(nameof(ReadValueImpl),BindingFlagsEx.Static)!;
            var read = c.StaticCall(readMethod, stream, session);
            return read;
        }

        public override void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression fieldValue, Expression session)
        {
            var writeMethod =  typeof(StringSerializer).GetMethod(nameof(WriteValueImpl),BindingFlagsEx.Static)!;
            c.EmitStaticCall(writeMethod, stream, fieldValue, session);
        }

        public override Type GetElementType()
        {
            return typeof(string);
        }
    }
}