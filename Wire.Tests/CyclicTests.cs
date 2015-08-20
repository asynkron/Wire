using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class CyclicTests
    {
        [TestMethod]
        public void CanSerializeCyclicReferences()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences:true));
            var bar = new Bar();
            bar.Self = bar;
            bar.XYZ = 234;

            serializer.Serialize(bar, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Bar>(stream);
            Assert.AreSame(actual,actual.Self);
            Assert.AreEqual(bar.XYZ,actual.XYZ);
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
