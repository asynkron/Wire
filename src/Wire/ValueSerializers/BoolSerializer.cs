// -----------------------------------------------------------------------
//   <copyright file="BoolSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.IO;

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

        private static void WriteValueImpl(Stream stream, bool b)
        {
            stream.WriteByte((byte) (b ? 1 : 0));
        }
    }
}