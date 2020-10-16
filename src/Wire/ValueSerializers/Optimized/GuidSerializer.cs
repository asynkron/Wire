// -----------------------------------------------------------------------
//   <copyright file="GuidSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;

namespace Wire.ValueSerializers
{
    public class GuidSerializer : SessionAwareValueSerializer<Guid>
    {
        public const byte Manifest = 11;
        public static readonly GuidSerializer Instance = new GuidSerializer();

        private GuidSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => 16;

        private static void WriteValueImpl(IBufferWriter<byte> stream, Guid g, int size)
        {
            var span = stream.GetSpan(size);
            g.TryWriteBytes(span);
            stream.Advance(size);
        }

        private static Guid ReadValueImpl(Stream stream,byte[] bytes)
        {
            stream.Read(bytes, 0, 16);
            var span = bytes.AsSpan(0, 16);
            return new Guid(span);
        }
    }
}