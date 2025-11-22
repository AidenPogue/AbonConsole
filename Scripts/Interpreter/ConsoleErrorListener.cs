using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using System.IO;
using UnityEngine;

namespace Terasievert.AbonConsole.Interpreter
{
    public class ConsoleErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Debug.LogError($"Syntax error on '{offendingSymbol.Text}' in \"{e.Context.GetText()}\": {msg}");
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }
}
