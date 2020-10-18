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
using Wire.Compilation;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class Int32Serializer : ValueSerializer
    {
        public const byte Manifest = 8;
        public const int Size = sizeof(int);
        public static readonly Int32Serializer Instance = new Int32Serializer();

        private Int32Serializer()
        {
        }
        
        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = Manifest;
            stream.Advance(1);
        }
        
        //used by the serializer, going from virtual calls to static calls

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session) => WriteValueImpl(stream,(int)value);

        public override object ReadValue(Stream stream, DeserializerSession session) => ReadValueImpl(stream, session.GetBuffer(Size));

        public override int PreallocatedByteBufferSize => Size;

        public override Type GetElementType() => typeof(int);

        //used from other serializers
        public static void WriteValue(IBufferWriter<byte> stream, int value) => WriteValueImpl(stream, value);

        //the actual impls
        private static void WriteValueImpl(IBufferWriter<byte> stream, int value)
        {
            var span = stream.GetSpan(Size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(Size);
        }
        
        public static int ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToInt32(bytes, 0);
        }
        
        //core generation
        
        public override void EmitWriteValue(Compiler<ObjectWriter> c, Expression stream, Expression value, Expression session)
        {
            var method = typeof(Int32Serializer).GetMethod(nameof(WriteValueImpl), BindingFlagsEx.Static)!;
            c.EmitStaticCall(method, stream, value);
        }

        public override Expression EmitReadValue(Compiler<ObjectReader> c, Expression stream, Expression session, FieldInfo field)
        {
            var method = typeof(Int32Serializer).GetMethod(nameof(ReadValueImpl), BindingFlagsEx.Static)!;
            var byteArray = c.GetVariable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            return c.StaticCall(method, stream, byteArray);
        }
    }
}