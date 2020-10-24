// -----------------------------------------------------------------------
//   <copyright file="Compiler.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//       Copyright (C) 2016-2016 Akka.NET Team <https://github.com/akkadotnet>
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;
using FastExpressionCompiler.LightExpression;
using Wire.Extensions;
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Wire.Compilation
{
    public class Compiler
    {
        private readonly List<Expression> _content = new List<Expression>();
        private readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly AssemblyBuilder _asm;
        private readonly ModuleBuilder _mod;
        private readonly TypeBuilder _tb;
        private readonly Type _baseType;
        private readonly MethodInfo _baseMethod;

        public Compiler(MethodInfo baseMethod)
        {
            
            //public enum ReflectionPermissionFlag
            ReflectionPermission restrictedMemberAccessPerm =
                new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess);
            restrictedMemberAccessPerm.Assert();

            _baseType = baseMethod.DeclaringType!;
            _asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("myasm"), AssemblyBuilderAccess.Run);

            _mod = _asm.DefineDynamicModule("mymod");
            _tb = _mod.DefineType("mytype", TypeAttributes.Class, _baseType);
            _baseMethod = baseMethod;
        }

        public Expression NewObject(Type type)
        {
            if (type.IsValueType)
            {
                return Expression.Default(type);
            }

            var defaultCtor = type.GetConstructor(Array.Empty<Type>());
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
        
        public Expression Parameter(string name,Type type)
        {
            var exp = Expression.Parameter(type, name);
            _parameters.Add(exp);
            return exp;
        }

        public Expression Variable<T>(string name)
        {
            var exp = Expression.Variable(typeof(T), name);
            _variables.Add(exp);
            return exp;
        }

        public Expression Variable(string name, Type type)
        {
            var exp = Expression.Variable(type, name);
            _variables.Add(exp);
            return exp;
        }

        public Expression GetVariable<T>(string name)
        {
            var existing = _variables.FirstOrDefault(v => v.Name == name && v.Type == typeof(T));
            if (existing == null) throw new Exception("Variable not found " + name);

            return existing;
        }

        public Expression Constant(object value)
        {
            return Expression.Constant(value);
        }

        public Expression CastOrUnbox(Expression value, Type type)
        {
            return type.IsValueType
                // ReSharper disable once AssignNullToNotNullAttribute
                ? Expression.Unbox(value, type)
                // ReSharper disable once AssignNullToNotNullAttribute
                : Expression.Convert(value, type);
        }

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

        public Expression Call(MethodInfo method, Expression target, params Expression[] arguments)
        {
            return Expression.Call(target, method, arguments);
        }

        public Expression StaticCall(MethodInfo method, params Expression[] arguments)
        {
            return Expression.Call(null, method, arguments);
        }

        public Expression ReadField(FieldInfo field, Expression target)
        {
            return Expression.Field(target, field);
        }

        public Expression WriteField(FieldInfo field, Expression typedTarget, Expression value)
        {
            var accessExp = Expression.Field(typedTarget, field);
            var writeExp = Expression.Assign(accessExp, value);
            return writeExp;
        }

        public Type Compile()
        {
            try
            {
                var body = ToBlock();
                var parameters = _parameters.ToArray();


                var parameterTypes = _baseMethod
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                var method = _tb.DefineMethod("mymethod",
                    MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    _baseMethod.ReturnType,
                    parameterTypes
                );

                var genericArguments = _baseMethod.GetGenericArguments();
                if (genericArguments.Any())
                {
                    var genericArgNames = genericArguments
                        .Select(ga => ga.Name)
                        .ToArray();

                    var constraintBuilders = method.DefineGenericParameters(genericArgNames);

                    for (var i = 0; i < genericArguments.Length; i++)
                    {
                        var constraint = genericArguments[i];
                        var constraintBuilder = constraintBuilders[i];
                        constraintBuilder.SetBaseTypeConstraint(constraint.BaseType);
                        constraintBuilder.SetInterfaceConstraints(constraint.GetInterfaces());
                        constraintBuilder.SetGenericParameterAttributes(constraint.GenericParameterAttributes);
                        if (constraint.BaseType != null)
                        {
                            constraintBuilder.SetBaseTypeConstraint(constraint.BaseType);
                        }
                        else
                        {
                            constraintBuilder.SetInterfaceConstraints(constraint.GetInterfaces());
                        }
                    }
                }

                _tb.DefineMethodOverride(method, _baseMethod);

                var il = method.GetILGenerator();
                method.CompileFastToIL(il, body, parameters);

                return _tb.CreateType()!;
                //return lambda.CompileFast<TDel>();
            }
            catch(Exception x)
            {
                Console.WriteLine(x);
                throw;
            }
        }



        public Expression Convert<T>(Expression value)
        {
            return Expression.Convert(value, typeof(T));
        }

        public Expression WriteVar(Expression variable, Expression value)
        {
            return Expression.Assign(variable, value);
        }

        public void Emit(Expression expression)
        {
            _content.Add(expression);
        }

        public Expression Convert(Expression value, Type type)
        {
            return Expression.Convert(value, type);
        }

        private Expression ToBlock()
        {
            if (!_content.Any()) _content.Add(Expression.Empty());

            return Expression.Block(_variables.ToArray(), _content.ToArray());
        }
    }
}