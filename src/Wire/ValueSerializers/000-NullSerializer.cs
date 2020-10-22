// -----------------------------------------------------------------------
//   <copyright file="NullSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Buffers;

namespace Wire.ValueSerializers
{
    public class NullSerializer : ValueSerializer
    {
        public const byte Manifest = 0;
        public static readonly NullSerializer Instance = new NullSerializer();

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public static void WriteManifestImpl<TBufferWriter>(Writer<TBufferWriter> writer)
            where TBufferWriter : IBufferWriter<byte>
        {
            writer.Write(Manifest);
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
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