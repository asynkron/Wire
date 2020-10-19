using System.IO;
using JetBrains.Annotations;

namespace Wire.Compilation
{
    [PublicAPI]
    public abstract class Reader
    {
        public abstract object Read(Stream stream, DeserializerSession session);
    }
}