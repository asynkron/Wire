// -----------------------------------------------------------------------
//   <copyright file="DelegateSerializerFactory.cs" company="Asynkron HB">
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
    public class DelegateSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsSubclassOf(typeof(Delegate));

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var methodInfoSerializer = serializer.GetSerializerByType(typeof(MethodInfo));
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            var delegateSerializer = new DelegateSerializer(preserveObjectReferences,methodInfoSerializer,type);
            typeMapping.TryAdd(type, delegateSerializer);
            return delegateSerializer;
        }
        
        private class DelegateSerializer : ObjectSerializer
        {
            private bool _preserveObjectReferences;
            private ValueSerializer _methodInfoSerializer;

            public DelegateSerializer(bool preserveObjectReferences, ValueSerializer methodInfoSerializer, Type type) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _methodInfoSerializer = methodInfoSerializer;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var target = stream.ReadObject(session);
                var method = (MethodInfo) stream.ReadObject(session);
                var @delegate = method.CreateDelegate(Type, target);
                return @delegate;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
            {
                var d = (Delegate) value;
                var method = d.GetMethodInfo();
                writer.WriteObjectWithManifest(d.Target, session);
                //less lookups, slightly faster
                writer.WriteObject(method, Type, _methodInfoSerializer, _preserveObjectReferences, session);
            }
        }
    }
}