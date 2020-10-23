using System.IO;
using JetBrains.Annotations;

namespace Wire.Compilation
{
    [PublicAPI]
    public abstract class ObjectReader
    {
        public abstract object Read(Stream stream, DeserializerSession session);
    }
}