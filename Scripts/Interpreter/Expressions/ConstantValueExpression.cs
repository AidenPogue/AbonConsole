using System;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    /// <summary>
    /// An expression that represents a literal/constant value
    /// </summary>
    public class ConstantValueExpression : ConsoleExpression
    {
        private readonly object value;
        private readonly Type type;

        public override Type ReturnType => type;

        public override object EvaluateValueOnly() => value;

        public ConstantValueExpression(object value, Type type)
        {
            this.value = value;
            this.type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override string ToString()
        {
            return $"{nameof(ConstantValueExpression)}: {value.ConsoleToString()} ({ReturnType.Name})";
        }
    }
}
