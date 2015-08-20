using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class ToSurrogateSerializer : ValueSerializer
    {
        private readonly Func<object, object> _translator;
        private readonly ValueSerializer _surrogateSerializer;

        public ToSurrogateSerializer(Func<object, object> translator, ValueSerializer surrogateSerializer)
        {
            _translator = translator;
            _surrogateSerializer = surrogateSerializer;
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            _surrogateSerializer.WriteManifest(stream, type, session);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var surrogateValue = _translator(value);
            _surrogateSerializer.WriteValue(stream,surrogateValue,session);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            throw new NotSupportedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }


    public class FromSurrogateSerializer : ValueSerializer
    {
        private readonly Func<object, object> _translator;
        private readonly ValueSerializer _surrogateSerializer;

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

        public override object ReadValue(Stream stream, SerializerSession session)
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
