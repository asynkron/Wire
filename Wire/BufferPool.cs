using System;
using System.Threading;

namespace Wire
{
    internal static class BufferPool
    {
        internal static void Flush()
        {
            for (int i = 0; i < Pool.Length; i++)
            {
                Interlocked.Exchange(ref Pool[i], null); // and drop the old value on the floor
            }
        }

        const int PoolSize = 20;
        internal const int BufferLength = 1024;
        private static readonly object[] Pool = new object[PoolSize];

        internal static byte[] GetBuffer()
        {
            for (int i = 0; i < Pool.Length; i++)
            {
                object tmp;
                if ((tmp = Interlocked.Exchange(ref Pool[i], null)) != null) return (byte[])tmp;
            }
            return new byte[BufferLength];
        }
        internal static void ResizeAndFlushLeft(ref byte[] buffer, int toFitAtLeastBytes, int copyFromIndex, int copyBytes)
        {

            // try doubling, else match
            int newLength = buffer.Length * 2;
            if (newLength < toFitAtLeastBytes) newLength = toFitAtLeastBytes;

            byte[] newBuffer = new byte[newLength];
            if (copyBytes > 0)
            {
                Buffer.BlockCopy(buffer, copyFromIndex, newBuffer, 0, copyBytes);
            }
            if (buffer.Length == BufferLength)
            {
                ReleaseBufferToPool(ref buffer);
            }
            buffer = newBuffer;
        }
        internal static void ReleaseBufferToPool(ref byte[] buffer)
        {
            if (buffer == null) return;
            if (buffer.Length == BufferLength)
            {
                for (int i = 0; i < Pool.Length; i++)
                {
                    if (Interlocked.CompareExchange(ref Pool[i], buffer, null) == null)
                    {
                        break; // found a null; swapped it in
                    }
                }
            }
            // if no space, just drop it on the floor
            buffer = null;
        }
    }
}