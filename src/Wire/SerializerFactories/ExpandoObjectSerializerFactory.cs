// -----------------------------------------------------------------------
//   <copyright file="ExpandoObjectSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ExpandoObjectSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type == typeof(ExpandoObject);

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            var elementSerializer = serializer.GetSerializerByType(typeof(DictionaryEntry));
            var expandoObjectSerializer = new ExpandoObjectSerializer(preserveObjectReferences, elementSerializer,type);
            typeMapping.TryAdd(type, expandoObjectSerializer);
            return expandoObjectSerializer;
        }
        
        private class ExpandoObjectSerializer : ObjectSerializer
        {
            private readonly bool _preserveObjectReferences;
            private readonly ValueSerializer _elementSerializer;

            public ExpandoObjectSerializer(bool preserveObjectReferences, ValueSerializer elementSerializer ,Type type) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _elementSerializer = elementSerializer;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var instance = (IDictionary<string, object>) Activator.CreateInstance(Type)!;

                if (_preserveObjectReferences) session.TrackDeserializedObject(instance);
                var count = stream.ReadInt32(session);
                for (var i = 0; i < count; i++)
                {
                    var entry = (KeyValuePair<string, object>) stream.ReadObject(session);
                    instance.Add(entry);
                }

                return instance;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
            {
                if (_preserveObjectReferences) session.TrackSerializedObject(value);
                var dict = (IDictionary<string, object>) value;
                // ReSharper disable once PossibleNullReferenceException
                Int32Serializer.WriteValue(writer, dict.Count);
                foreach (var item in dict)
                    writer.WriteObject(item, typeof(DictionaryEntry), _elementSerializer,
                        _preserveObjectReferences, session);
                // elementSerializer.WriteValue(stream,item,session);
            }
        }
    }
}