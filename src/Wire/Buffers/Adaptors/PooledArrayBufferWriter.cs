using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Wire.Buffers.Adaptors
{
    /// <summary>
    /// A <see cref="IBufferWriter{T}"/> implementation implemented using pooled arrays.
    /// </summary>
    public struct PooledArrayBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] _buffer;
        private int _bytesWritten;
        private const int MinRequestSize = 256;

        internal PooledArrayBufferWriter(int sizeHint)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(sizeHint, MinRequestSize));
            _bytesWritten = 0;
        }

        public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _bytesWritten);

        /// <inheritdoc />
        public void Advance(int count)
        {
            _bytesWritten += count;
            if (_bytesWritten > _buffer.Length)
            {
                ThrowInvalidCount();
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void ThrowInvalidCount() => throw new InvalidOperationException("Cannot advance past the end of the buffer");
            }
        }

        /// <inheritdoc />
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_bytesWritten + sizeHint > _buffer.Length)
            {
                Resize(sizeHint);
            }

            return _buffer.AsMemory(_bytesWritten);
        }

        /// <inheritdoc />
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (_bytesWritten + sizeHint > _buffer.Length)
            {
                Resize(sizeHint);
            }

            return _buffer.AsSpan(_bytesWritten);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_buffer is object)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }

        private void Resize(int sizeHint)
        {
            if (sizeHint < MinRequestSize)
            {
                sizeHint = MinRequestSize;
            }

            var newBuffer = ArrayPool<byte>.Shared.Rent(_bytesWritten + sizeHint);
            _buffer.CopyTo(newBuffer, 0);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }
    }
}
