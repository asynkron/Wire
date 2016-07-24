using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ByteArrayTypeLookup = System.Collections.Concurrent.ConcurrentDictionary<byte[], System.Type>;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        public const byte ManifestVersion = 251;
        public const byte ManifestFull = 255;
        public const byte ManifestIndex = 254;

        private static readonly ByteArrayTypeLookup TypeNameLookup =
            new ByteArrayTypeLookup(new ByteArrayEqualityComparer());

        private readonly byte[] _manifest;
        private readonly byte[] _manifestWithVersionInfo;

        private volatile bool _isInitialized;
        private ObjectReader _reader;
        private ObjectWriter _writer;

        private byte[] GetTypeManifest(IReadOnlyCollection<byte[]> fieldNames)
        {
            IEnumerable<byte> result = new[] { (byte)fieldNames.Count };
            foreach (var name in fieldNames)
            {
                var encodedLength = BitConverter.GetBytes(name.Length);
                result = result.Concat(encodedLength);
                result = result.Concat(name);
            }
            var versionTolerantHeader = result.ToArray();
            return versionTolerantHeader;
        }

        public ObjectSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
            //TODO: remove version info
            var typeName = type.GetShortAssemblyQualifiedName();
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once AssignNullToNotNullAttribute
            var typeNameBytes = Utils.StringToBytes(typeName);

            var fields = ReflectionEx.GetFieldInfosForType(type);
            var fieldNames = fields.Select(field => Utils.StringToBytes(field.Name)).ToList();
            var versionInfo = GetTypeManifest(fieldNames);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new[] {ManifestFull}
                    .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                    .Concat(typeNameBytes)
                    .ToArray(); //serializer id 255 + assembly qualified name

            //this is the same as the above, but including all field names of the type, in alphabetical order
            _manifestWithVersionInfo =
                new[] { ManifestVersion }
                    .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                    .Concat(typeNameBytes)
                    .Concat(versionInfo)
                    .ToArray(); //serializer id 255 + assembly qualified name + versionInfo

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

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(Type))
            {
                session.TrackSerializedType(Type);
                if (session.Serializer.Options.VersionTolerance)
                    stream.Write(_manifestWithVersionInfo);
                else
                    stream.Write(_manifest);
            }
            else
            {
                var typeIdentifier = session.GetTypeIdentifier(Type);
                stream.Write(new[] {ManifestIndex});
                stream.WriteUInt16((ushort) typeIdentifier);
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

        public void Initialize(ObjectReader reader, ObjectWriter writer)
        {
            _reader = reader;
            _writer = writer;
            _isInitialized = true;
        }

        private static Type GetTypeFromManifestName(Stream stream, DeserializerSession session)
        {
            var bytes = stream.ReadLengthEncodedByteArray(session);

            return TypeNameLookup.GetOrAdd(bytes, b =>
            {
                var shortName = Utils.BytesToString(b, 0, b.Length);
                var typename = Utils.ToQualifiedAssemblyName(shortName);
                return Type.GetType(typename, true);
            });
        }

        public static Type GetTypeFromManifestFull(Stream stream, DeserializerSession session)
        {
            var type = GetTypeFromManifestName(stream, session);
            session.TrackDeserializedType(type);
            return type;
        }

        public static Type GetTypeFromManifestVersion(Stream stream, DeserializerSession session)
        {
            var type = GetTypeFromManifestName(stream, session);

            var fieldCount = stream.ReadByte();
            for (int i = 0; i < fieldCount; i++)
            {
                var fieldName = stream.ReadLengthEncodedByteArray(session);

            }

            session.TrackDeserializedTypeWithVersion(type, null);
            return type;
        }

        public static Type GetTypeFromManifestIndex(Stream stream, DeserializerSession session)
        {
            var typeId = stream.ReadUInt16(session);
            var type = session.GetTypeFromTypeId(typeId);
            return type;
        }
    }
}