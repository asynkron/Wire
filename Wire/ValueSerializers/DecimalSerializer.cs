using System;
using System.IO;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class DecimalSerializer : ValueSerializer
    {
        public const byte Manifest = 14;
        public static readonly DecimalSerializer Instance = new DecimalSerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var data = decimal.GetBits((decimal) value);
            Int32Serializer.WriteValueImpl(stream, data[0], session);
            Int32Serializer.WriteValueImpl(stream, data[1], session);
            Int32Serializer.WriteValueImpl(stream, data[2], session);
            Int32Serializer.WriteValueImpl(stream, data[3], session);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var parts = new[]
            {
                Int32Serializer.ReadValueImpl(stream,session),
                Int32Serializer.ReadValueImpl(stream,session),
                Int32Serializer.ReadValueImpl(stream,session),
                Int32Serializer.ReadValueImpl(stream,session)
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