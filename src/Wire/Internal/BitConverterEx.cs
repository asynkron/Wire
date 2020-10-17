// -----------------------------------------------------------------------
//   <copyright file="NoAllocBitConverter.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wire.Internal
{
    /// <summary>
    ///     Provides methods not allocating the byte buffer but using <see cref="SerializerSession.GetBuffer" /> to lease a
    ///     buffer.
    /// </summary>
    internal static class BitConverterEx
    {
        internal static readonly UTF8Encoding Utf8 = (UTF8Encoding) Encoding.UTF8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteLengthEncodedString(string? str, IBufferWriter<byte> stream, out int byteCount)
        {
            //if first byte is 0 = null
            //if first byte is 254 or less, then length is value - 1
            //if first byte is 255 then the next 4 bytes are an int32 for length
            if (str == null)
            {
                var span = stream.GetSpan(1);
                span[0] = 0;
                stream.Advance(1);
                byteCount = 1;
                return;
            }
            

            byteCount = Utf8.GetByteCount(str);
            if (byteCount < 254) //short string
            {
                var span = stream.GetSpan(byteCount + 1);
                span[0] = (byte) (byteCount + 1);
                Utf8.GetBytes(str, span[1..]);
                byteCount+=1;
            }
            else //long string
            {
                var span = stream.GetSpan(byteCount + 1 + 4);
                
                //signal int count
                span[0] = 255;
                //int count
                BitConverter.TryWriteBytes(span[1..], byteCount);
                //write actual string content
                Utf8.GetBytes(str, span[(1+4)..]);
                byteCount += 1 + 4;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryWriteBytes(Span<byte> span, DateTime dateTime)
        {
            BitConverter.TryWriteBytes(span, dateTime.Ticks);
            BitConverter.TryWriteBytes(span[8..],  (byte) dateTime.Kind);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryWriteBytes(Span<byte> span, DateTimeOffset dateTimeOffset)
        {
            var minutes = (short) (dateTimeOffset.Offset.Ticks / TimeSpan.TicksPerMinute);
            
            BitConverter.TryWriteBytes(span, dateTimeOffset.Ticks);
            BitConverter.TryWriteBytes(span[8..], minutes);
            BitConverter.TryWriteBytes(span[10..],  (byte) dateTimeOffset.DateTime.Kind);
        }
    }
}