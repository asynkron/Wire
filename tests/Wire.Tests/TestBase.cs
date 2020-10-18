// -----------------------------------------------------------------------
//   <copyright file="TestBase.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Buffers;
using System.IO;
using Wire.Buffers;
using Wire.Buffers.Adaptors;
using Xunit;

namespace Wire.Tests
{
    public abstract class TestBase
    {
        private readonly MemoryStream _stream;
        private Serializer _serializer;
        private readonly IBufferWriter<byte> _buffer;

        protected TestBase()
        {
            _serializer = new Serializer();
            _stream = new MemoryStream();
            _buffer = new PoolingStreamBufferWriter(_stream,256);
        }

        protected void CustomInit(Serializer serializer)
        {
            _serializer = serializer;
        }

        public void Reset()
        {
            _stream.Position = 0;
        }

        public void Serialize(object o)
        {
            _serializer.Serialize(o, _buffer);
        }

        public T Deserialize<T>()
        {
            return _serializer.Deserialize<T>(_stream);
        }

        public void AssertMemoryStreamConsumed()
        {
            Assert.Equal(_stream.Length, _stream.Position);
        }
    }
}