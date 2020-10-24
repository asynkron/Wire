using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Wire.Compilation
{
    [PublicAPI]
    public abstract class ObjectReader
    {
 
        
        [System.Security.SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public abstract object Read(Stream stream, DeserializerSession session);
    }
}