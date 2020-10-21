// -----------------------------------------------------------------------
//   <copyright file="ArraySerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsOneDimensionalArray();

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var elementType = type.GetElementType();
            var elementSerializer = serializer.GetSerializerByType(elementType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            
            var arraySerializer = new ArraySerializer<T>(preserveObjectReferences, type, elementType, elementSerializer);
            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
        
        private class ArraySerializer<T> : ObjectSerializer
        {
            private readonly ValueSerializer _elementSerializer;
            private readonly Type _elementType;
            private readonly bool _preserveObjectReferences;

            public ArraySerializer(bool preserveObjectReferences, Type type, Type elementType,
                ValueSerializer elementSerializer) : base(type)
            {
                _preserveObjectReferences = preserveObjectReferences;
                _elementType = elementType;
                _elementSerializer = elementSerializer;
            }

            public override object ReadValue(Stream stream, DeserializerSession session)
            {
                var length = stream.ReadInt32(session);
                var array = new T[length];
                if (_preserveObjectReferences) session.TrackDeserializedObject(array);
                for (var i = 0; i < length; i++)
                {
                    var value = (T) stream.ReadObject(session);
                    array[i]=value;
                }

                return array;
            }

            public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                if (_preserveObjectReferences) session.TrackSerializedObject(value);
                var array = (T[]) value;

                Int32Serializer.WriteValue(writer, array.Length);
                foreach (var element in array)
                    writer.WriteObject(element, _elementType, _elementSerializer, _preserveObjectReferences, session);
            }
        }
    }
}