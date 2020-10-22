// // -----------------------------------------------------------------------
// //   <copyright file="EnumerableSerializerFactory.cs" company="Asynkron HB">
// //       Copyright (C) 2015-2017 Asynkron HB All rights reserved
// //   </copyright>
// // -----------------------------------------------------------------------
//
// using System;
// using System.Collections;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Wire.Buffers;
// using Wire.Extensions;
// using Wire.ValueSerializers;
//
// namespace Wire.SerializerFactories
// {
//     public class EnumerableSerializerFactory : ValueSerializerFactory
//     {
//         public override bool CanSerialize(Serializer serializer, Type type)
//         {
//             //TODO: check for constructor with IEnumerable<T> param
//
//             if (!type.GetMethods().Any(m => m.Name == "AddRange" || m.Name == "Add")) return false;
//
//             if (type.GetProperty("Count") == null) return false;
//
//             var isGenericEnumerable = GetEnumerableType(type) != null;
//             if (isGenericEnumerable) return true;
//
//             if (typeof(ICollection).IsAssignableFrom(type)) return true;
//
//             return false;
//         }
//
//         public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);
//
//         private static Type? GetEnumerableType(Type type) =>
//             type
//                 .GetInterfaces()
//                 .Where(
//                     intType =>
//                         intType.IsGenericType &&
//                         intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
//                 .Select(intType => intType.GetGenericArguments()[0])
//                 .FirstOrDefault();
//
//         public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
//             ConcurrentDictionary<Type, ValueSerializer> typeMapping)
//         {
//             var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
//
//             var elementType = GetEnumerableType(type) ?? typeof(object);
//             var elementSerializer = serializer.GetSerializerByType(elementType);
//
//             var enumerableSerializer = new EnumerableSerializer<T>(preserveObjectReferences,elementType,elementSerializer,type);
//             typeMapping.TryAdd(type, enumerableSerializer);
//
//             return enumerableSerializer;
//         }
//         
//         private class EnumerableSerializer<T> : ObjectSerializer
//         {
//             private readonly bool _preserveObjectReferences;
//             private readonly ValueSerializer _elementSerializer;
//             private readonly Type _elementType;
//
//             public EnumerableSerializer(bool preserveObjectReferences, Type elementType, ValueSerializer elementSerializer ,Type type) : base(type)
//             {
//                 _preserveObjectReferences = preserveObjectReferences;
//                 _elementSerializer = elementSerializer;
//                 _elementType = elementType;
//             }
//
//             public override object ReadValue(Stream stream, DeserializerSession session)
//             {
//                 var instance = Activator.CreateInstance(Type)!;
//                 if (_preserveObjectReferences) session.TrackDeserializedObject(instance);
//
//                 var count = stream.ReadInt32(session);
//
//                 if (addRange != null)
//                 {
//                     var items = Array.CreateInstance(elementType, count);
//                     for (var i = 0; i < count; i++)
//                     {
//                         var value = stream.ReadObject(session);
//                         items.SetValue(value, i);
//                     }
//                     //HACK: this needs to be fixed, codegenerated or whatever
//
//                     addRange.Invoke(instance, new object[] {items});
//                     return instance;
//                 }
//
//                 if (add != null)
//                     for (var i = 0; i < count; i++)
//                     {
//                         var value = stream.ReadObject(session);
//                         add.Invoke(instance, new[] {value});
//                     }
//
//                 return instance;
//             }
//
//             public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
//             {
//                 if (_preserveObjectReferences) session.TrackSerializedObject(value);
//                 IEnumerable<T> typed = (IEnumerable<T>)value;
//                 Int32Serializer.WriteValue(writer, typed.Count());
//
//                 // ReSharper disable once PossibleNullReferenceException
//                 foreach (var element in typed)
//                     writer.WriteObject(element, _elementType, _elementSerializer, _preserveObjectReferences, session);
//
//             }
//         }
//     }
// }