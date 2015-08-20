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
    public class ArrayTests
    {
        private Serializer serializer;
        private MemoryStream stream;


        [TestInitialize]
        public void Setup()
        {
            serializer = new Serializer();
            stream = new MemoryStream();
        }

        private void Reset()
        {
            stream.Position = 0;
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
            serializer.Serialize(expected, stream);
            Reset();
            var actual = serializer.Deserialize<Something[]>(stream);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializePrimitiveArray()
        {
            var expected = new[] {DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.Today,};
            serializer.Serialize(expected, stream);
            Reset();
            var actual = serializer.Deserialize<DateTime[]>(stream);
            CollectionAssert.AreEqual(expected,actual);
        }
    }
}
