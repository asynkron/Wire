using System;
using System.IO;
using System.Reflection;
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
        public readonly SerializerOptions Options;
        public readonly ICodeGenerator CodeGenerator = new DefaultCodeGenerator();

        public Serializer() : this(new SerializerOptions())
        {
        }

        public Serializer([NotNull] SerializerOptions options)
        {
            Options = options;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetCustomSerialzer([NotNull] Type type)
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
            //add it to the serializer lookup incase of recursive serialization
            if (!_serializers.TryAdd(type, serializer)) return _serializers[type];
            //build the serializer IL code
            CodeGenerator.BuildSerializer(this, (ObjectSerializer) serializer);
            //just ignore if this fails, another thread have already added an identical serialzer
            return serializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetCustomDeserialzer([NotNull] Type type)
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
            if (ReferenceEquals(type.GetTypeInfo().Assembly, ReflectionEx.CoreAssembly))
            {
                //faster than hash lookup you know...
                if (type == TypeEx.StringType)
                    return StringSerializer.Instance;

                if (type == TypeEx.Int32Type)
                    return Int32Serializer.Instance;

                if (type == TypeEx.Int64Type)
                    return Int64Serializer.Instance;

                if (type == TypeEx.Int16Type)
                    return Int16Serializer.Instance;

                if (type == TypeEx.UInt32Type)
                    return UInt32Serializer.Instance;

                if (type == TypeEx.UInt64Type)
                    return UInt64Serializer.Instance;

                if (type == TypeEx.UInt16Type)
                    return UInt16Serializer.Instance;

                if (type == TypeEx.ByteType)
                    return ByteSerializer.Instance;

                if (type == TypeEx.SByteType)
                    return SByteSerializer.Instance;

                if (type == TypeEx.BoolType)
                    return BoolSerializer.Instance;

                if (type == TypeEx.DateTimeType)
                    return DateTimeSerializer.Instance;

                if (type == TypeEx.GuidType)
                    return GuidSerializer.Instance;

                if (type == TypeEx.FloatType)
                    return FloatSerializer.Instance;

                if (type == TypeEx.DoubleType)
                    return DoubleSerializer.Instance;

                if (type == TypeEx.DecimalType)
                    return DecimalSerializer.Instance;

                if (type == TypeEx.CharType)
                    return CharSerializer.Instance;

                if (type == TypeEx.ByteArrayType)
                    return ByteArraySerializer.Instance;

                if (type == TypeEx.TypeType)
                    return TypeSerializer.Instance;

                if (type == TypeEx.RuntimeType)
                    return TypeSerializer.Instance;

                if (type.IsOneDimensionalPrimitiveArray())
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetCustomSerialzer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByManifest([NotNull] Stream stream, [NotNull] DeserializerSession session)
        {
            var first = stream.ReadByte();
            switch (first)
            {
                case NullSerializer.Manifest:
                    return NullSerializer.Instance;
//TODO: hmm why havent I added 1?
                case Int64Serializer.Manifest:
                    return Int64Serializer.Instance;
                case Int16Serializer.Manifest:
                    return Int16Serializer.Instance;
                case ByteSerializer.Manifest:
                    return ByteSerializer.Instance;
                case DateTimeSerializer.Manifest:
                    return DateTimeSerializer.Instance;
                case BoolSerializer.Manifest:
                    return BoolSerializer.Instance;
                case StringSerializer.Manifest:
                    return StringSerializer.Instance;
                case Int32Serializer.Manifest:
                    return Int32Serializer.Instance;
                case ByteArraySerializer.Manifest:
                    return ByteArraySerializer.Instance;
                case GuidSerializer.Manifest:
                    return GuidSerializer.Instance;
                case FloatSerializer.Manifest:
                    return FloatSerializer.Instance;
                case DoubleSerializer.Manifest:
                    return DoubleSerializer.Instance;
                case DecimalSerializer.Manifest:
                    return DecimalSerializer.Instance;
                case CharSerializer.Manifest:
                    return CharSerializer.Instance;
                case TypeSerializer.Manifest:
                    return TypeSerializer.Instance;
                case UInt16Serializer.Manifest:
                    return UInt16Serializer.Instance;
                case UInt32Serializer.Manifest:
                    return UInt32Serializer.Instance;
                case UInt64Serializer.Manifest:
                    return UInt64Serializer.Instance;
                case SByteSerializer.Manifest:
                    return SByteSerializer.Instance;
                case ObjectReferenceSerializer.Manifest:
                    return ObjectReferenceSerializer.Instance;
                case ConsistentArraySerializer.Manifest:
                    return ConsistentArraySerializer.Instance;
                case ObjectSerializer.ManifestFull:
                {
                    var type = TypeEx.GetTypeFromManifestFull(stream, session);
                    return GetCustomDeserialzer(type);
                }
                case ObjectSerializer.ManifestVersion:
                {
                    var type = TypeEx.GetTypeFromManifestVersion(stream, session);
                    return GetCustomDeserialzer(type);
                }
                case ObjectSerializer.ManifestIndex:
                {
                    var type = TypeEx.GetTypeFromManifestIndex(stream, session);
                    return GetCustomDeserialzer(type);
                }
                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }
    }
}