using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        private readonly Dictionary<object, int> _objects;
        private readonly Dictionary<Type, int> _typeToIdentifier;
        public readonly Serializer Serializer;
        private int _nextObjectId;
        private int _nextTypeId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _typeToIdentifier = new Dictionary<Type, int>();
            if (serializer.Options.PreserveObjectReferences)
            {
                _objects = new Dictionary<object, int>();
            }
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
            
        public bool TryGetObjectId(object obj, out int objectId)
        {
            return _objects.TryGetValue(obj, out objectId);
        }

        public bool ShouldWriteTypeManifest(Type type)
        {
            if (!_typeToIdentifier.ContainsKey(type))
            {
                _typeToIdentifier.Add(type, _nextTypeId);
                _nextTypeId++;
                return true;
            }
            return false;
        }

        public int GetTypeIdentifier(Type type)
        {
            return _typeToIdentifier[type];
        }
    }
}