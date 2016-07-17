using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class CustomObjectTests : TestBase
    {
        [TestMethod]
        public void CanSerializeTypeObject()
        {
            var expected = typeof (ArgumentException);
            Serialize(expected);
            Reset();
            var actual = Deserialize<Type>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeNull()
        {
            var expected = new Something
            {
                Else = null
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void CanSerializeException()
        {
            var expected = new ArgumentException("foo","bar");
            Serialize(expected);
            Reset();
            var actual = Deserialize<ArgumentException>();
            Assert.AreEqual(expected.ParamName, actual.ParamName);
            Assert.AreEqual(expected.Message, actual.Message);
        }
        [TestMethod]
        public void CanSerializePolymorphicObject()
        {
            var expected = new Something
            {
                Else = new OtherElse
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
            var expected = new Something
            {
                BoolProp = true,
                Int32Prop = 123,
                NullableInt32PropHasValue = 888,
                StringProp = "hello"
            };


            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeObjects()
        {
            var expected1 = new Something
            {
                StringProp = "First"
            };
            var expected2 = new Something
            {
                StringProp = "Second"
            };
            var expected3 = new Something
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

        [TestMethod]
        public void CanSerializeTuple()
        {
            var expected = Tuple.Create("hello");
            Serialize(expected);
            Reset();
            var actual = Deserialize<Tuple<string>>();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void CanSerializeType()
        {
            var expected = Tuple.Create(GetType(), GetType(), (Type)null);
            Serialize(expected);
            Reset();
            var actual = Deserialize<Tuple<Type, Type, Type>>();
            Assert.AreEqual(expected, actual);
        }
    }
}