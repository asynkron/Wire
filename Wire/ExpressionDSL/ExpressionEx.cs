using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wire.ExpressionDSL
{
    public class Compiler
    {
        private readonly List<Expression> _expressions = new List<Expression>();
        private readonly List<Expression> _content = new List<Expression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();

        public int NewObject(Type type)
        {
            var exp = ExpressionEx.GetNewExpression(type);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }

        public int Parameter<T>(string name)
        {
            var exp = ExpressionEx.Parameter<T>(name);
            _parameters.Add(exp);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }

        public int Variable<T>(string name)
        {
            var exp = ExpressionEx.Variable<T>(name);
            _variables.Add(exp);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }

        public int Constant(object value)
        {
            var constant = value.ToConstant();
            _expressions.Add(constant);
            return _expressions.Count - 1;
        }

        public int CastOrUnbox(int value, Type type)
        {
            var exp = _expressions[value].CastOrUnbox(type);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }
        
        public void EmitCall(MethodInfo method, int target, params int[] arguments)
        {
            var targetExp = _expressions[target];
            var argumentsExp = arguments.Select(n => _expressions[n]).ToArray();
            var call = Expression.Call(targetExp, method, argumentsExp);
            _content.Add(call);
        }

        public void EmitStaticCall(MethodInfo method, params int[] arguments)
        {
            var argumentsExp = arguments.Select(n => _expressions[n]).ToArray();
            var call = Expression.Call(null, method, argumentsExp);
            _content.Add(call);
        }

        public int ReadField(FieldInfo field, int target)
        {
            var targetExp = _expressions[target];
            var accessExp = Expression.Field(targetExp, field);
            _expressions.Add(accessExp);
            return _expressions.Count - 1;
        }

        public int WriteField(FieldInfo field, int target,int value)
        {
            var targetExp = _expressions[target];
            var valueExp = _expressions[value];
            var accessExp = Expression.Field(targetExp, field);
            var writeExp = Expression.Assign(accessExp, valueExp);
            _expressions.Add(writeExp);
            return _expressions.Count - 1;
        }

        public Expression ToBlock()
        {
            return _content.ToBlock(_variables.ToArray());
        }

        public T Compile<T>()
        {
            var body = ToBlock();
            var parameters = _parameters.ToArray();
            var res = Expression.Lambda<T>(body, parameters).Compile();
            return res;
        }
        public int ConvertTo<T>(int value)
        {
            var valueExp = _expressions[value];
            var con = valueExp.ConvertTo<T>();
            _expressions.Add(con);
            return _expressions.Count - 1;
        }
    }
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

        public static Expression Read(this FieldInfo field,Expression target)
        {
            return Expression.Field(target, field);
        }

        public static Expression Assign(this FieldInfo field, Expression target,Expression value)
        {
            var access = Expression.Field(target, field);
            return Expression.Assign(access, value);
        }

        public static Expression Assign(this ParameterExpression target, Expression value)
        {
            return Expression.Assign(target, value);
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
