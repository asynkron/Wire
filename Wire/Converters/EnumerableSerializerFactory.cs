using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public class EnumerableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer Serializer, Type type)
        {
           // return false;

            if (typeof (ICollection).IsAssignableFrom(type) && type.GetMethod("AddRange") != null)
                return true;

            return false;
        }

        static Type GetEnumerableType(Type type)
        {
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType
                    && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return typeof(object);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            
            var x = new ObjectSerializer(type);
            typeMapping.TryAdd(type, x);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            Type elementType = GetEnumerableType(type);
            var elementSerializer = serializer.GetSerializerByType(elementType);

            x._writer = (stream, o, session) =>
            {

                var enumerable = o as ICollection;

                stream.WriteInt32(enumerable.Count);
                
                foreach (var value in enumerable)
                {
                    stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
                }
            };

            x._reader = (stream, session) =>
            {
                var count = stream.ReadInt32(session);
                var items = Array.CreateInstance(elementType, count);
                for (int i = 0; i < count; i++)
                {
                    var value = stream.ReadObject(session);
                    items.SetValue(value,i);
                }

                var instance = Activator.CreateInstance(type);
                var addRange = type.GetMethod("AddRange");
                addRange.Invoke(instance, new object[] {items});
                return instance;
            };
            return x;
        }
    }
}
