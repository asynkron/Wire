using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Wire.ValueSerializers;

namespace Wire
{
    public class Serializer
    {
        private static readonly Type Int32Type = typeof (int);
        private static readonly Type Int64Type = typeof (long);
        private static readonly Type Int16Type = typeof (short);
        private static readonly Type ByteType = typeof (byte);
        private static readonly Type BoolType = typeof (bool);
        private static readonly Type DateTimeType = typeof (DateTime);
        private static readonly Type StringType = typeof (string);
        private static readonly Type GuidType = typeof (Guid);
        private static readonly Type FloatType = typeof (float);
        private static readonly Type DoubleType = typeof (double);
        private static readonly Type DecimalType = typeof (decimal);
        private static readonly Type CharType = typeof (char);
        private static readonly Type ByteArrayType = typeof (byte[]);
        private static readonly Type TypeType = typeof (Type);
        private static readonly Type RuntimeType = Type.GetType("System.RuntimeType");
        private static readonly Assembly CoreaAssembly = typeof (int).Assembly;

        private readonly ConcurrentDictionary<Type, ValueSerializer> _deserializers =
            new ConcurrentDictionary<Type, ValueSerializer>();

        private readonly ConcurrentDictionary<Type, ValueSerializer> _serializers =
            new ConcurrentDictionary<Type, ValueSerializer>();

        internal readonly SerializerOptions Options;

        public Serializer() : this ( new SerializerOptions())
        {
        }

        public Serializer(SerializerOptions options) 
        {
            Options = options;
        }

        internal static bool IsPrimitiveType(Type type)
        {
            return type == Int32Type ||
                   type == Int64Type ||
                   type == Int16Type ||
                   type == DateTimeType ||
                   type == BoolType ||
                   type == StringType ||
                   type == GuidType ||
                   type == FloatType ||
                   type == DoubleType ||
                   type == DecimalType ||
                   type == CharType;
            //add TypeSerializer with null support
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
            var session = new SerializerSession(this);
            var s = GetDeserializerByManifest(stream, session);
            return (T) s.ReadValue(stream, session);
        }

        public ValueSerializer GetSerializerByType(Type type)
        {
            //TODO: code generate this
            //ValueSerializer tmp;
            //if (_primitiveSerializers.TryGetValue(type, out tmp))
            //{
            //    return tmp;
            //}

            if (ReferenceEquals(type.Assembly, CoreaAssembly))
            {
                if (type == StringType)
                    return StringSerializer.Instance;

                if (type == Int32Type)
                    return Int32Serializer.Instance;

                if (type == Int64Type)
                    return Int64Serializer.Instance;

                if (type == Int16Type)
                    return Int16Serializer.Instance;

                if (type == ByteType)
                    return ByteSerializer.Instance;

                if (type == BoolType)
                    return BoolSerializer.Instance;

                if (type == DateTimeType)
                    return DateTimeSerializer.Instance;

                if (type == GuidType)
                    return GuidSerializer.Instance;

                if (type == FloatType)
                    return FloatSerializer.Instance;

                if (type == DoubleType)
                    return DoubleSerializer.Instance;

                if (type == DecimalType)
                    return DecimalSerializer.Instance;

                if (type == CharType)
                    return CharSerializer.Instance;

                if (type == ByteArrayType)
                    return ByteArraySerializer.Instance;

                if (type == TypeType || type == RuntimeType)
                    return TypeSerializer.Instance;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetCustomSerialzer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByType(Type type)
        {
            if (ReferenceEquals(type.Assembly, CoreaAssembly))
            {
                if (type == StringType)
                    return StringSerializer.Instance;

                if (type == Int32Type)
                    return Int32Serializer.Instance;

                if (type == Int64Type)
                    return Int64Serializer.Instance;

                if (type == Int16Type)
                    return Int16Serializer.Instance;

                if (type == ByteType)
                    return ByteSerializer.Instance;

                if (type == BoolType)
                    return BoolSerializer.Instance;

                if (type == DateTimeType)
                    return DateTimeSerializer.Instance;

                if (type == GuidType)
                    return GuidSerializer.Instance;

                if (type == FloatType)
                    return FloatSerializer.Instance;

                if (type == DoubleType)
                    return DoubleSerializer.Instance;

                if (type == DecimalType)
                    return DecimalSerializer.Instance;

                if (type == CharType)
                    return CharSerializer.Instance;

                if (type == ByteArrayType)
                    return ByteArraySerializer.Instance;

                if (type == TypeType || type == RuntimeType)
                    return TypeSerializer.Instance;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetCustomDeserialzer(type);

            return serializer;
        }

        public ValueSerializer GetDeserializerByManifest(Stream stream, SerializerSession session)
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
                case ObjectReferenceSerializer.Manifest:
                    return ObjectReferenceSerializer.Instance;
                case ConsistentArraySerializer.Manifest:
                    return ConsistentArraySerializer.Instance;
                case ObjectSerializer.Manifest:
                {
                    var type = GetNamedTypeFromManifest(stream, session);
                    return GetCustomDeserialzer(type);
                }
                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }

        private static readonly ConcurrentDictionary<byte[],Type> TypeNameLookup = new ConcurrentDictionary<byte[], Type>(new ByteArrayEqualityComparer()); 

        public Type GetNamedTypeFromManifest(Stream stream, SerializerSession session)
        {
            var bytes = (byte[]) ByteArraySerializer.Instance.ReadValue(stream, session);
            
            return TypeNameLookup.GetOrAdd(bytes, b =>
            {
                var typename = Encoding.UTF8.GetString(b);
                return Type.GetType(typename);
            });
        }
    }
}