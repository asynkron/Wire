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

        public static Expression ReadFrom(this FieldInfo field,Expression target)
        {
            return Expression.Field(target, field);
        }

    }
}
