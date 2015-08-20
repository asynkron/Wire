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

        public static int ReadInt32(this Stream self, SerializerSession session)
        {
            var buffer = session.GetBuffer(4);
            self.Read(buffer, 0, 4);
            int res = BitConverter.ToInt32(buffer, 0);
            return res;
        }

        public static byte[] ReadLengthEncodedByteArray(this Stream self,SerializerSession session)
        {
            var length = self.ReadInt32(session);
            var buffer = session.GetBuffer(length);
            self.Read(buffer, 0, length);
            return buffer;
        }

        public static void WriteLengthEncodedByteArray(this Stream self, byte[] bytes)
        {
            self.WriteInt32(bytes.Length);
            self.Write(bytes,0,bytes.Length);
        }

        public static void Write(this Stream self, byte[] bytes)
        {
            self.Write(bytes,0,bytes.Length);
        }
    }
}