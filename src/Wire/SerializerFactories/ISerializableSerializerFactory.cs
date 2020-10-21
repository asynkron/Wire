// -----------------------------------------------------------------------
//   <copyright file="ISerializableSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    // ReSharper disable once InconsistentNaming
    public class ISerializableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return typeof(ISerializable).IsAssignableFrom(type);
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var serializableSerializer = new ISerializableSerializer(type);

            return serializableSerializer;
        }
        
        private class ISerializableSerializer : ObjectSerializer
        {
            private readonly ConstructorInfo _ctor;

            public ISerializableSerializer(Type type) : base(type)
            {
                _ctor = Type.GetConstructor(BindingFlagsEx.All, null,
                    new[] {typeof(SerializationInfo), typeof(StreamingContext)}, null)!;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var dict = stream.ReadObject(session) as Dictionary<string, object>;
                var info = new SerializationInfo(Type, new FormatterConverter());
                // ReSharper disable once PossibleNullReferenceException
                foreach (var item in dict) info.AddValue(item.Key, item.Value);
                
                var instance = _ctor.Invoke(new object[] {info, new StreamingContext()});
                var deserializationCallback = instance as IDeserializationCallback;
                deserializationCallback?.OnDeserialization(this);
                return instance;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
            {
                var info = new SerializationInfo(Type, new FormatterConverter());
                var serializable = value as ISerializable;
                serializable!.GetObjectData(info, new StreamingContext());
                var dict = new Dictionary<string, object>();
                foreach (var item in info) dict.Add(item.Name, item!.Value!);
                writer.WriteObjectWithManifest(dict, session);
            }
        }
    }
}