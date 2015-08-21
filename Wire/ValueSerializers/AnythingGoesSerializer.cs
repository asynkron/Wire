using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Wire.ValueSerializers
{
    //TODO: serializer that custom plugins can use to handle special types
    public class AnythingGoesSerializer : ValueSerializer
    {
        private Type _type;
        private byte[] _manifest;

        public AnythingGoesSerializer(Type type)
        {
            _type = type;

            var bytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new byte[] { 255 } //same as object serializer
                    .Concat(BitConverter.GetBytes(bytes.Length))
                    .Concat(bytes)
                    .ToArray(); //serializer id 255 + assembly qualified name
        }
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            throw new NotImplementedException();
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            throw new NotImplementedException();
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            throw new NotImplementedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}
