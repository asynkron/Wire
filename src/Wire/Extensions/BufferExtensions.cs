using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Wire.Buffers;
using Wire.ValueSerializers;
using Wire.ValueSerializers.Optimized;

namespace Wire.Extensions
{
    public static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObjectWithManifest<TBufferWriter>(this Writer<TBufferWriter> writer, object? value,
            SerializerSession session) where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null) //value is null
            {
                NullSerializer.WriteManifestImpl(writer);
                return;
            }

            if (session.Serializer.Options.PreserveObjectReferences &&
                session.TryGetObjectId(value, out var existingId))
            {
                //write the serializer manifest
                ObjectReferenceSerializer.WriteManifestImpl(writer);
                //write the object reference id
                ObjectReferenceSerializer.WriteValueImpl(writer, existingId, session);
                return;
            }

            //TODO. check is vType is same as expected serializer, if so, use that
            var vType = value.GetType();
            var s2 = session.Serializer.GetSerializerByType(vType);
            s2.WriteManifest(writer, session);
            s2.WriteValue(writer, value, session);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObject<TBufferWriter>(this Writer<TBufferWriter> writer, object? value, Type expectedType,
            ValueSerializer expectedValueSerializer,
            bool preserveObjectReferences, SerializerSession session)  where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null) //value is null
            {
                NullSerializer.WriteManifestImpl(writer);
                return;
            }

            if (preserveObjectReferences && session.TryGetObjectId(value, out var existingId))
            {
                //write the serializer manifest
                ObjectReferenceSerializer.WriteManifestImpl(writer);
                //write the object reference id
                ObjectReferenceSerializer.WriteValueImpl(writer, existingId, session);
                return;
            }

            var vType = value.GetType();
            var s2 = expectedValueSerializer;
            if (vType != expectedType)
                //value is of subtype, lookup the serializer for that type
                s2 = session.Serializer.GetSerializerByType(vType);
            //lookup serializer for subtype
            s2.WriteManifest(writer, session);
            s2.WriteValue(writer, value, session);
        }
    }
}