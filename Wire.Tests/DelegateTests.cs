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

        public class Dummy2
        {
            public Func<int> Del;
            public void Create()
            {
                var a = 3;
                Del = () => a+1;
            }
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

        [TestMethod]
        public void CanSerializeObjectWithClosure()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));

            var dummy = new Dummy2();
            dummy.Create();

            serializer.Serialize(dummy, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Dummy2>(stream);
            Assert.IsNotNull(res);
            var actual = res.Del();
            Assert.AreEqual(4,actual);
        }
    }
}
