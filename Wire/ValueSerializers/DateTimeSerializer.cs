using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class DateTimeSerializer : SessionAwareValueSerializer<DateTime>
    {
        public const byte Manifest = 5;
        public const int Size = sizeof(long) + sizeof(byte);
        public static readonly DateTimeSerializer Instance = new DateTimeSerializer();

        public DateTimeSerializer() : base(Manifest, () => WriteValueImpl, ()=> ReadValueImpl)
        {
        }

        private static unsafe void WriteValueImpl(Stream stream, DateTime dateTime, SerializerSession session)
        {
            //datetime size is 9 ticks + kind
            var bytes1 = session.GetBuffer(Size);
            fixed (byte* b = bytes1)
                *((long*) b) = dateTime.Ticks;
            bytes1[Size-1] = (byte) dateTime.Kind;

            stream.Write(bytes1, 0, Size);
        }

        public static DateTime ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var dateTime = ReadDateTime(stream, session);
            return dateTime;
        }

        private static DateTime ReadDateTime(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size );
            var ticks = BitConverter.ToInt64(buffer, 0);
            var kind = (DateTimeKind) buffer[Size-1]; //avoid reading a single byte from the stream
            var dateTime = new DateTime(ticks, kind);
            return dateTime;
        }
    }
}