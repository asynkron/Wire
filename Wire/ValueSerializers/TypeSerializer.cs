using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public const byte Manifest = 16;
        public static readonly TypeSerializer Instance = new TypeSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                stream.WriteByte(NullSerializer.Manifest);
            }
            else
            {
                var type = (Type)value;
                int existingId;
                if (session.Serializer.Options.PreserveObjectReferences && session.TryGetObjectId(type, out existingId))
                {
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, null, session);
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                { 
                    //type was not written before, add it to the tacked object list
                    if (session.Serializer.Options.PreserveObjectReferences)
                    {
                        session.TrackSerializedObject(type);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AssignNullToNotNullAttribute
                    session.Serializer.Options.TypeResolver.WriteType(stream, type, session);
                }
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var first = stream.ReadByte();
            switch (first)
            {
                case NullSerializer.Manifest:
                    return null;

                case ObjectReferenceSerializer.Manifest:
                    return ObjectReferenceSerializer.Instance.ReadValue(stream, session);

                case ObjectSerializer.ManifestFull:
                case ObjectSerializer.ManifestIndex:
                    var type = session.Serializer.Options.TypeResolver.ReadType(stream, first, session);
                    if (session.Serializer.Options.PreserveObjectReferences)
                    {
                        session.TrackDeserializedObject(type);
                    }
                    return type;

                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }

        public override Type GetElementType()
        {
            return typeof (Type);
        }
    }
}
