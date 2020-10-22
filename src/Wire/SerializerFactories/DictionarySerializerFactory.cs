// -----------------------------------------------------------------------
//   <copyright file="DefaultDictionarySerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class DictionarySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => IsDictionary(type);

        private static bool IsDictionary(Type type) => 
            type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        public override bool CanDeserialize(Serializer serializer, Type type) => IsDictionary(type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var elementSerializer = serializer.GetSerializerByType(typeof(DictionaryEntry));
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            var ser = new DictionarySerializer(preserveObjectReferences,elementSerializer, type);
            typeMapping.TryAdd(type, ser);

            return ser;
        }
        
        private class DictionarySerializer : ObjectSerializer
        {
            private readonly bool _preserveObjectReferences;
            private readonly ValueSerializer _elementSerializer;

            public DictionarySerializer(bool preserveObjectReferences, ValueSerializer elementSerializer ,Type type) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _elementSerializer = elementSerializer;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var count = stream.ReadInt32(session);
                var instance = (IDictionary) Activator.CreateInstance(Type, count)!;
                if (_preserveObjectReferences) session.TrackDeserializedObject(instance);

                for (var i = 0; i < count; i++)
                {
                    var entry = (DictionaryEntry) stream.ReadObject(session);
                    instance.Add(entry.Key, entry.Value);
                }

                return instance;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                if (_preserveObjectReferences) session.TrackSerializedObject(value);
                var dict = value as IDictionary;
                // ReSharper disable once PossibleNullReferenceException
                Int32Serializer.WriteValue(writer, dict.Count);
                foreach (DictionaryEntry item in dict)
                    writer.WriteObject(item, typeof(DictionaryEntry), _elementSerializer,
                        _preserveObjectReferences, session);
            }
        }
    }
}