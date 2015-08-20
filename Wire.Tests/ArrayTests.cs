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
    public class ArrayTests : TestBase
    {
      
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
