using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        private static readonly ConcurrentDictionary<Type, byte[]> AssemblyQualifiedNames =
            new ConcurrentDictionary<Type, byte[]>();

        public Type Type { get; }

        public Action<Stream, object, SerializerSession> Writer { get; }
        public Func<Stream, SerializerSession, object> Reader { get; }

        public ObjectSerializer(Type type, Action<Stream, object, SerializerSession> writer,
            Func<Stream, SerializerSession, object> reader)
        {
            Type = type;
            Writer = writer;
            Reader = reader;
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(255); //write manifest identifier, 
            var bytes = AssemblyQualifiedNames.GetOrAdd(type, t =>
            {
                var name = t.AssemblyQualifiedName;
                var b = Encoding.UTF8.GetBytes(name);
                return b;
            });
            ByteArraySerializer.Instance.WriteValue(stream, bytes, session); //write the encoded name of the type
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