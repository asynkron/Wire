using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Wire.Compilation
{
    public abstract class IlExpression
    {
        public abstract void Emit(IlCompilerContext ctx);
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
            ctx.StackDepth++;
        }
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
            ctx.Il.Emit(OpCodes.Newobj,ctor);
            ctx.StackDepth++;
        }
    }

    public class IlParameter : IlExpression
    {
        private readonly int _parameterIndex;
        private readonly Type _type;

        public IlParameter(int parameterIndex,Type type)
        {
            _parameterIndex = parameterIndex;
            _type = type;
        }

        public override void Emit(IlCompilerContext ctx)
        {
            ctx.Il.Emit(OpCodes.Ldarg, _parameterIndex);
            ctx.StackDepth++;
        }
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
            ctx.Il.Emit(OpCodes.Unbox, _type);
        }
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
        }
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
        }
    }

    public static class ExpressionEx
    {
        public static ConstantExpression ToConstant(this object self)
        {
            return Expression.Constant(self);
        }


        public static Expression GetNewExpression(Type type)
        {
#if SERIALIZATION
            var defaultCtor = type.GetTypeInfo().GetConstructor(new Type[] {});
            var il = defaultCtor?.GetMethodBody()?.GetILAsByteArray();
            var sideEffectFreeCtor = il != null && il.Length <= 8; //this is the size of an empty ctor
            if (sideEffectFreeCtor)
            {
                //the ctor exists and the size is empty. lets use the New operator
                return Expression.New(defaultCtor);
            }
#endif
            var emptyObjectMethod = typeof(TypeEx).GetTypeInfo().GetMethod(nameof(TypeEx.GetEmptyObject));
            var emptyObject = Expression.Call(null, emptyObjectMethod, type.ToConstant());

            return emptyObject;
        }
    }
}