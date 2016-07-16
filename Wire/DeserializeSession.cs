using System;
using IntToObjectLookup = System.Collections.Generic.Dictionary<int, object>;
using IntToTypeLookup = System.Collections.Generic.Dictionary<int, System.Type>;
namespace Wire
{
    public class DeserializerSession
    {
        private readonly byte[] _buffer;
        private readonly IntToTypeLookup _identifierToType;
        private readonly IntToObjectLookup _objectById;
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