// -----------------------------------------------------------------------
//   <copyright file="Int32Serializer.cs" company="Asynkron HB">
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
using T = System.SByte;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : ValueSerializer
    {
        public const byte Manifest = 20;
        public const int Size = sizeof(sbyte);
        public static readonly SByteSerializer Instance = new SByteSerializer();

        private SByteSerializer()
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        public override void WriteManifest<TBufferWriter>(Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        //used by the serializer, going from virtual calls to static calls

        public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            WriteValueImpl(writer, (sbyte) value);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return ReadValueImpl(stream, session.GetBuffer(Size));
        }

        public override Type GetElementType()
        {
            return typeof(sbyte);
        }

        //the actual impls
        private static void WriteValueImpl<TBufferWriter>(Writer<TBufferWriter> writer, sbyte value)
            where TBufferWriter : IBufferWriter<byte>
        {
            writer.Write(value);
        }

        public static unsafe sbyte ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            var @byte = (byte) stream.ReadByte();
            return *(sbyte*) &@byte;
        }

        //core generation

        public override void EmitWriteValue<TBufferWriter>(Compiler<ObjectWriter<TBufferWriter>> c, Expression writer,
            Expression value,
            Expression session)
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl), BindingFlagsEx.Static)!;
            c.EmitStaticCall(method, writer, value);
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session,
            FieldInfo field)
        {
            var method = GetType().GetMethod(nameof(ReadValueImpl), BindingFlagsEx.Static)!;
            var byteArray = c.GetVariable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            return c.StaticCall(method, stream, byteArray);
        }
    }
}