using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));
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
        public void CanSerializeDictionaryPreserveObjectReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));

            var arr1 = new[] {1, 2, 3};
            var arr2 = new[] { 1, 2, 3 };
            var obj = new Dictionary<int, int[]>
            {
                [1] = arr1,
                [2] = arr2,
                [3] = arr1,
            };

            serializer.Serialize(obj, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Dictionary<int, int[]>>(stream);

            Assert.AreSame(res[1], res[3]);
            Assert.AreNotSame(res[1], res[2]);
        }

        [TestMethod]
        public void CanSerializeCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(versionTolerance: true, preserveObjectReferences: true));
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
        public long Baz { get; set; }
        public Bar B2 { get; set; }
    }

    public class Bar
    {
        public long Boo { get; set; }
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