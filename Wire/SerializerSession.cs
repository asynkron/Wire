using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        public const int MinBufferSize = 9;
        private readonly Dictionary<object, int> _objects;
        public readonly Serializer Serializer;
        private Dictionary<Type, ushort> _trackedTypes;
        private byte[] _buffer = new byte[MinBufferSize];
        private Type _firstTrackedType;

        private int _length;
        private int _nextObjectId;
        private ushort _nextTypeId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
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
            return !TryGetValue(type, out index);
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length)
                return _buffer;

            length = Math.Max(length, _buffer.Length*2);

            _buffer = new byte[length];

            return _buffer;
        }

        public bool TryGetValue(Type key, out ushort value)
        {
            switch (_length)
            {
                case 0:
                    value = 0;
                    return false;
                case 1:
                    if (key == _firstTrackedType)
                    {
                        value = 0;
                        return true;
                    }
                    value = 0;
                    return false;
                default:
                    return _trackedTypes.TryGetValue(key, out value);
            }
        }

        public void TrackSerializedType(Type type)
        {
            switch (_length)
            {
                case 0:
                    _firstTrackedType = type;
                    _length = 1;
                    break;
                case 1:
                    _trackedTypes = new Dictionary<Type, ushort> {{_firstTrackedType, 0}, {type, _nextTypeId}};
                    _length = 2;
                    break;
                default:
                    _trackedTypes.Add(type, _nextTypeId);
                    _length++;
                    break;
            }
            _nextTypeId++;
        }
    }
}