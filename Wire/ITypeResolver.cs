using System;
using System.IO;

namespace Wire
{
    public interface ITypeResolver
    {
        void WriteType(Stream stream, Type type, SerializerSession session);
        Type ReadType(Stream stream, int firstManifest, DeserializerSession session);
    }
}
