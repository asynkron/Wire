// -----------------------------------------------------------------------
//   <copyright file="FieldInfoSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class FieldInfoSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsSubclassOf(typeof(FieldInfo));

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var fieldInfoSerializer = new FieldInfoSerializer(type);
            typeMapping.TryAdd(type, fieldInfoSerializer);

            return fieldInfoSerializer;
        }
        
        private class FieldInfoSerializer : ObjectSerializer
        {
            public FieldInfoSerializer(Type type) : base(type)
            {
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var name = stream.ReadString(session);
                var owner = stream.ReadObject(session) as Type;
                
                var field = owner!
                    .GetField(name!,
                        BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
                return field;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                var field = (FieldInfo) value;
                var name = field.Name;
                var owner = field.DeclaringType;
                StringSerializer.WriteValueImpl(writer, name);
                writer.WriteObjectWithManifest(owner, session);
            }
        }
    }
}