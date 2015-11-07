using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        private readonly byte[] _manifest;
        private Func<Stream, DeserializerSession, object> _reader;
        private Action<Stream, object, SerializerSession> _writer;
        public const byte Manifest = 255;
        public const byte TypeNameHeader = 1;
        public const byte TypeIdentifier = 2;

        private volatile bool _isInitialized = false;
        public ObjectSerializer(Type type)
        {
            Type = type;
            var typeNameBytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new[] {Manifest, TypeNameHeader}
                    .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                    .Concat(typeNameBytes)
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
            if (session.ShouldWriteTypeManifest(type))
            {
                stream.Write(_manifest);               
            }
            else
            {
                var typeIdentifier = session.GetTypeIdentifier(type);
                stream.Write(new []{Manifest, TypeIdentifier});
                stream.WriteInt32(typeIdentifier);
            }
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {

            _writer(stream, value, session);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _reader(stream, session);
        }

        public override Type GetElementType()
        {
            return Type;
        }

        public void Initialize(Func<Stream, DeserializerSession, object> reader, Action<Stream, object, SerializerSession> writer)
        {
            _reader = reader;
            _writer = writer;
            _isInitialized = true;
        }

        private static readonly ConcurrentDictionary<byte[], Type> TypeNameLookup = new ConcurrentDictionary<byte[], Type>(new ByteArrayEqualityComparer());

        private static Type GetTypeFromManifestName(Stream stream, DeserializerSession session)
        {
            var bytes = (byte[])ByteArraySerializer.Instance.ReadValue(stream, session);

            return TypeNameLookup.GetOrAdd(bytes, b =>
            {
                var typename = Encoding.UTF8.GetString(b);
                return Type.GetType(typename, true);
            });
        }

        public static Type GetTypeFromManifest(Stream stream, DeserializerSession session)
        {
            var x = stream.ReadByte();
            if (x == TypeNameHeader)
            {
                var type = GetTypeFromManifestName(stream, session);
                session.TrackDeserializedType(type);
                return type;
            }
            if (x == TypeIdentifier)
            {
                var typeId = stream.ReadInt32(session);
                var type = session.GetTypeFromTypeId(typeId);
                return type;
            }
            throw new Exception("Unknown object type");
        }
    }
}