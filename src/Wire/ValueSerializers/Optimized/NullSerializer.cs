// -----------------------------------------------------------------------
//   <copyright file="NullSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class NullSerializer : ValueSerializer
    {
        public const byte Manifest = 0;
        public static readonly NullSerializer Instance = new NullSerializer();

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = Manifest;
            stream.Advance(1);
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return null;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }
    }
}