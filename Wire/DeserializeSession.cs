using System;
using IntToObjectLookup = System.Collections.Generic.Dictionary<int, object>;
using IntToTypeLookup = System.Collections.Generic.Dictionary<int, System.Type>;
using TypeToVersionInfoLookup = System.Collections.Generic.Dictionary<System.Type, Wire.TypeVersionInfo>;
namespace Wire
{
    public class TypeVersionInfo
    {

    }
    public class DeserializerSession
    {
        private byte[] _buffer;
        private readonly IntToTypeLookup _identifierToType;
        private readonly IntToObjectLookup _objectById;
        private readonly TypeToVersionInfoLookup _versionInfoByType;
        public readonly Serializer Serializer;
        private int _nextObjectId;
        private int _nextTypeId;

        public DeserializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[8];
            _identifierToType = new IntToTypeLookup();
            if (serializer.Options.PreserveObjectReferences)
            {
                _objectById = new IntToObjectLookup();
            }
            if (serializer.Options.VersionTolerance)
            {
                _versionInfoByType = new TypeToVersionInfoLookup();
            }
        }

        public byte[] GetBuffer(int length)
        {
            if (length > _buffer.Length)
            {
                _buffer = new byte[length];
            }

            return _buffer;
        }

        public void TrackDeserializedObject(object obj)
        {
            _objectById.Add(_nextObjectId++, obj);
        }

        public object GetDeserializedObject(int id)
        {
            return _objectById[id];
        }

        public void TrackDeserializedType(Type type)
        {
            _identifierToType.Add(_nextTypeId, type);
            _nextTypeId++;
        }

        public Type GetTypeFromTypeId(int typeId)
        {
            return _identifierToType[typeId];
        }

        public void TrackDeserializedTypeWithVersion(Type type, TypeVersionInfo versionInfo)
        {
            TrackDeserializedType(type);
            _versionInfoByType.Add(type, versionInfo);
        }

        public TypeVersionInfo GetVersionInfo(Type type)
        {
            return _versionInfoByType[type];
        }
    }
}