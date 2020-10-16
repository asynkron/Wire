// -----------------------------------------------------------------------
//   <copyright file="UInt64Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class UInt64Serializer : SessionAwareValueSerializer<ulong>
    {
        public const byte Manifest = 19;
        private const int Size = sizeof(ulong);
        public static readonly UInt64Serializer Instance = new UInt64Serializer();

        private UInt64Serializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(Stream stream, ulong ul, byte[] bytes)
        {
            BitConverter.TryWriteBytes(bytes, ul);
            stream.Write(bytes, 0, Size);
        }

        private static ulong ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}