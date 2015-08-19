using System;
using System.IO;

namespace Wire
{
    public static class StreamExtensions
    {

        public static void WriteInt32(this Stream self, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            self.Write(bytes, 0, bytes.Length);
        }
    }
}
