using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsOneDimensionalArray();

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        private static void WriteValues<T>(T[] array,Stream stream,Type elementType, ValueSerializer elementSerializer, SerializerSession session)
        {
            stream.WriteInt32(array.Length);
            var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;
            foreach (var value in array)
            {
                stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
            }
        }
        private static void ReadValues<T>(int length, Stream stream, DeserializerSession session, T[] array)
        {
            for (var i = 0; i < length; i++)
            {
                var value = (T)stream.ReadObject(session);
                array[i] = value;
            }
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var arraySerializer = new ObjectSerializer(type);
            
            var elementType = type.GetElementType();
            var elementSerializer = serializer.GetSerializerByType(elementType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            //TODO: code gen this part
            ObjectReader reader = (stream, session) =>
            {
                var length = stream.ReadInt32(session);
                var array = Array.CreateInstance(elementType, length); //create the array
                if (preserveObjectReferences)
                {
                    session.TrackDeserializedObject(array);
                }

                ReadValues(length, stream, session, (dynamic) array);

                return array;
            };
            ObjectWriter writer = (stream, arr, session) =>
            {
                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(arr);
                }

                WriteValues((dynamic) arr, stream, elementType, elementSerializer, session);
 
            };
            arraySerializer.Initialize(reader, writer);
            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
    }
}