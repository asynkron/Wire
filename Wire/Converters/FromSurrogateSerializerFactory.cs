using System;
using System.Collections.Concurrent;
using Wire.ValueSerializers;
using System.Linq;

namespace Wire.Converters
{
    public class FromSurrogateSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            Surrogate surrogate = serializer.Options.Surrogates.FirstOrDefault(s => s.To.IsAssignableFrom(type));
            return surrogate != null;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            Surrogate surrogate = serializer.Options.Surrogates.FirstOrDefault(s => s.To.IsAssignableFrom(type));
            ValueSerializer objectSerializer = new ObjectSerializer(surrogate.To);
            var fromSurrogateSerializer = new FromSurrogateSerializer(surrogate.FromSurrogate, objectSerializer);
            typeMapping.TryAdd(type, fromSurrogateSerializer);

           
            CodeGenerator.BuildSerializer(serializer, surrogate.To, (ObjectSerializer)objectSerializer);
            return fromSurrogateSerializer;
        }
    }
}
