// -----------------------------------------------------------------------
//   <copyright file="DeserializeSession.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using IntToObjectLookup = System.Collections.Generic.List<object>;
using IntToTypeLookup = System.Collections.Generic.List<System.Type>;

namespace Wire
{
    public class
        DeserializerSession : IDisposable
    {
        private readonly IntToObjectLookup _objectById = null!;
        private readonly int _offset;
        public readonly Serializer Serializer;
        private byte[] _buffer = ArrayPool<byte>.Shared.Rent(256);
        private IntToTypeLookup? _identifierToType;

        public DeserializerSession(Serializer serializer)
        {
            Serializer = serializer;
            if (serializer.Options.PreserveObjectReferences) _objectById = new IntToObjectLookup(1);

            _offset = serializer.Options.KnownTypes.Length;
        }

        public byte[] GetBuffer(int length)
        {
            if (length <= _buffer.Length) return _buffer;

            length = Math.Max(length, _buffer.Length * 2);
            
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = ArrayPool<byte>.Shared.Rent(length);

            return _buffer;
        }

        public void TrackDeserializedObject(object obj)
        {
            _objectById.Add(obj);
        }

        public object GetDeserializedObject(int id)
        {
            return _objectById[id];
        }

        public void TrackDeserializedType(Type type)
        {
            _identifierToType ??= new IntToTypeLookup(1);
            _identifierToType.Add(type);
        }

        public Type GetTypeFromTypeId(int typeId)
        {
            if (typeId < _offset) return Serializer.Options.KnownTypes[typeId];
            if (_identifierToType == null) throw new ArgumentException(nameof(typeId));

            return _identifierToType[typeId - _offset];
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
    }
}