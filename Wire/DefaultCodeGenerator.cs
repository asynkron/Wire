using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wire.ExpressionDSL;
using Wire.ValueSerializers;

namespace Wire
{
    public class DefaultCodeGenerator : ICodeGenerator
    {
        public void BuildSerializer(Serializer serializer, ObjectSerializer objectSerializer)
        {
            var type = objectSerializer.Type;
            var fields = type.GetFieldInfosForType();
            var writer = GetFieldsWriter(serializer, fields);
            var reader = GetFieldsReader(serializer, fields, type);

            objectSerializer.Initialize(reader, writer);
        }

        private ObjectReader GetFieldsReader(Serializer serializer, IEnumerable<FieldInfo> fields, Type type)
        {
            var c = new Compiler<ObjectReader>();
            var stream = c.Parameter<Stream>("stream");
            var session = c.Parameter<DeserializerSession>("session");
            var newExpression = c.NewObject(type);
            var target = c.Variable<object>("target");
            var assignNewObjectToTarget = c.WriteVar(target, newExpression);

            c.Emit(assignNewObjectToTarget);

            if (serializer.Options.PreserveObjectReferences)
            {
                var trackDeserializedObjectMethod =
                    typeof(DeserializerSession).GetMethod(nameof(DeserializerSession.TrackDeserializedObject));

                c.EmitCall(trackDeserializedObjectMethod, session, target);
            }

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

            foreach (var field in fields)
            {
                var s = serializer.GetSerializerByType(field.FieldType);

                int read;
                if (!serializer.Options.VersionTolerance && field.FieldType.IsWirePrimitive())
                {
                    //Only optimize if property names are not included.
                    //if they are included, we need to be able to skip past unknown property data
                    //e.g. if sender have added a new property that the receiveing end does not yet know about
                    //which we cannot do w/o a manifest
                    var method = typeof(ValueSerializer).GetMethod(nameof(ValueSerializer.ReadValue));
                    var ss = c.Constant(s);
                    read = c.Call(method, ss, stream, session);
                }
                else
                {
                    var method = typeof(StreamExtensions).GetMethod(nameof(StreamExtensions.ReadObject));
                    read = c.StaticCall(method, stream, session);
                }

                var typedTarget = c.CastOrUnbox(target, type);

                var typedRead = c.Convert(read, field.FieldType);
                var assign = c.WriteField(field, typedTarget, typedRead);
                c.Emit(assign);
            }
            c.Emit(target);

            var readAllFields = c.Compile();
            return readAllFields;
        }

        //this generates a FieldWriter that writes all fields by unrolling all fields and calling them individually
        //no loops involved
        private ObjectWriter GetFieldsWriter(Serializer serializer, IEnumerable<FieldInfo> fields)
        {
            var c = new Compiler<ObjectWriter>();

            var stream = c.Parameter<Stream>("stream");
            var target = c.Parameter<object>("target");
            var session = c.Parameter<SerializerSession>("session");
            var preserveReferences = c.Constant(serializer.Options.PreserveObjectReferences);

            if (serializer.Options.PreserveObjectReferences)
            {
                var method = typeof(SerializerSession).GetMethod(nameof(SerializerSession.TrackSerializedObject));

                c.EmitCall(method, session, target);
            }

            foreach (var field in fields)
            {
                //get the serializer for the type of the field
                var valueSerializer = serializer.GetSerializerByType(field.FieldType);
                //runtime Get a delegate that reads the content of the given field

                var cast = c.CastOrUnbox(target, field.DeclaringType);
                var readField = c.ReadField(field, cast);

                //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
                if (!serializer.Options.VersionTolerance && field.FieldType.IsWirePrimitive())
                {
                    //primitive types does not need to write any manifest, if the field type is known
                    valueSerializer.EmitWriteValue(c, stream, readField, session);
                }
                else
                {
                    var converted = c.ConvertTo<object>(readField);
                    var valueType = field.FieldType;
                    if (field.FieldType.IsNullable())
                    {
                        var nullableType = field.FieldType.GetNullableElement();
                        valueSerializer = serializer.GetSerializerByType(nullableType);
                        valueType = nullableType;
                    }

                    var vs = c.Constant(valueSerializer);
                    var vt = c.Constant(valueType);

                    var method = typeof(StreamExtensions).GetMethod(nameof(StreamExtensions.WriteObject));

                    c.EmitStaticCall(method, stream, converted, vt, vs, preserveReferences, session);
                }
            }

            return c.Compile();
            ;
        }
    }
}