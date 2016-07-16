using System;
using System.Collections.Concurrent;
using System.Reflection;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class ExceptionSerializerFactory : ValueSerializerFactory
    {
        private static readonly TypeInfo ExceptionTypeInfo = typeof(Exception).GetTypeInfo();
        private readonly FieldInfo _className;
        private readonly FieldInfo _innerException;
        private readonly FieldInfo _stackTraceString;
        private readonly FieldInfo _remoteStackTraceString;
        private readonly FieldInfo _message;

        public ExceptionSerializerFactory()
        {
            var bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            _className = ExceptionTypeInfo.GetField("_className", bf);
            _innerException = ExceptionTypeInfo.GetField("_innerException", bf);
            _message = ExceptionTypeInfo.GetField("_message", bf);
            _remoteStackTraceString = ExceptionTypeInfo.GetField("_remoteStackTraceString", bf);
            _stackTraceString = ExceptionTypeInfo.GetField("_stackTraceString", bf);
        }

        public override bool CanSerialize(Serializer serializer, Type type)
        {
            return ExceptionTypeInfo.IsAssignableFrom(type.GetTypeInfo());
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var exceptionSerializer = new ObjectSerializer(type);
            exceptionSerializer.Initialize((stream, session) =>
            {
                var exception = Activator.CreateInstance(type);
                var className = StringSerializer.Instance.ReadValue(stream, session);
                var message = StringSerializer.Instance.ReadValue(stream, session);
                var remoteStackTraceString = StringSerializer.Instance.ReadValue(stream, session);
                var stackTraceString = StringSerializer.Instance.ReadValue(stream, session);
                var innerException = stream.ReadObject(session);

                _className.SetValue(exception,className);
                _message.SetValue(exception, message);
                _remoteStackTraceString.SetValue(exception, remoteStackTraceString);
                _stackTraceString.SetValue(exception, stackTraceString);
                _innerException.SetValue(exception,innerException);
                return exception;
            }, (stream, exception, session) =>
            {
                var className = _className.GetValue(exception);
                var message = _message.GetValue(exception);
                var remoteStackTraceString = _remoteStackTraceString.GetValue(exception);
                var stackTraceString = _stackTraceString.GetValue(exception);
                var innerException = _innerException.GetValue(exception);

                StringSerializer.Instance.WriteValue(stream, className, session);
                StringSerializer.Instance.WriteValue(stream, message, session);
                StringSerializer.Instance.WriteValue(stream, remoteStackTraceString, session);
                StringSerializer.Instance.WriteValue(stream, stackTraceString, session);
                stream.WriteObjectWithManifest(innerException, session);
            });
            typeMapping.TryAdd(type, exceptionSerializer);
            return exceptionSerializer;
        }
    }
}