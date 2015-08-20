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
    public class CustomObjectTests : TestBase
    {
        [TestMethod]
        public void CanSerializeNull()
        {
            var expected = new Something()
            {
                Else = null
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializePolymorphicObject()
        {
            var expected = new Something()
            {
                Else = new OtherElse()
                {
                    Name = "Foo",
                    More = "Bar"
                }
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeObject()
        {
            var expected = new Something()
            {

            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void CanSerializeObjects()
        {
            var expected1 = new Something()
            {
                StringProp = "First"
            };
            var expected2 = new Something()
            {
                StringProp = "Second"
            };
            var expected3 = new Something()
            {
                StringProp = "Last"
            };
            Serialize(expected1);
            Serialize(expected2);
            Serialize(expected3);
            Reset();
            Assert.AreEqual(expected1, Deserialize<Something>());
            Assert.AreEqual(expected2, Deserialize<Something>());
            Assert.AreEqual(expected3, Deserialize<Something>());
        }

    }
}
