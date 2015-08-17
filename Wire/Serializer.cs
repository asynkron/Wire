using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Wire.ValueSerializers;

namespace Wire
{
    public class SerializerOptions
    {
        public SerializerOptions(bool includePropertyNames = false)
        {
            IncludePropertyNames = includePropertyNames;
        }
        public bool IncludePropertyNames { get; }
    }


    public class Serializer
    {
        private readonly Dictionary<Type, ValueSerializer> _serializers = new Dictionary<Type, ValueSerializer>();
        public SerializerOptions Options { get; }

        public Serializer()
        {
            Options = new SerializerOptions();
        }

        public Serializer(SerializerOptions options)
        {
            Options = options;
        }

        private static readonly Dictionary<Type, ValueSerializer> PrimitiveSerializers = new Dictionary
            <Type, ValueSerializer>
        {
            [typeof(int)] = Int32Serializer.Instance,
            [typeof(long)] = Int64Serializer.Instance,
            [typeof(short)] = Int16Serializer.Instance,
            [typeof(byte)] = ByteSerializer.Instance,
            [typeof(DateTime)] = DateTimeSerializer.Instance,
            [typeof(string)] = StringSerializer.Instance,
            [typeof(double)] = DoubleSerializer.Instance,
            [typeof(float)] = FloatSerializer.Instance,
            [typeof(Guid)] = GuidSerializer.Instance,
        };

        public static bool IsPrimitiveType(Type type)
        {
            return type == typeof (int) ||
                   type == typeof (long) ||
                   type == typeof (short) ||
                   type == typeof (DateTime) ||
                   type == typeof (bool) ||
                   type == typeof (string) ||
                   type == typeof (Guid) ||
                   type == typeof (float) ||
                   type == typeof (double) ||
                   type == typeof (decimal) ||
                   type == typeof (char);
        }


        private ValueSerializer GetSerialzerForPoco(Type type)
        {
            ValueSerializer serializer;
            if (!_serializers.TryGetValue(type, out serializer))
            {
                serializer = BuildSerializer(type);
                _serializers.Add(type, serializer);
            }
            return serializer;
        }

        private ValueSerializer BuildSerializer(Type type)
        {
            var fields = GetFieldsForType(type);

            var fieldWriters = new List<Action<Stream, object, SerializerSession>>();
            var fieldReaders = new List<Action<Stream, object, SerializerSession>>();
            var fieldNames = new List<byte[]>();
            foreach (var field in fields)
            {
                
                byte[] fieldName = Encoding.UTF8.GetBytes(field.Name);
                fieldNames.Add(fieldName);
                fieldWriters.Add(GenerateFieldSerializer(type, field));
                fieldReaders.Add(GenerateFieldDeserializer(field));
            }

            Action<Stream,object,SerializerSession> writer = (stream, o, session) =>
            {
                for (var index = 0; index < fieldWriters.Count; index++)
                {
                    if (session.Serializer.Options.IncludePropertyNames)
                    {
                        ByteArraySerializer.Instance.WriteValue(stream, fieldNames[index], session);
                    }
                    var fieldWriter = fieldWriters[index];
                    fieldWriter(stream, o, session);
                }
            };
            Func < Stream, SerializerSession, object> reader = (stream, session) =>
            {
                var instance = Activator.CreateInstance(type);
                for (var index = 0; index < fieldReaders.Count; index++)
                {
                    if (session.Serializer.Options.IncludePropertyNames)
                    {
                        var fieldName = (byte[]) ByteArraySerializer.Instance.ReadValue(stream, session);
                        //TODO: check if correct field
                    }

                    var fieldReader = fieldReaders[index];
                    fieldReader(stream, instance, session);
                }
                return instance;
            };
            var serializer = new ObjectSerializer(type,writer,reader);
            return serializer;
        }

        private static FieldInfo[] GetFieldsForType(Type type)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            var current = type;
            while (current != null)
            {
                var tfields = current.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                fieldInfos.AddRange(tfields);
                current = current.BaseType;
            }
            var fields = fieldInfos.OrderBy(f => f.Name) .ToArray();
            return fields;
        }

        private  Action<Stream, object, SerializerSession> GenerateFieldDeserializer(FieldInfo field)
        {
            var s = GetSerializerByType(field.FieldType);
            if (IsPrimitiveType(field.FieldType))
            {
                Action<Stream, object, SerializerSession> fieldReader = (stream, o, session) =>
                {
                    var value = s.ReadValue(stream, session);
                    field.SetValue(o, value);
                };
                return fieldReader;
            }
            else
            {
                Action<Stream, object, SerializerSession> fieldReader = (stream, o, session) =>
                {
                    var s2 = session.Serializer.GetSerializerByManifest(stream, session);
                    var value = s2.ReadValue(stream, session);
                    field.SetValue(o, value);
                };
                return fieldReader;
            }
        }

        private Action<Stream, object, SerializerSession> GenerateFieldSerializer(Type type, FieldInfo field)
        {
            var s = GetSerializerByType(field.FieldType);
            var getFieldValue = GenerateFieldReader(type, field);
            if (IsPrimitiveType(field.FieldType))
            {
                //primitive types does not need to write any manifest, if the field type is known
                //nor can they be null (StringSerializer has it's own null handling)
                Action<Stream, object, SerializerSession> fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);
                    s.WriteValue(stream, value, session);
                };
                return fieldWriter;
            }
            else
            {
                Action<Stream, object, SerializerSession> fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);
                    if (value == null) //value is null
                    {
                        NullSerializer.Instance.WriteManifest(stream, null, session);
                    }
                    else
                    {
                        var vType = value.GetType();
                        var s2 = s;
                        if (vType != field.FieldType)
                        {
                            //value is of subtype, lookup the serializer for that type
                            s2 = session.Serializer.GetSerializerByType(vType);
                        }
                        //lookup serializer for subtype
                        s2.WriteManifest(stream, vType, session);
                        s2.WriteValue(stream, value, session);
                    }
                };
                return fieldWriter;
            }
            
        }

        private static Func<object, object> GenerateFieldReader(Type type, FieldInfo f)
        {
            var param = Expression.Parameter(typeof (object));
            Expression castParam = Expression.Convert(param, type);
            Expression x = Expression.Field(castParam, f);
            Expression castRes = Expression.Convert(x, typeof (object));
            var getFieldValue = Expression.Lambda<Func<object, object>>(castRes, param).Compile();
            return getFieldValue;
        }

        public void Serialize(object obj, Stream stream)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var session = new SerializerSession
            {
                Buffer = new byte[100],
                Serializer = this
            };
            
            var type = obj.GetType();
            var s = GetSerializerByType(obj.GetType());
            s.WriteManifest(stream, type, session);
            s.WriteValue(stream, obj, session);
        }

        public T Deserialize<T>(Stream stream)
        {
            var session = new SerializerSession
            {
                Buffer = new byte[100],
                Serializer = this
            };
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

            if (type == typeof(int))
                return Int32Serializer.Instance;

            if (type == typeof(long))
                return Int64Serializer.Instance;

            if (type == typeof(short))
                return Int16Serializer.Instance;

            if (type == typeof(byte))
                return ByteSerializer.Instance;

            if (type == typeof(bool))
                return BoolSerializer.Instance;

            if (type == typeof(DateTime))
                return DateTimeSerializer.Instance;

            if (type == typeof(string))
                return StringSerializer.Instance;

            if (type == typeof(Guid))
                return GuidSerializer.Instance;

            if (type == typeof(float))
                return FloatSerializer.Instance;

            if (type == typeof(double))
                return DoubleSerializer.Instance;

            if (type == typeof(decimal))
                return DecimalSerializer.Instance;

            if (type == typeof(char))
                return CharSerializer.Instance;

            if (type == typeof(byte[]))
                return ByteArraySerializer.Instance;

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (IsPrimitiveType(elementType))
                {
                    return ConsistentArraySerializer.Instance;
                }
                throw new NotSupportedException(""); //array of other types
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
                    var type = GetNamedTypeFromManifest(stream, session);
                    return GetSerialzerForPoco(type);
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
 