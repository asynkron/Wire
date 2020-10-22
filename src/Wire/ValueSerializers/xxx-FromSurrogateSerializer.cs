// -----------------------------------------------------------------------
//   <copyright file="FromSurrogateSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;

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

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            throw new NotSupportedException();
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
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