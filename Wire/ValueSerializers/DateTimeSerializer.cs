using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DateTimeSerializer : SessionAwareValueSerializer<DateTime>
    {
        public const byte Manifest = 5;
        public const int Size = sizeof(long);
        public static readonly DateTimeSerializer Instance = new DateTimeSerializer();

        public DateTimeSerializer() : base(Manifest, () => WriteValueImpl, ()=> ReadValueImpl)
        {
        }

        static void WriteValueImpl(Stream stream, DateTime dateTime, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(dateTime.Ticks, session);
            stream.Write(bytes, 0, Size);
            var kindByte = (byte) dateTime.Kind;
            stream.WriteByte(kindByte);
        }

        public static DateTime ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            var ticks = BitConverter.ToInt64(buffer, 0);
            var kind = (DateTimeKind) stream.ReadByte();
            var dateTime = new DateTime(ticks, kind);
            return dateTime;
        }
    }
}