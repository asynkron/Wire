using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class FromSurrogateSerializer : ValueSerializer
    {
        private readonly ValueSerializer _surrogateSerializer;
        private readonly Func<object, object> _translator;

        public FromSurrogateSerializer(Func<object, object> translator, ValueSerializer surrogateSerializer)
        {
            _translator = translator;
            _surrogateSerializer = surrogateSerializer;
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            throw new NotSupportedException();
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            throw new NotSupportedException();
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            var surrogateValue = _surrogateSerializer.ReadValue(stream, session);
            var value = _translator(surrogateValue);
            return value;
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}