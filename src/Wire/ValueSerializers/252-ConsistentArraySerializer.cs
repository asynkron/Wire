// -----------------------------------------------------------------------
//   <copyright file="ConsistentArraySerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;

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
            var length = stream.ReadInt32(session);
            var array = Array.CreateInstance(elementType, length); //create the array
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackDeserializedObject(array);
            
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

        public override void WriteManifest<TBufferWriter>(Writer<TBufferWriter> writer, SerializerSession session)
        {
            writer.Write(Manifest);
        }

        public override void WriteValue<TBufferWriter>(Writer<TBufferWriter> writer, object value, SerializerSession session)
        {
            var array = (Array) value;
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackSerializedObject(value);
            var elementType = value.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(writer, session); //write array element type

            foreach (var element in array)
            {
                elementSerializer.WriteValue(writer, element, session);
            }
        }
    }
}