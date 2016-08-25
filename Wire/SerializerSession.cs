using System;
using System.Collections.Generic;

namespace Wire
{
    public class SerializerSession
    {
        public const int MinBufferSize = 9;
        private readonly Dictionary<object, int> _objects;
        public readonly Serializer Serializer;
        private readonly LinkedList<Type> _trackedTypes = new LinkedList<Type>();
        private byte[] _buffer = new byte[MinBufferSize];

        private int _nextObjectId;
        private readonly ushort _nextTypeId;

        public SerializerSession(Serializer serializer)
        {
            Serializer = serializer;
            if (serializer.Options.PreserveObjectReferences)
            {
                _objects = new Dictionary<object, int>();
            }
            _nextTypeId = (ushort)(serializer.Options.KnownTypes.Length );
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
            if (_trackedTypes.Count == 0)
            {
                value = 0;
                return false;
            }

            ushort j = _nextTypeId;
            foreach (var t in _trackedTypes)
            {
                if (key == t)
                {
                    value = j;
                    return true;
                }
                j++;
            }
            //for (ushort i = 0; i < _trackedTypes.Count; i++)
            //{
            //    var t = _trackedTypes[i];
            //    if (t != key) continue;
            //    value = (ushort)(i + _nextTypeId);
            //    return true;
            //}

            value = 0;
            return false;
        }

        public void TrackSerializedType(Type type)
        {
            _trackedTypes.AddLast(type);
           // _trackedTypes.Add(type);
        }
    }
}