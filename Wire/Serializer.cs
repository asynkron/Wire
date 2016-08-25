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
        private ValueSerializer GetCustomSerializer([NotNull] Type type)
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
                var wrapper = new KnownTypeObjectSerializer((ObjectSerializer) serializer, index);
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
            if (ReferenceEquals(type.GetTypeInfo().Assembly, ReflectionEx.CoreAssembly))
            {
                //faster than hash lookup you know...

                if (ReferenceEquals(type, TypeEx.SystemObject))
                    return SystemObjectSerializer.Instance;
                
                if (ReferenceEquals(type, TypeEx.StringType))
                    return StringSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.Int32Type))
                    return Int32Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.GuidType))
                    return GuidSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.DateTimeType))
                    return DateTimeSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.Int64Type))
                    return Int64Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.Int16Type))
                    return Int16Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.UInt32Type))
                    return UInt32Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.UInt64Type))
                    return UInt64Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.UInt16Type))
                    return UInt16Serializer.Instance;

                if (ReferenceEquals(type, TypeEx.ByteType))
                    return ByteSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.SByteType))
                    return SByteSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.BoolType))
                    return BoolSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.FloatType))
                    return FloatSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.DoubleType))
                    return DoubleSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.DecimalType))
                    return DecimalSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.CharType))
                    return CharSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.ByteArrayType))
                    return ByteArraySerializer.Instance;

                if (ReferenceEquals(type, TypeEx.TypeType))
                    return TypeSerializer.Instance;

                if (ReferenceEquals(type, TypeEx.RuntimeType))
                    return TypeSerializer.Instance;

                if (type.IsOneDimensionalPrimitiveArray())
                    return ConsistentArraySerializer.Instance;
            }

            var serializer = GetCustomSerializer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByManifest([NotNull] Stream stream, [NotNull] DeserializerSession session)
        {
            var first = stream.ReadByte();
            switch (first)
            {
                case 1:
                    return SystemObjectSerializer.Instance;
                case 10:
                    throw new NotSupportedException("Unknown manifest value");
                case NullSerializer.Manifest:
                    return NullSerializer.Instance;
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