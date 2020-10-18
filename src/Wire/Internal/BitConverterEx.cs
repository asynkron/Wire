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