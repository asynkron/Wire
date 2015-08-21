using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class CollectionTests : TestBase
    {
        //TODO: HashSet causes stack overflow on serialization right now
        [TestMethod]
        public void CanSerializeSet()
        {
            var expected = new HashSet<Something>
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something(),
                null
            };

            Serialize(expected);
            Reset();
            var actual = Deserialize<HashSet<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        //TODO: Dictionary causes stack overflow on serialization right now
        [TestMethod]
        public void CanSerializeDictionary()
        {
            var expected = new Dictionary<string, string>
            {
                ["abc"] = "def",
                ["ghi"] = "jkl,"
            };

            Serialize(expected);
            Reset();
            var actual = Deserialize<Dictionary<string, string>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeList()
        {
            var expected = new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(), new Something(), null
            }.ToList();

            Serialize(expected);
            Reset();
            var actual = Deserialize<List<Something>>();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeArray()
        {
            var expected = new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something(), null
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something[]>();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializePrimitiveArray()
        {
            var expected = new[] {DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.Today};
            Serialize(expected);
            Reset();
            var actual = Deserialize<DateTime[]>();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}