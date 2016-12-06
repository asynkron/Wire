// //-----------------------------------------------------------------------
// // <copyright file="DelegateSerializerFactory.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class DelegateSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.GetTypeInfo().IsSubclassOf(typeof(Delegate));
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var os = new ObjectSerializer(type);
            typeMapping.TryAdd(type, os);
            var methodInfoSerializer = serializer.GetSerializerByType(typeof(MethodInfo));
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            ObjectReader reader = (stream, session) =>
            {
                var target = stream.ReadObject(session);
                var method = (MethodInfo) stream.ReadObject(session);
                var del = method.CreateDelegate(type, target);
                return del;
            };
            ObjectWriter writer = (stream, value, session) =>
            {
                var d = (Delegate) value;
                var method = d.GetMethodInfo();
                stream.WriteObjectWithManifest(d.Target, session);
                //less lookups, slightly faster
                stream.WriteObject(method, type, methodInfoSerializer, preserveObjectReferences, session);
            };
            os.Initialize(reader, writer);
            return os;
        }
    }
}