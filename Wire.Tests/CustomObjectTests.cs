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
            var expected = typeof(ArgumentException);
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


        //this uses a lightweight serialization of exceptions to conform to .NET core's lack of ISerializable
        //all custom exception information will be lost.
        //only message, inner exception, stacktrace and the bare minimum will be preserved.
        [TestMethod]
        public void CanSerializeException()
        {
            var expected = new Exception("hello wire");
            Serialize(expected);
            Reset();
            var actual = Deserialize<Exception>();
            Assert.AreEqual(expected.StackTrace, actual.StackTrace);
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
        public void CanSerializeStruct()
        {
            var expected = new StuctValue
            {
                Prop1 = "hello",
                Prop2 = 123,
            };


            Serialize(expected);
            Reset();
            var actual = Deserialize<StuctValue>();
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
                StringProp = "hello",
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
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanEmptyObject()
        {
            var expected = new Empty();

            Serialize(expected);
            Reset();
            var actual = Deserialize<Empty>();
            Assert.AreEqual(expected, actual);
        }
    }
}