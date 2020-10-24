using System;
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
            var enumSerializer = new EnumSerializer(type);
            typeMapping.TryAdd(type, enumSerializer);
            return enumSerializer;
        }
        
        private class EnumSerializer : ObjectSerializer
        {
            public EnumSerializer(Type type) : base(type)
            {
            }

            public override object? ReadValue(Stream stream, DeserializerSession session)
            {
                var bytes = session.GetBuffer(4);
                var intValue = Int32Serializer.ReadValueImpl(stream,bytes);
                return Enum.ToObject(Type, intValue);
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                Int32Serializer.WriteValue(ref writer,(int)value);
            }
        }
    }
}