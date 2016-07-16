using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsOneDimensionalArray();
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        private static void WriteValues<T>(IReadOnlyList<T> array,Stream stream,Type elementType, ValueSerializer elementSerializer, SerializerSession session)
        {
            stream.WriteInt32(array.Count);
            var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;
            for (var i = 0; i < array.Count; i++)
            {
                var value = array[i];
                stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
            }
        }
        private static T[] ReadValues<T>(int length, Stream stream, DeserializerSession session, T[] array)
        {
            for (var i = 0; i < length; i++)
            {
                var value = (T)stream.ReadObject(session);
                array[i] = value;
            }
            return array;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var arraySerializer = new ObjectSerializer(type);
            var elementType = type.GetElementType();
            var elementSerializer = serializer.GetSerializerByType(elementType);
            //TODO: code gen this part
            arraySerializer.Initialize((stream, session) =>
            {
                var length = stream.ReadInt32(session);
                var array = Array.CreateInstance(elementType, length); //create the array

                return ReadValues(length, stream, session, (dynamic)array);
            }, (stream, arr, session) =>
            {                
                WriteValues((dynamic)arr,stream,elementType,elementSerializer,session);   
            });
            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
    }
}