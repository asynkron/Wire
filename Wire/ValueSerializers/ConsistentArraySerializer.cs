using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ConsistentArraySerializer : ValueSerializer
    {
        public const byte Manifest = 252;
        public static readonly ConsistentArraySerializer Instance = new ConsistentArraySerializer();

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackSerializedObject(value);
            }

            var array = (Array)value;
            var elementType = array.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(stream, session); //write array element type

            if (elementType.IsFixedSizeType())
                WriteValuesFixedSize(array, elementType, stream, elementSerializer, session);
            else if (session.Serializer.Options.UseDynamicCode)
                WriteValues((dynamic)array, stream, elementSerializer, session);
            else
                WriteValuesNonGeneric(array, stream, elementSerializer, session);
        }

        private static void WriteValuesFixedSize(Array array, Type elementType, Stream stream, ValueSerializer elementSerializer, SerializerSession session)
        {
            var length = array.Length;
            stream.WriteInt32(length);
            var size = elementType.GetTypeSize();
            byte[] result = new byte[length * size];
            Buffer.BlockCopy(array, 0, result, 0, result.Length);
            stream.Write(result);
        }

        private static void WriteValues<T>(T[] array, Stream stream, ValueSerializer elementSerializer, SerializerSession session)
        {
            stream.WriteInt32(array.Length);
            foreach (var value in array)
            {
                elementSerializer.WriteValue(stream, value, session);
            }
        }

        private static void WriteValuesNonGeneric(Array array, Stream stream, ValueSerializer elementSerializer, SerializerSession session)
        {
            var length = array.Length;
            stream.WriteInt32(length);
            for (int i = 0; i < length; i++)
            {
                var value = array.GetValue(i);
                elementSerializer.WriteValue(stream, value, session);
            }
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var elementSerializer = session.Serializer.GetDeserializerByManifest(stream, session);
            //read the element type
            var elementType = elementSerializer.GetElementType();
            //get the element type serializer
            var length = stream.ReadInt32(session);
            var array = Array.CreateInstance(elementType, length); //create the array
            if (session.Serializer.Options.PreserveObjectReferences)
            {
                session.TrackDeserializedObject(array);
            }

            if (elementType.IsFixedSizeType())
                ReadValuesFixedSize(stream, array, elementType, elementSerializer, session);
            else if (session.Serializer.Options.UseDynamicCode)
                ReadValues(stream, (dynamic)array, elementSerializer, session);
            else
                ReadValuesNonGeneric(stream, array, elementSerializer, session);

            return array;
        }

        private static void ReadValuesFixedSize(Stream stream, Array array, Type elementType, ValueSerializer elementSerializer, DeserializerSession session)
        {
            var size = elementType.GetTypeSize();
            var temp = new byte[array.Length * size];
            stream.Read(temp, 0, temp.Length);
            Buffer.BlockCopy(temp, 0, array, 0, temp.Length);
        }

        private static void ReadValues<T>(Stream stream, T[] array, ValueSerializer elementSerializer, DeserializerSession session)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var value = elementSerializer.ReadValue(stream, session);
                array[i] = (T)value;
            }
        }

        private static void ReadValuesNonGeneric(Stream stream, Array array, ValueSerializer elementSerializer, DeserializerSession session)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var value = elementSerializer.ReadValue(stream, session);
                array.SetValue(value, i);
            }
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }
    }
}
