using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    internal class BitConverterEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetBytes(short value, byte[] buffer, int index)
        {
            buffer[index] = (byte)(value >> 8);
            buffer[index+1] = (byte)(value & 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void GetBytes(long value, byte[] buffer, int index)
        {
            fixed (byte* b = buffer )
            {
                *(long*) (b + index) = value;
            }
        }
    }
}
