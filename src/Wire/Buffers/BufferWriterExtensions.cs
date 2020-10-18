using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Wire.Buffers
{
    public static class BufferWriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Writer<TBufferWriter> CreateWriter<TBufferWriter>(this TBufferWriter buffer, SerializerSession session) where TBufferWriter : IBufferWriter<byte>
        {
            if (session is null)
            {
                ThrowSessionNull();
            }

            return Writer.Create(buffer, session);

            void ThrowSessionNull() => throw new ArgumentNullException(nameof(session));
        }
    }
}