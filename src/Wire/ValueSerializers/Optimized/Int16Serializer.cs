// -----------------------------------------------------------------------
//   <copyright file="Int16Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class Int16Serializer : SessionAwareValueSerializer<short>
    {
        public const byte Manifest = 3;
        private const int Size = sizeof(short);
        public static readonly Int16Serializer Instance = new Int16Serializer();

        private Int16Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, short value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        private static short ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToInt16(bytes, 0);
        }
    }
}