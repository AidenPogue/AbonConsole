using System;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class FieldOrPropertyWriteExpression : FieldOrPropertyAccessExpression
    {
        protected readonly ConsoleExpression valueToWrite;

        public FieldOrPropertyWriteExpression(MemberInfo fieldOrProperty, ConsoleExpression valueToWrite) : base(fieldOrProperty)
        {
            this.valueToWrite = valueToWrite ?? throw new ArgumentNullException(nameof(valueToWrite));
            if (fieldOrProperty.IsReadOnly())
            {
                throw new ArgumentException("The field or property is not writable");
            }
        }

        public override Type ReturnType => typeof(void);

        //Jank double format
        protected virtual string resultMessageFormat => $"{fieldOrProperty.MemberType} {fieldOrProperty.DeclaringType.Name}.{fieldOrProperty.Name} was set to {{0}}";

        public override object EvaluateValueOnly()
        {
            WriteValueAndReturn();
            return null;
        }

        private object WriteValueAndReturn()
        {
            var toWrite = valueToWrite.EvaluateValueOnly();
            if (fieldOrProperty is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(null, toWrite);
            }
            else
            {
                (fieldOrProperty as PropertyInfo).SetValue(null, toWrite);
            }
            return toWrite;
        }

        public override Result Evaluate()
        {
            var written = WriteValueAndReturn();
            return new Result(written, string.Format(resultMessageFormat, written.ConsoleToString()), ReturnType);
        }

        public override string ToString()
        {
            return $"{nameof(FieldOrPropertyWriteExpression)}: {fieldOrProperty.GetReturnType().Name} {fieldOrProperty.DeclaringType.Name}.{fieldOrProperty.Name} <- {valueToWrite}";
        }
    }
}
