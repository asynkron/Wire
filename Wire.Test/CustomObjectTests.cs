using System;
using System.Linq.Expressions;
using Xunit;

namespace Wire.Tests
{
    
    public class CustomObjectTests : TestBase
    {
        [Fact]
        public void CanSerializeTypeObject()
        {
            var expected = typeof (ArgumentException);
            Serialize(expected);
            Reset();
            var actual = Deserialize<Type>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanSerializeNull()
        {
            var expected = new Something
            {
                Else = null
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void CanSerializeException()
        {
            var expected = new ArgumentException("foo","bar");
            Serialize(expected);
            Reset();
            var actual = Deserialize<ArgumentException>();
            Assert.Equal(expected.ParamName, actual.ParamName);
            Assert.Equal(expected.Message, actual.Message);
        }
        [Fact]
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
            Assert.Equal(expected, actual);
        }

        [Fact]
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
            Assert.Equal(expected, actual);
        }

        [Fact]
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
            Assert.Equal(expected1, Deserialize<Something>());
            Assert.Equal(expected2, Deserialize<Something>());
            Assert.Equal(expected3, Deserialize<Something>());
        }

        [Fact]
        public void CanSerializeTuple()
        {
            var expected = Tuple.Create("hello");
            Serialize(expected);
            Reset();
            var actual = Deserialize<Tuple<string>>();
            Assert.Equal(expected,actual);
        }
    }
}