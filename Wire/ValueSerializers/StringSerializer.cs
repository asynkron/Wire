using System;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class StringSerializer : ValueSerializer
    {
        public const byte Manifest = 7;
        public static readonly StringSerializer Instance = new StringSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
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
                var bytes = Encoding.UTF8.GetBytes((string) value);
                if (bytes.Length < 254)
                {
                    stream.WriteByte((byte)(bytes.Length+1));
                    stream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    stream.WriteByte(255);
                    stream.WriteInt32(bytes.Length);
                    stream.Write(bytes,0,bytes.Length);
                }
                
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var length = stream.ReadByte();
            if (length == 0)
                return null;

            if (length == 255)
            {
                length = stream.ReadInt32(session);
            }
            else
            {
                length--;
            }

            var buffer = session.GetBuffer(length);

            stream.Read(buffer, 0, length);
            var res = Encoding.UTF8.GetString(buffer, 0, length);
            return res;
        }

        public override Type GetElementType()
        {
            return typeof (string);
        }
    }
}