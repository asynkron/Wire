using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Wire.Tests
{
    [TestClass]
    public class PrimitivesTest
    {
        [TestMethod]
        public void CanSerializeString()
        {
            var serializer = new Serializer();
            var stream = new MemoryStream();
            serializer.Serialize("hello", stream);
            stream.Position = 0;
            var res = serializer.Deserialize<string>(stream);
            Assert.AreEqual("hello", res);
        }
    }
}
