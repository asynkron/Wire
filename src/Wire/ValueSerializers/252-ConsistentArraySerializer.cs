// -----------------------------------------------------------------------
//   <copyright file="ConsistentArraySerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using Wire.Extensions;
using Wire.ValueSerializers.Optimized;

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

            if (elementType.IsFixedSizeType())
            {
                var size = elementType.GetTypeSize();
                var totalSize = size * length;
                var buffer = session.GetBuffer(totalSize);
                stream.Read(buffer, 0, totalSize);
                Buffer.BlockCopy(buffer, 0, array, 0, totalSize);
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    var value = elementSerializer.ReadValue(stream, session); //read the element value
                    array.SetValue(value, i); //set the element value
                }
            }

            return array;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override void WriteManifest(IBufferWriter<byte> stream, SerializerSession session)
        {
            var span = stream.GetSpan(1);
            span[0] = Manifest;
            stream.Advance(1);
        }

        // private static void WriteValues<T>(T[] array, Stream stream, Type elementType, ValueSerializer elementSerializer,
        // private static object ReadValues<T>(Stream stream, DeserializerSession session, bool preserveObjectReferences)

        public override void WriteValue(IBufferWriter<byte> stream, object value, SerializerSession session)
        {
            if (session.Serializer.Options.PreserveObjectReferences) session.TrackSerializedObject(value);
            var elementType = value.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(stream, session); //write array element type
            // ReSharper disable once PossibleNullReferenceException
            //TODO fix this
            WriteValues((dynamic) value, stream, elementSerializer, session);
        }

        private static void WriteValues<T>(T[] array, IBufferWriter<byte> stream, ValueSerializer elementSerializer,
            SerializerSession session)
        {
            Int32Serializer.WriteValue(stream, array.Length);
            if (typeof(T).IsFixedSizeType())
            {
                var size = typeof(T).GetTypeSize();
                
                var result = new byte[array.Length * size];
                Buffer.BlockCopy(array, 0, result, 0, result.Length);
                var destination = stream.GetSpan(result.Length);
                result.CopyTo(destination);
                stream.Advance(result.Length);
            }
            else
            {
                foreach (var value in array) elementSerializer.WriteValue(stream, value!, session);
            }
        }
    }
}