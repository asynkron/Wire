using System.Buffers;
using JetBrains.Annotations;
using Wire.Buffers;

namespace Wire.Compilation
{
    [PublicAPI]
    public abstract class Writer
    {
        public abstract void ObjectWriter<TBufferWriter>(ref Writer<TBufferWriter> writer, object obj,
            SerializerSession session) where TBufferWriter : IBufferWriter<byte>;
    }
}