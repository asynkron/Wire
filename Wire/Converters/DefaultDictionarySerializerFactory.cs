using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public class DefaultDictionarySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return IsInterface(type);
        }

        private static bool IsInterface(Type type)
        {
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof (Dictionary<,>);
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
                var instance = (IDictionary) Activator.CreateInstance(type, count);
                for (var i = 0; i < count; i++)
                {
                    var entry = (DictionaryEntry) stream.ReadObject(session);
                    instance.Add(entry.Key, entry.Value);
                }
                return instance;
            };

            ValueWriter writer = (stream, obj, session) =>
            {
                var dict = obj as IDictionary;
                stream.WriteInt32(dict.Count);
                foreach (var item in dict)
                {
                    stream.WriteObject(item, typeof (DictionaryEntry), elementSerializer,
                        serializer.Options.PreserveObjectReferences, session);
                    // elementSerializer.WriteValue(stream,item,session);
                }
            };
            ser.Initialize(reader, writer);
            return ser;
        }
    }
}