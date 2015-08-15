using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire
{
    public class SerializerSession
    {
        public byte[] Buffer { get; set; }

        public byte[] GetBuffer(int length)
        {
            if (length <= Buffer.Length)
                return Buffer;
            return new byte[length];
        }
    }
}
