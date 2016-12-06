// //-----------------------------------------------------------------------
// // <copyright file="FSharpMapSerializerFactory.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class FSharpMapSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) =>
            type.FullName.StartsWith("Microsoft.FSharp.Collections.FSharpMap`2");

        public override bool CanDeserialize(Serializer serializer, Type type) =>
            CanSerialize(serializer, type);

        private static Type GetKeyType(Type type)
        {
            return GetGenericArgument(type, 0);
        }

        private static Type GetValyeType(Type type)
        {
            return GetGenericArgument(type, 1);
        }

        private static Type GetGenericArgument(Type type, int index)
        {
            return type
                .GetTypeInfo()
                .GetInterfaces()
                .Where(
                    intType =>
                        intType.GetTypeInfo().IsGenericType &&
                        intType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .Select(intType => intType.GetTypeInfo().GetGenericArguments()[index])
                .FirstOrDefault();
        }


        private static TypedArray CompileToDelegate(MethodInfo method, Type argType)
        {
            var arg = Expression.Parameter(typeof(object));
            var castArg = Expression.Convert(arg, argType);
            var call = Expression.Call(method, new Expression[] {castArg});
            var castRes = Expression.Convert(call, typeof(object));
            var lambda = Expression.Lambda<TypedArray>(castRes, arg);
            var compiled = lambda.Compile();
            return compiled;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var x = new ObjectSerializer(type);
            typeMapping.TryAdd(type, x);

            var keyType = GetKeyType(type);
            var valueType = GetValyeType(type);
            var tupleType = typeof(Tuple<,>).MakeGenericType(keyType, valueType);
            var arrType = tupleType.MakeArrayType();

            var mapModule = type.GetTypeInfo().Assembly.GetType("Microsoft.FSharp.Collections.MapModule");
            var ofArray = mapModule.GetTypeInfo().GetMethod("OfArray");
            var ofArrayConcrete = ofArray.MakeGenericMethod(keyType, valueType);
            var ofArrayCompiled = CompileToDelegate(ofArrayConcrete, arrType);

            var toArray = mapModule.GetTypeInfo().GetMethod("ToArray");
            var toArrayConcrete = toArray.MakeGenericMethod(keyType, valueType);
            var toArrayCompiled = CompileToDelegate(toArrayConcrete, type);

            var arrSerializer = serializer.GetSerializerByType(arrType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            ObjectWriter writer = (stream, o, session) =>
            {
                var arr = toArrayCompiled(o);
                arrSerializer.WriteValue(stream, arr, session);
                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(o);
                }
            };

            ObjectReader reader = (stream, session) =>
            {
                var arr = arrSerializer.ReadValue(stream, session);
                var res = ofArrayCompiled(arr);
                return res;
            };
            x.Initialize(reader, writer);
            return x;
        }
    }
}