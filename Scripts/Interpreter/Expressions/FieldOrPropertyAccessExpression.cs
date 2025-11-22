using System;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public abstract class FieldOrPropertyAccessExpression : ConsoleExpression
    {
        protected readonly MemberInfo fieldOrProperty;

        public FieldOrPropertyAccessExpression(MemberInfo fieldOrProperty)
        {
            this.fieldOrProperty = fieldOrProperty ?? throw new ArgumentNullException(nameof(fieldOrProperty));

            if (fieldOrProperty.MemberType != MemberTypes.Field && fieldOrProperty.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("The member info must be a field or property.", nameof(fieldOrProperty));
            }
        }
    }
}
