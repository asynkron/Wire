using System.Runtime.CompilerServices;
using System.Text;

namespace Wire.Extensions
{
    internal static class StringEx
    {
        private static readonly UTF8Encoding Utf8 = (UTF8Encoding) Encoding.UTF8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] ToUtf8Bytes(this string str)
        {
            return Utf8.GetBytes(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8Bytes(byte[] bytes,int offset, int count)
        {
            return Utf8.GetString(bytes,offset,count);
        }
    }
}