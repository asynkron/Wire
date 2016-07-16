using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Wire.ValueSerializers;
using TypeSerializerLookup = System.Collections.Concurrent.ConcurrentDictionary<System.Type, Wire.ValueSerializers.ValueSerializer>;
namespace Wire
{
    public class Serializer
    {

        private static readonly Assembly CoreAssembly = typeof (int).GetTypeInfo().Assembly;

        private readonly TypeSerializerLookup _deserializers = new TypeSerializerLookup();

        private readonly TypeSerializerLookup _serializers = new TypeSerializerLookup();

        internal readonly SerializerOptions Options;

        public Serializer() : this(new SerializerOptions())
        {
        }

        public Serializer(SerializerOptions options)
        {
            Options = options;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetCustomSerialzer(Type type)
        {
            ValueSerializer serializer;
            if (!_serializers.TryGetValue(type, out serializer))
            {
                foreach (var valueSerializerFactory in Options.ValueSerializerFactories)
                {
                    if (valueSerializerFactory.CanSerialize(this, type))
                    {
                        return valueSerializerFactory.BuildSerializer(this, type, _serializers);
                    }
                }

                serializer = new ObjectSerializer(type);
                _serializers.TryAdd(type, serializer);
                CodeGenerator.BuildSerializer(this, type, (ObjectSerializer) serializer);
                //just ignore if this fails, another thread have already added an identical serialzer
            }
            return serializer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetCustomDeserialzer(Type type)
        {
            ValueSerializer serializer;
            if (!_deserializers.TryGetValue(type, out serializer))
            {
                foreach (var valueSerializerFactory in Options.ValueSerializerFactories)
                {
                    if (valueSerializerFactory.CanDeserialize(this, type))
                    {
                        return valueSerializerFactory.BuildSerializer(this, type, _deserializers);
                    }
                }

                serializer = new ObjectSerializer(type);

                _deserializers.TryAdd(type, serializer);
                CodeGenerator.BuildSerializer(this, type, (ObjectSerializer) serializer);
                //just ignore if this fails, another thread have already added an identical serialzer
            }
            return serializer;
        }

        //this returns a delegate for serializing a specific "field" of an instance of type "type"

        public void Serialize(object obj, Stream stream)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var session = new SerializerSession(this);

            var type = obj.GetType();
            var s = GetSerializerByType(type);
            s.WriteManifest(stream, type, session);
            s.WriteValue(stream, obj, session);
        }

        public T Deserialize<T>(Stream stream)
        {
            var session = new DeserializerSession(this);
            var s = GetDeserializerByManifest(stream, session);
            return (T) s.ReadValue(stream, session);
        }

        public object Deserialize(Stream stream)
        {
            var session = new DeserializerSession(this);
            var s = GetDeserializerByManifest(stream, session);
            return s.ReadValue(stream, session);
        }

        public ValueSerializer GetSerializerByType(Type type)
        {
            if (ReferenceEquals(type.GetTypeInfo().Assembly, CoreAssembly))
            {
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
            }

            if (type.IsArray && type.GetArrayRank() == 1)
            {
                var elementType = type.GetElementType();
                if (TypeEx.IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetCustomSerialzer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByType(Type type)
        {
            if (ReferenceEquals(type.GetTypeInfo().Assembly, CoreAssembly))
            {
                if (type == TypeEx.StringType)
                    return StringSerializer.Instance;

                if (type == TypeEx.UInt32Type)
                    return UInt32Serializer.Instance;

                if (type == TypeEx.UInt64Type)
                    return UInt64Serializer.Instance;

                if (type == TypeEx.UInt16Type)
                    return UInt16Serializer.Instance;

                if (type == TypeEx.Int32Type)
                    return Int32Serializer.Instance;

                if (type == TypeEx.Int64Type)
                    return Int64Serializer.Instance;

                if (type == TypeEx.Int16Type)
                    return Int16Serializer.Instance;

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

                if (type == TypeEx.TypeType || type == TypeEx.RuntimeType)
                    return TypeSerializer.Instance;
            }

            if (type.IsArray && type.GetArrayRank() == 1)
            {
                var elementType = type.GetElementType();
                if (TypeEx.IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetCustomDeserialzer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByManifest(Stream stream, DeserializerSession session)
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
                    var type = ObjectSerializer.GetTypeFromManifestFull(stream, session);
                    return GetCustomDeserialzer(type);
                }
                case ObjectSerializer.ManifestIndex:
                {
                    var type = ObjectSerializer.GetTypeFromManifestIndex(stream, session);
                    return GetCustomDeserialzer(type);
                }
                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }
    }
}