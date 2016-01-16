using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class FSharpListSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.FullName.StartsWith("Microsoft.FSharp.Collections.FSharpList`1");
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        private static Type GetEnumerableType(Type type)
        {
            return type.GetInterfaces()
                .Where(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(intType => intType.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var x = new ObjectSerializer(type);
            typeMapping.TryAdd(type, x);

            var elementType = GetEnumerableType(type);
            var arrType = elementType.MakeArrayType();
            var listModule = type.Assembly.GetType("Microsoft.FSharp.Collections.ListModule");
            var method = listModule.GetMethod("OfArray");

            ValueWriter writer = (stream, o, session) =>
            {
                var e = (IEnumerable) o;
                var list = new ArrayList();
                foreach (var element in e)
                    list.Add(element);

                var arr = list.ToArray(elementType);

                var arrSerializer = serializer.GetSerializerByType(arrType);
                arrSerializer.WriteValue(stream,arr,session);
            };

            ValueReader reader = (stream, session) =>
            {
               
                var arrSerializer = serializer.GetSerializerByType(arrType);

                var items = (Array)arrSerializer.ReadValue(stream, session);
             
                //TODO: codegen this
                var concrete = method.MakeGenericMethod(elementType);
                var res = concrete.Invoke(null, new object[] { items});

                return res;
            };
            x.Initialize(reader, writer);
            return x;
        }
    }
}