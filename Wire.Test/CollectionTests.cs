using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Wire.Tests
{
    public class CollectionTests : TestBase
    {
        [Fact]
        public void CanSerializeSetOfInt()
        {
            var expected = new HashSet<int>
            {
                1,2,3,4,5
            };

            Serialize(expected);
            Reset();
            var actual = Deserialize<HashSet<int>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeStack()
        {
            var expected = new Stack<Something>();
            expected.Push(new Something
            {
                BoolProp = true,
                Else = new Else
                {
                    Name = "Yoho"
                },
                Int32Prop = 999,
                StringProp = "Yesbox!"
            });


            expected.Push(new Something());

            expected.Push(new Something());

            Serialize(expected);
            Reset();
            var actual = Deserialize<Stack<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
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
            Assert.Equal(expected, actual);
        }

        [Fact]
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
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanSerializeByteArray()
        {
            var expected = new byte[]
            {
                1, 2, 3, 4
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<byte[]>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Issue18()
        {
            var msg = new byte[] {1, 2, 3, 4};
            var serializer = new Serializer(new SerializerOptions(true, preserveObjectReferences: true));

            byte[] serialized;
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(msg, ms);
                serialized = ms.ToArray();
            }

            byte[] deserialized;
            using (var ms = new MemoryStream(serialized))
            {
                deserialized = serializer.Deserialize<byte[]>(ms);
            }

            Assert.True(msg.SequenceEqual(deserialized));
        }


        [Fact]
        public void CanSerializeArrayOfTuples()
        {
            var expected = new[]
            {
                Tuple.Create(1, 2, 3),
                Tuple.Create(4, 5, 6),
                Tuple.Create(7, 8, 9)
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Tuple<int, int, int>[]>();
            Assert.Equal(expected, actual);
        }


        //TODO: add support for multi dimentional arrays
        [Fact(Skip = "Not implemented")]
        public void CanSerializeMultiDimentionalArray()
        {
            var expected = new double[3, 3, 3];
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        expected[i, j, k] = i + j + k;
                    }
                }
            }
            Serialize(expected);
            Reset();
            var actual = Deserialize<double[,,]>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanSerializePrimitiveArray()
        {
            var expected = new[] {DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.Today};
            Serialize(expected);
            Reset();
            var actual = Deserialize<DateTime[]>();
            Assert.Equal(expected, actual);
        }
    }
}