// -----------------------------------------------------------------------
//   <copyright file="Compiler.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//       Copyright (C) 2016-2016 Akka.NET Team <https://github.com/akkadotnet>
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FastExpressionCompiler.LightExpression;
using System.Reflection;
using Wire.Extensions;

namespace Wire.Compilation
{
    public class Compiler<TDel> where TDel:class
    {
        private readonly List<Expression> _content = new List<Expression>();
        private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

        public Expression NewObject(Type type)
        {
            if (type.IsValueType)
            {
                var x = Expression.Constant(Activator.CreateInstance(type),type);
                // var convert = Expression.Convert(x, typeof(object));
                return x;
            }

            var defaultCtor = type.GetConstructor(new Type[] { });
            var il = defaultCtor?.GetMethodBody()?.GetILAsByteArray();
            var sideEffectFreeCtor = il != null && il.Length <= 8; //this is the size of an empty ctor
            if (sideEffectFreeCtor)
                //the ctor exists and the size is empty. lets use the New operator
                return Expression.New(defaultCtor);

            var emptyObjectMethod = typeof(TypeEx).GetMethod(nameof(TypeEx.GetEmptyObject));
            var emptyObject = Expression.Call(null, emptyObjectMethod, Expression.Constant((object) type));

            return emptyObject;
        }

        public Expression Parameter<T>(string name)
        {
            var exp = Expression.Parameter(typeof(T), name);
            _parameters.Add(exp);
            return exp;
        }

        public Expression Variable<T>(string name)
        {
            var exp = Expression.Variable(typeof(T), name);
            _variables.Add(exp);
            return exp;
        }
        
        public Expression Variable(string name,Type type)
        {
            var exp = Expression.Variable(type, name);
            _variables.Add(exp);
            return exp;
        }

        public Expression GetVariable<T>(string name)
        {
            var existing = _variables.First(v => v.Name == name && v.Type == typeof(T));
            if (existing == null) throw new Exception("Variable not found.");

            return existing;
        }

        public Expression Constant(object value) => Expression.Constant(value);

        public Expression CastOrUnbox(Expression value, Type type) =>
            type.IsValueType
                // ReSharper disable once AssignNullToNotNullAttribute
                ? Expression.Unbox(value, type) //unbox Expression.Unbox(value, type) //TODO fix
                // ReSharper disable once AssignNullToNotNullAttribute
                : Expression.Convert(value, type);

        public void EmitCall(MethodInfo method, Expression target, params Expression[] arguments)
        {
            var targetExp = target;
            var argumentsExp = arguments;
            var call = Expression.Call(targetExp, method, argumentsExp);
            _content.Add(call);
        }

        public void EmitStaticCall(MethodInfo method, params Expression[] arguments)
        {
            var argumentsExp = arguments;
            var call = Expression.Call(null, method, argumentsExp);
            _content.Add(call);
        }

        public Expression Call(MethodInfo method, Expression target, params Expression[] arguments) => Expression.Call(target, method, arguments);

        public Expression StaticCall(MethodInfo method, params Expression[] arguments) => Expression.Call(null, method, arguments);

        public Expression ReadField(FieldInfo field, Expression target) => Expression.Field(target, field);

        public Expression WriteField(FieldInfo field, Expression typedTarget, Expression value)
        {
            var accessExp = Expression.Field(typedTarget, field);
            var writeExp = Expression.Assign(accessExp, value);
            return writeExp;
        }

        public Expression WriteReadonlyField(FieldInfo field, Expression target, Expression value)
        {
            var method = typeof(FieldInfo)
                .GetMethod(nameof(FieldInfo.SetValue), new[] {typeof(object), typeof(object)})!;
            var fld = Constant(field);
            var valueToObject = Convert<object>(value);
            return Call(method, fld, target, valueToObject);
        }

        public TDel Compile()
        {
            var lambda = GetLambdaExpression();
            var debug = lambda.ToCSharpString();
            var debug2 = lambda.ToExpressionString();

            if (debug.Length > 300)
            {
                
            }
            
            try
            {
                var res =  lambda.CompileFast<TDel>(); //TODO. does this work?
                return res;
            }
            catch (Exception x)
            {
                throw;
            }
        }

        public LambdaExpression GetLambdaExpression()
        {
            var body = ToBlock();
            var parameters = _parameters.ToArray();
            var lambda = Expression.Lambda(body, parameters);
            return lambda;
        }

        public Expression Convert<T>(Expression value) => Expression.Convert(value, typeof(T));

        public Expression WriteVar(Expression variable, Expression value) => Expression.Assign(variable, value);

        public void Emit(Expression expression)
        {
            _content.Add(expression);
        }

        public Expression Convert(Expression value, Type type) => Expression.Convert(value, type);

        public Expression ToBlock()
        {
            if (!_content.Any()) _content.Add(Expression.Empty());

            return Expression.Block(_variables.ToArray(), _content.ToArray());
        }
    }
}