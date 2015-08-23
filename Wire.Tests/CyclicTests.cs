using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class CyclicTests
    {
        [TestMethod]
        public void CanSerializeDeepCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences: true));
            var root = new Root();
            var bar = new Bar();
            bar.Self = bar;
            bar.XYZ = 234;
            root.B1 = bar;
            root.B2 = bar;

            serializer.Serialize(root, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Root>(stream);
            Assert.AreSame(actual.B1, actual.B1);
            Assert.AreSame(actual.B1, actual.B2);
            Assert.AreSame(actual.B1, actual.B1.Self);
            Assert.AreSame(actual.B1, actual.B2.Self);
        }

        [TestMethod]
        public void CanSerializeCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences: true));
            var bar = new Bar();
            bar.Self = bar;
            bar.XYZ = 234;

            serializer.Serialize(bar, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Bar>(stream);
            Assert.AreSame(actual, actual.Self);
            Assert.AreEqual(bar.XYZ, actual.XYZ);
        }

        [TestMethod]
        public void CanSerializeMultiLevelCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences: true));
            var a = new A();
            var b = new B();
            a.B = b;
            b.A = a;

            serializer.Serialize(a, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<A>(stream);
            Assert.AreSame(actual, actual.B.A);
        }
    }

    public class Root
    {
        public Bar B1 { get; set; }
        public Bar B2 { get; set; }
    }

    public class Bar
    {
        public Bar Self { get; set; }
        public int XYZ { get; set; }
    }

    public class A
    {
        public B B { get; set; }
    }

    public class B
    {
        public A A { get; set; }
    }
}