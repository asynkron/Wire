// -----------------------------------------------------------------------
//   <copyright file="DateTimeOffsetSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    public class DateTimeOffsetSerializer : SessionAwareValueSerializer<DateTimeOffset>
    {
        public const byte Manifest = 10;
        public const int Size = sizeof(long) + sizeof(byte) + sizeof(short);
        public static readonly DateTimeOffsetSerializer Instance = new DateTimeOffsetSerializer();

        private DateTimeOffsetSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public override int PreallocatedByteBufferSize => Size;

        private static void WriteValueImpl(Stream stream, DateTimeOffset dateTimeOffset, byte[] bytes)
        {
            BitConverterEx.TryWriteBytes(bytes, dateTimeOffset);
            stream.Write(bytes, 0, Size);
        }

        private static DateTimeOffset ReadValueImpl(Stream stream, byte[] bytes)
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
            var dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.FromMinutes(offsetMinutes));
            return dateTimeOffset;
        }
    }
}