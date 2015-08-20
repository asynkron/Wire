using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly Assembly CoreaAssembly = typeof (int).Assembly;

        //private static readonly Dictionary<Type, ValueSerializer> PrimitiveSerializers = new Dictionary
        //    <Type, ValueSerializer>
        //{
        //    [typeof (int)] = Int32Serializer.Instance,
        //    [typeof (long)] = Int64Serializer.Instance,
        //    [typeof (short)] = Int16Serializer.Instance,
        //    [typeof (byte)] = ByteSerializer.Instance,
        //    [typeof (DateTime)] = DateTimeSerializer.Instance,
        //    [typeof (string)] = StringSerializer.Instance,
        //    [typeof (double)] = DoubleSerializer.Instance,
        //    [typeof (float)] = FloatSerializer.Instance,
        //    [typeof (Guid)] = GuidSerializer.Instance
        //};

        private readonly ConcurrentDictionary<Type, ValueSerializer> _serializers =
            new ConcurrentDictionary<Type, ValueSerializer>();

        internal readonly SerializerOptions Options;

        public Serializer()
        {
            Options = new SerializerOptions();
        }

        public Serializer(SerializerOptions options)
        {
            Options = options;
        }

        public static bool IsPrimitiveType(Type type)
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueSerializer GetSerialzerForPoco(Type type)
        {
            ValueSerializer serializer;
            if (!_serializers.TryGetValue(type, out serializer))
            {
                if (type.IsArray)
                {
                    serializer = new ArraySerializer(type);
                    _serializers.TryAdd(type, serializer);
                    return serializer;
                }

                Surrogate surrogate = Options.Surrogates.FirstOrDefault(s => s.From.IsAssignableFrom(type));
                if (surrogate != null)
                {

                    serializer = CodeGenerator.BuildSerializer(this, surrogate.To);
                    var toSurrogateSerializer = new ToSurrogateSerializer(surrogate.ToSurrogate, serializer);
                    _serializers.TryAdd(type, toSurrogateSerializer);


                    var fromSurrogateSerializer = new FromSurrogateSerializer(surrogate.FromSurrogate, serializer);
                    _serializers.TryAdd(surrogate.To, fromSurrogateSerializer);

                    return toSurrogateSerializer;
                }

                serializer = CodeGenerator.BuildSerializer(this, type);
                _serializers.TryAdd(type, serializer);
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
            var s = GetSerializerByType(obj.GetType());
            s.WriteManifest(stream, type, session);
            s.WriteValue(stream, obj, session);
        }

        public T Deserialize<T>(Stream stream)
        {
            var session = new SerializerSession(this);
            var s = GetSerializerByManifest(stream, session);
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
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
            }

            var serializer = GetSerialzerForPoco(type);

            return serializer;
        }

        public ValueSerializer GetSerializerByManifest(Stream stream, SerializerSession session)
        {
            var first = stream.ReadByte();
            switch (first)
            {
                case 0:
                    return NullSerializer.Instance;
//TODO: hmm why havent I added 1?
                case 2:
                    return Int64Serializer.Instance;
                case 3:
                    return Int16Serializer.Instance;
                case 4:
                    return ByteSerializer.Instance;
                case 5:
                    return DateTimeSerializer.Instance;
                case 6:
                    return BoolSerializer.Instance;
                case 7:
                    return StringSerializer.Instance;
                case 8:
                    return Int32Serializer.Instance;
                case 9:
                    return ByteArraySerializer.Instance;
                //insert
                case 11:
                    return GuidSerializer.Instance;
                case 12:
                    return FloatSerializer.Instance;
                case 13:
                    return DoubleSerializer.Instance;
                case 14:
                    return DecimalSerializer.Instance;
                case 15:
                    return CharSerializer.Instance;
                case 254:
                    return ConsistentArraySerializer.Instance;
                case 255:
                {
                    var type = GetNamedTypeFromManifest(stream, session);
                    return GetSerialzerForPoco(type);
                }
                default:
                    throw new NotSupportedException("Unknown manifest value");
            }
        }

        public Type GetNamedTypeFromManifest(Stream stream, SerializerSession session)
        {
            var bytes = (byte[]) ByteArraySerializer.Instance.ReadValue(stream, session);
            var typename = Encoding.UTF8.GetString(bytes);
            var type = Type.GetType(typename);
            return type;
        }
    }
}