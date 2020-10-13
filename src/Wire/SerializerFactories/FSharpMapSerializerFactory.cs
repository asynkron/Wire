// -----------------------------------------------------------------------
//   <copyright file="FSharpMapSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class FSharpMapSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.FullName!.StartsWith("Microsoft.FSharp.Collections.FSharpMap`2");
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

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
                .GetInterfaces()
                .Where(
                    intType =>
                        intType.IsGenericType &&
                        intType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .Select(intType => intType.GetGenericArguments()[index])
                .FirstOrDefault();
        }

        private static TypedArray CompileToDelegate(MethodInfo method, Type argType)
        {
            var arg = Expression.Parameter(typeof(object));
            var castArg = Expression.Convert(arg, argType);
            var call = Expression.Call(method, new Expression[] {castArg});
            var castRes = Expression.Convert(call, typeof(object));
            var lambda = Expression.Lambda<TypedArray>(castRes, arg);
            var compiled = lambda.CompileFast();
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

            var mapModule = type.Assembly.GetType("Microsoft.FSharp.Collections.MapModule");
            var ofArray = mapModule.GetMethod("OfArray");
            var ofArrayConcrete = ofArray.MakeGenericMethod(keyType, valueType);
            var ofArrayCompiled = CompileToDelegate(ofArrayConcrete, arrType);

            var toArray = mapModule.GetMethod("ToArray");
            var toArrayConcrete = toArray.MakeGenericMethod(keyType, valueType);
            var toArrayCompiled = CompileToDelegate(toArrayConcrete, type);

            var arrSerializer = serializer.GetSerializerByType(arrType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            void Writer(Stream stream, object o, SerializerSession session)
            {
                var arr = toArrayCompiled(o);
                arrSerializer.WriteValue(stream, arr, session);
                if (preserveObjectReferences) session.TrackSerializedObject(o);
            }

            object Reader(Stream stream, DeserializerSession session)
            {
                var arr = arrSerializer.ReadValue(stream, session);
                var res = ofArrayCompiled(arr);
                return res;
            }

            x.Initialize(Reader, Writer);
            return x;
        }
    }
}