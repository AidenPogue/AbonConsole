using System;
using System.Linq;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    /// <summary>
    /// Expression that returns the boolean not of another expression.
    /// </summary>
    // This implicit op shit is a mess.
    public class BooleanNotExpression : ConsoleExpression
    {
        private readonly ConsoleExpression expression;
        private readonly Func<object, object> boolImplicitOperator;

        public BooleanNotExpression(ConsoleExpression expression, MethodInfo boolImplicitOperator) : this(expression, o => boolImplicitOperator.Invoke(null, new [] { o }))
        {
            this.expression = expression ?? throw new ArgumentNullException(nameof(expression));

            if (boolImplicitOperator is not null && (boolImplicitOperator.ReturnType != typeof(bool) || boolImplicitOperator.GetParameters().FirstOrDefault().ParameterType.IsAssignableOrCastableFrom(expression.ReturnType)))
            {
                throw new ArgumentException("The provided MethodInfo cannot be used as an implicit operator");
            }
        }

        public BooleanNotExpression(ConsoleExpression expression, Func<object, object> boolImplicitOperator)
        {
            this.expression = expression;
            this.boolImplicitOperator = boolImplicitOperator;
        }

        public BooleanNotExpression(ConsoleExpression expression) : this(expression, (Func<object, object>)null)
        {

        }

        public override Type ReturnType => typeof(bool);

        public override object EvaluateValueOnly()
        {
            var value = expression.EvaluateValueOnly();

            bool asBool = false;

            if (boolImplicitOperator is not null)
            {
                try
                {
                    asBool = (bool)boolImplicitOperator(value);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("The implicit operator did not accept the expression value, or did not return a bool.", e);
                }
                
            }
            else
            {
                try
                {
                    asBool = (bool)value;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("The expression cannot be cast to a boolean, and no implicit operator was provided.", e);
                }
            }

            return !asBool;
        }

        public override string ToString()
        {
            return $"{nameof(BooleanNotExpression)}: !{expression}";
        }
    }
}
