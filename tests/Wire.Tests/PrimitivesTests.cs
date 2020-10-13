// -----------------------------------------------------------------------
//   <copyright file="PrimitivesTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using Xunit;

namespace Wire.Tests
{
    public class PrimitivesTest : TestBase
    {
        private void SerializeAndAssert(object expected)
        {
            Serialize(expected);
            Reset();
            var res = Deserialize<object>();
            Assert.Equal(expected, res);
            AssertMemoryStreamConsumed();
        }

        [Fact]
        public void CanSerializeBool()
        {
            SerializeAndAssert(true);
        }

        [Fact]
        public void CanSerializeByte()
        {
            SerializeAndAssert((byte) 123);
        }

        [Fact]
        public void CanSerializeDateTime()
        {
            SerializeAndAssert(DateTime.UtcNow);
        }

        [Fact]
        public void CanSerializeDateTimeOffset()
        {
            SerializeAndAssert(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(14)));
        }

        [Fact]
        public void CanSerializeDecimal()
        {
            SerializeAndAssert(123m);
        }

        [Fact]
        public void CanSerializeDouble()
        {
            SerializeAndAssert(123d);
        }

        [Fact]
        public void CanSerializeGuid()
        {
            SerializeAndAssert(Guid.NewGuid());
        }

        [Fact]
        public void CanSerializeInt16()
        {
            SerializeAndAssert((short) 123);
        }

        [Fact]
        public void CanSerializeInt32()
        {
            SerializeAndAssert(123);
        }

        [Fact]
        public void CanSerializeInt64()
        {
            SerializeAndAssert(123L);
        }

        [Fact]
        public void CanSerializeLongString()
        {
            var s = new string('x', 1000);
            SerializeAndAssert(s);
        }

        [Fact]
        public void CanSerializeSByte()
        {
            SerializeAndAssert((sbyte) 123);
        }

        [Fact]
        public void CanSerializeString()
        {
            SerializeAndAssert("hello");
        }

        [Fact]
        public void CanSerializeTuple1()
        {
            SerializeAndAssert(Tuple.Create("abc"));
        }

        [Fact]
        public void CanSerializeTuple2()
        {
            SerializeAndAssert(Tuple.Create(1, 123));
        }

        [Fact]
        public void CanSerializeTuple3()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3));
        }

        [Fact]
        public void CanSerializeTuple4()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3, 4));
        }

        [Fact]
        public void CanSerializeTuple5()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3, 4, 5));
        }

        [Fact]
        public void CanSerializeTuple6()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void CanSerializeTuple7()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
        }

        [Fact]
        public void CanSerializeTuple8()
        {
            SerializeAndAssert(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
        }

        [Fact]
        public void CanSerializeUInt16()
        {
            SerializeAndAssert((ushort) 123);
        }

        [Fact]
        public void CanSerializeUInt32()
        {
            SerializeAndAssert((uint) 123);
        }

        [Fact]
        public void CanSerializeUInt64()
        {
            SerializeAndAssert((ulong) 123);
        }

        [Fact]
        public void CanSerializeEnum()
        {
            SerializeAndAssert(Fruit.Banana);
        }
    }

    public enum Fruit
    {
        Apple,
        Banana,
        Cheese
    }
}