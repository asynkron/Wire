using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Wire.ValueSerializers;

namespace Wire
{
    public class DefaultTypeResolver : ITypeResolver
    {
        private static readonly ConcurrentDictionary<Type, byte[]> Type2NameMap =
            new ConcurrentDictionary<Type, byte[]>();

        private static readonly ConcurrentDictionary<byte[], Type> Name2TypeMap =
            new ConcurrentDictionary<byte[], Type>(new ByteArrayEqualityComparer());

        private static byte[] GetManifestNameFromType(Type type)
        {
            return Type2NameMap.GetOrAdd(type, t =>
            {
                var typeName = type.GetShortAssemblyQualifiedName();
                var typeNameBytes = Encoding.UTF8.GetBytes(typeName);
                return new[] { ObjectSerializer.ManifestFull }
                    .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                    .Concat(typeNameBytes)
                    .ToArray(); //serializer id 255 + assembly qualified name
            });
        }

        private static Type GetTypeFromManifestName(Stream stream, DeserializerSession session)
        {
            var bytes = (byte[])ByteArraySerializer.Instance.ReadValue(stream, session);

            return Name2TypeMap.GetOrAdd(bytes, b =>
            {
                var shortName = Encoding.UTF8.GetString(b);
                var typename = Utils.ToQualifiedAssemblyName(shortName);
                return Type.GetType(typename, true);
            });
        }

        public void WriteType(Stream stream, Type type, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(type))
            {
                stream.Write(GetManifestNameFromType(type));
            }
            else
            {
                var typeIdentifier = session.GetTypeIdentifier(type);
                stream.Write(new[] { ObjectSerializer.ManifestIndex });
                stream.WriteUInt16((ushort)typeIdentifier);
            }
        }

        public Type ReadType(Stream stream, int firstManifest, DeserializerSession session)
        {
            if (firstManifest == ObjectSerializer.ManifestFull)
            {
                var type = GetTypeFromManifestName(stream, session);
                session.TrackDeserializedType(type);
                return type;
            }
            else
            {
                var typeId = stream.ReadUInt16(session);
                var type = session.GetTypeFromTypeId(typeId);
                return type;
            }
        }
    }
}
