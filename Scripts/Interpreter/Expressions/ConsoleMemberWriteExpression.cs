using System;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class ConsoleMemberWriteExpression : FieldOrPropertyWriteExpression, IConsoleMemberExpresssion
    {
        private readonly ConsoleMember consoleMember;
        public ConsoleMemberWriteExpression(ConsoleMember consoleMember, ConsoleExpression valueToWrite) : base(consoleMember.MemberInfo, valueToWrite)
        {
            this.consoleMember = consoleMember ?? throw new ArgumentNullException(nameof(consoleMember));
        }

        public ConsoleMember ConsoleMember => consoleMember;

        public override object EvaluateValueOnly()
        {
            if (consoleMember.IsCheat && !CheatsEnabled)
            {
                throw new CheatsDisabledException(ConsoleMember);
            }

            return base.EvaluateValueOnly();
        }

        public override string ToString()
        {
            return $"{nameof(ConsoleMemberWriteExpression)}: {consoleMember.MemberInfo.GetReturnType().Name} {consoleMember.Name} <- {valueToWrite}";
        }
    }
}
