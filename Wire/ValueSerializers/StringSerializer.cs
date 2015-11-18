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
            if (value == null)
            {
                stream.WriteInt32(-1);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes((string) value);
                stream.WriteLengthEncodedByteArray(bytes);
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var length = (int) Int32Serializer.Instance.ReadValue(stream, session);
            if (length == -1)
                return null;

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