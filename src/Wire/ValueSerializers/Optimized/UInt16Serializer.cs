// -----------------------------------------------------------------------
//   <copyright file="UInt16Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;

namespace Wire.ValueSerializers
{
    public class UInt16Serializer : SessionAwareValueSerializer<ushort>
    {
        public const byte Manifest = 17;
        private const int Size = sizeof(ushort);
        public static readonly UInt16Serializer Instance = new UInt16Serializer();

        private UInt16Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, ushort value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        public static void WriteValueImpl(IBufferWriter<byte> stream, ushort value)
        {
            WriteValueImpl(stream, value, Size);
        }

        private static ushort ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}