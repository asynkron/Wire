// -----------------------------------------------------------------------
//   <copyright file="ConstructorInfoSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ConstructorInfoSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsSubclassOf(typeof(ConstructorInfo));

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var constructorInfoSerializer = new ConstructorInfoSerializer(type);
            typeMapping.TryAdd(type, constructorInfoSerializer);

            return constructorInfoSerializer;
        }
        
        private class ConstructorInfoSerializer : ObjectSerializer
        {
            public ConstructorInfoSerializer(Type type) : base(type)
            {
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var owner = stream.ReadObject(session) as Type;
                var arguments = stream.ReadObject(session) as Type[];
                var ctor = owner.GetConstructor(arguments);
                return ctor;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
            {
                var ctor = (ConstructorInfo) value;
                var owner = ctor.DeclaringType;
                var arguments = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                writer.WriteObjectWithManifest(owner, session);
                writer.WriteObjectWithManifest(arguments, session);
            }
        }
    }
}