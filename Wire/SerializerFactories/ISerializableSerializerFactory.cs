using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    // ReSharper disable once InconsistentNaming
    public class ISerializableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return typeof (ISerializable).IsAssignableFrom(type);
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var serializableSerializer = new ObjectSerializer(type);
            typeMapping.TryAdd(type, serializableSerializer);
            ValueReader reader = (stream, session) =>
            {
                var dict = stream.ReadObject(session) as Dictionary<string, object>;
                var info = new SerializationInfo(type, new FormatterConverter());
                // ReSharper disable once PossibleNullReferenceException
                foreach (var item in dict)
                {
                    info.AddValue(item.Key, item.Value);
                }

                // protected Dictionary(SerializationInfo info, StreamingContext context);
                var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default;
                var ctor = type.GetConstructor(flags, null,
                    new[] {typeof (SerializationInfo), typeof (StreamingContext)}, null);
                var instance = ctor.Invoke(new object[] {info, new StreamingContext()});
                var deserializationCallback = instance as IDeserializationCallback;
                deserializationCallback?.OnDeserialization(this);
                return instance;
            };

            ValueWriter writer = (stream, o, session) =>
            {
                var info = new SerializationInfo(type, new FormatterConverter());
                var serializable = o as ISerializable;
                // ReSharper disable once PossibleNullReferenceException
                serializable.GetObjectData(info, new StreamingContext());
                var dict = new Dictionary<string, object>();
                foreach (var item in info)
                {
                    dict.Add(item.Name, item.Value);
                }
                var dictSerializer = serializer.GetSerializerByType(typeof (Dictionary<string, object>));
                stream.WriteObject(dict, typeof (Dictionary<string, object>), dictSerializer,
                    serializer.Options.PreserveObjectReferences, session);
            };
            serializableSerializer.Initialize(reader, writer);
            

            return serializableSerializer;
        }
    }
}