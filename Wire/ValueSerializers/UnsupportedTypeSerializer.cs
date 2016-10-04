using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wire.Compilation;
using Wire.Internal;

namespace Wire.ValueSerializers
{
    //https://github.com/AsynkronIT/Wire/issues/115

    public class UnsupportedTypeException:Exception
    {
        public Type Type;
        public UnsupportedTypeException(Type t, string msg):base(msg)
        { }
    }
    public class UnsupportedTypeSerializer:ValueSerializer
    {
        private string errorMessage = "";
        private Type invalidType;
        public UnsupportedTypeSerializer(Type t, string msg)
        {
            errorMessage = msg;
            invalidType = t;
        }
        public override int EmitReadValue([NotNull] ICompiler<ObjectReader> c, int stream, int session, [NotNull] FieldInfo field)
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
        public override void EmitWriteValue(ICompiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
        public override object ReadValue([NotNull] Stream stream, [NotNull] DeserializerSession session)
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
        public override void WriteManifest([NotNull] Stream stream, [NotNull] SerializerSession session)
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
        public override void WriteValue([NotNull] Stream stream, object value, [NotNull] SerializerSession session)
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
        public override Type GetElementType()
        {
            throw new UnsupportedTypeException(invalidType, errorMessage);
        }
    }
}
