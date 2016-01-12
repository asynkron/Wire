using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ConsistentArraySerializer : ValueSerializer
    {
        public const byte Manifest = 252;
        public static readonly ConsistentArraySerializer Instance = new ConsistentArraySerializer();

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var elementSerializer = session.Serializer.GetDeserializerByManifest(stream, session);
            //read the element type
            var elementType = elementSerializer.GetElementType();
            //get the element type serializer
            var length = (int) Int32Serializer.Instance.ReadValue(stream, session); //read the array length
            var array = Array.CreateInstance(elementType, length); //create the array
            for (var i = 0; i < length; i++)
            {
                var value = elementSerializer.ReadValue(stream, session); //read the element value
                array.SetValue(value, i); //set the element value
            }
            return array;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var elementType = value.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(stream, elementType, session); //write array element type
            // ReSharper disable once PossibleNullReferenceException
            WriteValues((dynamic)value, stream,elementSerializer,session);
        }

        private static void WriteValues<T>(T[] array, Stream stream, ValueSerializer elementSerializer, SerializerSession session)
        {
            stream.WriteInt32(array.Length);           
            for (int i = 0; i < array.Length; i++)
            {
                var value = array[i];
                elementSerializer.WriteValue(stream, value, session);
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
    }
}