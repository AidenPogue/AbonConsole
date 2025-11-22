using System;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class FieldOrPropertyReadExpression : FieldOrPropertyAccessExpression
    {
        public FieldOrPropertyReadExpression(MemberInfo fieldOrProperty) : base(fieldOrProperty)
        {
        }

        public override Type ReturnType => fieldOrProperty.GetReturnType();

        public override object EvaluateValueOnly()
        {
            if (fieldOrProperty is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue(null);
            }
            else
            {
                return (fieldOrProperty as PropertyInfo).GetValue(null);
            }
        }

        public override string ToString()
        {
            return $"{nameof(FieldOrPropertyReadExpression)}: {ReturnType.Name} {fieldOrProperty.DeclaringType.Name}.{fieldOrProperty.Name}";
        }
    }
}
