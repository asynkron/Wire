using System.Runtime.CompilerServices;
using System.Text;

namespace Wire.Extensions
{
    internal static class StringEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] ToUtf8Bytes(this string str)
        {
            return NoAllocBitConverter.Utf8.GetBytes(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string FromUtf8Bytes(byte[] bytes,int offset, int count)
        {
            return NoAllocBitConverter.Utf8.GetString(bytes,offset,count);
        }
    }
}