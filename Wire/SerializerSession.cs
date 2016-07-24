using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        private byte[] _buffer;
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
            if (!serializer.Options.VersionTolerance)
            {
                //known types can only be used when version intolerant as we lack type information
                foreach (var type in serializer.Options.KnownTypes)
                {
                    TrackSerializedType(type);
                }
            }
        }

        public byte[] GetBuffer(int length)
        {
            if (length < 8)
                length = 8;

            if (_buffer == null || length > _buffer.Length)
            {
                _buffer = new byte[length];
            }

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
                throw new Exception("Error tracking object ", x);
            }
        }

        public bool TryGetObjectId(object obj, out int objectId)
        {
            return _objects.TryGetValue(obj, out objectId);
        }

        public bool ShouldWriteTypeManifest(Type type,out int index)
        {
            return !_typeToIdentifier.TryGetValue(type, out index);
        }

        public void TrackSerializedType(Type type)
        {
            _typeToIdentifier.Add(type, _nextTypeId);
            _nextTypeId++;
        }

        public int GetTypeIdentifier(Type type)
        {
            return _typeToIdentifier[type];
        }
    }
}