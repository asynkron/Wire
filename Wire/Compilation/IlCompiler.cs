using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wire.Compilation
{
    public class IlCompilerContext
    {
        public IlCompilerContext(ILGenerator il)
        {
            Il = il;
        }

        public ILGenerator Il { get; }
        public int StackDepth { get; set; }
    }
    public class IlCompiler<TDel> :ICompiler<TDel>
    {
        private readonly List<IlExpression> _expressions = new List<IlExpression>();
        private readonly List<IlParameter> _parameters = new List<IlParameter>();
        private readonly List<IlVariable> _variables = new List<IlVariable>();
        private readonly ILGenerator _il;
        private readonly DynamicMethod _method;
        private readonly IlCompilerContext _context;


        public IlCompiler()
        {
            var delegateType = typeof(TDel);
            var invoke = delegateType.GetMethod("Invoke");
            var returnType = invoke.ReturnType;
            
            var parameterTypes = invoke.GetParameters().Select(a => a.ParameterType).ToArray();

            _method = new DynamicMethod("foo", returnType, parameterTypes);
            _il = _method.GetILGenerator();
            _context = new IlCompilerContext(_il);
            if (returnType != typeof(void))
                _context.StackDepth --;
        }
        public int NewObject(Type type)
        {
            var exp = new IlNew(type);
            _expressions.Add(exp);

            return _expressions.Count - 1;
        }

        public int Parameter<T>(string name)
        {
            _method.DefineParameter(_parameters.Count, ParameterAttributes.None, name);
            var exp = new IlParameter(_parameters.Count,typeof(T));
            _expressions.Add(exp);
            _parameters.Add(exp);

            return _expressions.Count - 1;
        }

        public int Variable<T>(string name)
        {
            _il.DeclareLocal(typeof(T));
            
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
            throw new NotSupportedException();
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
            var call = new IlCall(_expressions[target], method, arguments.Select(a => _expressions[a]).ToArray());
            call.Emit(_context);
        }

        public void EmitStaticCall(MethodInfo method, params int[] arguments)
        {
            var call = new IlCallStatic(method, arguments.Select(a => _expressions[a]).ToArray());
            call.Emit(_context);
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



            while (_context.StackDepth-- > 0)
            {
                _context.Il.Emit(OpCodes.Pop);
            }
            _il.Emit(OpCodes.Ret);
            var del = (TDel)(object)_method.CreateDelegate(typeof(TDel));
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
            var exp = _expressions[value];
            exp.Emit(_context);
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
