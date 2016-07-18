using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wire
{
    public static class Utils
    {
        private static readonly string CoreAssemblyName = GetCoreAssemblyName();

        private static string GetCoreAssemblyName()
        {
            var name = 1.GetType().AssemblyQualifiedName;
            var part = name.Substring(name.IndexOf(", Version", StringComparison.Ordinal));
            return part;
        }

        public static string GetShortAssemblyQualifiedName(this Type self)
        {
            var name = self.AssemblyQualifiedName.Replace(CoreAssemblyName, ",%core%");
            return name;
        }

        public static string ToQualifiedAssemblyName(string shortName)
        {
            return shortName.Replace(",%core%", CoreAssemblyName);
        }

#if UNSAFE
        public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                var l = a1.Length;
                for (var i = 0; i < l/8; i++, x1 += 8, x2 += 8)
                    if (*(long*) x1 != *(long*) x2) return false;
                if ((l & 4) != 0)
                {
                    if (*(int*) x1 != *(int*) x2) return false;
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*(short*) x1 != *(short*) x2) return false;
                    x1 += 2;
                    x2 += 2;
                }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }
#else
        public static bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
#endif

        public static bool IsFixedSizeType(Type type)
        {
            return type == typeof (int) ||
                   type == typeof (long) ||
                   type == typeof (bool) ||
                   type == typeof (ushort) ||
                   type == typeof (uint) ||
                   type == typeof (ulong);
        }

        public static int GetTypeSize(Type type)
        {
            if (type == typeof (int))
                return sizeof (int);
            if (type == typeof(long))
                return sizeof (long);
            if (type == typeof (bool))
                return sizeof (bool);
            if (type == typeof (ushort))
                return sizeof (ushort);
            if (type == typeof (uint))
                return sizeof (uint);
            if (type == typeof (ulong))
                return sizeof (ulong);

            throw new NotSupportedException();
        }

        private static readonly UTF8Encoding Utf8 = (UTF8Encoding) Encoding.UTF8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] StringToBytes(string str)
        {
            return Utf8.GetBytes(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string BytesToString(byte[] bytes,int offset, int count)
        {
            return Utf8.GetString(bytes,offset,count);
        }
    }
}