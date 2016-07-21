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
            var buffer = new byte[length];
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

        public static void WriteObjectWithManifest(this Stream stream, object value, SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, null, session);
            }
            else
            {
                int existingId;
                if (session.Serializer.Options.PreserveObjectReferences && session.TryGetObjectId(value, out existingId))
                {
                    //write the serializer manifest
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, null, session);
                    //write the object reference id
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                {
                    var vType = value.GetType();
                    var s2  = session.Serializer.GetSerializerByType(vType);
                    s2.WriteManifest(stream, vType, session);
                    s2.WriteValue(stream, value, session);
                }
            }
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

        public static void WriteString(this Stream stream, object value)
        {
            //null = 0
            // [0]
            //length < 255 gives length + 1 as a single byte + payload
            // [B length+1] [Payload]
            //others gives 254 + int32 length + payload
            // [B 254] [I Length] [Payload]
            if (value == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                var bytes = Utils.StringToBytes((string) value);
                if (bytes.Length < 254)
                {
                    stream.WriteByte((byte) (bytes.Length + 1));
                    stream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    stream.WriteByte(255);
                    stream.WriteInt32(bytes.Length);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public static object ReadString(this Stream stream, DeserializerSession session)
        {
            var length = stream.ReadByte();
            switch (length)
            {
                case 0:
                    return null;
                case 255:
                    length = stream.ReadInt32(session);
                    break;
                default:
                    length--;
                    break;
            }

            var buffer = session.GetBuffer(length);

            stream.Read(buffer, 0, length);
            var res = Utils.BytesToString(buffer, 0, length);
            return res;
        }
    }
}