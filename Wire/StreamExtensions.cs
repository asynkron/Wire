using System;
using System.IO;
using Wire.ValueSerializers;

namespace Wire
{
    public static class StreamExtensions
    {
        public static int ReadUInt16(this Stream self, DeserializerSession session)
        {
            var buffer = session.GetBuffer(2);
            self.Read(buffer, 0, 2);
            var res = BitConverter.ToUInt16(buffer, 0);
            return res;
        }

        public static void WriteUInt16(this Stream self, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            self.Write(bytes, 0, bytes.Length);
        }

        public static void WriteInt32(this Stream self, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            self.Write(bytes, 0, bytes.Length);
        }

        public static int ReadInt32(this Stream self, DeserializerSession session)
        {
            var buffer = session.GetBuffer(4);
            self.Read(buffer, 0, 4);
            var res = BitConverter.ToInt32(buffer, 0);
            return res;
        }

        public static byte[] ReadLengthEncodedByteArray(this Stream self, DeserializerSession session)
        {
            var length = self.ReadInt32(session);
            var buffer = session.GetBuffer(length);
            self.Read(buffer, 0, length);
            return buffer;
        }

        public static void WriteLengthEncodedByteArray(this Stream self, byte[] bytes)
        {
            self.WriteInt32(bytes.Length);
            self.Write(bytes, 0, bytes.Length);
        }

        public static void Write(this Stream self, byte[] bytes)
        {
            self.Write(bytes, 0, bytes.Length);
        }

        public static void WriteObject(this Stream stream, object value, Type valueType, ValueSerializer valueSerializer,
            bool preserveObjectReferences, SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, null, session);
            }
            else
            {
                int existingId;
                if (preserveObjectReferences && session.TryGetObjectId(value, out existingId))
                {
                    //write the serializer manifest
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, null, session);
                    //write the object reference id
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                {
                    var vType = value.GetType();
                    var s2 = valueSerializer;
                    if (vType != valueType)
                    {
                        //value is of subtype, lookup the serializer for that type
                        s2 = session.Serializer.GetSerializerByType(vType);
                    }
                    //lookup serializer for subtype
                    s2.WriteManifest(stream, vType, session);
                    s2.WriteValue(stream, value, session);
                }
            }
        }

        public static object ReadObject(this Stream stream, DeserializerSession session)
        {
            var s = session.Serializer.GetDeserializerByManifest(stream, session);
            var value = s.ReadValue(stream, session); //read the element value
            return value;
        }
    }
}