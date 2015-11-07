using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DateTimeSerializer : ValueSerializer
    {
        public static readonly DateTimeSerializer Instance = new DateTimeSerializer();
        public const byte Manifest = 5;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes(((DateTime) value).Ticks);
            stream.Write(bytes);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var size = sizeof (long);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            var ticks = BitConverter.ToInt64(buffer, 0);
            return new DateTime(ticks);
        }

        public override Type GetElementType()
        {
            return typeof (DateTime);
        }
    }
}