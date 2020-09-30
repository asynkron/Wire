// -----------------------------------------------------------------------
//   <copyright file="GuidSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class GuidSerializer : SessionIgnorantValueSerializer<Guid>
    {
        public const byte Manifest = 11;
        public static readonly GuidSerializer Instance = new GuidSerializer();

        private GuidSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        private static void WriteValueImpl(Stream stream, Guid g)
        {
            var bytes = g.ToByteArray();
            stream.Write(bytes);
        }

        private static Guid ReadValueImpl(Stream stream)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            return new Guid(buffer);
        }
    }
}