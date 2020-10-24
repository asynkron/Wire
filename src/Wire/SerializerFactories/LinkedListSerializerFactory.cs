// -----------------------------------------------------------------------
//   <copyright file="LinkedListSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;
#pragma warning disable 8321

namespace Wire.SerializerFactories
{
    public class LinkedListSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(LinkedList<>);
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var elementType = type.GetGenericArguments()[0];
            // ReSharper disable once RedundantAssignment
            var elementSerializer = serializer.GetSerializerByType(elementType);
            // ReSharper disable once RedundantAssignment
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            
            var linkedListSerializer = GenericCaller.RunGeneric<ObjectSerializer>(elementType, () =>
            {
                LinkedListSerializer<T> Create<T>() =>
                    new LinkedListSerializer<T>(preserveObjectReferences, type, elementType,
                        elementSerializer);
            });

            typeMapping.TryAdd(type, linkedListSerializer);
            return linkedListSerializer;
        }

        private class LinkedListSerializer<T> : ObjectSerializer
        {
            private readonly ValueSerializer _elementSerializer;
            private readonly Type _elementType;
            private readonly bool _preserveObjectReferences;

            public LinkedListSerializer(bool preserveObjectReferences, Type type, Type elementType,
                ValueSerializer elementSerializer) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _elementType = elementType;
                _elementSerializer = elementSerializer;
            }

            public override object? ReadValue(Stream stream, DeserializerSession session)
            {
                var length = stream.ReadInt32(session);
                var linkedList = new LinkedList<T>();
                if (_preserveObjectReferences) session.TrackDeserializedObject(linkedList);
                for (var i = 0; i < length; i++)
                {
                    var value = (T) stream.ReadObject(session);
                    linkedList.AddLast(value);
                }

                return linkedList;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                if (_preserveObjectReferences) session.TrackSerializedObject(value);
                var linkedList = (LinkedList<T>) value;

                Int32Serializer.WriteValue(ref writer, linkedList.Count);
                foreach (var element in linkedList)
                   writer.WriteObject(element, _elementType, _elementSerializer, _preserveObjectReferences, session);
            }
        }
    }
}