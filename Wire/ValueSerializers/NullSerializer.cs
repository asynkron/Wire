using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire.ValueSerializers
{
    public class NullSerializer : ValueSerializer
    {
        public static readonly NullSerializer Instance = new NullSerializer();
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(0);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {            
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return null;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }
    }
}
