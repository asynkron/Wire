﻿// -----------------------------------------------------------------------
//   <copyright file="SystemObjectSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;

namespace Wire.ValueSerializers
{
    public class SystemObjectSerializer : ValueSerializer
    {
        public const byte Manifest = 1;
        public static readonly SystemObjectSerializer Instance = new SystemObjectSerializer();

        public override void WriteManifest<TBufferWriter>(Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return new object();
        }

        public override Type GetElementType()
        {
            return typeof(object);
        }
    }
}