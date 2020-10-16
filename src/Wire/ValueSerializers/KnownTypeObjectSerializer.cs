// -----------------------------------------------------------------------
//   <copyright file="KnownTypeObjectSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class KnownTypeObjectSerializer : ValueSerializer
    {
        private readonly ObjectSerializer _serializer;
        private readonly byte[] _typeIdentifierBytes;

        public KnownTypeObjectSerializer(ObjectSerializer serializer, ushort typeIdentifier)
        {
            _serializer = serializer;
            _typeIdentifierBytes = BitConverter.GetBytes(typeIdentifier);
        }

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            stream.WriteByte(ObjectSerializer.ManifestIndex);
            stream.Write(_typeIdentifierBytes);
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            _serializer.WriteValue(stream, value, session);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _serializer.ReadValue(stream, session);
        }

        public override Type GetElementType()
        {
            return _serializer.GetElementType();
        }
    }
}