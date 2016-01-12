using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class DictionarySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return IsInterface(type);
        }

        private static bool IsInterface(Type type)
        {
            return type
                .GetInterfaces()
                .Select(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IDictionary<,>))
                .Any(isDict => isDict);
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return IsInterface(type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var ser = new ObjectSerializer(type);
            var elementSerializer = serializer.GetSerializerByType(typeof (DictionaryEntry));

            ValueReader reader = (stream, session) =>
            {
                var count = stream.ReadInt32(session);
                var entries = new DictionaryEntry[count];
                for (var i = 0; i < count; i++)
                {
                    var entry = (DictionaryEntry) stream.ReadObject(session);
                    entries[i] = entry;
                }
                return null;
            };

            ValueWriter writer = (stream, obj, session) =>
            {
                var dict = obj as IDictionary;
                // ReSharper disable once PossibleNullReferenceException
                stream.WriteInt32(dict.Count);
                foreach (var item in dict)
                {
                    stream.WriteObject(item, typeof (DictionaryEntry), elementSerializer,
                        serializer.Options.PreserveObjectReferences, session);
                    // elementSerializer.WriteValue(stream,item,session);
                }
            };
            ser.Initialize(reader, writer);
            typeMapping.TryAdd(type, ser);
            return ser;
        }
    }
}