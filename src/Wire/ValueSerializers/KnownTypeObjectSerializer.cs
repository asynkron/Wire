// -----------------------------------------------------------------------
//   <copyright file="KnownTypeObjectSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using System.Linq;
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
            _typeIdentifierBytes = new[] {ObjectSerializer.ManifestIndex}.Concat(BitConverter.GetBytes(typeIdentifier)).ToArray();
        }

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var size = _typeIdentifierBytes.Length;
            var span = stream.GetSpan(size);
            _typeIdentifierBytes.CopyTo(span);
            stream.Advance(size);
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