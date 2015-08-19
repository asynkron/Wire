using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    public class BufferStream
    {
        private byte[] _buffer;
        private int _index = 0;

        public BufferStream()
        {
            _buffer = BufferPool.GetBuffer();
        }

        public void WriteByte(byte b)
        {
            EnsureCapacity(1);
            _buffer[_index] = b;
            _index ++;
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            Buffer.BlockCopy(bytes,offset,_buffer,_index,count);
            _index += count;
        }

        public void WriteInt32(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes,0,3);            
        }    

        private void EnsureCapacity(int i)
        {
            if (i + _index >= _buffer.Length)
            {
                BufferPool.ResizeAndFlushLeft(ref _buffer, i,0,_buffer.Length);
            }
        }

        public void Release()
        {
            BufferPool.ReleaseBufferToPool(ref _buffer);
        }
    }
}
