// -----------------------------------------------------------------------
//   <copyright file="ByteArraySerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public const byte Manifest = 9;
        public static readonly ByteArraySerializer Instance = new ByteArraySerializer();

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            var bytes = (byte[]) value;
            Int32Serializer.WriteValue(writer, bytes.Length);
            writer.EnsureContiguous(bytes.Length);
            var destination = writer.WritableSpan;
            bytes.CopyTo(destination);
            writer.AdvanceSpan(bytes.Length);
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