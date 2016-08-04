using System;
using JetBrains.Annotations;
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
        public const int MinBufferSize = 8;
        private byte[] _buffer;
        private readonly IntToTypeLookup _identifierToType;
        private readonly IntToObjectLookup _objectById;
        private readonly TypeToVersionInfoLookup _versionInfoByType;
        public readonly Serializer Serializer;

        public DeserializerSession([NotNull] Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[MinBufferSize];
            _identifierToType = new IntToTypeLookup(capacity:1);
            if (serializer.Options.PreserveObjectReferences)
            {
                _objectById = new IntToObjectLookup(capacity:1);
            }
            if (serializer.Options.VersionTolerance)
            {
                _versionInfoByType = new TypeToVersionInfoLookup();
            }
            else
            {
                foreach (var type in serializer.Options.KnownTypes)
                {
                    TrackDeserializedType(type);
                }
            }
        }

        public byte[] GetBuffer(int length)
        {
            if (_buffer != null && length <= _buffer.Length) return _buffer;
            if (_buffer != null)
            {
                length = Math.Max(length, _buffer.Length * 2);
            }

            Array.Resize(ref _buffer,length);

           // _buffer = new byte[length];

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
            _identifierToType.Add(type);
        }

        public Type GetTypeFromTypeId(int typeId)
        {
            return _identifierToType[typeId];
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