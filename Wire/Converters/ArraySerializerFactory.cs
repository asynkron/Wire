using System;
using System.Collections.Concurrent;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return type.IsArray;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var arraySerializer = new ObjectSerializer(type);
            var elementType = type.GetElementType();
            arraySerializer.Initialize((stream, session) =>
            {
                var length = stream.ReadInt32(session);
                var array = Array.CreateInstance(elementType, length); //create the array

                for (var i = 0; i < length; i++)
                {
                    var value = stream.ReadObject(session);
                    array.SetValue(value, i); //set the element value
                }
                return array;
            }, (stream, arr, session) =>
            {
                var array = arr as Array;
                var elementSerializer = session.Serializer.GetSerializerByType(elementType);
                stream.WriteInt32(array.Length);
                var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;

                for (var i = 0; i < array.Length; i++) //write the elements
                {
                    var value = array.GetValue(i);
                    stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
                }
            });
            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
    }
}