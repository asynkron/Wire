using System;
using System.Collections.Concurrent;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsArray;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            ValueSerializer arraySerializer = new ArraySerializer(type);
            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
    }
}