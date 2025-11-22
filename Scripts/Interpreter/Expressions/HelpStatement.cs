using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    public class HelpStatement : ConsoleExpression
    {
        private readonly ConsoleMember member;

        public HelpStatement(ConsoleMember member)
        {
            this.member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public override Type ReturnType => typeof(void);

        public override object EvaluateValueOnly()
        {
            return null;
        }

        public override Result Evaluate()
        {
            return new Result(null, member.ToString(), ReturnType);
        }

        public override string ToString()
        {
            return $"{nameof(HelpStatement)}: {member.Name}";
        }
    }
}
