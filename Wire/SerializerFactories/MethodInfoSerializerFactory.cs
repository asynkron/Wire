using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class MethodInfoSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.GetTypeInfo().IsSubclassOf(typeof(MethodInfo));
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
            ObjectReader reader = (stream, session) =>
            {
                var name = stream.ReadString(session);
                var owner = stream.ReadObject(session) as Type;
                var arguments = stream.ReadObject(session) as Type[];

                var method = owner.GetTypeInfo().GetMethod(
                    name,
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, 
                    null,
                    CallingConventions.Any,
                    arguments,
                    null);
                return method;
            };
            ObjectWriter writer = (stream, obj, session) =>
            {
                var method = (MethodInfo) obj;
                var name = method.Name;
                var owner = method.DeclaringType;
                var arguments = method.GetParameters().Select(p => p.ParameterType).ToArray();

                stream.WriteString(name);
                stream.WriteObjectWithManifest(owner, session);
                stream.WriteObjectWithManifest(arguments, session);
            };
            os.Initialize(reader, writer);

            return os;
        }
    }
}