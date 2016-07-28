using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wire.Compilation
{
    public class IlCompilerContext
    {
        public IlCompilerContext(ILGenerator il, Type selfType)
        {
            Il = il;
            SelfType = selfType;
        }

        public ILGenerator Il { get; }
        public int StackDepth { get; set; }
        public Type SelfType { get; }
    }
    public class IlCompiler<TDel> :ICompiler<TDel>
    {
        private readonly List<IlExpression> _expressions = new List<IlExpression>();
        private readonly List<IlParameter> _parameters = new List<IlParameter>();
        private readonly List<IlVariable> _variables = new List<IlVariable>();
        private readonly List<object> _constants = new List<object>();
        private readonly List<Action<IlCompilerContext>> _lazyEmits = new List<Action<IlCompilerContext>>();


        public IlCompiler()
        {

        }
        public int NewObject(Type type)
        {
            var exp = new IlNew(type);
            _expressions.Add(exp);

            return _expressions.Count - 1;
        }

        public int Parameter<T>(string name)
        {

            var exp = new IlParameter(_parameters.Count+1,typeof(T),name);
            _expressions.Add(exp);
            _parameters.Add(exp);

            return _expressions.Count - 1;
        }

        public int Variable<T>(string name)
        {
            var exp = new IlVariable(_variables.Count, typeof(T));
            _expressions.Add(exp);
            _variables.Add(exp);

            return _expressions.Count - 1;
        }

        public int Constant(object value)
        {
            if (value is bool)
            {
                _expressions.Add(new IlBool((bool)value));
                return _expressions.Count - 1;
            }
            _constants.Add(value);
            _expressions.Add(new IlRuntimeConstant(value, _expressions.Count));
            return _expressions.Count - 1;
        }

        public int CastOrUnbox(int value, Type type)
        {
            var valueExp = _expressions[value];
            if (type.IsValueType)
                _expressions.Add(new IlUnbox(type, valueExp));
            else
                _expressions.Add(new IlCastClass(type, valueExp));
            return _expressions.Count - 1;
        }

        public void EmitCall(MethodInfo method, int target, params int[] arguments)
        {
            var call = Call(method, target, arguments);
            Emit(call);
        }

        public void EmitStaticCall(MethodInfo method, params int[] arguments)
        {
            var call = StaticCall(method, arguments);
            Emit(call);
        }

        public int Call(MethodInfo method, int target, params int[] arguments)
        {
            var call = new IlCall(_expressions[target], method, arguments.Select(a => _expressions[a]).ToArray());
            _expressions.Add(call);
            return _expressions.Count - 1;
        }

        public int StaticCall(MethodInfo method, params int[] arguments)
        {
            var call = new IlCallStatic(method, arguments.Select(a => _expressions[a]).ToArray());
            _expressions.Add(call);
            return _expressions.Count - 1;
        }

        public int ReadField(FieldInfo field, int target)
        {
            var exp = new IlReadField(field, _expressions[target]);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }

        public int WriteField(FieldInfo field, int target, int value)
        {
            var exp = new IlWriteField(field, _expressions[target],_expressions[value]);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }


        public TDel Compile()
        {
            var delegateType = typeof(TDel);
            var invoke = delegateType.GetMethod("Invoke");

            object self = null;
            if (_constants.Any())
            {
                var tupleTypes = _constants.Select(c => c.GetType()).ToArray();
                var genericTupleFactory =
                    typeof(Tuple)
                        .GetMethods()
                        .First(m => m.Name == "Create" && m.GetParameters().Length == tupleTypes.Length);
                var tupleFactory = genericTupleFactory.MakeGenericMethod(tupleTypes);

                self = tupleFactory.Invoke(null, _constants.ToArray());                               
            }
            var selfType = self?.GetType() ?? typeof(object);
            var parameterTypes = invoke.GetParameters().Select(a => a.ParameterType).ToArray();
            var parametersWithSelf = new[] { selfType }.Concat(parameterTypes).ToArray();
            var returnType = invoke.ReturnType;
            var method = new DynamicMethod("foo", returnType, parametersWithSelf,true);

            var il = method.GetILGenerator();
            var context = new IlCompilerContext(il,selfType);
            if (returnType != typeof(void))
                context.StackDepth--;

            foreach (var variable in _variables)
            {
                il.DeclareLocal(variable.Type());
            }

            foreach (var p in _parameters)
            {
                method.DefineParameter(p.ParameterIndex, ParameterAttributes.None, p.Name);
            }

            _lazyEmits.ForEach(e => e(context));

            il.Emit(OpCodes.Ret);

            if (context.StackDepth != 0)
                throw new NotSupportedException("Stack error");

            method.DefineParameter(0, ParameterAttributes.None, "this");
    
            var del = (TDel) (object) method.CreateDelegate(typeof(TDel), self);
            return del;

        }

        public int WriteVar(int variable, int value)
        {
            var variableExp = _expressions[variable] as IlVariable;
            var valueExp = _expressions[value];
            var exp = new IlWriteVariable(variableExp,valueExp);
            _expressions.Add(exp);
            return _expressions.Count - 1;
        }

        public void Emit(int value)
        {
            Action<IlCompilerContext> action = ctx =>
            {
                var exp = _expressions[value];
                exp.Emit(ctx);
            };
            _lazyEmits.Add(action);
        }

        public int CastOrBox<T>(int value)
        {
            return CastOrBox(value, typeof(T));
        }

        public int CastOrBox(int value, Type type)
        {
            var valueExp = _expressions[value];
            if (valueExp.Type().IsValueType)
                _expressions.Add(new IlBox(valueExp.Type(), valueExp));
            else
                _expressions.Add(new IlCastClass(type, valueExp));
            return _expressions.Count - 1;
        }
    }
}
