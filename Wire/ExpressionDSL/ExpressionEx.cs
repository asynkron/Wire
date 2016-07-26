using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wire.ExpressionDSL
{
    public static class ExpressionEx
    {
        public static ConstantExpression ToConstant(this object self)
        {
            return Expression.Constant(self);
        }

        public static BlockExpression ToBlock(this List<Expression> expressions, params ParameterExpression[] variables)
        {
            if (!expressions.Any())
            {
                expressions.Add(Expression.Empty());
            }

            return Expression.Block(variables,expressions);
        }

        public static ParameterExpression Variable<T>(string name)
        {
            return Expression.Variable(typeof(T), name);
        }

        public static ParameterExpression Variable<T>()
        {
            return Expression.Variable(typeof(T));
        }

        public static ParameterExpression Parameter<T>(string name)
        {
            return Expression.Parameter(typeof(T),name);
        }

        public static ParameterExpression Parameter<T>()
        {
            return Expression.Parameter(typeof(T));
        }

        public static List<Expression> Expressions()
        {
            return new List<Expression>();
        }

        public static Expression ConvertTo(this Expression expression, Type type)
        {
            return Expression.Convert(expression, type);
        }

        public static Expression ConvertTo<T>(this Expression expression)
        {
            return Expression.Convert(expression, typeof(T));
        }

        public static Expression AccessFrom(this FieldInfo field,Expression target)
        {
            return Expression.Field(target, field);
        }

        public static Expression CastOrUnbox(this Expression expression, Type type)
        {
            var cast = type.GetTypeInfo().IsValueType
                // ReSharper disable once AssignNullToNotNullAttribute
                ? Expression.Unbox(expression, type)
                // ReSharper disable once AssignNullToNotNullAttribute
                : Expression.Convert(expression, type);
            return cast;
        }

        public static Expression GetNewExpression(Type type)
        {
            var defaultCtor = type.GetConstructor(new Type[] {});
            var il = defaultCtor?.GetMethodBody()?.GetILAsByteArray();
            var sideEffectFreeCtor = il != null && il.Length <= 8; //this is the size of an empty ctor
            if (sideEffectFreeCtor)
            {
                //the ctor exists and the size is empty. lets use the New operator
                return Expression.New(defaultCtor);
            }
            var emptyObjectMethod = typeof(TypeEx).GetMethod(nameof(TypeEx.GetEmptyObject));
            var emptyObject = Expression.Call(null, emptyObjectMethod, type.ToConstant());

            return emptyObject;
        }
    }
}
