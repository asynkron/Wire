using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        public Action<Stream, object, SerializerSession> Writer { get; set; }
        public Func<Stream, SerializerSession, object> Reader { get; set; }

        private static readonly ConcurrentDictionary<Type, byte[]> AssemblyQualifiedNames = new ConcurrentDictionary<Type, byte[]>();
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
    }
}