// -----------------------------------------------------------------------
//   <copyright file="DefaultCodeGenerator.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FastExpressionCompiler.LightExpression;
using Wire.Buffers;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.Compilation
{
    public static class SerializerCompiler
    {
        public const string PreallocatedByteBuffer = nameof(PreallocatedByteBuffer);

        public static void BuildSerializer(Serializer serializer, ObjectSerializer objectSerializer)
        {
            var type = objectSerializer.Type;
            var fields = type.GetFieldInfosForType();
            
            var writer = GetFieldsWriter(serializer, fields,type, out var bufferSize);
            var reader = GetFieldsReader(serializer, fields, type);

            objectSerializer.Initialize(reader, writer, bufferSize);
        }

        private static ObjectReader GetFieldsReader(Serializer serializer, FieldInfo[] fields,
            Type type)
        {
            var c = new Compiler(typeof(ObjectReader).GetMethod(nameof(ObjectReader.Read)));
            var stream = c.Parameter<Stream>("stream");
            var session = c.Parameter<DeserializerSession>("session");
            var newExpression = c.NewObject(type);
            var target = c.Variable("target", type);
            var assignNewObjectToTarget = c.WriteVar(target, newExpression);

            c.Emit(assignNewObjectToTarget);

            if (serializer.Options.PreserveObjectReferences)
            {
                var trackDeserializedObjectMethod =
                    typeof(DeserializerSession)
                        .GetMethod(nameof(DeserializerSession.TrackDeserializedObject))!;

                c.EmitCall(trackDeserializedObjectMethod, session, target);
            }

            var serializers = fields.Select(field => serializer.GetSerializerByType(field.FieldType)).ToArray();

            var bufferSize =
                serializers.Length != 0 ? serializers.Max(s => s.PreallocatedByteBufferSize) : 0;
            if (bufferSize > 0)
                EmitBuffer(c, bufferSize, session,
                    typeof(DeserializerSession).GetMethod(nameof(DeserializerSession.GetBuffer))!);

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var s = serializers[i];

                Expression read;
                if (field.FieldType.IsSealed)
                {
                    read = s.EmitReadValue(c, stream, session, field);
                }
                else
                {
                    var readMethod = typeof(StreamEx)
                             .GetMethod(nameof(StreamEx.ReadObjectTyped))!
                         .MakeGenericMethod(field.FieldType);
                     
                     read = c.StaticCall(readMethod, stream, session);
                }

                var assignReadToField = c.WriteField(field, target, read);
                c.Emit(assignReadToField);
            }

            c.Emit(c.Convert(target, typeof(object)));

            Type t = c.Compile();
            var instance = Activator.CreateInstance(t);
            return (ObjectReader)instance;
        }

        private static void EmitBuffer(Compiler c, int bufferSize, Expression session,
            MethodInfo getBuffer)
        {
            var size = c.Constant(bufferSize);
            var buffer = c.Variable<byte[]>(SerializerCompiler.PreallocatedByteBuffer);
            var bufferValue = c.Call(getBuffer, session, size);
            var assignBuffer = c.WriteVar(buffer, bufferValue);
            c.Emit(assignBuffer);
        }

        //this generates a FieldWriter that writes all fields by unrolling all fields and calling them individually
        //no loops involved
        private static ObjectWriter GetFieldsWriter(Serializer serializer, IEnumerable<FieldInfo> fields,
            Type type,
            out int bufferSize)
        {
            var c = new Compiler(typeof(ObjectWriter).GetMethod(nameof(ObjectWriter.Write))!);

            var stream = c.Parameter("writer",typeof(Writer<>).MakeByRefType());
            var target = c.Parameter<object>("target");
            var session = c.Parameter<SerializerSession>("session");
            var preserveReferences = c.Constant(serializer.Options.PreserveObjectReferences);

            if (serializer.Options.PreserveObjectReferences)
            {
                var method =
                    typeof(SerializerSession).GetMethod(nameof(SerializerSession.TrackSerializedObject))!;

                c.EmitCall(method, session, target);
            }

            var fieldsArray = fields.ToArray();
            var serializers = fieldsArray.Select(field => serializer.GetSerializerByType(field.FieldType)).ToArray();

            bufferSize = serializers.Length != 0 ? serializers.Max(s => s.PreallocatedByteBufferSize) : 0;

            // if (bufferSize > 0)
            //     EmitBuffer2(c,stream, bufferSize,
            //         typeof(IBufferWriter<byte>)
            //             .GetMethod(nameof(IBufferWriter<byte>.GetSpan))!);

            for (var i = 0; i < fieldsArray.Length; i++)
            {
                var field = fieldsArray[i];
                //get the serializer for the type of the field
                var valueSerializer = serializers[i];
                //runtime Get a delegate that reads the content of the given field

                var cast = c.CastOrUnbox(target, field.DeclaringType!);
                var readField = c.ReadField(field, cast);

                //if the type is one of our special primitives, ignore manifest as the content will always only be of this type
                if (field.FieldType.IsSealed)
                {
                    //primitive types does not need to write any manifest, if the field type is known
                    valueSerializer.EmitWriteValue(c, stream, readField, session);
                }
                else
                {
                    var converted = c.Convert<object>(readField);
                    var valueType = field.FieldType;
                    if (field.FieldType.IsNullable())
                    {
                        var nullableType = field.FieldType.GetNullableElement();
                        valueSerializer = serializer.GetSerializerByType(nullableType);
                        valueType = nullableType;
                    }

                    var vs = c.Constant(valueSerializer);
                    var vt = c.Constant(valueType);

                    var method = typeof(BufferExtensions).GetMethod(nameof(BufferExtensions.WriteObject))!;

                    c.EmitStaticCall(method, stream, converted, vt, vs, preserveReferences, session);
                }
            }
            

            Type t = c.Compile();
            var instance = Activator.CreateInstance(t);
            return (ObjectWriter)instance;
        }
    }
}