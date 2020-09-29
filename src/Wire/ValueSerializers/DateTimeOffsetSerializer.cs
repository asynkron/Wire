// -----------------------------------------------------------------------
//   <copyright file="DateTimeOffsetSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

namespace Wire.ValueSerializers
{
    using System;
    using System.IO;
    using Internal;

    public class DateTimeOffsetSerializer : SessionAwareByteArrayRequiringValueSerializer<DateTimeOffset>
    {
        public const byte Manifest = 10;
        public const int Size = sizeof(long) + sizeof(byte) + sizeof(short);
        public static readonly DateTimeOffsetSerializer Instance = new DateTimeOffsetSerializer();

        public DateTimeOffsetSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(Stream stream, DateTimeOffset dateTimeOffset, byte[] bytes)
        {
            NoAllocBitConverter.GetBytes(dateTimeOffset, bytes);
            stream.Write(bytes, 0, Size);
        }

        public static DateTimeOffset ReadValueImpl(Stream stream, byte[] bytes)
        {
            var dateTimeOffset = ReadDateTimeOffset(stream, bytes);
            return dateTimeOffset;
        }

        private static DateTimeOffset ReadDateTimeOffset(Stream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, Size);
            var ticks = BitConverter.ToInt64(bytes, 0);
            var offsetMinutes = BitConverter.ToInt16(bytes, 8);

            var kind = (DateTimeKind) bytes[Size - 1]; //avoid reading a single byte from the stream
            var dateTime = new DateTime(ticks, kind);

            stream.Read(bytes, 0, Size);
            var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.FromMinutes(offsetMinutes));
            return dateTimeOffset;
        }
    }
}