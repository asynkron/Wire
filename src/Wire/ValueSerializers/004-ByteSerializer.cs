// -----------------------------------------------------------------------
//   <copyright file="ByteSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : SessionIgnorantValueSerializer<byte>
    {
        public const byte Manifest = 4;
        public static readonly ByteSerializer Instance = new ByteSerializer();

        private ByteSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        private static void WriteValueImpl(IBufferWriter<byte> stream, byte b)
        {
            var span = stream.GetSpan(1);
            span[0] = b;
            stream.Advance(1);
        }

        private static byte ReadValueImpl(Stream stream)
        {
            return (byte) stream.ReadByte();
        }
    }
}