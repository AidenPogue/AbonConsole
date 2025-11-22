using Antlr4.Runtime;
using System.Collections.Generic;
using System.Linq;
using Terasievert.AbonConsole.AntlrGenerated;
using Terasievert.AbonConsole.Interpreter.Expressions;

namespace Terasievert.AbonConsole.Interpreter
{
    /// <summary>
    /// Parses and interprets a single consolke command.
    /// </summary>
    public class ConsoleInterpreter
    {
        private static readonly ConsoleErrorListener errorListener = new();

        private readonly IDictionary<string, ConsoleMember> memberMap;

        public ConsoleInterpreter(IDictionary<string, ConsoleMember> memberMap)
        {
            this.memberMap = memberMap;
        }

        public ConsoleExpression[] InterpretExpressions(ICharStream charStream)
        {
            var lex = new ConsoleLexer(charStream);
            var tStream = new CommonTokenStream(lex);
            var parse = new ConsoleParser(tStream);
            parse.AddErrorListener(errorListener);
            parse.ErrorHandler = new BailErrorStrategy();

            var cmd = parse.command();

            var visitor = new ConsoleExpressionVisitor(memberMap);
            var statements = cmd.statement();

            return statements.Select(visitor.Visit).ToArray();
        }
    }
}
