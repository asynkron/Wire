using System;
using System.IO;
using System.Text;

namespace Wire.ValueSerializers
{
    public class TypeSerializer : ValueSerializer
    {
        public static readonly TypeSerializer Instance = new TypeSerializer();
        public const byte Manifest = 16;

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(Manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                stream.WriteInt32(-1);
            }
            else
            {
                var type = (Type) value;
                var name = type.AssemblyQualifiedName;
                var bytes = Encoding.UTF8.GetBytes(name);
                stream.WriteLengthEncodedByteArray(bytes);
            }
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var length = (int) Int32Serializer.Instance.ReadValue(stream, session);
            if (length == -1)
                return null;

            var buffer = session.GetBuffer(length);
            stream.Read(buffer, 0, length);
            var name = Encoding.UTF8.GetString(buffer, 0, length);
            var type = Type.GetType(name);
            return type;
        }

        public override Type GetElementType()
        {
            return typeof (Type);
        }
    }
}