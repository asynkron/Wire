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
using T = System.Single;

namespace Wire.ValueSerializers
{
    public class FloatSerializer : ValueSerializer
    {
        public const byte Manifest = 12;
        public const int Size = sizeof(T);
        public static readonly FloatSerializer Instance = new FloatSerializer();

        private FloatSerializer()
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
            BitConverter.TryWriteBytes(writer.WritableSpan, value);
            writer.AdvanceSpan(Size);
        }

        public static T ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToSingle(bytes, 0);
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