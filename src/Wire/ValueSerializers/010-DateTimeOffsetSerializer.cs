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
using Wire.Internal;
using T = System.DateTimeOffset;

namespace Wire.ValueSerializers
{
    public class DateTimeOffsetSerializer : ValueSerializer
    {
        public const byte Manifest = 10;
        public const int Size =  sizeof(long) + sizeof(byte) + sizeof(short);
        public static readonly DateTimeOffsetSerializer Instance = new DateTimeOffsetSerializer();

        private DateTimeOffsetSerializer()
        {
        }
        
        public override void WriteManifest<TBufferWriter>(Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }
        
        //used by the serializer, going from virtual calls to static calls

        public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session) =>
            WriteValueImpl(writer,(T)value);

        public override object ReadValue(Stream stream, DeserializerSession session) => ReadValueImpl(stream, session.GetBuffer(Size));

        public override int PreallocatedByteBufferSize => Size;

        public override Type GetElementType() => typeof(T);

        //the actual impls
        private static void WriteValueImpl<TBufferWriter>(Writer<TBufferWriter> writer, T value) where TBufferWriter:IBufferWriter<byte>
        {
            writer.Allocate(Size);
            BitConverterEx.TryWriteBytes(writer.WritableSpan, value);
            writer.AdvanceSpan(Size);
        }

        public static T ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            var ticks = BitConverter.ToInt64(bytes, 0);
            var offsetMinutes = BitConverter.ToInt16(bytes, 8);
            var kind = (DateTimeKind) bytes[Size - 1]; //
            var dateTime = new DateTime(ticks, kind);
            var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.FromMinutes(offsetMinutes));
            return dateTimeOffset;
        }
        
        //core generation
        
        public override void EmitWriteValue<TBufferWriter> (Compiler<ObjectWriter<TBufferWriter>> c, Expression writer, Expression value,
            Expression session) 
        {
            var method = GetType().GetMethod(nameof(WriteValueImpl), BindingFlagsEx.Static)!;
            c.EmitStaticCall(method, writer, value);
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session, FieldInfo field)
        {
            var method = GetType().GetMethod(nameof(ReadValueImpl), BindingFlagsEx.Static)!;
            var byteArray = c.GetVariable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            return c.StaticCall(method, stream, byteArray);
        }
    }
}