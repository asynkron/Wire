using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wire.ValueSerializers;

namespace Wire.Converters
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

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var x = type
                .GetInterfaces()
                .First(t => (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IDictionary<,>)));
            Type keyType = x.GetGenericArguments()[0];
            Type valueType = x.GetGenericArguments()[1];

            var ser =  new ObjectSerializer(type);
            ValueReader reader = null;
            ValueWriter writer = null;
            ser.Initialize(reader,writer);
            return ser;
        }

        private void DoStuff<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            
        }
    }
}
