using System;
using System.Collections.Generic;

namespace Wire
{
    public class DeserializerSession
    {
        private readonly byte[] _buffer;
        private readonly Dictionary<int, object> _objectById;
        private readonly Dictionary<object, int> _objects;
        private readonly Dictionary<int, Type> _identifierToType;
        public readonly Serializer Serializer;
        private int _nextObjectId;
        private int _nextTypeId;

        public DeserializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[8];
            _identifierToType = new Dictionary<int, Type>();
            if (serializer.Options.PreserveObjectReferences)
            {
                _objects = new Dictionary<object, int>();
                _objectById = new Dictionary<int, object>();
            }
        }

        public byte[] GetBuffer(int length)
        {
            if (length > _buffer.Length)
                return new byte[length];

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

        public bool TryGetObjectId(object obj, out int objectId)
        {
            return _objects.TryGetValue(obj, out objectId);
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
    }
}