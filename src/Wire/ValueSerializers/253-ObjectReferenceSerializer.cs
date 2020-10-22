// -----------------------------------------------------------------------
//   <copyright file="ObjectReferenceSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class ObjectReferenceSerializer : ValueSerializer
    {
        public const byte Manifest = 253;
        public static readonly ObjectReferenceSerializer Instance = new ObjectReferenceSerializer();

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public static void WriteManifestImpl<TBufferWriter>(Writer<TBufferWriter> writer)
            where TBufferWriter : IBufferWriter<byte>
        {
            writer.Write(Manifest);
        }

        public static void WriteValueImpl<TBufferWriter>(Writer<TBufferWriter> writer, int value)
            where TBufferWriter : IBufferWriter<byte>
        {
            writer.Write(value);
        }

        public static object ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var id = stream.ReadInt32(session);
            var obj = session.GetDeserializedObject(id);
            return obj;
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            WriteValueImpl(writer, (int) value);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var id = stream.ReadInt32(session);
            var obj = session.GetDeserializedObject(id);
            return obj;
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}