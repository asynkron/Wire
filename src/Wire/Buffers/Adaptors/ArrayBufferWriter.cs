using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Wire.Buffers.Adaptors
{
    /// <summary>
    /// An implementation of <see cref="IBufferWriter{T}"/> which writes to an array.
    /// </summary>
    public struct ArrayBufferWriter : IBufferWriter<byte>
    {
        private readonly byte[] _buffer;
        private int _bytesWritten;

        internal ArrayBufferWriter(byte[] buffer)
        {
            _buffer = buffer;
            _bytesWritten = 0;
        }

        public byte[] Buffer => _buffer;

        public Memory<byte> Memory => _buffer.AsMemory(0, _bytesWritten);

        public int BytesWritten => _bytesWritten;

        public void Advance(int count)
        {
            if (_bytesWritten > _buffer.Length)
            {
                ThrowInvalidCount();
                [MethodImpl(MethodImplOptions.NoInlining)]
                static void ThrowInvalidCount() => throw new InvalidOperationException("Cannot advance past the end of the buffer");
            }

            _bytesWritten += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_bytesWritten + sizeHint > _buffer.Length)
            {
                ThrowInsufficientCapacity(sizeHint);
            }

            return _buffer.AsMemory(_bytesWritten);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (_bytesWritten + sizeHint > _buffer.Length)
            {
                ThrowInsufficientCapacity(sizeHint);
            }

            return _buffer.AsSpan(_bytesWritten);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowInsufficientCapacity(int sizeHint) => throw new InvalidOperationException($"Insufficient capacity to perform the requested operation. Buffer size is {_buffer.Length}. Current length is {_bytesWritten} and requested size increase is {sizeHint}");
    }
}
