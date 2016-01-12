using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Wire.ValueSerializers;
using static System.Linq.Expressions.Expression;

namespace Wire
{
    public class CodeGenerator
    {
        public static void BuildSerializer(Serializer serializer, Type type, ObjectSerializer generatedSerializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (generatedSerializer == null)
                throw new ArgumentNullException(nameof(generatedSerializer));

            var fields = GetFieldInfosForType(type);

            var fieldWriters = new List<ValueWriter>();
            var fieldReaders = new List<Action<Stream, object, DeserializerSession>>();
            var fieldNames = new List<byte[]>();

            foreach (var field in fields)
            {
                var fieldName = Encoding.UTF8.GetBytes(field.Name);
                fieldNames.Add(fieldName);
                fieldWriters.Add(GenerateFieldInfoSerializer(serializer, field));
                fieldReaders.Add(GenerateFieldInfoDeserializer(serializer, type, field));
            }


            //concat all fieldNames including their length encoding and field count as header
            var versionTolerantHeader =
                fieldNames.Aggregate(Enumerable.Repeat((byte) fieldNames.Count, 1),
                    (current, fieldName) => current.Concat(BitConverter.GetBytes(fieldName.Length)).Concat(fieldName))
                    .ToArray();

            ValueWriter writeallFields;

            if (fieldWriters.Any())
            {
                writeallFields = GenerateWriteAllFieldsDelegate(fieldWriters);
            }
            else
            {
                writeallFields = (a, b, c) => { };
            }

            if (Debugger.IsAttached)
            {
                var tmp = writeallFields;
                writeallFields = (stream, o, session) =>
                {
                    try
                    {
                        tmp(stream, o, session);
                    }
                    catch (Exception x)
                    {
                        throw new Exception($"Unable to write all fields of {type.Name}", x);
                    }
                };
            }

            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            var versiontolerance = serializer.Options.VersionTolerance;
            ValueWriter writer = (stream, o, session) =>
            {
                if (versiontolerance)
                {
                    //write field count - cached
                    stream.Write(versionTolerantHeader);
                }


                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(o);
                }

                writeallFields(stream, o, session);
            };

            var reader = MakeReader(serializer, type, preserveObjectReferences, fields, fieldNames, fieldReaders);
            generatedSerializer.Initialize(reader, writer);
        }

        private static ValueReader MakeReader(Serializer serializer, Type type, bool preserveObjectReferences,
            FieldInfo[] fields,
            List<byte[]> fieldNames, List<Action<Stream, object, DeserializerSession>> fieldReaders)
        {
            ValueReader reader = (stream, session) =>
            {
                //create instance without calling constructor
                var instance = FormatterServices.GetUninitializedObject(type);
                if (preserveObjectReferences)
                {
                    session.TrackDeserializedObject(instance);
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
                        if (!Utils.UnsafeCompare(fieldName, fieldNames[i]))
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
            return reader;
        }

        private static ValueWriter GenerateWriteAllFieldsDelegate(
            List<ValueWriter> fieldWriters)
        {
            if (fieldWriters == null)
                throw new ArgumentNullException(nameof(fieldWriters));

            var streamParam = Parameter(typeof (Stream));
            var objectParam = Parameter(typeof (object));
            var sessionParam = Parameter(typeof (SerializerSession));
            var xs = fieldWriters
                .Select(Constant)
                .Select(
                    fieldWriterExpression =>
                        Invoke(fieldWriterExpression, streamParam, objectParam, sessionParam))
                .ToList();
            var body = Block(xs);
            var writeallFields =
                Lambda<ValueWriter>(body, streamParam, objectParam,
                    sessionParam)
                    .Compile();
            return writeallFields;
        }

        private static FieldInfo[] GetFieldInfosForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var fieldInfos = new List<FieldInfo>();
            var current = type;
            while (current != null)
            {
                var tfields =
                    current
                        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(f => !f.IsDefined(typeof (NonSerializedAttribute)))
                        .Where(f => !f.IsStatic)
                        .Where(f => f.FieldType != typeof (IntPtr))
                        .Where(f => f.FieldType != typeof (UIntPtr))
                        .Where(f => f.Name != "_syncRoot"); //HACK: ignore these 

                fieldInfos.AddRange(tfields);
                current = current.BaseType;
            }
            var fields = fieldInfos.OrderBy(f => f.Name).ToArray();
            return fields;
        }

        private static Action<Stream, object, DeserializerSession> GenerateFieldInfoDeserializer(Serializer serializer,
            Type type, FieldInfo field)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var s = serializer.GetSerializerByType(field.FieldType);

            Action<object, object> setter;
            if (field.IsInitOnly)
            {
                setter = field.SetValue;
            }
            else
            {
                var targetExp = Parameter(typeof (object), "target");
                var valueExp = Parameter(typeof (object), "value");

                // ReSharper disable once PossibleNullReferenceException
                Expression castTartgetExp = field.DeclaringType.IsValueType
                    ? Unbox(targetExp, type)
                    : Convert(targetExp, type);
                Expression castValueExp = Convert(valueExp, field.FieldType);
                var fieldExp = Field(castTartgetExp, field);
                var assignExp = Assign(fieldExp, castValueExp);
                setter = Lambda<Action<object, object>>(assignExp, targetExp, valueExp).Compile();
            }


            if (!serializer.Options.VersionTolerance && Serializer.IsPrimitiveType(field.FieldType))
            {
                //Only optimize if property names are not included.
                //if they are included, we need to be able to skip past unknown property data
                //e.g. if sender have added a new property that the receiveing end does not yet know about
                //which we cannot do w/o a manifest
                Action<Stream, object, DeserializerSession> fieldReader = (stream, o, session) =>
                {
                    var value = s.ReadValue(stream, session);
                    setter(o, value);
                };
                return fieldReader;
            }
            else
            {
                Action<Stream, object, DeserializerSession> fieldReader = (stream, o, session) =>
                {
                    var value = stream.ReadObject(session);
                    setter(o, value);
                };
                return fieldReader;
            }
        }

        private static ValueWriter GenerateFieldInfoSerializer(Serializer serializer, FieldInfo field)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (field == null)
                throw new ArgumentNullException(nameof(field));

            //get the serializer for the type of the field
            var valueSerializer = serializer.GetSerializerByType(field.FieldType);
            //runtime generate a delegate that reads the content of the given field
            var getFieldValue = GenerateFieldInfoReader(field);

            //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
            if (!serializer.Options.VersionTolerance && Serializer.IsPrimitiveType(field.FieldType))
            {
                //primitive types does not need to write any manifest, if the field type is known
                //nor can they be null (StringSerializer has it's own null handling)
                ValueWriter fieldWriter = (stream, o, session) =>
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

                ValueWriter fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);

                    stream.WriteObject(value, valueType, valueSerializer, preserveObjectReferences, session);
                };
                return fieldWriter;
            }
        }

        private static Func<object, object> GenerateFieldInfoReader(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var param = Parameter(typeof (object));
            // ReSharper disable once PossibleNullReferenceException
            Expression castParam = field.DeclaringType.IsValueType
                ? Unbox(param, field.DeclaringType)
                : Convert(param, field.DeclaringType);
            Expression readField = Field(castParam, field);
            Expression castRes = Convert(readField, typeof (object));
            var getFieldValue = Lambda<Func<object, object>>(castRes, param).Compile();

            if (Debugger.IsAttached)
            {
                return target =>
                {
                    try
                    {
                        return getFieldValue(target);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to read value of field {field.Name}", ex);
                    }
                };
            }
            return getFieldValue;
        }
    }
}