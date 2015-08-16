using System;
using System.IO;

namespace Wire.ValueSerializers
{

    public abstract class ValueSerializer
    {
        public abstract void WriteManifest(Stream stream, Type type, SerializerSession session);
        public abstract void WriteValue(Stream stream, object value, SerializerSession session);
        public abstract object ReadValue(Stream stream, SerializerSession session);
    }
}
