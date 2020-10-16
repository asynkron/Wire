// -----------------------------------------------------------------------
//   <copyright file="ByteSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteSerializer : SessionIgnorantValueSerializer<byte>
    {
        public const byte Manifest = 4;
        public static readonly ByteSerializer Instance = new ByteSerializer();

        private ByteSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        private static void WriteValueImpl(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }

        private static byte ReadValueImpl(Stream stream)
        {
            return (byte) stream.ReadByte();
        }
    }
}