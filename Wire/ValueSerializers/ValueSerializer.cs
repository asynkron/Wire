using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Wire.Compilation;

namespace Wire.ValueSerializers
{
    public abstract class ValueSerializer
    {
        public abstract void WriteManifest(Stream stream, SerializerSession session);
        public abstract void WriteValue(Stream stream, object value, SerializerSession session);
        public abstract object ReadValue(Stream stream, DeserializerSession session);
        public abstract Type GetElementType();

        public virtual void EmitWriteValue(Compiler<ObjectWriter> c, int stream, int fieldValue, int session)
        {
            var converted = c.CastOrBox<object>(fieldValue);
            var method = typeof(ValueSerializer).GetMethod(nameof(WriteValue));

            //write it to the value serializer
            var vs = c.Constant(this);
            c.EmitCall(method, vs, stream, converted, session);
        }

        public static MethodInfo GetStaticVoid(LambdaExpression expression)
        {
            var unaryExpression = (UnaryExpression) expression.Body;
            var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
            var methodCallObject = (ConstantExpression) methodCallExpression.Object;
            var method = (MethodInfo) methodCallObject.Value;

            if (method.IsStatic == false)
            {
                throw new ArgumentException($"Method {method.Name} should be static.");
            }

            if (method.ReturnType != typeof(void))
            {
                throw new ArgumentException($"Method {method.Name} should return void.");
            }

            return method;
        }
    }
}