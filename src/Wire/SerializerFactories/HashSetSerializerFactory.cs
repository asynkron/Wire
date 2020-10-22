// // -----------------------------------------------------------------------
// //   <copyright file="HashSetSerializerFactory.cs" company="Asynkron HB">
// //       Copyright (C) 2015-2017 Asynkron HB All rights reserved
// //   </copyright>
// // -----------------------------------------------------------------------
//
// using System;
// using System.Buffers;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.IO;
// using System.Reflection;
// using Wire.Buffers;
// using Wire.Extensions;
// using Wire.ValueSerializers;
//
// namespace Wire.SerializerFactories
// {
//     public class HashSetSerializerFactory : ValueSerializerFactory
//     {
//         public override bool CanSerialize(Serializer serializer, Type type)
//         {
//             return IsInterface(type);
//         }
//
//         private static bool IsInterface(Type type)
//         {
//             return type.IsGenericType &&
//                    type.GetGenericTypeDefinition() == typeof(HashSet<>);
//         }
//
//         public override bool CanDeserialize(Serializer serializer, Type type)
//         {
//             return IsInterface(type);
//         }
//
//         public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
//             ConcurrentDictionary<Type, ValueSerializer> typeMapping)
//         {
//             var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
//
//             var elementType = type.GetGenericArguments()[0];
//             var elementSerializer = serializer.GetSerializerByType(elementType);
//
//             var ser = new HashSetSerializer<T>(preserveObjectReferences, type,elementType,elementSerializer);
//             typeMapping.TryAdd(type, ser);
//             
//
//             return ser;
//         }
//         
//         private class HashSetSerializer<T> : ObjectSerializer
//         {
//             private readonly ValueSerializer _elementSerializer;
//             private readonly Type _elementType;
//             private readonly bool _preserveObjectReferences;
//
//             public HashSetSerializer(bool preserveObjectReferences, Type type, Type elementType,
//                 ValueSerializer elementSerializer) : base(type)
//             {
//                 _preserveObjectReferences = preserveObjectReferences;
//                 _elementType = elementType;
//                 _elementSerializer = elementSerializer;
//             }
//
//             public override object ReadValue(Stream stream, DeserializerSession session)
//             {
//                 var set = new HashSet<T>();
//                 if (_preserveObjectReferences) session.TrackDeserializedObject(set);
//                 var count = stream.ReadInt32(session);
//                 for (var i = 0; i < count; i++)
//                 {
//                     var item = (T) stream.ReadObject(session);
//                     set.Add(item);
//                 }
//
//                 return set;
//             }
//
//             public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value,
//                 SerializerSession session)
//             {
//                 var hashset = (HashSet<T>) value; 
//                 if (_preserveObjectReferences) session.TrackSerializedObject(hashset);
//                 // ReSharper disable once PossibleNullReferenceException
//                 Int32Serializer.WriteValue(writer, hashset.Count);
//                 foreach (var item in hashset)
//                     writer.WriteObject(item, _elementType, _elementSerializer, _preserveObjectReferences, session);            }
//         }
//     }
// }