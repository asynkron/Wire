using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Wire.Buffers;

namespace Wire.Compilation
{
    [PublicAPI]
    public abstract class ObjectWriter
    {
        [System.Security.SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract void Write<TBufferWriter>(ref Writer<TBufferWriter> writer, object obj,
            SerializerSession session) where TBufferWriter : IBufferWriter<byte>;
    }
}