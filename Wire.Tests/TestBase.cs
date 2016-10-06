using System.IO;
using Xunit;

namespace Wire.Tests
{
    public abstract class TestBase
    {
        private Serializer _serializer;
        private MemoryStream _stream;

        protected TestBase()
        {
            _serializer = new Serializer();
            _stream = new MemoryStream();
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
            _serializer.Serialize(o, _stream);
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