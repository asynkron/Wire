// -----------------------------------------------------------------------
//   <copyright file="ExceptionSerializerFactory.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ExceptionSerializerFactory : ValueSerializerFactory
    {
        private static readonly Type ExceptionTypeInfo = typeof(Exception);

        public override bool CanSerialize(Serializer serializer, Type type) => ExceptionTypeInfo.IsAssignableFrom(type);

        public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var exceptionSerializer = new ExceptionSerializer(type);
            typeMapping.TryAdd(type, exceptionSerializer);
            return exceptionSerializer;
        }
        
        private class ExceptionSerializer : ObjectSerializer
        {
            private readonly FieldInfo _innerException = ExceptionTypeInfo.GetField("_innerException", BindingFlagsEx.All)!;
            private readonly FieldInfo _message = ExceptionTypeInfo.GetField("_message", BindingFlagsEx.All)!;
            private readonly FieldInfo _remoteStackTraceString = ExceptionTypeInfo.GetField("_remoteStackTraceString", BindingFlagsEx.All)!;
            private readonly FieldInfo _stackTraceString = ExceptionTypeInfo.GetField("_stackTraceString", BindingFlagsEx.All)!;

            public ExceptionSerializer(Type type) : base(type)
            {
            }

            public override object? ReadValue(Stream stream, DeserializerSession session)
            {
                var exception = Activator.CreateInstance(Type);
                var message = stream.ReadString(session);
                var remoteStackTraceString = stream.ReadString(session);
                var stackTraceString = stream.ReadString(session);
                var innerException = stream.ReadObject(session);
                _message.SetValue(exception, message);
                _remoteStackTraceString.SetValue(exception, remoteStackTraceString);
                _stackTraceString.SetValue(exception, stackTraceString);
                _innerException.SetValue(exception, innerException);
                return exception;
            }

            public override void WriteValue<TBufferWriter>(ref Writer<TBufferWriter> writer, object value,
                SerializerSession session)
            {
                var exception = (Exception)value;
                var message = (string) _message.GetValue(exception)!;
                var remoteStackTraceString = (string) _remoteStackTraceString.GetValue(exception)!;
                var stackTraceString = (string) _stackTraceString.GetValue(exception)!;
                var innerException = _innerException.GetValue(exception)!;
                StringSerializer.WriteValueImpl(ref writer, message);
                StringSerializer.WriteValueImpl(ref writer, remoteStackTraceString);
                StringSerializer.WriteValueImpl(ref writer, stackTraceString);
                writer.WriteObjectWithManifest(innerException, session);
            }
        }
    }
}