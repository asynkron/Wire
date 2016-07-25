using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Wire.ValueSerializers;

namespace Wire
{
    public interface ICodeGenerator
    {
        void BuildSerializer(Serializer serializer, ObjectSerializer objectSerializer);
    }

    public class DefaultCodeGenerator : ICodeGenerator
    {
        public  void BuildSerializer(Serializer serializer, ObjectSerializer objectSerializer)
        {
            var type = objectSerializer.Type;
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (objectSerializer == null)
                throw new ArgumentNullException(nameof(objectSerializer));

            var fields = ReflectionEx.GetFieldInfosForType(type);

            var fieldWriters = new List<ObjectWriter>();
            var fieldReaders = new List<FieldReader>();

            foreach (var field in fields)
            {
                fieldWriters.Add(GetObjectWriter(serializer, field));
                fieldReaders.Add(GetFieldReader(serializer, type, field));
            }

            var writeFields = fieldWriters.Any() ? 
                GetFieldsWriter(fieldWriters) : 
                ((a, b, c) => { }); //empty writer

            //if the debugger is attached, wrap the fields writer in a try catch block and throw readable exceptions
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
            ObjectWriter writer = (stream, o, session) =>
            {
                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(o);
                }

                writeFields(stream, o, session);
            };

            var reader = serializer.Options.VersionTolerance ? 
                GetVersionTolerantReader(type, preserveObjectReferences, fields, fieldReaders) : 
                GetVersionIntolerantReader(type, preserveObjectReferences, fieldReaders);
            
            objectSerializer.Initialize(reader, writer);
        }



        private ObjectReader GetVersionIntolerantReader(
            Type type,
            bool preserveObjectReferences,
            IEnumerable<FieldReader> fieldReaders)
        {

            var expressions = new List<Expression>();

            var newExpression = GetNewExpression(type);
            var targetObject = Expression.Variable(typeof(object),"target");
            var assignTarget = Expression.Assign(targetObject, newExpression);
            var streamParam = Expression.Parameter(typeof(Stream));
            var sessionParam = Expression.Parameter(typeof(DeserializerSession));

            expressions.Add(assignTarget);

            if (preserveObjectReferences)
            {
                var trackDeserializedObjectMethod =
                typeof(DeserializerSession).GetMethod(nameof(DeserializerSession.TrackDeserializedObject));
                var call = Expression.Call(sessionParam, trackDeserializedObjectMethod, targetObject);
                expressions.Add(call);
            }

            foreach (var r in fieldReaders)
            {
                var c = Expression.Constant(r);
                var i = Expression.Invoke(c, streamParam, targetObject, sessionParam);
                expressions.Add(i);
            }
            expressions.Add(targetObject);

            var body = Expression.Block(new[] { targetObject }, expressions);

            var readAllFields = Expression
                .Lambda<ObjectReader>(body, streamParam, sessionParam)
                .Compile();

            return readAllFields;
        }

        private static Expression GetNewExpression(Type type)
        {
            var defaultCtor = type.GetConstructor(new Type[] { });
            var il = defaultCtor?.GetMethodBody()?.GetILAsByteArray();
            var sideEffectFreeCtor = il != null && il.Length <= 8;
            if (sideEffectFreeCtor)
            {
                return Expression.New(defaultCtor);
            }
            var emptyObjectMethod = typeof(TypeEx).GetMethod(nameof(TypeEx.GetEmptyObject));
            var emptyObject = Expression.Call(null, emptyObjectMethod, Expression.Constant(type));

            return emptyObject;
        }

        private  ObjectReader GetVersionTolerantReader(Type type,
            bool preserveObjectReferences,
            IReadOnlyList<FieldInfo> fields,
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

                var versionInfo = session.GetVersionInfo(type);
                

                //for (var i = 0; i < storedFieldCount; i++)
                //{
                //    var fieldName = stream.ReadLengthEncodedByteArray(session);
                //    if (!Utils.UnsafeCompare(fieldName, fieldNames[i]))
                //    {
                //        //TODO: field name mismatch
                //        //this should really be a compare less equal or greater
                //        //to know if the field is added or removed

                //        //1) if names are equal, read the value and assign the field

                //        //2) if the field is less than the expected field, then this field is an unknown new field
                //        //we need to read this object and just ignore its content.

                //        //3) if the field is greater than the expected, we need to check the next expected until
                //        //the current is less or equal, then goto 1)
                //    }
                //}

                //this should be moved up in the version tolerant loop
                foreach (var fieldReader in fieldReaders)
                {
                    fieldReader(stream, instance, session);
                }

                return instance;
            };
            return reader;
        }

        //this generates a FieldWriter that writes all fields by unrolling all fields and calling them individually
        //no loops involved
        private  FieldsWriter GetFieldsWriter(IReadOnlyList<ObjectWriter> fieldWriters)
        {
            if (fieldWriters == null)
                throw new ArgumentNullException(nameof(fieldWriters));

            var streamParam = Expression.Parameter(typeof(Stream));
            var objectParam = Expression.Parameter(typeof(object));
            var sessionParam = Expression.Parameter(typeof(SerializerSession));
            var xs = fieldWriters
                .Select(Expression.Constant)
                .Select(fieldWriterExpression => 
                    Expression.Invoke(fieldWriterExpression, streamParam, objectParam, sessionParam))
                .ToList();

            var body = Expression.Block(xs);
            var writeallFields =
                Expression.Lambda<FieldsWriter>(body, streamParam, objectParam,
                    sessionParam)
                    .Compile();
            return writeallFields;
        }

        private  FieldReader GetFieldReader(
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
            
            FieldInfoWriter setter;// = GetSetDelegate(field);
            if (field.IsInitOnly)
            {
                //TODO: field is readonly, can we set it via IL or only via reflection
                setter = field.SetValue;
            }
            else
            {
                var targetExp = Expression.Parameter(typeof(object), "target");
                var valueExp = Expression.Parameter(typeof(object), "value");

                // ReSharper disable once PossibleNullReferenceException
                Expression castTartgetExp = field.DeclaringType.GetTypeInfo().IsValueType
                    ? Expression.Unbox(targetExp, type)
                    : Expression.Convert(targetExp, type);
                Expression castValueExp = Expression.Convert(valueExp, field.FieldType);
                var fieldExp = Expression.Field(castTartgetExp, field);
                var assignExp = Expression.Assign(fieldExp, castValueExp);
                setter = Expression.Lambda<FieldInfoWriter>(assignExp, targetExp, valueExp).Compile();
            }

            var s = serializer.GetSerializerByType(field.FieldType);
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

        private  ObjectWriter GetObjectWriter(Serializer serializer, FieldInfo field)
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

        private  FieldInfoReader GetFieldInfoReader(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var param = Expression.Parameter(typeof(object));
            // ReSharper disable once PossibleNullReferenceException
            Expression castParam = field.DeclaringType.GetTypeInfo().IsValueType
                // ReSharper disable once AssignNullToNotNullAttribute
                ? Expression.Unbox(param, field.DeclaringType)
                // ReSharper disable once AssignNullToNotNullAttribute
                : Expression.Convert(param, field.DeclaringType);
            Expression readField = Expression.Field(castParam, field);
            Expression castRes = Expression.Convert(readField, typeof(object));
            var getFieldValue = Expression.Lambda<FieldInfoReader>(castRes, param).Compile();

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