using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        public Type Type { get; }

        private readonly Action<Stream, object, SerializerSession> _writer;
        private readonly Func<Stream, SerializerSession, object> _reader;
        private readonly byte[] _manifest;

        public ObjectSerializer(Type type, Action<Stream, object, SerializerSession> writer,
            Func<Stream, SerializerSession, object> reader)
        {
            Type = type;
            _writer = writer;
            _reader = reader;
            _manifest = new byte[] {255}.Union(Encoding.UTF8.GetBytes(type.AssemblyQualifiedName)).ToArray(); //serializer id 255 + assembly qualified name
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            ByteArraySerializer.Instance.WriteValue(stream, _manifest, session); //write the encoded name of the type
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