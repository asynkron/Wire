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
            var ofArray = listModule.GetMethod("OfArray");
            var ofArrayConcrete = ofArray.MakeGenericMethod(elementType);
            var toArray = listModule.GetMethod("ToArray");
            var toArrayConcrete = toArray.MakeGenericMethod(elementType);

            ValueWriter writer = (stream, o, session) =>
            {
                //TODO: codegen this
                var arr = toArrayConcrete.Invoke(null, new[] {o});
                var arrSerializer = serializer.GetSerializerByType(arrType);
                arrSerializer.WriteValue(stream,arr,session);
            };

            ValueReader reader = (stream, session) =>
            {               
                var arrSerializer = serializer.GetSerializerByType(arrType);
                var items = (Array)arrSerializer.ReadValue(stream, session);             
                //TODO: codegen this                
                var res = ofArrayConcrete.Invoke(null, new object[] { items});
                return res;
            };
            x.Initialize(reader, writer);
            return x;
        }
    }
}