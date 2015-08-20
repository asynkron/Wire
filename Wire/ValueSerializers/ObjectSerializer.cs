using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        private readonly byte[] _manifest;
        public Func<Stream, SerializerSession, object> _reader;
        public Action<Stream, object, SerializerSession> _writer;

        public ObjectSerializer(Type type)
        {
            Type = type;
            var bytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new byte[] {255}
                    .Concat(BitConverter.GetBytes(bytes.Length))
                    .Concat(bytes)
                    .ToArray(); //serializer id 255 + assembly qualified name
        }

        public Type Type { get; }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            _writer(stream, value, session);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return _reader(stream, session);
        }

        public override Type GetElementType()
        {
            return Type;
        }
    }
}