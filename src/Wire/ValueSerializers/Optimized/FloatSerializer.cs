// -----------------------------------------------------------------------
//   <copyright file="FloatSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class FloatSerializer : SessionAwareValueSerializer<float>
    {
        public const byte Manifest = 12;
        private const int Size = sizeof(float);
        public static readonly FloatSerializer Instance = new FloatSerializer();

        private FloatSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, float value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        private static float ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}