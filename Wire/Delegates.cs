using System.IO;

namespace Wire
{
    public delegate object ValueReader(Stream stream, DeserializerSession session);

    public delegate void ValueWriter(Stream stream, object obj, SerializerSession session);
}