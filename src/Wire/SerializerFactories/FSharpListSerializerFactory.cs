// // -----------------------------------------------------------------------
// //   <copyright file="FSharpListSerializerFactory.cs" company="Asynkron HB">
// //       Copyright (C) 2015-2017 Asynkron HB All rights reserved
// //   </copyright>
// // -----------------------------------------------------------------------
//
// using System;
// using System.Buffers;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using FastExpressionCompiler.LightExpression;
// using Wire.Buffers;
// using Wire.Extensions;
// using Wire.ValueSerializers;
//
// namespace Wire.SerializerFactories
// {
//     public class FSharpListSerializerFactory : ValueSerializerFactory
//     {
//         public override bool CanSerialize(Serializer serializer, Type type)
//         {
//             return type.FullName.StartsWith("Microsoft.FSharp.Collections.FSharpList`1");
//         }
//
//         public override bool CanDeserialize(Serializer serializer, Type type)
//         {
//             return CanSerialize(serializer, type);
//         }
//
//         private static Type GetEnumerableType(Type type)
//         {
//             return type
//                 .GetInterfaces()
//                 .Where(
//                     intType =>
//                         intType.IsGenericType &&
//                         intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
//                 .Select(intType => intType.GetGenericArguments()[0])
//                 .FirstOrDefault();
//         }
//
//         private static TypedArray CompileToDelegate(MethodInfo method, Type argType)
//         {
//             var arg = Expression.Parameter(typeof(object));
//             var castArg = Expression.Convert(arg, argType);
//             var call = Expression.Call(method, new Expression[] {castArg});
//             var castRes = Expression.Convert(call, typeof(object));
//             var lambda = Expression.Lambda<TypedArray>(castRes, arg);
//             var compiled = lambda.CompileFast();
//             return compiled;
//         }
//
//         public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
//             ConcurrentDictionary<Type, ValueSerializer> typeMapping)
//         {
//
//
//             var elementType = GetEnumerableType(type);
//             var arrType = elementType.MakeArrayType();
//             var listModule = type.Assembly.GetType("Microsoft.FSharp.Collections.ListModule");
//             var ofArray = listModule.GetMethod("OfArray");
//             var ofArrayConcrete = ofArray.MakeGenericMethod(elementType);
//             var ofArrayCompiled = CompileToDelegate(ofArrayConcrete, arrType);
//             var toArray = listModule.GetMethod("ToArray");
//             var toArrayConcrete = toArray.MakeGenericMethod(elementType);
//             var toArrayCompiled = CompileToDelegate(toArrayConcrete, type);
//             var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
//             var arrSerializer = serializer.GetSerializerByType(arrType);
//             
//             var x = new ObjectSerializer(type);
//             typeMapping.TryAdd(type, x);
//             
//             return x;
//         }
//         
//         private class FSharpListSerializer<T> : ObjectSerializer
//         {
//             private readonly ValueSerializer _elementSerializer;
//             private readonly Type _elementType;
//             private readonly bool _preserveObjectReferences;
//
//             public FSharpListSerializer(bool preserveObjectReferences, Type type, Type elementType,
//                 ValueSerializer elementSerializer) : base(type)
//             {
//                 _preserveObjectReferences = preserveObjectReferences;
//                 _elementType = elementType;
//                 _elementSerializer = elementSerializer;
//             }
//
//             public override object ReadValue(Stream stream, DeserializerSession session)
//             {
//                 var items = (T[]) arrSerializer.ReadValue(stream, session);
//
//                 if (_preserveObjectReferences) session.TrackDeserializedObject(items);
//                 return res;
//             }
//
//             public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
//                 SerializerSession session)
//             {
//                 var typed = (IEnumerable<T>) value;
//                 var arr = typed.ToArray();
//                 
//                 arrSerializer.WriteValue(stream, arr, session);
//                 if (preserveObjectReferences) session.TrackSerializedObject(o);            }
//         }
//     }
// }