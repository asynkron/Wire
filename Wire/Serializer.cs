using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Wire.Extensions;
using Wire.ValueSerializers;
using TypeSerializerLookup =
    System.Collections.Concurrent.ConcurrentDictionary<System.Type, Wire.ValueSerializers.ValueSerializer>;

namespace Wire
{
    public class Serializer
    {
        private readonly TypeSerializerLookup _deserializers = new TypeSerializerLookup();
        private readonly TypeSerializerLookup _serializers = new TypeSerializerLookup();
        private readonly ValueSerializer[] _deserializerLookup = new ValueSerializer[256];
        public readonly SerializerOptions Options;
        public readonly ICodeGenerator CodeGenerator = new DefaultCodeGenerator();

        public Serializer() : this(new SerializerOptions())
        {
        }

        public Serializer([NotNull] SerializerOptions options)
        {
            Options = options;
            AddSerializers();
            AddDeserializers();
        }

        private void AddDeserializers()
        {
            _deserializerLookup[NullSerializer.Manifest] = NullSerializer.Instance;
            _deserializerLookup[SystemObjectSerializer.Manifest] =
                SystemObjectSerializer.Instance;
            _deserializerLookup[Int64Serializer.Manifest] =
                Int64Serializer.Instance;
            _deserializerLookup[Int16Serializer.Manifest] =
                Int16Serializer.Instance;
            _deserializerLookup[ByteSerializer.Manifest] =
                ByteSerializer.Instance;
            _deserializerLookup[DateTimeSerializer.Manifest] =
                DateTimeSerializer.Instance;
            _deserializerLookup[BoolSerializer.Manifest] =
                BoolSerializer.Instance;
            _deserializerLookup[StringSerializer.Manifest] =
                StringSerializer.Instance;
            _deserializerLookup[Int32Serializer.Manifest] =
                Int32Serializer.Instance;
            _deserializerLookup[ByteArraySerializer.Manifest] =
                ByteArraySerializer.Instance;
            //10 not yet used
            _deserializerLookup[GuidSerializer.Manifest] =
                GuidSerializer.Instance;
            _deserializerLookup[FloatSerializer.Manifest] =
                FloatSerializer.Instance;
            _deserializerLookup[DoubleSerializer.Manifest] =
                DoubleSerializer.Instance;
            _deserializerLookup[DecimalSerializer.Manifest] =
                DecimalSerializer.Instance;
            _deserializerLookup[CharSerializer.Manifest] =
                CharSerializer.Instance;
            _deserializerLookup[TypeSerializer.Manifest] =
                TypeSerializer.Instance;
            _deserializerLookup[UInt16Serializer.Manifest] =
                UInt16Serializer.Instance;
            _deserializerLookup[UInt32Serializer.Manifest] =
                UInt32Serializer.Instance;
            _deserializerLookup[UInt64Serializer.Manifest] =
                UInt64Serializer.Instance;
            _deserializerLookup[SByteSerializer.Manifest] =
                SByteSerializer.Instance;
            _deserializerLookup[ConsistentArraySerializer.Manifest] =
                ConsistentArraySerializer.Instance;
        }

        private void AddSerializers()
        {
            AddValueSerializer<Guid>(GuidSerializer.Instance);

            AddValueSerializer<string>(StringSerializer.Instance);

            AddValueSerializer<byte>(ByteSerializer.Instance);
            AddValueSerializer<short>(Int16Serializer.Instance);
            AddValueSerializer<int>(Int32Serializer.Instance);
            AddValueSerializer<long>(Int64Serializer.Instance);

            AddValueSerializer<sbyte>(SByteSerializer.Instance);
            AddValueSerializer<ushort>(UInt16Serializer.Instance);
            AddValueSerializer<uint>(UInt32Serializer.Instance);
            AddValueSerializer<ulong>(UInt64Serializer.Instance);

            AddValueSerializer<bool>(BoolSerializer.Instance);
            AddValueSerializer<float>(FloatSerializer.Instance);
            AddValueSerializer<double>(DoubleSerializer.Instance);
            AddValueSerializer<decimal>(DecimalSerializer.Instance);

            AddValueSerializer<object>(SystemObjectSerializer.Instance);
            AddValueSerializer<char>(CharSerializer.Instance);
            AddValueSerializer<byte[]>(ByteArraySerializer.Instance);
            AddValueSerializer<DateTime>(DateTimeSerializer.Instance);

            AddValueSerializer<Type>(TypeSerializer.Instance);
            AddValueSerializer(TypeSerializer.Instance, TypeEx.RuntimeType);
        }

        private void AddValueSerializer(ValueSerializer instance,Type type)
        {
            _serializers.TryAdd(type, instance);
        }
        private void AddValueSerializer<T>(ValueSerializer instance)
        {
            _serializers.TryAdd(typeof(T), instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetCustomDeserializer([NotNull] Type type)
        {
            ValueSerializer serializer;

            //do we already have a deserializer for this type?
            if (_deserializers.TryGetValue(type, out serializer))
                return serializer;

            //is there a deserializer factory that can handle this type?
            foreach (var valueSerializerFactory in Options.ValueSerializerFactories)
            {
                if (valueSerializerFactory.CanDeserialize(this, type))
                {
                    return valueSerializerFactory.BuildSerializer(this, type, _deserializers);
                }
            }

            //none of the above, lets create a POCO object deserializer
            serializer = new ObjectSerializer(type);
            //add it to the serializer lookup incase of recursive serialization
            if (!_deserializers.TryAdd(type, serializer)) return _deserializers[type];
            //build the serializer IL code
            CodeGenerator.BuildSerializer(this, (ObjectSerializer) serializer);
            return serializer;
        }

        //this returns a delegate for serializing a specific "field" of an instance of type "type"

        public void Serialize(object obj, [NotNull] Stream stream)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var session = new SerializerSession(this);

            var type = obj.GetType();
            var s = GetSerializerByType(type);
            s.WriteManifest(stream, session);
            s.WriteValue(stream, obj, session);
        }

        public T Deserialize<T>([NotNull] Stream stream)
        {
            var session = new DeserializerSession(this);
            var s = GetDeserializerByManifest(stream, session);
            return (T) s.ReadValue(stream, session);
        }

        public object Deserialize([NotNull] Stream stream)
        {
            var session = new DeserializerSession(this);
            var s = GetDeserializerByManifest(stream, session);
            return s.ReadValue(stream, session);
        }

        public ValueSerializer GetSerializerByType([NotNull] Type type)
        {
            ValueSerializer serializer;

            //do we already have a serializer for this type?
            if (_serializers.TryGetValue(type, out serializer))
                return serializer;

            //is there a serializer factory that can handle this type?
            foreach (var valueSerializerFactory in Options.ValueSerializerFactories)
            {
                if (valueSerializerFactory.CanSerialize(this, type))
                {
                    return valueSerializerFactory.BuildSerializer(this, type, _serializers);
                }
            }

            //none of the above, lets create a POCO object serializer
            serializer = new ObjectSerializer(type);
            if (Options.KnownTypesDict.ContainsKey(type))
            {
                var index = Options.KnownTypesDict[type];
                var wrapper = new KnownTypeObjectSerializer((ObjectSerializer)serializer, index);
                if (!_serializers.TryAdd(type, wrapper))
                    return _serializers[type];

                //build the serializer IL code
                CodeGenerator.BuildSerializer(this, (ObjectSerializer)serializer);
                //just ignore if this fails, another thread have already added an identical serializer
                return wrapper;
            }
            else
            {
                if (!_serializers.TryAdd(type, serializer))
                    return _serializers[type];

                //build the serializer IL code
                CodeGenerator.BuildSerializer(this, (ObjectSerializer)serializer);
                //just ignore if this fails, another thread have already added an identical serializer
                return serializer;
            }
            //add it to the serializer lookup incase of recursive serialization

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueSerializer GetDeserializerByManifest([NotNull] Stream stream, [NotNull] DeserializerSession session)
        {
            var first = stream.ReadByte();
            if (first <= 250) return _deserializerLookup[first];
            switch (first)
            {
                case ObjectReferenceSerializer.Manifest:
                    return ObjectReferenceSerializer.Instance;
                case ObjectSerializer.ManifestFull:
                {
                    var type = TypeEx.GetTypeFromManifestFull(stream, session);
                    return GetCustomDeserializer(type);
                }
                case ObjectSerializer.ManifestVersion:
                {
                    var type = TypeEx.GetTypeFromManifestVersion(stream, session);
                    return GetCustomDeserializer(type);
                }
                case ObjectSerializer.ManifestIndex:
                {
                    var type = TypeEx.GetTypeFromManifestIndex(stream, session);
                    return GetCustomDeserializer(type);
                }
                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }
    }
}