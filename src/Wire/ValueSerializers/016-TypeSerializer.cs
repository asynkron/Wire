// -----------------------------------------------------------------------
//   <copyright file="TypeSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public const byte Manifest = 16;
        public static readonly TypeSerializer Instance = new TypeSerializer();

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(TypeEx.RuntimeType, out var typeIdentifier))
            {
                writer.Write(Manifest);
            }
            else
            {
                writer.Write(ObjectSerializer.ManifestIndex);
                writer.Write(typeIdentifier);
            }
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            if (value == null)
            {
                StringSerializer.WriteValueImpl(ref writer, null);
            }
            else
            {
                var type = (Type) value;
                if (session.Serializer.Options.PreserveObjectReferences &&
                    session.TryGetObjectId(type, out var existingId))
                {
                    ObjectReferenceSerializer.WriteManifestImpl(ref writer);
                    ObjectReferenceSerializer.WriteValueImpl(ref writer, existingId);
                }
                else
                {
                    if (session.Serializer.Options.PreserveObjectReferences) session.TrackSerializedObject(type);
                    //type was not written before, add it to the tacked object list
                    var name = type.GetShortAssemblyQualifiedName();
                    StringSerializer.WriteValueImpl(ref writer, name);
                }
            }
        }

        public override object? ReadValue(Stream stream, DeserializerSession session)
        {
            var shortname = stream.ReadString(session);
            if (shortname == null) return null;

            var name = TypeEx.ToQualifiedAssemblyName(shortname);
            var type = Type.GetType(name, true);

            //add the deserialized type to lookup
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackDeserializedObject(type);
            return type;
        }

        public override Type GetElementType()
        {
            return typeof(Type);
        }
    }
}