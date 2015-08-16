using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        public Type Type { get; }

        public Action<Stream, object, SerializerSession> Writer { get; }
        public Func<Stream, SerializerSession, object> Reader { get; }
        public byte[] AssemblyQualifiedName { get; }

        public ObjectSerializer(Type type, Action<Stream, object, SerializerSession> writer,
            Func<Stream, SerializerSession, object> reader)
        {
            Type = type;
            Writer = writer;
            Reader = reader;
            AssemblyQualifiedName = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(255); //write manifest identifier,            
            ByteArraySerializer.Instance.WriteValue(stream, AssemblyQualifiedName, session); //write the encoded name of the type
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            Writer(stream, value, session);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return Reader(stream, session);
        }

        public override Type GetElementType()
        {
            return Type;
        }
    }
}