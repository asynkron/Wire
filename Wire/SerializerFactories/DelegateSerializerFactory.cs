using System;
using System.Collections.Concurrent;
using System.Reflection;
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
            return CanSerialize(serializer,type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var os = new ObjectSerializer(type);
            typeMapping.TryAdd(type, os);
            ObjectReader reader = (stream, session) =>
            {
                var target = stream.ReadObject(session);
                var method = (MethodInfo)stream.ReadObject(session);
                var del = Delegate.CreateDelegate(type, target, method, true);
                return del;
            };
            ObjectWriter writer = (stream, value, session) =>
            {
                var d = (Delegate)value;
                stream.WriteObjectWithManifest(d.Target,session);
                stream.WriteObjectWithManifest(d.Method,session);
            };
            os.Initialize(reader,writer);
            return os;
        }
    }
}
