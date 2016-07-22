using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ArraySerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => type.IsOneDimensionalArray();

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        // WriteValues

        private static MethodInfo s_writeValuesMethod = typeof(ArraySerializerFactory).GetMethod("WriteValues", BindingFlags.NonPublic | BindingFlags.Static);

        private delegate void WriteValuesDelegate(object obj, Stream stream, Type elementType, ValueSerializer elementSerializer, SerializerSession session);

        private static void WriteValues<T>(object obj, Stream stream, Type elementType, ValueSerializer elementSerializer, SerializerSession session)
        {
            var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;
            if (preserveObjectReferences)
            {
                session.TrackSerializedObject(obj);
            }
            var array = (T[])obj;
            stream.WriteInt32(array.Length);
            foreach (var value in array)
            {
                stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
            }
        }

        private static void WriteValuesNonGeneric(object obj, Stream stream, Type elementType, ValueSerializer elementSerializer, SerializerSession session)
        {
            var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;
            if (preserveObjectReferences)
            {
                session.TrackSerializedObject(obj);
            }
            var array = (Array)obj;
            stream.WriteInt32(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                var value = array.GetValue(i);
                stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
            }
        }

        // ReadValues

        private static MethodInfo s_readValuesMethod = typeof(ArraySerializerFactory).GetMethod("ReadValues", BindingFlags.NonPublic | BindingFlags.Static);

        private delegate object ReadValuesDelegate(Stream stream, DeserializerSession session);

        private static object ReadValues<T>(Stream stream, DeserializerSession session)
        {
            var length = stream.ReadInt32(session);
            var array = new T[length];
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(array);
            }
            for (var i = 0; i < length; i++)
            {
                var value = (T)stream.ReadObject(session);
                array[i] = value;
            }
            return array;
        }

        private static object ReadValuesNonGeneric(Stream stream, Type elementType, DeserializerSession session)
        {
            var length = stream.ReadInt32(session);
            var array = Array.CreateInstance(elementType, length);
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(array);
            }
            for (var i = 0; i < length; i++)
            {
                var value = stream.ReadObject(session);
                array.SetValue(value, i);
            }
            return array;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var arraySerializer = new ObjectSerializer(type);
            var elementType = type.GetElementType();
            var elementSerializer = serializer.GetSerializerByType(elementType);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            if (serializer.Options.UseDynamicCode)
            {
                var writeValues = (WriteValuesDelegate)s_writeValuesMethod.MakeGenericMethod(elementType).CreateDelegate(typeof(WriteValuesDelegate));
                var readValues = (ReadValuesDelegate)s_readValuesMethod.MakeGenericMethod(elementType).CreateDelegate(typeof(ReadValuesDelegate));
                arraySerializer.Initialize(
                    (stream, session) => readValues(stream, session),
                    (stream, obj, session) => writeValues(obj, stream, elementType, elementSerializer, session));
            }
            else
            {
                arraySerializer.Initialize(
                    (stream, session) => ReadValuesNonGeneric(stream, elementType, session),
                    (stream, obj, session) => WriteValuesNonGeneric(obj, stream, elementType, elementSerializer, session));
            }

            typeMapping.TryAdd(type, arraySerializer);
            return arraySerializer;
        }
    }
}
