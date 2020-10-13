// -----------------------------------------------------------------------
//   <copyright file="Something.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;

namespace Wire.Tests
{
    public struct StuctValue
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
    }

    public class Empty : IEquatable<Empty>
    {
        public bool Equals(Empty other)
        {
            if (other == null) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Empty) obj);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        public static bool operator ==(Empty left, Empty right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Empty left, Empty right)
        {
            return !Equals(left, right);
        }
    }

    public class Something : IEquatable<Something>
    {
        public string StringProp { get; set; }
        public int Int32Prop { get; set; }
        public bool BoolProp { get; set; }
        public int? NullableInt32PropNoValue { get; set; } = null;
        public int? NullableInt32PropHasValue { get; set; } = 123;
        public Else Else { get; set; }

        public bool Equals(Something other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(StringProp, other.StringProp) && Int32Prop == other.Int32Prop &&
                   BoolProp == other.BoolProp && NullableInt32PropNoValue == other.NullableInt32PropNoValue &&
                   NullableInt32PropHasValue == other.NullableInt32PropHasValue && Equals(Else, other.Else);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Something) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StringProp != null ? StringProp.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Int32Prop;
                hashCode = (hashCode * 397) ^ BoolProp.GetHashCode();
                hashCode = (hashCode * 397) ^ NullableInt32PropNoValue.GetHashCode();
                hashCode = (hashCode * 397) ^ NullableInt32PropHasValue.GetHashCode();
                hashCode = (hashCode * 397) ^ (Else != null ? Else.GetHashCode() : 0);
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
    }

    public class Else : IEquatable<Else>
    {
        public string Name { get; set; }

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
            if (obj.GetType() != GetType()) return false;
            return Equals((Else) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(Else left, Else right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Else left, Else right)
        {
            return !Equals(left, right);
        }
    }

    public class OtherElse : Else, IEquatable<OtherElse>
    {
        public string More { get; set; }

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
            if (obj.GetType() != GetType()) return false;
            return Equals((OtherElse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (More != null ? More.GetHashCode() : 0);
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
    }
}