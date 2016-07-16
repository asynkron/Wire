using System.IO;

namespace Wire
{
    //Reads an entire object from a stream, including manifests
    public delegate object ObjectReader(Stream stream, DeserializerSession session);

    //Writes an entire object to a stream, including manifests
    public delegate void ObjectWriter(Stream stream, object obj, SerializerSession session);

    public delegate void FieldReader(Stream stream, object obj, DeserializerSession session);

    //Writes the content of an object to a stream
    public delegate void FieldsWriter(Stream stream, object obj, SerializerSession session);
}