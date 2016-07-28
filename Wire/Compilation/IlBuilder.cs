using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wire.Compilation
{
    public class IlBuilder
    {
        private readonly List<IlExpression> _expressions = new List<IlExpression>();
        protected List<IlParameter> Parameters { get; } = new List<IlParameter>();
        protected List<IlVariable> Variables { get; } = new List<IlVariable>();
        protected List<object> Constants { get; } = new List<object>();
        protected List<Action<IlCompilerContext>> LazyEmits { get; } = new List<Action<IlCompilerContext>>();

        public int NewObject(Type type)
        {
            var typeExp = Constant(type);
            var t = _expressions[typeExp];
            var exp = new IlNew(type,t);
            _expressions.Add(exp);

            return _expressions.Count - 1;
        }

        public int Parameter<T>(string name)
        {

            var exp = new IlParameter(Parameters.Count+1,typeof(T),name);
            _expressions.Add(exp);
            Parameters.Add(exp);

            return _expressions.Count - 1;
        }

        public int Variable<T>(string name)
        {
            var exp = new IlVariable(Variables.Count, typeof(T));
            _expressions.Add(exp);
            Variables.Add(exp);

            return _expressions.Count - 1;
        }

        public int Constant(object value)
        {
            if (value is bool)
            {
                _expressions.Add(new IlBool((bool)value));
                return _expressions.Count - 1;
            }
            
            _expressions.Add(new IlRuntimeConstant(value, Constants.Count));
            Constants.Add(value);
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
            LazyEmits.Add(action);
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