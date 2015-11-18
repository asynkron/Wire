using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class ImmutableCollectionTests : TestBase
    {
        [TestMethod]
        public void CanSerializeImmutableHashSet()
        {
            var expected = ImmutableHashSet.CreateRange(new[]
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
                new Something()
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableHashSet<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeImmutableSortedSet()
        {
            var expected = ImmutableSortedSet.CreateRange(new[]
            {
                "abc",
                "abcd",
                "abcde"
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableSortedSet<string>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeImmutableDictionary()
        {
            var expected = ImmutableDictionary.CreateRange(new Dictionary<string, Something>
            {
                ["a1"] = new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                ["a2"] = new Something(),
                ["a3"] = new Something(),
                ["a4"] = null
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableDictionary<string, Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeImmutableQueue()
        {
            var expected = ImmutableQueue.CreateRange(new[]
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
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableQueue<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        //[TestMethod]
        public void CanSerializeImmutableStack()
        {
            var expected = ImmutableStack.CreateRange(new[]
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
                new Something()
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableStack<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeImmutableArray()
        {
            var expected = ImmutableArray.CreateRange(new[]
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
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableArray<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeImmutableList()
        {
            var expected = ImmutableList.CreateRange(new[]
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
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableList<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }
    }
}