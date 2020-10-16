// -----------------------------------------------------------------------
//   <copyright file="UInt32Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt32Serializer : SessionAwareValueSerializer<uint>
    {
        public const byte Manifest = 18;
        private const int Size = sizeof(uint);
        public static readonly UInt32Serializer Instance = new UInt32Serializer();

        private UInt32Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, uint value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        private static uint ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}