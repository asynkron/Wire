// -----------------------------------------------------------------------
//   <copyright file="ImmutableCollectionsTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Wire.Tests
{
    public class ImmutableCollectionTests : TestBase
    {
        //[Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }
        
        [Fact]
        public void CanSerializeImmutablePrimitiveArray()
        {
            var expected = ImmutableArray.CreateRange(new[]
            {
                1,2,3,4,5
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableArray<int>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }
        
        [Fact]
        public void CanSerializeImmutablePrimitiveDictionary()
        {
            var expected = ImmutableDictionary.CreateRange(new Dictionary<string, int>
            {
                ["a1"] = 1,
                ["a2"] = 2,
                ["a3"] = 3,
                ["a4"] = 4
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableDictionary<string, int>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutablePrimitiveList()
        {
            var expected = ImmutableList.CreateRange(new[]
            {
                1,2,3,4,5
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableList<int>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }
        
        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }
        
        
        [Fact]
        public void CanSerializeImmutablePrimitiveQueue()
        {
            var expected = ImmutableQueue.CreateRange(new[]
            {
                1,2,3,4,5,6
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableQueue<int>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }
    }
}