using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        private Serializer serializer;
        private MemoryStream stream;

        [TestInitialize]
        public void Setup()
        {
            serializer = new Serializer();
            stream = new MemoryStream();
        }

        public void Reset()
        {
            stream.Position = 0;
        }

        public void Serialize(object o)
        {
            serializer.Serialize(o, stream);
        }

        public T Deserialize<T>()
        {
            return serializer.Deserialize<T>(stream);
        }
    }
}