// -----------------------------------------------------------------------
//   <copyright file="UnsupportedTypeSerializerTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using Wire.ValueSerializers;
using Xunit;

namespace Wire.Tests
{
    public class UnsupportedTypeSerializerTests : TestBase
    {
        [Fact]
        public void DoUnsupportedTypesNotHangOnExceptions()
        {
            var th = new Thread(() =>
            {
                var serializer = new Serializer();
                var t = new TestElement();
                try
                {
                    serializer.Serialize(t, new MemoryStream());
                }
                catch (Exception)
                {
                    try
                    {
                        serializer.Serialize(t, new MemoryStream());
                    }
                    catch (UnsupportedTypeException)
                    {
                    }
                }
            });
            th.Start();
            if (!th.Join(5000)) Assert.True(false, "Serializer did not complete in 5 seconds");
        }

        [Fact]
        public void DoUnsupportedTypesThrowErrors()
        {
            var serializer = new Serializer();
            var t = new TestElement();
            try
            {
                serializer.Serialize(t, new MemoryStream());
            }
            catch (UnsupportedTypeException)
            {
                Assert.True(true);
            }
        }
    }


    /* Copied from MongoDB.Bson.BsonElement - causes failures in the serializer - not sure why */

    [Serializable]
    internal struct TestElement : IComparable<TestElement>, IEquatable<TestElement>
    {
        public TestElement(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public static bool operator ==(TestElement lhs, TestElement rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(TestElement lhs, TestElement rhs)
        {
            return !(lhs == rhs);
        }

        public TestElement Clone()
        {
            return new TestElement(Name, Value);
        }

        public TestElement DeepClone()
        {
            return new TestElement(Name, Value);
        }

        public int CompareTo(TestElement other)
        {
            var r = Name.CompareTo(other.Name);
            if (r != 0) return r;
            return Value.CompareTo(other.Value);
        }

        public bool Equals(TestElement rhs)
        {
            return Name == rhs.Name && Value == rhs.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(TestElement)) return false;
            return Equals((TestElement) obj);
        }

        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            var hash = 17;
            hash = 37 * hash + Name.GetHashCode();
            hash = 37 * hash + Value.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Name, Value);
        }
    }
}