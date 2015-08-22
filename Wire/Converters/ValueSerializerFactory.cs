using System;
using System.Collections.Concurrent;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public abstract class ValueSerializerFactory
    {
        public abstract bool CanSerialize(Serializer Serializer, Type type);
        public abstract bool CanDeserialize(Serializer Serializer, Type type);

        public abstract ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping);
    }
}