using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire.Tests
{
    public class Something : IEquatable<Something>
    {
        public bool Equals(Something other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(StringProp, other.StringProp) && Int32Prop == other.Int32Prop && BoolProp == other.BoolProp && NullableInt32PropNoValue == other.NullableInt32PropNoValue && NullableInt32PropHasValue == other.NullableInt32PropHasValue && Equals(Else, other.Else);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Something) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (StringProp != null ? StringProp.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Int32Prop;
                hashCode = (hashCode*397) ^ BoolProp.GetHashCode();
                hashCode = (hashCode*397) ^ NullableInt32PropNoValue.GetHashCode();
                hashCode = (hashCode*397) ^ NullableInt32PropHasValue.GetHashCode();
                hashCode = (hashCode*397) ^ (Else != null ? Else.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Something left, Something right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Something left, Something right)
        {
            return !Equals(left, right);
        }

        public string StringProp { get; set; }
        public int Int32Prop { get; set; }
        public bool BoolProp { get; set; }
        public int? NullableInt32PropNoValue { get; set; } = null;
        public int? NullableInt32PropHasValue { get; set; } = 123;
        public Else Else { get; set; }
    }

    public class Else : IEquatable<Else>
    {
        public bool Equals(Else other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Else) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Else left, Else right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Else left, Else right)
        {
            return !Equals(left, right);
        }

        public string Name { get; set; }
    }

    public class OtherElse : Else, IEquatable<OtherElse>
    {
        public bool Equals(OtherElse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(More, other.More);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OtherElse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (More != null ? More.GetHashCode() : 0);
            }
        }

        public static bool operator ==(OtherElse left, OtherElse right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OtherElse left, OtherElse right)
        {
            return !Equals(left, right);
        }

        public string More { get; set; }
    }


}
