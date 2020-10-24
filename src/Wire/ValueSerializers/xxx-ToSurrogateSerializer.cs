// -----------------------------------------------------------------------
//   <copyright file="ToSurrogateSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Wire.Buffers;
using Wire.Extensions;

namespace Wire.ValueSerializers
{
    public class ToSurrogateSerializer : ValueSerializer
    {
        private readonly Func<object, object> _translator;

        public ToSurrogateSerializer(Func<object, object> translator)
        {
            _translator = translator;
        }

        public override void WriteManifest<TBufferWriter>(ref Writer<TBufferWriter> writer, SerializerSession session)
        {
            //intentionally left blank
        }

        public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
            SerializerSession session)
        {
            var surrogateValue = _translator(value);
            writer.WriteObjectWithManifest(surrogateValue, session);
        }

        public override object? ReadValue(Stream stream, DeserializerSession session)
        {
            throw new NotSupportedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }
    }
}