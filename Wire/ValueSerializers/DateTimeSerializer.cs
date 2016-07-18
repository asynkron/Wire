using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DateTimeSerializer : ValueSerializer
    {
        public const byte Manifest = 5;
        public static readonly DateTimeSerializer Instance = new DateTimeSerializer();

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var dateTime = (DateTime)value;
            var bytes = BitConverter.GetBytes(dateTime.Ticks);
            stream.Write(bytes);
            var kindByte = (byte)dateTime.Kind;
            stream.WriteByte(kindByte);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            const int size = sizeof(long);
            var buffer = session.GetBuffer(size);
            stream.Read(buffer, 0, size);
            var ticks = BitConverter.ToInt64(buffer, 0);
            var kind = (DateTimeKind)stream.ReadByte();
            var dateTime = new DateTime(ticks, kind);
            return dateTime;
        }

        public override Type GetElementType()
        {
            return typeof (DateTime);
        }
    }
}