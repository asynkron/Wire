using System;
using System.Collections.Concurrent;
using Wire.ValueSerializers;
using System.Linq;

namespace Wire.Converters
{
    public class ToSurrogateSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            Surrogate surrogate = serializer.Options.Surrogates.FirstOrDefault(s => s.From.IsAssignableFrom(type));
            return surrogate != null;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            Surrogate surrogate = serializer.Options.Surrogates.FirstOrDefault(s => s.From.IsAssignableFrom(type));
            ValueSerializer objectSerializer = new ObjectSerializer(surrogate.To);
            var toSurrogateSerializer = new ToSurrogateSerializer(surrogate.ToSurrogate, objectSerializer);
            typeMapping.TryAdd(type, toSurrogateSerializer);          

            CodeGenerator.BuildSerializer(serializer, surrogate.To, (ObjectSerializer)objectSerializer);
            return toSurrogateSerializer;
        }
    }
}
