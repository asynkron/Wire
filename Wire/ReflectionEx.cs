using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Wire
{
    internal static class TypeEx
    {
        internal static readonly Type Int32Type = typeof(int);
        internal static readonly Type Int64Type = typeof(long);
        internal static readonly Type Int16Type = typeof(short);
        internal static readonly Type UInt32Type = typeof(uint);
        internal static readonly Type UInt64Type = typeof(ulong);
        internal static readonly Type UInt16Type = typeof(ushort);
        internal static readonly Type ByteType = typeof(byte);
        internal static readonly Type SByteType = typeof(sbyte);
        internal static readonly Type BoolType = typeof(bool);
        internal static readonly Type DateTimeType = typeof(DateTime);
        internal static readonly Type StringType = typeof(string);
        internal static readonly Type GuidType = typeof(Guid);
        internal static readonly Type FloatType = typeof(float);
        internal static readonly Type DoubleType = typeof(double);
        internal static readonly Type DecimalType = typeof(decimal);
        internal static readonly Type CharType = typeof(char);
        internal static readonly Type ByteArrayType = typeof(byte[]);
        internal static readonly Type TypeType = typeof(Type);
        internal static readonly Type RuntimeType = Type.GetType("System.RuntimeType");

        internal static bool IsPrimitiveType(Type type)
        {
            return type == Int32Type ||
                   type == Int64Type ||
                   type == Int16Type ||
                   type == UInt32Type ||
                   type == UInt64Type ||
                   type == UInt16Type ||
                   type == ByteType ||
                   type == SByteType ||
                   type == DateTimeType ||
                   type == BoolType ||
                   type == StringType ||
                   type == GuidType ||
                   type == FloatType ||
                   type == DoubleType ||
                   type == DecimalType ||
                   type == CharType;
            //add TypeSerializer with null support
        }

#if !SERIALIZATION
        private static readonly Func<Type, object> getUninitializedObjectDelegate = (Func<Type, object>)
            typeof(string)
                .GetTypeInfo()
                .Assembly
                .GetType("System.Runtime.Serialization.FormatterServices")
                ?.GetTypeInfo()
                ?.GetMethod("GetUninitializedObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                ?.CreateDelegate(typeof(Func<Type, object>));

        public static object GetEmptyObject(this Type type)
        {
            return getUninitializedObjectDelegate(type);
        }
#else
        public static object GetEmptyObject(this Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }
#endif
    }

    public static class BindingFlagsEx
    {
        public const BindingFlags All = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    }

    public static class ReflectionEx
    {
    }
}