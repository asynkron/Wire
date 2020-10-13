using System;
using System.Collections.Concurrent;
using System.IO;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class EnumSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsEnum;

        public override bool CanDeserialize(Serializer serializer, Type type) => type.IsEnum;

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var enumSerializer = new ObjectSerializer(type);

            object Reader(Stream stream, DeserializerSession session)
            {
                var intValue = Int32Serializer.Instance.ReadValue(stream,session);
                return Enum.ToObject(type, intValue);
            }

            void Writer(Stream stream, object enumValue, SerializerSession session)
            {
                Int32Serializer.Instance.WriteValue(stream,enumValue,session);
            }

            enumSerializer.Initialize(Reader, Writer);
            typeMapping.TryAdd(type, enumSerializer);
            return enumSerializer;
        }
    }
}