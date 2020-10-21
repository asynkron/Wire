using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using Wire.Buffers;
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
                var bytes = session.GetBuffer(4);
                var intValue = Int32Serializer.ReadValueImpl(stream,bytes);
                return Enum.ToObject(type, intValue);
            }
            void Writer<TBufferWriter>(Writer<TBufferWriter> writer, object enumValue, SerializerSession session) where TBufferWriter:IBufferWriter<byte>
            {
                Int32Serializer.WriteValue(writer,(int)enumValue);
            }

            enumSerializer.Initialize(null, Writer);
            typeMapping.TryAdd(type, enumSerializer);
            return enumSerializer;
        }
    }
}