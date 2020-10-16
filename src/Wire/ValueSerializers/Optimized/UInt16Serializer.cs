// -----------------------------------------------------------------------
//   <copyright file="UInt16Serializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Internal;

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

        private static void WriteValueImpl(Stream stream, ushort u, byte[] bytes)
        {
            BitConverter.TryWriteBytes(bytes, u);
            stream.Write(bytes, 0, Size);
        }

        public static void WriteValueImpl(Stream stream, ushort u, SerializerSession session)
        {
            WriteValueImpl(stream, u, session.GetBuffer(Size));
        }

        private static ushort ReadValueImpl(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}