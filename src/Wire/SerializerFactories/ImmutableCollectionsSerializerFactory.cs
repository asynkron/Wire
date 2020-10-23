// -----------------------------------------------------------------------
//   <copyright file="ImmutableCollectionsSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;
#pragma warning disable 8321

namespace Wire.SerializerFactories
{
    public class ImmutableCollectionsSerializerFactory : ValueSerializerFactory
    {
        private const string ImmutableCollectionsNamespace = "System.Collections.Immutable";
        private const string ImmutableCollectionsAssembly = "System.Collections.Immutable";

        public override bool CanSerialize(Serializer serializer, Type type)
        {
            if (type.Namespace == null || !type.Namespace.Equals(ImmutableCollectionsNamespace)) return false;
            var isGenericEnumerable = GetEnumerableType(type) != null;
            if (isGenericEnumerable) return true;

            return false;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        private static Type? GetEnumerableType(Type type)
        {
            return type
                .GetInterfaces()
                .Where(
                    intType =>
                        intType.IsGenericType &&
                        intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(intType => intType.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var elementType = GetEnumerableType(type) ?? typeof(object);
            var elementSerializer = serializer.GetSerializerByType(elementType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            var typeName = type.Name;
            var genericSuffixIdx = typeName.IndexOf('`');
            typeName = genericSuffixIdx != -1 ? typeName.Substring(0, genericSuffixIdx) : typeName;
            var creatorType =
                Type.GetType(
                    ImmutableCollectionsNamespace + "." + typeName + ", " + ImmutableCollectionsAssembly, true)!;

            var genericTypes = elementType.IsGenericType
                ? elementType.GetGenericArguments()
                : new[] {elementType};
            
            var createRange = creatorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(methodInfo => methodInfo.Name == "CreateRange" && methodInfo.GetParameters().Length == 1)
                .MakeGenericMethod(genericTypes);
            
            var s = GenericCaller.RunGeneric<ObjectSerializer>(elementType, () =>
            {
                ObjectSerializer Create<T>() =>
                    new ImmutableCollectionSerializer<T>(preserveObjectReferences, type, elementType,
                        elementSerializer, createRange);
            });

            typeMapping.TryAdd(type, s);

            return s;
        }
        
        private class ImmutableCollectionSerializer<T> : ObjectSerializer
        {
            private readonly ValueSerializer _elementSerializer;
            private readonly Type _elementType;
            private readonly bool _preserveObjectReferences;
            private readonly MethodInfo _createRange;

            public ImmutableCollectionSerializer(bool preserveObjectReferences, Type type, Type elementType,
                ValueSerializer elementSerializer, MethodInfo createRange) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _elementType = elementType;
                _elementSerializer = elementSerializer;
                _createRange = createRange;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var count = stream.ReadInt32(session);
                var items = Array.CreateInstance(_elementType, count);
                for (var i = 0; i < count; i++)
                {
                    var value = stream.ReadObject(session);
                    items.SetValue(value, i);
                }

                var instance = _createRange.Invoke(null, new object[] {items})!;
                if (_preserveObjectReferences) session.TrackDeserializedObject(instance);
                return instance;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                if (value is not ICollection enumerable)
                {
                    // object can be IEnumerable but not ICollection i.e. ImmutableQueue
                    var e = (IEnumerable) value;
                    var list = e.Cast<object>().ToList(); //

                    enumerable = list;
                }

                Int32Serializer.WriteValue(ref writer, enumerable.Count);
                foreach (var element in enumerable)
                    writer.WriteObject(element, _elementType, _elementSerializer, _preserveObjectReferences, session);
                if (_preserveObjectReferences) session.TrackSerializedObject(value);
                
            }
        }
    }
}