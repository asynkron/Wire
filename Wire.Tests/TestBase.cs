using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        private Serializer _serializer;
        private MemoryStream _stream;

        [TestInitialize]
        public void Setup()
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
            Assert.AreEqual(_stream.Length, _stream.Position);
        }
    }
}