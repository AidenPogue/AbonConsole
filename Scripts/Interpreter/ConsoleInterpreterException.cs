using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;

namespace Terasievert.AbonConsole.Interpreter
{
    /// <summary>
    /// Special exception to wrap exceptions caused by a user's entered command.
    /// </summary>
    public class ConsoleInterpreterException : Exception
    {
        private readonly string tokenString;

        /// <summary>
        /// Creates a new instance of <see cref="ConsoleInterpreterException"/> with the specified context and offending tree
        /// </summary>
        /// <param name="context">The rule context in which the error occurred.</param>
        /// <param name="offendingTree">The syntax tree of the node or context within the context that caused the error. If non null, the tree's text will be bolded in the error message.</param>
        /// <param name="message"></param>
        /// <exception cref="InvalidOperationException">Thrown if the context's token stream was not available</exception>
        public ConsoleInterpreterException(ParserRuleContext context, ISyntaxTree offendingTree, string message) : base(message)
        {

            // Get start and stop tokens of the context
            int startIndex = context.Start.StartIndex;
            int stopIndex = context.Stop.StopIndex;

            // Get the raw text of the context
            string contextText = context.Start.InputStream.GetText(new Interval(startIndex, stopIndex));

            if (offendingTree is null)
            {
                tokenString = contextText;
                return;
            }

            var nodeStart = offendingTree is ParserRuleContext ctx ? ctx.Start.StartIndex : offendingTree is IToken token ? token.StartIndex : throw new ArgumentException();
            var nodeStop = offendingTree is ParserRuleContext ctx1 ? ctx1.Stop.StopIndex : offendingTree is IToken token1 ? token1.StopIndex : throw new ArgumentException();

            // Get the relative offsets within the context
            int relativeStart = nodeStart - startIndex;
            int relativeStop = nodeStop - startIndex;

            // Insert bold tags
            string before = contextText.Substring(0, relativeStart);
            string middle = contextText.Substring(relativeStart, relativeStop - relativeStart + 1);
            string after = contextText.Substring(relativeStop + 1);

            string modifiedText = before + "<u><b>" + middle + "</u></b>" + after;

            tokenString = modifiedText;
        }

        public ConsoleInterpreterException(ParserRuleContext context, string message) : this(context, null, message) { }


        private string ErrorNodeTransformer(string errorNodeText)
        {
            return $"<b>{errorNodeText}</b>";
        }

        public override string ToString()
        {
            return $"Error interpreting \"{tokenString}\": {Message} {"|" + InnerException?.ToString() ?? ""}";
        }
    }
}
