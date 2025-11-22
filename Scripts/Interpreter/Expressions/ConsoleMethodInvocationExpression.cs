using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class ConsoleMethodInvocationExpression : MethodInvocationExpression, IConsoleMemberExpresssion
    {
        private readonly ConsoleMember member;

        public ConsoleMethodInvocationExpression(ConsoleMember member, IList<ConsoleExpression> parameters) : base(member.MemberInfo as MethodBase, parameters)
        {
            this.member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public override object EvaluateValueOnly()
        {
            if (member.IsCheat && !CheatsEnabled)
            {
                throw new CheatsDisabledException(member);
            }

            return base.EvaluateValueOnly();
        }

        public ConsoleMember ConsoleMember => member;

        public override string ToString()
        {
            return $"{nameof(ConsoleMethodInvocationExpression)}: {member.Name}{GetParameterListString()}";
        }
    }
}
