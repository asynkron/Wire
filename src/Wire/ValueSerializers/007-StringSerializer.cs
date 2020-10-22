// -----------------------------------------------------------------------
//   <copyright file="StringSerializer.cs" company="Asynkron HB">
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
using Wire.Extensions;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class StringSerializer : ValueSerializer
    {
        public const byte Manifest = 7;
        public static readonly StringSerializer Instance = new StringSerializer();

        public static void WriteValueImpl<TBufferWriter>(Writer<TBufferWriter> writer, string? value)
            where TBufferWriter : IBufferWriter<byte>
        {
            //if first byte is 0 = null
            //if first byte is 254 or less, then length is value - 1
            //if first byte is 255 then the next 4 bytes are an int32 for length
            if (value == null)
            {
                writer.Write((byte) 0);
                return;
            }

            var byteCount = BitConverterEx.Utf8.GetByteCount(value);
            if (byteCount < 254) //short string
            {
                writer.Allocate(byteCount + 1);
                var span = writer.WritableSpan;
                span[0] = (byte) (byteCount + 1);
                BitConverterEx.Utf8.GetBytes(value, span[1..]);
                writer.AdvanceSpan(byteCount + 1);
                return;
            }

            writer.Allocate(byteCount + 1 + 4);

            var span2 = writer.WritableSpan;
            //signal int count
            span2[0] = 255;
            //int count
            BitConverter.TryWriteBytes(span2[1..], byteCount);
            //write actual string content
            BitConverterEx.Utf8.GetBytes(value, span2[(1 + 4)..]);
            writer.AdvanceSpan(byteCount + 1 + 4);
        }

        private static string ReadValueImpl(Stream stream, DeserializerSession session)
        {
            return stream.ReadString(session!)!;
        }

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            WriteValueImpl(writer, (string) value);
        }

        public override object? ReadValue(Stream stream, DeserializerSession session)
        {
            return ReadValueImpl(stream, session);
        }

        public override Expression EmitReadValue(Compiler c, Expression stream, Expression session,
            FieldInfo field)
        {
            var method = GetType().GetMethod(nameof(ReadValueImpl), BindingFlagsEx.Static)!;
            return c.StaticCall(method, stream, session);
        }

        public override void EmitWriteValue(Compiler c, Expression writer,
            Expression value,
            Expression session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl), BindingFlagsEx.Static)!;
            c.EmitStaticCall(method, writer, value);
        }

        public override Type GetElementType()
        {
            return typeof(string);
        }
    }
}