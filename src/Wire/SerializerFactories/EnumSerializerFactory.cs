using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using Wire.ValueSerializers;
using Wire.ValueSerializers.Optimized;

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
                var bytes = session.GetBuffer(4);
                var intValue = Int32Serializer.ReadValueImpl(stream,bytes);
                return Enum.ToObject(type, intValue);
            }

            static void Writer(IBufferWriter<byte> stream, object enumValue, SerializerSession session)
            {
                Int32Serializer.WriteValue(stream,(int)enumValue);
            }

            enumSerializer.Initialize(Reader, Writer);
            typeMapping.TryAdd(type, enumSerializer);
            return enumSerializer;
        }
    }
}