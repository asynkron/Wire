using System;
using System.Buffers;
using System.IO;

namespace Wire.Buffers
{
    /// <summary>
    /// An implementation of <see cref="IBufferWriter{T}"/> which writes to a <see cref="MemoryStream"/>.
    /// </summary>
    public struct MemoryStreamBufferWriter : IBufferWriter<byte>
    {
        private readonly MemoryStream _stream;
        private const int MinRequestSize = 256;
        private byte[] _bytes;
        private int _bytesWritten;

        public MemoryStreamBufferWriter(MemoryStream stream)
        {
            _bytesWritten = 0;
            _stream = stream;
            _bytes = _stream.GetBuffer();
        }

        /// <inheritdoc />
        public void Advance(int count)
        {
            _bytesWritten += count;
            _stream.Position += count;
        }

        /// <inheritdoc />
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (sizeHint < MinRequestSize)
            {
                sizeHint = MinRequestSize;
            }

            if (_bytes.Length - _bytesWritten < sizeHint)
            {
                _stream.Capacity += sizeHint;
                _stream.SetLength(_stream.Capacity);
                _bytes = _stream.GetBuffer();
            }

            return _bytes.AsSpan()[_bytesWritten..];
        }
    }
}