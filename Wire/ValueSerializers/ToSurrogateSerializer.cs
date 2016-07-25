using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ToSurrogateSerializer : ValueSerializer
    {
        private readonly ValueSerializer _surrogateSerializer;
        private readonly Func<object, object> _translator;
        private readonly Type _type;

        public ToSurrogateSerializer(Func<object, object> translator, Type type, ValueSerializer surrogateSerializer)
        {
            _type = type;
            _translator = translator;
            _surrogateSerializer = surrogateSerializer;
        }

        public override void WriteManifest(Stream stream, SerializerSession session)
        {
            //    _surrogateSerializer.WriteManifest(stream, type, session);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var surrogateValue = _translator(value);
            stream.WriteObjectWithManifest(surrogateValue, session);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            throw new NotSupportedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}