using System;
using Wire.Internal;
using IntToObjectLookup = System.Collections.Generic.List<object>;
using IntToTypeLookup = System.Collections.Generic.List<System.Type>;
using TypeToVersionInfoLookup = System.Collections.Generic.Dictionary<System.Type, Wire.TypeVersionInfo>;
namespace Wire
{
    public class TypeVersionInfo
    {

    }
    public class 
        DeserializerSession
    {
        public const int MinBufferSize = 9;
        private byte[] _buffer;
        private IntToTypeLookup _identifierToType;
        private readonly IntToObjectLookup _objectById;
        private readonly TypeToVersionInfoLookup _versionInfoByType;
        public readonly Serializer Serializer;
        private readonly int _offset;

        public DeserializerSession([NotNull] Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[MinBufferSize];
            if (serializer.Options.PreserveObjectReferences)
            {
                _objectById = new IntToObjectLookup(capacity:1);
            }
            if (serializer.Options.VersionTolerance)
            {
                _versionInfoByType = new TypeToVersionInfoLookup();
            }
            _offset = serializer.Options.KnownTypes.Length;
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length)
                return _buffer;
           
            length = Math.Max(length, _buffer.Length * 2);

            _buffer = new byte[length];

            return _buffer;
        }

        public void TrackDeserializedObject([NotNull]object obj)
        {
            _objectById.Add(obj);
        }

        public object GetDeserializedObject(int id)
        {
            return _objectById[id];
        }

        public void TrackDeserializedType([NotNull]Type type)
        {
            if (_identifierToType == null)
            {
                _identifierToType = new IntToTypeLookup(capacity:1);
            }
            _identifierToType.Add(type);
        }

        public Type GetTypeFromTypeId(int typeId)
        {

            if (typeId < _offset)
            {
                return Serializer.Options.KnownTypes[typeId];
            }
            if (_identifierToType == null)
                throw new ArgumentException(nameof(typeId));

            return _identifierToType[typeId - _offset];
        }

        public void TrackDeserializedTypeWithVersion([NotNull]Type type, [NotNull] TypeVersionInfo versionInfo)
        {
            TrackDeserializedType(type);
            _versionInfoByType.Add(type, versionInfo);
        }

        public TypeVersionInfo GetVersionInfo([NotNull]Type type)
        {
            return _versionInfoByType[type];
        }
    }
}