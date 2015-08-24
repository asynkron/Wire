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
        public int NextObjectId;

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
            if (length <= _buffer.Length)
                return _buffer;
            return new byte[length];
        }

        public void TrackSerializedObject(object obj)
        {
            try
            {
                _objects.Add(obj, NextObjectId++);
            }
            catch (Exception x)
            {
                throw new Exception($"Error tracking object ",x);
            }
        }

        public void TrackDeserializedObject(object obj)
        {
            _objectById.Add(NextObjectId++,obj);
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