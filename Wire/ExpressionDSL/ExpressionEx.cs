using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
    }
}
