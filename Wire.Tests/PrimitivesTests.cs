using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class PrimitivesTest : TestBase
    {
        [TestMethod]
        public void CanSerializeBool()
        {
            SerializeAndAssert(true);
        }

        [TestMethod]
        public void CanSerializeGuid()
        {
            SerializeAndAssert(Guid.NewGuid());
        }

        [TestMethod]
        public void CanSerializeDateTime()
        {
            SerializeAndAssert(DateTime.Now);
        }

        [TestMethod]
        public void CanSerializeDecimal()
        {
            SerializeAndAssert(123m);
        }

        [TestMethod]
        public void CanSerializeDouble()
        {
            SerializeAndAssert(123d);
        }


        [TestMethod]
        public void CanSerializeByte()
        {
            SerializeAndAssert((byte) 123);
        }
        [TestMethod]
        public void CanSerializeSByte()
        {
            SerializeAndAssert((sbyte)123);
        }

        [TestMethod]
        public void CanSerializeInt16()
        {
            SerializeAndAssert((short) 123);
        }

        [TestMethod]
        public void CanSerializeInt64()
        {
            SerializeAndAssert(123L);
        }

        [TestMethod]
        public void CanSerializeInt32()
        {
            SerializeAndAssert(123);
        }

        [TestMethod]
        public void CanSerializeUInt16()
        {
            SerializeAndAssert((UInt16)123);
        }

        [TestMethod]
        public void CanSerializeUInt64()
        {
            SerializeAndAssert((UInt64)123);
        }

        [TestMethod]
        public void CanSerializeUInt32()
        {
            SerializeAndAssert((UInt32)123);
        }

        [TestMethod]
        public void CanSerializeString()
        {
            SerializeAndAssert("hello");
        }

        private void SerializeAndAssert(object expected)
        {
            Serialize(expected);
            Reset();
            var res = Deserialize<object>();
            Assert.AreEqual(expected, res);
        }
    }
}