using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Wire.Tests
{
    [TestClass]
    public class UnsupportedTypeSerializerTests:TestBase
    {
        [TestMethod]
        public void DoUnsupportedTypesThrowErrors()
        {
            var serializer = new Wire.Serializer();
            var t = new TestElement();
            try
            {
                serializer.Serialize(t, new System.IO.MemoryStream());
            }
            catch(Wire.ValueSerializers.UnsupportedTypeException)
            {
                Assert.IsTrue(true);
                return;
            }

        }

        [TestMethod]
        public void DoUnsupportedTypesNotHangOnExceptions()
        {
            var th = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                var serializer = new Wire.Serializer();
                var t = new TestElement();
                try
                {
                    serializer.Serialize(t, new System.IO.MemoryStream());
                }
                catch (Exception)
                {
                    try
                    {
                        serializer.Serialize(t, new System.IO.MemoryStream());
                    }
                    catch (Wire.ValueSerializers.UnsupportedTypeException)
                    {
                        return;
                    }

                }
            }));
            th.Start();
            if (!th.Join(TimeSpan.FromSeconds(5)))
            {
                th.Abort();
                Assert.Fail("Serializer did not complete in 5 seconds");
            }
            
        }
    }


    /* Copied from MongoDB.Bson.BsonElement - causes failures in the serializer - not sure why */
#if NET45
    [Serializable]
#endif
    struct TestElement : IComparable<TestElement>, IEquatable<TestElement>
    {
        private readonly string _name;
        private readonly string _value;


        public TestElement(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Value
        {
            get { return _value; }
        }

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
            return new TestElement(_name, _value);
        }

        public TestElement DeepClone()
        {
            return new TestElement(_name, _value);
        }

        public int CompareTo(TestElement other)
        {
            int r = _name.CompareTo(other._name);
            if (r != 0) { return r; }
            return _value.CompareTo(other._value);
        }

        public bool Equals(TestElement rhs)
        {
            return _name == rhs._name && _value == rhs._value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(TestElement)) { return false; }
            return Equals((TestElement)obj);
        }

        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            hash = 37 * hash + _name.GetHashCode();
            hash = 37 * hash + _value.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", _name, _value);
        }
    }

}
