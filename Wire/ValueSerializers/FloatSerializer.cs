// //-----------------------------------------------------------------------
// // <copyright file="FloatSerializer.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class FloatSerializer : SessionAwareByteArrayRequiringValueSerializer<float>
    {
        public const byte Manifest = 12;
        public const int Size = sizeof(float);
        public static readonly FloatSerializer Instance = new FloatSerializer();

        public FloatSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        public static void WriteValueImpl(Stream stream, float f, byte[] bytes)
        {
            NoAllocBitConverter.GetBytes(f, bytes);
            stream.Write(bytes, 0, Size);
        }

        public static float ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}