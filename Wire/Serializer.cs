using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Wire.ValueSerializers;

namespace Wire
{
    public class SerializerOptions
    {
        public SerializerOptions(bool versionTolerance = false)
        {
            VersionTolerance = versionTolerance;
        }

        public readonly bool VersionTolerance;
    }


    public class Serializer
    {
        private readonly ConcurrentDictionary<Type, ValueSerializer> _serializers = new ConcurrentDictionary<Type, ValueSerializer>();
        internal readonly SerializerOptions Options;
        private static readonly Type Int32Type = typeof(int);
        private static readonly Type Int64Type = typeof(long);
        private static readonly Type Int16Type = typeof(short);
        private static readonly Type ByteType = typeof(byte);
        private static readonly Type BoolType = typeof(bool);
        private static readonly Type DateTimeType = typeof(DateTime);
        private static readonly Type StringType = typeof(string);
        private static readonly Type GuidType = typeof(Guid);
        private static readonly Type FloatType = typeof(float);
        private static readonly Type DoubleType = typeof(double);
        private static readonly Type DecimalType = typeof(decimal);
        private static readonly Type CharType = typeof(char);
        private static readonly Type ByteArrayType = typeof(byte[]);
        private static readonly Assembly CoreaAssembly = typeof (int).Assembly;

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
                serializer = BuildSerializer(type);
                _serializers.TryAdd(type, serializer); //just ignore if this fails, another thread have already added an identical serialzer
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

            var fieldCount = BitConverter.GetBytes(fields.Length);
            var all = fieldCount.AsEnumerable();
            foreach (var fieldName in fieldNames)
            {
                all = all
                    .Concat(BitConverter.GetBytes(fieldName.Length))
                    .Concat(fieldName);
            }

            var allBytes = all.ToArray();


            Action<Stream, object, SerializerSession> writer = (stream, o, session) =>
            {
                if (session.Serializer.Options.VersionTolerance)
                {
                    //write field count - cached
                    stream.Write(allBytes, 0, allBytes.Length);
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var index = 0; index < fieldWriters.Count; index++)
                    {
                        var fieldWriter = fieldWriters[index];
                        fieldWriter(stream, o, session);
                    }
                }
                else
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var index = 0; index < fieldWriters.Count; index++)
                    {
                        var fieldWriter = fieldWriters[index];
                        fieldWriter(stream, o, session);
                    }
                }
               
            };
            Func < Stream, SerializerSession, object> reader = (stream, session) =>
            {
                var instance = Activator.CreateInstance(type);

                var fieldsToRead = fields.Length;
                if (session.Serializer.Options.VersionTolerance)
                {
                    var storedFieldCount = (int)Int32Serializer.Instance.ReadValue(stream, session);
                    if (storedFieldCount != fieldsToRead)
                    {
                        //TODO: 

                    }

                    for (int i = 0; i < storedFieldCount; i++)
                    {
                        var fieldName = (byte[])ByteArraySerializer.Instance.ReadValue(stream, session);
                    }

                    for (int i = 0; i < storedFieldCount; i++)
                    {
                        var fieldReader = fieldReaders[i];
                        fieldReader(stream, instance, session);
                    }
                }
                else
                {
                    for (int i = 0; i < fieldsToRead; i++)
                    {
                        var fieldReader = fieldReaders[i];
                        fieldReader(stream, instance, session);
                    }
                }

                //var index = 0;
                //while (fieldsToRead > 0)
                //{
                //    fieldsToRead--;
                //    if (session.Serializer.Options.VersionTolerance)
                //    {
                //        var fieldName = (byte[])ByteArraySerializer.Instance.ReadValue(stream, session);
                //        if (!UnsafeCompare(fieldName, fieldNames[index]))
                //        {
                //            var foundName = Encoding.UTF8.GetString(fieldName);
                //            var expectedName = Encoding.UTF8.GetString(fieldNames[index]);
                //            if (string.CompareOrdinal(foundName, expectedName) < 0) //TODO: is it the reversed
                //            {
                //                throw new Exception($"Expected to find field with name '{expectedName}' but found '{foundName}' when deserializing object of type '{type.Name}'");
                //            }
                //            //get the manifest for the unknown field
                //            var s2 = GetSerializerByManifest(stream, session);
                //            //read the field value and ignore it
                //            s2.ReadValue(stream, session); //TODO: make it possible to ignore data being read, if types have been removed on receiveing end
                //            //stay on the same index, read the next field from the stream
                //            continue;
                //        }
                //    }

                //    var fieldReader = fieldReaders[index];
                //    fieldReader(stream, instance, session);
                //    index++;
                //}
                return instance;
            };
            var serializer = new ObjectSerializer(type,writer,reader);
            return serializer;
        }

        static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l/8; i++, x1 += 8, x2 += 8)
                    if (*((long*) x1) != *((long*) x2)) return false;
                if ((l & 4) != 0)
                {
                    if (*((int*) x1) != *((int*) x2)) return false;
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*((short*) x1) != *((short*) x2)) return false;
                    x1 += 2;
                    x2 += 2;
                }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
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
            if (IsPrimitiveType(field.FieldType) && !Options.VersionTolerance)
            {
                //Only optimize if property names are not included.
                //if they are included, we need to be able to skip past unknown property data
                //e.g. if sender have added a new property that the receiveing end does not yet know about
                //which we cannot do w/o a manifest
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

        //this returns a delegate for serializing a specific "field" of an instance of type "type"
        private Action<Stream, object, SerializerSession> GenerateFieldSerializer(Type type, FieldInfo field)
        {
            //get the serializer for the type of the field
            var s = GetSerializerByType(field.FieldType);
            //runtime generate a delegate that reads the content of the given field
            var getFieldValue = GenerateFieldReader(type, field);

            //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
            if (IsPrimitiveType(field.FieldType) && !Options.VersionTolerance)
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
                var fieldType = field.FieldType;
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof (Nullable<>))
                {
                    var nullableType = field.FieldType.GetGenericArguments()[0];
                    s = GetSerializerByType(nullableType);
                    fieldType = nullableType;
                }

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
                        if (vType != fieldType)
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
 