using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ObjectReferenceSerializer : ValueSerializer
    {
        public const byte Manifest = 253;
        public static readonly ObjectReferenceSerializer Instance = new ObjectReferenceSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            stream.WriteInt32((int) value);
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