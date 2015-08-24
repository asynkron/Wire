using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DecimalSerializer : ValueSerializer
    {
        public static readonly DecimalSerializer Instance = new DecimalSerializer();
        public const byte Manifest = 14;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var data = decimal.GetBits((decimal) value);
            stream.WriteInt32(data[0]);
            stream.WriteInt32(data[1]);
            stream.WriteInt32(data[2]);
            stream.WriteInt32(data[3]);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var parts = new[]
            {
                (int) Int32Serializer.Instance.ReadValue(stream, session),
                (int) Int32Serializer.Instance.ReadValue(stream, session),
                (int) Int32Serializer.Instance.ReadValue(stream, session),
                (int) Int32Serializer.Instance.ReadValue(stream, session)
            };
            var sign = (parts[3] & 0x80000000) != 0;

            var scale = (byte) ((parts[3] >> 16) & 0x7F);
            var newValue = new decimal(parts[0], parts[1], parts[2], sign, scale);
            return newValue;
        }

        public override Type GetElementType()
        {
            return typeof (decimal);
        }
    }
}