using System;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class ConsoleMemberReadExpression : FieldOrPropertyReadExpression, IConsoleMemberExpresssion
    {
        private readonly ConsoleMember consoleMember;
        public ConsoleMemberReadExpression(ConsoleMember consoleMember) : base(consoleMember.MemberInfo)
        {
            this.consoleMember = consoleMember ?? throw new ArgumentNullException(nameof(consoleMember));
        }

        public ConsoleMember ConsoleMember => consoleMember;

        public override string ToString()
        {
            return $"{nameof(ConsoleMemberReadExpression)}: {ReturnType.Name} {consoleMember.Name}";
        }
    }
}
