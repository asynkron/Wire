// //-----------------------------------------------------------------------
// // <copyright file="TypeSerializer.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public const byte Manifest = 16;
        public static readonly TypeSerializer Instance = new TypeSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            ushort typeIdentifier;
            if (session.ShouldWriteTypeManifest(TypeEx.RuntimeType, out typeIdentifier))
            {
                stream.WriteByte(Manifest);
            }
            else
            {
                stream.Write(new[] {ObjectSerializer.ManifestIndex});
                UInt16Serializer.WriteValueImpl(stream, typeIdentifier, session);
            }
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                StringSerializer.WriteValueImpl(stream, null, session);
            }
            else
            {
                var type = (Type) value;
                int existingId;
                if (session.Serializer.Options.PreserveObjectReferences && session.TryGetObjectId(type, out existingId))
                {
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, session);
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                {
                    if (session.Serializer.Options.PreserveObjectReferences)
                    {
                        session.TrackSerializedObject(type);
                    }
                    //type was not written before, add it to the tacked object list
                    var name = type.GetShortAssemblyQualifiedName();
                    StringSerializer.WriteValueImpl(stream, name, session);
                }
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var shortname = stream.ReadString(session);
            if (shortname == null)
                return null;

            var name = TypeEx.ToQualifiedAssemblyName(shortname);
            var type = Type.GetType(name, true);

            //add the deserialized type to lookup
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(type);
            }
            return type;
        }

        public override Type GetElementType()
        {
            return typeof(Type);
        }
    }
}