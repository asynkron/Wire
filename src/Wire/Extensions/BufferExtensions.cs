using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Wire.ValueSerializers;

namespace Wire.Extensions
{
    public static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObjectWithManifest(this IBufferWriter<byte> stream, object? value,
            SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, session);
                return;
            }

            if (session.Serializer.Options.PreserveObjectReferences &&
                session.TryGetObjectId(value, out var existingId))
            {
                //write the serializer manifest
                ObjectReferenceSerializer.Instance.WriteManifest(stream, session);
                //write the object reference id
                ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                return;
            }

            //TODO. check is vType is same as expected serializer, if so, use that
            var vType = value.GetType();
            var s2 = session.Serializer.GetSerializerByType(vType);
            s2.WriteManifest(stream, session);
            s2.WriteValue(stream, value, session);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObject(this IBufferWriter<byte> stream, object? value, Type expectedType,
            ValueSerializer expectedValueSerializer,
            bool preserveObjectReferences, SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, session);
                return;
            }

            if (preserveObjectReferences && session.TryGetObjectId(value, out var existingId))
            {
                //write the serializer manifest
                ObjectReferenceSerializer.Instance.WriteManifest(stream, session);
                //write the object reference id
                ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                return;
            }

            var vType = value.GetType();
            var s2 = expectedValueSerializer;
            if (vType != expectedType)
                //value is of subtype, lookup the serializer for that type
                s2 = session.Serializer.GetSerializerByType(vType);
            //lookup serializer for subtype
            s2.WriteManifest(stream, session);
            s2.WriteValue(stream, value, session);
        }
    }
}