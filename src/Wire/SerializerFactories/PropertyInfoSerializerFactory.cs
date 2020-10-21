// -----------------------------------------------------------------------
//   <copyright file="PropertyInfoSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class PropertyInfoSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsSubclassOf(typeof(PropertyInfo));
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var os = new PropertyInfoSerializer(type);
            typeMapping.TryAdd(type, os);
            return os;
        }

        private class PropertyInfoSerializer : ObjectSerializer
        {
            public PropertyInfoSerializer(Type type) : base(type)
            {
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var name = stream.ReadString(session);
                var owner = stream.ReadObject(session) as Type;

                var property = owner
                    .GetProperty(name,
                        BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return property;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
            {
                var property = (PropertyInfo) value;
                var name = property.Name;
                var owner = property.DeclaringType;
                StringSerializer.WriteValueImpl(writer, name);
                writer.WriteObjectWithManifest(owner, session);
            }
        }
    }
}