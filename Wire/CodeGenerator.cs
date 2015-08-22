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
    public class CodeGenerator
    {
        public static void BuildSerializer(Serializer serializer, Type type, ObjectSerializer result)
        {
            var fields = GetFieldsForType(type);

            var fieldWriters = new List<Action<Stream, object, SerializerSession>>();
            var fieldReaders = new List<Action<Stream, object, SerializerSession>>();
            var fieldNames = new List<byte[]>();

            foreach (var field in fields)
            {
                var fieldName = Encoding.UTF8.GetBytes(field.Name);
                fieldNames.Add(fieldName);
                fieldWriters.Add(GenerateFieldSerializer(serializer, type, field));
                fieldReaders.Add(GenerateFieldDeserializer(serializer,type, field));
            }


            //concat all fieldNames including their length encoding and field count as header
            var versionTolerantHeader =
                fieldNames.Aggregate(Enumerable.Repeat((byte) fieldNames.Count, 1),
                    (current, fieldName) => current.Concat(BitConverter.GetBytes(fieldName.Length)).Concat(fieldName))
                    .ToArray();

            Action<Stream, object, SerializerSession> writeallFields = null;

            if (fieldWriters.Any())
            {
                writeallFields = GenerateWriteAllFieldsDelegate(fieldWriters);
            }
            else
            {
                writeallFields = (_1, _2, _3) => { };
            }

            Action<Stream, object, SerializerSession> writer = (stream, o, session) =>
            {
                if (serializer.Options.VersionTolerance)
                {
                    //write field count - cached
                    stream.Write(versionTolerantHeader);
                }

                writeallFields(stream, o, session);
            };

            //avoid one level of invocation
            if (serializer.Options.VersionTolerance == false)
            {
                writer = writeallFields;
            }

            //  var readAllFieldsVersionIntolerant =  GenerateReadAllFieldsVersionIntolerant(fieldReaders);

            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            Func<Stream, SerializerSession, object> reader = (stream, session) =>
            {
                var instance = Activator.CreateInstance(type);
                if (preserveObjectReferences)
                {
                    session.ObjectById.Add(session.NextObjectId, instance);
                    session.NextObjectId++;
                }

                var fieldsToRead = fields.Length;
                if (serializer.Options.VersionTolerance)
                {
                    var storedFieldCount = stream.ReadByte();
                    if (storedFieldCount != fieldsToRead)
                    {
                        //TODO: 
                    }

                    for (var i = 0; i < storedFieldCount; i++)
                    {
                        var fieldName = stream.ReadLengthEncodedByteArray(session);
                        if (!UnsafeCompare(fieldName, fieldNames[i]))
                        {
                            //TODO
                        }
                    }

                    //   writeallFields(stream, instance, session);
                    for (var i = 0; i < storedFieldCount; i++)
                    {
                        var fieldReader = fieldReaders[i];
                        fieldReader(stream, instance, session);
                    }
                }
                else
                {
                    //  writeallFields(stream, instance, session);
                    for (var i = 0; i < fieldsToRead; i++)
                    {
                        var fieldReader = fieldReaders[i];
                        fieldReader(stream, instance, session);
                    }
                }

                return instance;
            };

            result._writer = writer;
            result._reader = reader;
        }

//        private static Action<Stream, object, SerializerSession> GenerateReadAllFieldsVersionIntolerant(List<Action<Stream, object, SerializerSession>> fieldReaders)
//        {
////TODO: handle version tolerance
//            var streamParam = Expression.Parameter(typeof (Stream));
//            var objectParam = Expression.Parameter(typeof (object));
//            var sessionParam = Expression.Parameter(typeof (SerializerSession));
//            var xs = fieldReaders
//                .Select(Expression.Constant)
//                .Select(
//                    fieldReaderExpression => Expression.Invoke(fieldReaderExpression, streamParam, objectParam, sessionParam))
//                .ToList();
//            var body = Expression.Block(xs);

//            Action<Stream, object, SerializerSession> readAllFields =
//                Expression.Lambda<Action<Stream, object, SerializerSession>>(body, streamParam, objectParam, sessionParam)
//                    .Compile();

//            return readAllFields;
//        }

        private static Action<Stream, object, SerializerSession> GenerateWriteAllFieldsDelegate(
            List<Action<Stream, object, SerializerSession>> fieldWriters)
        {
            var streamParam = Expression.Parameter(typeof (Stream));
            var objectParam = Expression.Parameter(typeof (object));
            var sessionParam = Expression.Parameter(typeof (SerializerSession));
            var xs = fieldWriters
                .Select(Expression.Constant)
                .Select(
                    fieldWriterExpression =>
                        Expression.Invoke(fieldWriterExpression, streamParam, objectParam, sessionParam))
                .ToList();
            var body = Expression.Block(xs);
            var writeallFields =
                Expression.Lambda<Action<Stream, object, SerializerSession>>(body, streamParam, objectParam,
                    sessionParam)
                    .Compile();
            return writeallFields;
        }

        public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                var l = a1.Length;
                for (var i = 0; i < l/8; i++, x1 += 8, x2 += 8)
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
            var fieldInfos = new List<FieldInfo>();
            var current = type;
            while (current != null)
            {
                var tfields =
                    current
                        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(f => !f.IsDefined(typeof (NonSerializedAttribute)))
                        .Where(f => !f.IsStatic)
                        .Where(f => f.Name != "_syncRoot"); //HACK: ignore these 

                fieldInfos.AddRange(tfields);
                current = current.BaseType;
            }
            var fields = fieldInfos.OrderBy(f => f.Name).ToArray();
            return fields;
        }

       

        private static Action<Stream, object, SerializerSession> GenerateFieldDeserializer(Serializer serializer,
            Type type, FieldInfo field)
        {
            var s = serializer.GetSerializerByType(field.FieldType);

            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");
            Expression castTartgetExp = Expression.Convert(targetExp, type);
            Expression castValueExp = Expression.Convert(valueExp, field.FieldType);
            MemberExpression fieldExp = Expression.Field(castTartgetExp, field);
            BinaryExpression assignExp = Expression.Assign(fieldExp, castValueExp);
            var setter = Expression.Lambda<Action<object, object>> (assignExp, targetExp, valueExp).Compile();

            if (!serializer.Options.VersionTolerance && Serializer.IsPrimitiveType(field.FieldType))
            {
                //Only optimize if property names are not included.
                //if they are included, we need to be able to skip past unknown property data
                //e.g. if sender have added a new property that the receiveing end does not yet know about
                //which we cannot do w/o a manifest
                Action<Stream, object, SerializerSession> fieldReader = (stream, o, session) =>
                {
                    var value = s.ReadValue(stream, session);
                    //setter(o, value);
                    //var x = field.GetValue(o);
                    //if (value != null && !value.Equals(x))
                    //{

                    //}
                    field.SetValue(o, value);
                };
                return fieldReader;
            }
            else
            {
                Action<Stream, object, SerializerSession> fieldReader = (stream, o, session) =>
                {
                    var value = stream.ReadObject(session);
                    field.SetValue(o, value);
                    //setter(o, value);
                    //var x = field.GetValue(o);
                    //if (value != null && !value.Equals(x))
                    //{

                    //}
                };
                return fieldReader;
            }
        }

        private static Action<Stream, object, SerializerSession> GenerateFieldSerializer(Serializer serializer,
            Type type, FieldInfo field)
        {
            //get the serializer for the type of the field
            var valueSerializer = serializer.GetSerializerByType(field.FieldType);
            //runtime generate a delegate that reads the content of the given field
            var getFieldValue = GenerateFieldReader(type, field);

            //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
            if (!serializer.Options.VersionTolerance && Serializer.IsPrimitiveType(field.FieldType))
            {
                //primitive types does not need to write any manifest, if the field type is known
                //nor can they be null (StringSerializer has it's own null handling)
                Action<Stream, object, SerializerSession> fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);
                    valueSerializer.WriteValue(stream, value, session);
                };
                return fieldWriter;
            }
            else
            {
                var valueType = field.FieldType;
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof (Nullable<>))
                {
                    var nullableType = field.FieldType.GetGenericArguments()[0];
                    valueSerializer = serializer.GetSerializerByType(nullableType);
                    valueType = nullableType;
                }
                var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

                Action<Stream, object, SerializerSession> fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);
                    stream.WriteObject(value, valueType, valueSerializer, preserveObjectReferences, session);
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
    }
}