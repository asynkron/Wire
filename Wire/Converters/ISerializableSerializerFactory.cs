using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Wire.ValueSerializers;

namespace Wire.Converters
{
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

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var serializableSerializer = new ObjectSerializer(type);
            ValueReader reader = (stream, session) =>
            {
                var dict = stream.ReadObject(session) as Dictionary<string, object>;
                var info = new SerializationInfo(type,new FormatterConverter());
                foreach (var item in dict)
                {
                    info.AddValue(item.Key,item.Value);
                }

                return null;
            };

            ValueWriter writer = (stream, o, session) =>
            {
                var info = new SerializationInfo(type, new FormatterConverter());
                var serializable = o as ISerializable;
                serializable.GetObjectData(info,new StreamingContext());
                var dict = new Dictionary<string, object>();
                foreach (var item in info)
                {
                    dict.Add(item.Name,item.Value);
                }
                var dictSerializer = serializer.GetSerializerByType(typeof (Dictionary<string, object>));
                stream.WriteObject(dict,typeof(Dictionary<string,object>),dictSerializer,serializer.Options.PreserveObjectReferences,session);
            };
            serializableSerializer.Initialize(reader, writer);
            typeMapping.TryAdd(type, serializableSerializer);
            
            return serializableSerializer;
        }
    }
}
