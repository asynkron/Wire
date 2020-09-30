// -----------------------------------------------------------------------
//   <copyright file="SByteSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Wire.ValueSerializers
{
    public class SByteSerializer : SessionIgnorantValueSerializer<sbyte>
    {
        public const byte Manifest = 20;
        public static readonly SByteSerializer Instance = new SByteSerializer();

        private SByteSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        private static unsafe void WriteValueImpl(Stream stream, sbyte @sbyte)
        {
            stream.WriteByte(*(byte*) &@sbyte);
        }

        private static unsafe sbyte ReadValueImpl(Stream stream)
        {
            var @byte = (byte) stream.ReadByte();
            return *(sbyte*) &@byte;
        }
    }
}