using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        private readonly byte[] _manifest;
        private Func<Stream, SerializerSession, object> _reader;
        private Action<Stream, object, SerializerSession> _writer;
        public const byte Manifest = 255;
        private volatile bool _isInitialized = false;
        public ObjectSerializer(Type type)
        {
            Type = type;
            var bytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new[] {Manifest}
                    .Concat(BitConverter.GetBytes(bytes.Length))
                    .Concat(bytes)
                    .ToArray(); //serializer id 255 + assembly qualified name

            //initialize reader and writer with dummy handlers that wait until the serializer is fully initialized
            _writer = (stream, o, session) =>
            {
                SpinWait.SpinUntil(() => _isInitialized);
                WriteValue(stream, o, session);
            };

            _reader = (stream, session) =>
            {
                SpinWait.SpinUntil(() => _isInitialized);
                return ReadValue(stream, session);
            };
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

        public void Initialize(Func<Stream, SerializerSession, object> reader, Action<Stream, object, SerializerSession> writer)
        {
            _reader = reader;
            _writer = writer;
            _isInitialized = true;
        }
    }
}