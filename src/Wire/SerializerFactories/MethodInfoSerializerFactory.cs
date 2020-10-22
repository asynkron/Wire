// -----------------------------------------------------------------------
//   <copyright file="MethodInfoSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class MethodInfoSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsSubclassOf(typeof(MethodInfo));
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var os = new MethodInfoSerializer(type);
            typeMapping.TryAdd(type, os);
            
            return os;
        }
        
        private class MethodInfoSerializer : ObjectSerializer
        {
            public MethodInfoSerializer(Type type) : base(type)
            {
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var name = stream.ReadString(session);
                var owner = stream.ReadObject(session) as Type;
                var arguments = stream.ReadObject(session) as Type[];


                var method = owner.GetMethod(name,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    CallingConventions.Any, arguments, null);
                return method;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                var method = (MethodInfo) value;
                var name = method.Name;
                var owner = method.DeclaringType;
                var arguments = method.GetParameters().Select(p => p.ParameterType).ToArray();
                StringSerializer.WriteValueImpl(ref writer, name);
                writer.WriteObjectWithManifest(owner, session);
                writer.WriteObjectWithManifest(arguments, session);
            }
        }
    }
}