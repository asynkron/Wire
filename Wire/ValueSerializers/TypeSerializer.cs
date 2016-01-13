using System;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public const byte Manifest = 16;
        public static readonly TypeSerializer Instance = new TypeSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(type))
            {
                stream.WriteByte(Manifest);
            }
            else
            {
                var typeIdentifier = session.GetTypeIdentifier(type);
                stream.Write(new[] { ObjectSerializer.ManifestIndex });
                stream.WriteUInt16((ushort) typeIdentifier);
            }
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                stream.WriteInt32(-1);
            }
            else
            {
                var type = (Type) value;
                int existingId;
                if (session.Serializer.Options.PreserveObjectReferences && session.TryGetObjectId(type, out existingId))
                {
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, null, session);
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                { 
                    //type was not written before, add it to the tacked object list
                    var name = type.GetShortAssemblyQualifiedName();
                    if (session.Serializer.Options.PreserveObjectReferences)
                    {
                        session.TrackSerializedObject(type);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var bytes = Encoding.UTF8.GetBytes(name);
                    stream.WriteLengthEncodedByteArray(bytes);
                }
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var length = (int) Int32Serializer.Instance.ReadValue(stream, session);
            if (length == -1)
                return null;

            var buffer = session.GetBuffer(length);
            stream.Read(buffer, 0, length);
            var shortname = Encoding.UTF8.GetString(buffer, 0, length);
            var name = Utils.ToQualifiedAssemblyName(shortname);
            var type = Type.GetType(name);
            //add the deserialized type to lookup
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(type);
            }
            return type;
        }

        public override Type GetElementType()
        {
            return typeof (Type);
        }
    }
}