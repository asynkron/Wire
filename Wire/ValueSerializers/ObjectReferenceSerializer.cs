using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire.ValueSerializers
{
    public class ObjectReferenceSerializer : ValueSerializer
    {
        public static readonly ObjectReferenceSerializer Instance = new ObjectReferenceSerializer();
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(253);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            stream.WriteInt32((int)value);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var id = stream.ReadInt32(session);
            var obj = session.ObjectById[id];
            return obj;
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}
