using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Wire.ValueSerializers;

namespace Wire.Extensions
{
    public static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(this IBufferWriter<byte> self, byte b)
        {
            var span = self.GetSpan(1);
            span[0] = b;
            self.Advance(1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this IBufferWriter<byte> self, Span<byte> source)
        {
            var destination = self.GetSpan(source.Length);
            source.CopyTo(destination);
            self.Advance(source.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
         public static void WriteObjectWithManifest(this IBufferWriter<byte> stream, object? value, SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, session);
            }
            else
            {
                if (session.Serializer.Options.PreserveObjectReferences &&
                    session.TryGetObjectId(value, out var existingId))
                {
                    //write the serializer manifest
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, session);
                    //write the object reference id
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                {
                    var vType = value.GetType();
                    var s2 = session.Serializer.GetSerializerByType(vType);
                    s2.WriteManifest(stream, session);
                    s2.WriteValue(stream, value, session);
                }
            }
        }

         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObject(this IBufferWriter<byte> stream, object? value, Type valueType,
            ValueSerializer valueSerializer,
            bool preserveObjectReferences, SerializerSession session)
        {
            if (value == null) //value is null
            {
                NullSerializer.Instance.WriteManifest(stream, session);
            }
            else
            {
                if (preserveObjectReferences && session.TryGetObjectId(value, out var existingId))
                {
                    //write the serializer manifest
                    ObjectReferenceSerializer.Instance.WriteManifest(stream, session);
                    //write the object reference id
                    ObjectReferenceSerializer.Instance.WriteValue(stream, existingId, session);
                }
                else
                {
                    var vType = value.GetType();
                    var s2 = valueSerializer;
                    if (vType != valueType)
                        //value is of subtype, lookup the serializer for that type
                        s2 = session.Serializer.GetSerializerByType(vType);
                    //lookup serializer for subtype
                    s2.WriteManifest(stream, session);
                    s2.WriteValue(stream, value, session);
                }
            }
        }
    }
}