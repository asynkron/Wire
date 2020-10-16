// -----------------------------------------------------------------------
//   <copyright file="DoubleSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DoubleSerializer : SessionAwareValueSerializer<double>
    {
        public const byte Manifest = 13;
        private const int Size = sizeof(double);
        public static readonly DoubleSerializer Instance = new DoubleSerializer();

        private DoubleSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, double value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        private static double ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}