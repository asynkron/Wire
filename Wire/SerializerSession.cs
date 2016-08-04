using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        public const int MinBufferSize = 8;
        private byte [] _buffer = new byte[MinBufferSize];
        private readonly Dictionary<object, int> _objects;
        private readonly FastTypeUShortDictionary _typeToIdentifier;
        public readonly Serializer Serializer;
        private int _nextObjectId;
        private ushort _nextTypeId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            _typeToIdentifier = new FastTypeUShortDictionary();
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

        public bool ShouldWriteTypeManifest(Type type, out ushort index)
        {
            return !_typeToIdentifier.TryGetValue(type, out index);
        }

        public void TrackSerializedType(Type type)
        {
            _typeToIdentifier.Add(type, _nextTypeId);
            _nextTypeId++;
        }

       public byte[] GetBuffer(int length)
        {
            if (_buffer != null && length <= _buffer.Length) return _buffer;
            if (_buffer != null)
            {
                length = Math.Max(length, _buffer.Length*2);
            }

            Array.Resize(ref _buffer, length);

            // _buffer = new byte[length];

            return _buffer;
        }
    }
}