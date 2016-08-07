using System;
using System.Runtime.CompilerServices;
using System.Text;
using Wire.ValueSerializers;

namespace Wire
{
    /// <summary>
    /// Provides methods not allocating the byte buffer but using <see cref="SerializerSession.GetBuffer"/> to lease a buffer.
    /// </summary>
    public static class NoAllocBitConverter
    {
        public static void GetBytes(char value, byte[] bytes)
        {
            GetBytes((short) value, bytes);
        }

        public static unsafe void GetBytes(short value, byte[] bytes)
        {
            fixed (byte* b = bytes)
                *((short*) b) = value;
        }

        public static unsafe void GetBytes(int value, byte[] bytes)
        {
            fixed (byte* b = bytes)
                *((int*) b) = value;
        }

        public static unsafe void GetBytes(long value, byte[] bytes)
        {
            fixed (byte* b = bytes)
                *((long*) b) = value;
        }

        public static void GetBytes(ushort value, byte[] bytes)
        {
            GetBytes((short) value, bytes);
        }

        public static void GetBytes(uint value, byte[] bytes)
        {
            GetBytes((int) value, bytes);
        }

        public static void GetBytes(ulong value, byte[] bytes)
        {
            GetBytes((long) value, bytes);
        }

        public static unsafe void GetBytes(float value, byte[] bytes)
        {
            GetBytes(*(int*) &value, bytes);
        }

        public static unsafe void GetBytes(double value, byte[] bytes)
        {
            GetBytes(*(long*) &value, bytes);
        }

        internal static readonly UTF8Encoding Utf8 = (UTF8Encoding) Encoding.UTF8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe byte[] GetBytes(string str, SerializerSession session, out int byteCount)
        {
            if (str == null)
            {
                byteCount = 1;
                return new[] {(byte) 0};
            }
            byteCount = Utf8.GetByteCount(str);
            if (byteCount < 254) //short string
            {
                byte[] bytes = session.GetBuffer(byteCount + 1);
                Utf8.GetBytes(str, 0, byteCount, bytes, 1);
                bytes[0] = (byte) (byteCount + 1);
                byteCount += 1;
                return bytes;
            }
            else //long string
            {
                byte[] bytes = session.GetBuffer(byteCount + 1 + 4);
                Utf8.GetBytes(str, 0, byteCount, bytes, 1 + 4);
                bytes[0] = 255;

                fixed (byte* b = bytes)
                    *((int*) b + 1) = byteCount;

                byteCount += 1 + 4;

                return bytes;
            }
        }

        public static unsafe void GetBytes(DateTime dateTime, byte[] bytes)
        {
            //datetime size is 9 ticks + kind
            fixed (byte* b = bytes)
                *((long*) b) = dateTime.Ticks;
            bytes[DateTimeSerializer.Size - 1] = (byte) dateTime.Kind;
        }
    }
}