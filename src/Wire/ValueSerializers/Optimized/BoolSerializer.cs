// -----------------------------------------------------------------------
//   <copyright file="BoolSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class BoolSerializer : SessionIgnorantValueSerializer<bool>
    {
        public const byte Manifest = 6;
        public static readonly BoolSerializer Instance = new BoolSerializer();

        private BoolSerializer() :
            base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        private static bool ReadValueImpl(Stream stream)
        {
            var b = stream.ReadByte();
            return b != 0;
        }

        private static void WriteValueImpl(IBufferWriter<byte> stream, bool b)
        {
            var span = stream.GetSpan(1);
            span[0] =b ? 1 : 0;
            stream.Advance(1);
        }
    }
}