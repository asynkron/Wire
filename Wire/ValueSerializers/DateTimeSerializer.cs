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

        private static unsafe void WriteValueImpl(Stream stream, DateTime dateTime, SerializerSession session)
        {
            var bytes1 = session.GetBuffer(Size+1);
            fixed (byte* b = bytes1)
                *((long*) b) = dateTime.Ticks;
            bytes1[Size] = (byte) dateTime.Kind;

            stream.Write(bytes1, 0, Size+1);
        }

        public static DateTime ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var dateTime = ReadDateTime(stream, session);
            return dateTime;
        }

        private static DateTime ReadDateTime(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size + 1);
            stream.Read(buffer, 0, Size + 1);
            var ticks = BitConverter.ToInt64(buffer, 0);
            var kind = (DateTimeKind) buffer[Size]; //avoid reading a single byte from the stream
            var dateTime = new DateTime(ticks, kind);
            return dateTime;
        }
    }
}