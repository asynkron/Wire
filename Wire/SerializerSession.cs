using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        private readonly byte[] _buffer;
        private readonly Dictionary<int, object> _objectById;
        private readonly Dictionary<object, int> _objects;
        public readonly Serializer Serializer;
        private int _nextObjectId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _buffer = new byte[8];
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

        public void TrackSerializedObject(object obj)
        {
            try
            {
                _objects.Add(obj, _nextObjectId++);
            }
            catch (Exception x)
            {
                throw new Exception($"Error tracking object ",x);
            }
        }

        public void TrackDeserializedObject(object obj)
        {
            _objectById.Add(_nextObjectId++,obj);
        }

        public object GetDeserializedObject(int id)
        {
            return _objectById[id];
        }

        public bool TryGetObjectId(object obj, out int objectId)
        {
            return _objects.TryGetValue(obj, out objectId);
        }
    }
}