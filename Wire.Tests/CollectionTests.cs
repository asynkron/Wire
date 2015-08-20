using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class CollectionTests : TestBase
    {
        //TODO: HashSet causes stack overflow on serialization right now
        [TestMethod,Ignore]
        public void CanSerializeSet()
        {
            var data = new[]
            {
                new Something()
                {
                    BoolProp = true,
                    Else = new Else()
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(), new Something(), null
            };

            var expected = new HashSet<Something>(data);

            Serialize(expected);
            Reset();
            var actual = Deserialize<HashSet<Something>>();
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void CanSerializeList()
        {
            var expected = new[]
            {
                new Something()
                {
                    BoolProp = true, Else = new Else()
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999, StringProp = "Yesbox!"
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
            var expected = new[] { new Something()
            {
                BoolProp = true,
                Else = new Else()
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
            var expected = new[] {DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.Today,};
            Serialize(expected);
            Reset();
            var actual = Deserialize<DateTime[]>();
            CollectionAssert.AreEqual(expected,actual);
        }
    }
}
