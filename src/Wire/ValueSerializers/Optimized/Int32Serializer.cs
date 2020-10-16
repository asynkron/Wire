// -----------------------------------------------------------------------
//   <copyright file="Int32Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class Int32Serializer : SessionAwareValueSerializer<int>
    {
        public const byte Manifest = 8;
        public const int Size = sizeof(int);
        public static readonly Int32Serializer Instance = new Int32Serializer();

        private Int32Serializer()
            : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(IBufferWriter<byte> stream, int value,int size)
        {
            var span = stream.GetSpan(size);   
            BitConverter.TryWriteBytes(span, value);
            stream.Advance(size);
        }

        public static void WriteValueImpl(IBufferWriter<byte> stream, int i)
        {
            WriteValueImpl(stream, i, Size);
        }

        public static int ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}