using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Wire.Compilation
{
    public abstract class IlExpression
    {
        public abstract void Emit(IlCompilerContext ctx);
        public abstract Type Type();
    }

    public class IlBool : IlExpression
    {
        private readonly bool _value;

        public IlBool(bool value)
        {
            _value = value;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            ctx.Il.Emit(_value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            ctx.StackDepth++;
        }

        public override Type Type() => typeof(bool);
    }

    public class IlRuntimeConstant : IlExpression
    {
        private readonly object _object;
        public int Index { get; }

        public IlRuntimeConstant(object value, int index)
        {
            _object = value;
            Index = index;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            var field = ctx.SelfType.GetFields(BindingFlagsEx.All)[Index];
            ctx.Il.Emit(OpCodes.Ldarg_0);
            ctx.Il.Emit(OpCodes.Ldfld,field);
            ctx.StackDepth++;
        }

        public override Type Type() => _object.GetType();
    }

    public class IlReadField : IlExpression
    {
        private readonly FieldInfo _field;
        private readonly IlExpression _target;

        public IlReadField(FieldInfo field, IlExpression target)
        {
            _field = field;
            _target = target;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _target.Emit(ctx);
            ctx.Il.Emit(OpCodes.Ldfld,_field);
            //we are still at the same stacksize as we consumed the target
        }

        public override Type Type() => _field.FieldType;
    }

    public class IlWriteVariable : IlExpression
    {
        private readonly IlVariable _variable;
        private readonly IlExpression _value;

        public IlWriteVariable(IlVariable variable, IlExpression value)
        {
            _variable = variable;
            _value = value;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _value.Emit(ctx);
            ctx.Il.Emit(OpCodes.Stloc, _variable.VariableIndex);
            ctx.StackDepth--;
        }

        public override Type Type()
        {
            throw new NotImplementedException();
        }
    }

    public class IlWriteField : IlExpression
    {
        private readonly FieldInfo _field;
        private readonly IlExpression _target;
        private readonly IlExpression _value;

        public IlWriteField(FieldInfo field, IlExpression target,IlExpression value)
        {
            _field = field;
            _target = target;
            _value = value;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _target.Emit(ctx);
            _value.Emit(ctx);
            ctx.Il.Emit(OpCodes.Stfld, _field);
            ctx.StackDepth-=2;
        }

        public override Type Type()
        {
            throw new NotImplementedException();
        }
    }

    public class IlNew : IlExpression
    {
        private readonly Type _type;

        public IlNew(Type type)
        {
            _type = type;
        }

        public override void Emit(IlCompilerContext ctx)
        {           
            var ctor = _type.GetConstructor(new Type[] {});
            ctx.Il.Emit(OpCodes.Newobj, ctor);
            ctx.StackDepth++;
        }

        public override Type Type() => _type;
    }

    public class IlParameter : IlExpression
    {
        public string Name { get; }
        public int ParameterIndex { get; }
        private readonly Type _type;

        public IlParameter(int parameterIndex,Type type,string name)
        {
            Name = name;
            ParameterIndex = parameterIndex;
            _type = type;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            ctx.Il.Emit(OpCodes.Ldarg, ParameterIndex);
            ctx.StackDepth++;
        }

        public override Type Type() => _type;
    }

    public class IlVariable : IlExpression
    {
        public int VariableIndex { get; }
        private readonly Type _type;

        public IlVariable(int variableIndex,Type type)
        {
            VariableIndex = variableIndex;
            _type = type;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            ctx.Il.Emit(OpCodes.Ldloc, VariableIndex);
            ctx.StackDepth++;
        }

        public override Type Type() => _type;
    }

    public class IlCastClass : IlExpression
    {
        private readonly Type _type;
        private readonly IlExpression _expression;

        public IlCastClass(Type type, IlExpression expression)
        {
            _type = type;
            _expression = expression;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _expression.Emit(ctx);
            ctx.Il.Emit(OpCodes.Castclass, _type);
        }

        public override Type Type() => _type;
    }

    public class IlBox : IlExpression
    {
        private readonly Type _type;
        private readonly IlExpression _expression;

        public IlBox(Type type, IlExpression expression)
        {
            _type = type;
            _expression = expression;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _expression.Emit(ctx);
            ctx.Il.Emit(OpCodes.Box, _type);
        }

        public override Type Type() => typeof(object);
    }

    public class IlUnbox : IlExpression
    {
        private readonly Type _type;
        private readonly IlExpression _expression;

        public IlUnbox(Type type, IlExpression expression)
        {
            _type = type;
            _expression = expression;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _expression.Emit(ctx);
            ctx.Il.Emit(OpCodes.Unbox_Any, _type);
        }

        public override Type Type() => _type;
    }

    public class IlCall : IlExpression
    {
        private readonly IlExpression _target;
        private readonly MethodInfo _method;
        private readonly IlExpression[] _args;

        public IlCall(IlExpression target, MethodInfo method, params IlExpression[] args)
        {
            if (args.Length != method.GetParameters().Length)
                throw new ArgumentException("Parameter count mismatch",nameof(args));

            _target = target;
            _method = method;
            _args = args;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            _target.Emit(ctx);
            ctx.StackDepth--;
            foreach (var arg in _args)
            {
                arg.Emit(ctx);
                ctx.StackDepth--;
            }
            ctx.Il.EmitCall(OpCodes.Call, _method, null);
            if (_method.ReturnType != typeof(void))
                ctx.StackDepth++;
        }

        public override Type Type() => _method.ReturnType;
    }

    public class IlCallStatic : IlExpression
    {
        private readonly MethodInfo _method;
        private readonly IlExpression[] _args;

        public IlCallStatic(MethodInfo method, params IlExpression[] args)
        {
            if (args.Length != method.GetParameters().Length)
                throw new ArgumentException("Parameter count mismatch", nameof(args));

            _method = method;
            _args = args;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            foreach (var arg in _args)
            {
                arg.Emit(ctx);
                ctx.StackDepth--;
            }
            ctx.Il.EmitCall(OpCodes.Call, _method, null);
            if (_method.ReturnType != typeof(void))
                ctx.StackDepth++;
        }

        public override Type Type() => _method.ReturnType;
    }
}