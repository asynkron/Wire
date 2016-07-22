using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class DelegateTests
    {
        public class Dummy
        {
            public int Prop { get; set; }
        }

        [TestMethod]
        public void CanSerializeDelegate()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));

            Action<Dummy> a = dummy => dummy.Prop = 1;
            serializer.Serialize(a,stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Action<Dummy>>(stream);
            Assert.IsNotNull(res);

            var d = new Dummy {Prop = 0};
            res(d);
            Assert.AreEqual(1,d.Prop);
        }
    }
}
