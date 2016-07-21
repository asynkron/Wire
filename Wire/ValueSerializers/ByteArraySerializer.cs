using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ByteArraySerializer : ValueSerializer
    {
        public const byte Manifest = 9;
        public static readonly ByteArraySerializer Instance = new ByteArraySerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = (byte[]) value;
            stream.WriteLengthEncodedByteArray(bytes);

            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackSerializedObject(bytes);
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var res = stream.ReadLengthEncodedByteArray(session);
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(res);
            }
            return res;

        }

        public override Type GetElementType()
        {
            return typeof (byte[]);
        }
    }
}