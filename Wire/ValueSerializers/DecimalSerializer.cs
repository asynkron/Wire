using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DecimalSerializer : ValueSerializer
    {
        public static readonly DecimalSerializer Instance = new DecimalSerializer();
        private readonly byte[] _manifest = { 14 };

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var data = decimal.GetBits((decimal) value);
            Int32Serializer.Instance.WriteValue(stream, (object)data[0], session);
            Int32Serializer.Instance.WriteValue(stream, (object)data[1], session);
            Int32Serializer.Instance.WriteValue(stream, (object)data[2], session);
            Int32Serializer.Instance.WriteValue(stream, (object)data[3], session);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var parts = new[]
                {
                    (int)Int32Serializer.Instance.ReadValue(stream, session),
                    (int)Int32Serializer.Instance.ReadValue(stream, session),
                    (int)Int32Serializer.Instance.ReadValue(stream, session),
                    (int)Int32Serializer.Instance.ReadValue(stream, session),
                };
            bool sign = (parts[3] & 0x80000000) != 0;

            byte scale = (byte)((parts[3] >> 16) & 0x7F);
            decimal newValue = new decimal(parts[0], parts[1], parts[2], sign, scale);
            return newValue;
        }

        public override Type GetElementType()
        {
            return typeof(decimal);
        }
    }
}