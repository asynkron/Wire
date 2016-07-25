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

        public class HasClosure
        {
            public Func<int> Del;
            public void Create()
            {
                var a = 3;
                Del = () => a+1;
            }
        }

        [TestMethod]
        public void CanSerializeMemberMethod()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Func<string> a = 123.ToString;
            serializer.Serialize(a, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Func<string>>(stream);
            Assert.IsNotNull(res);
            var actual = res();
            Assert.AreEqual("123", actual);
        }

        [TestMethod]
        public void CanSerializeDelegate()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Action<Dummy> a = dummy => dummy.Prop = 1;
            serializer.Serialize(a,stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Action<Dummy>>(stream);
            Assert.IsNotNull(res);

            var d = new Dummy {Prop = 0};
            res(d);
            Assert.AreEqual(1,d.Prop);
        }

        private static int StaticFunc(int a)
        {
            return a + 1;
        }

        [TestMethod]
        public void CanSerializeStaticDelegate()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Func<int, int> fun = StaticFunc;

            serializer.Serialize(fun, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Func<int, int>>(stream);
            Assert.IsNotNull(res);
            var actual = res(4);

            Assert.AreEqual(5, actual);
        }

        [TestMethod]
        public void CanSerializeObjectWithClosure()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            var hasClosure = new HasClosure();
            hasClosure.Create();

            serializer.Serialize(hasClosure, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<HasClosure>(stream);
            Assert.IsNotNull(res);
            var actual = res.Del();
            Assert.AreEqual(4,actual);
        }
    }
}
