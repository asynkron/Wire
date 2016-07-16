using System.IO;

namespace Wire
{
    public delegate object TypeReader(Stream stream, DeserializerSession session);

    public delegate void TypeWriter(Stream stream, object obj, SerializerSession session);

    public delegate void FieldReader(Stream stream, object obj, DeserializerSession session);
}