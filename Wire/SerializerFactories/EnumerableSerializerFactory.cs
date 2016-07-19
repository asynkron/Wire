using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class EnumerableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            //TODO: check for constructor with IEnumerable<T> param

            if (type.GetMethod("AddRange") == null && type.GetMethod("Add") == null)
                return false;

            if (type.GetProperty("Count") == null)
                return false;

            var isGenericEnumerable = GetEnumerableType(type) != null;
            if (isGenericEnumerable)
                return true;

            if (typeof (ICollection).IsAssignableFrom(type))
                return true;

            return false;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        private static Type GetEnumerableType(Type type)
        {
            return type.GetInterfaces()
                .Where(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                .Select(intType => intType.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var x = new ObjectSerializer(type);
            typeMapping.TryAdd(type, x);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            var elementType = GetEnumerableType(type) ?? typeof (object);
            var elementSerializer = serializer.GetSerializerByType(elementType);

            var countProperty = type.GetProperty("Count");
            Func<object, int> countGetter = o => (int)countProperty.GetValue(o);

            ValueWriter writer = (stream, o, session) =>
            {
                stream.WriteInt32(countGetter(o));
                var enumerable = o as IEnumerable;
                // ReSharper disable once PossibleNullReferenceException
                foreach (var value in enumerable)
                {
                    stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
                }
            };

            ValueReader reader = (stream, session) =>
            {
                var count = stream.ReadInt32(session);
                var items = Array.CreateInstance(elementType, count);
                for (var i = 0; i < count; i++)
                {
                    var value = stream.ReadObject(session);
                    items.SetValue(value, i);
                }
                //HACK: this needs to be fixed, codegenerated or whatever
                var instance = Activator.CreateInstance(type);
                var addRange = type.GetMethod("AddRange");
                if (addRange != null)
                {
                    addRange.Invoke(instance, new object[] {items});
                    return instance;
                }
                var add = type.GetMethod("Add");
                if (add != null)
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        add.Invoke(instance, new[] {items.GetValue(i)});
                    }
                }

                return instance;
            };
            x.Initialize(reader, writer);
            return x;
        }
    }
}