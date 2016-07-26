using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wire.ExpressionDSL
{
    public class Compiler
    {
        private readonly Dictionary<string,Expression> _namedValues = new Dictionary<string, Expression>();
        private readonly List<Expression> _content = new List<Expression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

        public void NewObject(Type type,string name)
        {
            var exp = ExpressionEx.GetNewExpression(type);
            _namedValues.Add(name,exp);
        }

        public void Parameter<T>(string name)
        {
            var exp = ExpressionEx.Parameter<T>(name);
            _namedValues.Add(name, exp);
        }

        public void Variable<T>(string name)
        {
            var exp = ExpressionEx.Variable<T>(name);
            _variables.Add(exp);
            _namedValues.Add(name, exp);
        }

        public void Constant(object value,string name)
        {
            var constant = value.ToConstant();
            _namedValues.Add(name,constant);
        }

        public void CastOrUnbox(string name, Type type)
        {
            var exp = _namedValues[name].CastOrUnbox(type);
            _namedValues.Add(name,exp);
        }

        public void Call(string name, MethodInfo method, string target, params string[] arguments)
        {
            var targetExp = _namedValues[target];
            var argumentsExp = arguments.Select(n => _namedValues[n]).ToArray();
            var call = Expression.Call(targetExp, method, argumentsExp);
            _namedValues.Add(name,call);
        }

        public void ReadField(string name, FieldInfo field, string target)
        {
            var targetExp = _namedValues[target];
            var accessExp = Expression.Field(targetExp, field);
            _namedValues.Add(name,accessExp);
        }

        public void WriteField(string name, FieldInfo field, string target,string value)
        {
            var targetExp = _namedValues[target];
            var valueExp = _namedValues[value];
            var accessExp = Expression.Field(targetExp, field);
            var writeExp = Expression.Assign(accessExp, valueExp);
            _namedValues.Add(name, writeExp);
        }

        public void Emit(string name)
        {
            _content.Add(_namedValues[name]);
        }

        public Expression ToBlock()
        {
            return Expression.Block(_variables, _content);
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
