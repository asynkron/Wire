using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Wire.ValueSerializers;
using static System.Linq.Expressions.Expression;

namespace Wire
{
    public class CodeGenerator
    {
        public static void BuildSerializer(Serializer serializer, Type type, ObjectSerializer objectSerializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (objectSerializer == null)
                throw new ArgumentNullException(nameof(objectSerializer));

            var fields = GetFieldInfosForType(type);

            var fieldWriters = new List<ObjectWriter>();
            var fieldReaders = new List<FieldReader>();
            var fieldNames = new List<byte[]>();

            foreach (var field in fields)
            {
                var fieldName = Encoding.UTF8.GetBytes(field.Name);
                fieldNames.Add(fieldName);
                fieldWriters.Add(GetValueWriter(serializer, field));
                fieldReaders.Add(GetFieldReader(serializer, type, field));
            }

            FieldsWriter writeFields;

            if (fieldWriters.Any())
            {
                writeFields = GetFieldsWriter(fieldWriters);
            }
            else
            {
                writeFields = (a, b, c) => { };
            }

            if (Debugger.IsAttached)
            {
                var tmp = writeFields;
                writeFields = (stream, o, session) =>
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
            var typeManifest = GetTypeManifest(fieldNames);
            ObjectWriter writer = (stream, o, session) =>
            {
                if (versiontolerance)
                {
                    stream.Write(typeManifest);
                }

                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(o);
                }

                writeFields(stream, o, session);
            };

            var reader = GetReader(serializer, type, preserveObjectReferences, fields, fieldNames, fieldReaders);
            objectSerializer.Initialize(reader, writer);
        }

        private static byte[] GetTypeManifest(IReadOnlyCollection<byte[]> fieldNames)
        {
            IEnumerable<byte> result = new[] {(byte) fieldNames.Count};
            foreach (var name in fieldNames)
                result = result.Concat(BitConverter.GetBytes(name.Length)).Concat(name);
            var versionTolerantHeader = result.ToArray();
            return versionTolerantHeader;
        }

        private static ObjectReader GetReader(
            Serializer serializer,
            Type type,
            bool preserveObjectReferences,
            IReadOnlyList<FieldInfo> fields,
            IReadOnlyList<byte[]> fieldNames,
            IReadOnlyList<FieldReader> fieldReaders)
        {
            ObjectReader reader = (stream, session) =>
            {
                //create instance without calling constructor
                var instance = type.GetEmptyObject();
                if (preserveObjectReferences)
                {
                    session.TrackDeserializedObject(instance);
                }

                var fieldsToRead = fields.Count;
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

        private static FieldsWriter GetFieldsWriter(IReadOnlyList<ObjectWriter> fieldWriters)
        {
            if (fieldWriters == null)
                throw new ArgumentNullException(nameof(fieldWriters));

            var streamParam = Parameter(typeof(Stream));
            var objectParam = Parameter(typeof(object));
            var sessionParam = Parameter(typeof(SerializerSession));
            var xs = fieldWriters
                .Select(Constant)
                .Select(fieldWriterExpression => 
                    Invoke(fieldWriterExpression, streamParam, objectParam, sessionParam))
                .ToList();

            var body = Block(xs);
            var writeallFields =
                Lambda<FieldsWriter>(body, streamParam, objectParam,
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
                        .GetTypeInfo()
                        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
#if SERIALIZATION
                        .Where(f => !f.IsDefined(typeof(NonSerializedAttribute)))
#endif
                        .Where(f => !f.IsStatic)
                        .Where(f => f.FieldType != typeof(IntPtr))
                        .Where(f => f.FieldType != typeof(UIntPtr))
                        .Where(f => f.Name != "_syncRoot"); //HACK: ignore these 

                fieldInfos.AddRange(tfields);
                current = current.GetTypeInfo().BaseType;
            }
            var fields = fieldInfos.OrderBy(f => f.Name).ToArray();
            return fields;
        }

        private static FieldReader GetFieldReader(
            Serializer serializer,
            Type type, 
            FieldInfo field)
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
                //TODO: field is readonly, can we set it via IL or only via reflection
                setter = field.SetValue;
            }
            else
            {
                var targetExp = Parameter(typeof(object), "target");
                var valueExp = Parameter(typeof(object), "value");

                // ReSharper disable once PossibleNullReferenceException
                Expression castTartgetExp = field.DeclaringType.GetTypeInfo().IsValueType
                    ? Unbox(targetExp, type)
                    : Convert(targetExp, type);
                Expression castValueExp = Convert(valueExp, field.FieldType);
                var fieldExp = Field(castTartgetExp, field);
                var assignExp = Assign(fieldExp, castValueExp);
                setter = Lambda<Action<object, object>>(assignExp, targetExp, valueExp).Compile();
            }


            if (!serializer.Options.VersionTolerance && field.FieldType.IsWirePrimitive())
            {
                //Only optimize if property names are not included.
                //if they are included, we need to be able to skip past unknown property data
                //e.g. if sender have added a new property that the receiveing end does not yet know about
                //which we cannot do w/o a manifest
                FieldReader fieldReader = (stream, o, session) =>
                {
                    var value = s.ReadValue(stream, session);
                    setter(o, value);
                };
                return fieldReader;
            }
            else
            {
                FieldReader fieldReader = (stream, o, session) =>
                {
                    var value = stream.ReadObject(session);
                    setter(o, value);
                };
                return fieldReader;
            }
        }

        private static ObjectWriter GetValueWriter(Serializer serializer, FieldInfo field)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (field == null)
                throw new ArgumentNullException(nameof(field));

            //get the serializer for the type of the field
            var valueSerializer = serializer.GetSerializerByType(field.FieldType);
            //runtime Get a delegate that reads the content of the given field
            var getFieldValue = GetFieldInfoReader(field);

            //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
            if (!serializer.Options.VersionTolerance && field.FieldType.IsWirePrimitive())
            {
                //primitive types does not need to write any manifest, if the field type is known
                //nor can they be null (StringSerializer has it's own null handling)
                ObjectWriter fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);
                    valueSerializer.WriteValue(stream, value, session);
                };
                return fieldWriter;
            }
            else
            {
                var valueType = field.FieldType;
                if (field.FieldType.GetTypeInfo().IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var nullableType = field.FieldType.GetTypeInfo().GetGenericArguments()[0];
                    valueSerializer = serializer.GetSerializerByType(nullableType);
                    valueType = nullableType;
                }
                var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

                ObjectWriter fieldWriter = (stream, o, session) =>
                {
                    var value = getFieldValue(o);

                    stream.WriteObject(value, valueType, valueSerializer, preserveObjectReferences, session);
                };
                return fieldWriter;
            }
        }

        private static Func<object, object> GetFieldInfoReader(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var param = Parameter(typeof(object));
            // ReSharper disable once PossibleNullReferenceException
            Expression castParam = field.DeclaringType.GetTypeInfo().IsValueType
                // ReSharper disable once AssignNullToNotNullAttribute
                ? Unbox(param, field.DeclaringType)
                // ReSharper disable once AssignNullToNotNullAttribute
                : Convert(param, field.DeclaringType);
            Expression readField = Field(castParam, field);
            Expression castRes = Convert(readField, typeof(object));
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

        public static Func<object, object> CompileToDelegate(MethodInfo method, Type argType)
        {
            var arg = Parameter(typeof(object));
            var castArg = Convert(arg, argType);
            var call = Call(method, new Expression[] {castArg});
            var castRes = Convert(call, typeof(object));
            var lambda = Lambda<Func<object, object>>(castRes, arg);
            var compiled = lambda.Compile();
            return compiled;
        }
    }
}