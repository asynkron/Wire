using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wire.ValueSerializers
{
    public class ObjectSerializer : ValueSerializer
    {
        public const byte ManifestFull = 255;
        public const byte ManifestIndex = 254;

        private volatile bool _isInitialized;
        private ValueReader _reader;
        private ValueWriter _writer;

        public ObjectSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;

            //initialize reader and writer with dummy handlers that wait until the serializer is fully initialized
            _writer = (stream, o, session) =>
            {
                SpinWait.SpinUntil(() => _isInitialized);
                WriteValue(stream, o, session);
            };

            _reader = (stream, session) =>
            {
                SpinWait.SpinUntil(() => _isInitialized);
                return ReadValue(stream, session);
            };
        }

        public Type Type { get; }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            session.Serializer.Options.TypeResolver.WriteType(stream, type, session);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            _writer(stream, value, session);
        }

        public override object ReadValue(Stream stream, DeserializerSession session)
        {
            return _reader(stream, session);
        }

        public override Type GetElementType()
        {
            return Type;
        }

        public void Initialize(ValueReader reader, ValueWriter writer)
        {
            _reader = reader;
            _writer = writer;
            _isInitialized = true;
        }
    }
}