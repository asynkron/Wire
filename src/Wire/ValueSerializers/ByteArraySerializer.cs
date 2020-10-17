// -----------------------------------------------------------------------
//   <copyright file="ByteArraySerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public const byte Manifest = 9;
        public static readonly ByteArraySerializer Instance = new ByteArraySerializer();

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = Manifest;
            stream.Advance(1);
        }

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            var bytes = (byte[]) value;
            Int32Serializer.WriteValueImpl(stream,bytes.Length);
            var destination = stream.GetSpan(bytes.Length);
            bytes.CopyTo(destination);
            stream.Advance(bytes.Length);
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackSerializedObject(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var res = stream.ReadLengthEncodedByteArray(session);
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackDeserializedObject(res);
            return res;
        }

        public override Type GetElementType()
        {
            return typeof(byte[]);
        }
    }
}