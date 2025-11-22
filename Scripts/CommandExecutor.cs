using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using Terasievert.AbonConsole.Interpreter;
using Terasievert.AbonConsole.Interpreter.Expressions;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    /// <summary>
    /// Static class with helper methods for executing console commands.
    /// </summary>
    public static class CommandExecutor
    {
        public static void ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            Debug.Log(ConsoleLogItem.UserEntryPrefix + command);

            var interp = new ConsoleInterpreter(ConsoleMemberStore.MemberMap);
            ConsoleExpression[] statements = null;

            try
            {
                statements = interp.InterpretExpressions(CharStreams.fromString(command));
            }
            catch (ParseCanceledException e)
            {
                if (e.InnerException is InputMismatchException rec)
                {
                    Debug.Log(rec.GetType().Name);
                    Debug.LogException(rec);
                    Debug.LogError($"Syntax error on '{rec.OffendingToken.Text}' in \"{rec.Context.GetText()}\": That's all I know.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            if (statements is null)
            {
                return;
            }

            foreach (var statement in statements)
            {
                ExecuteExpression(statement);
            }
        }

        public static void ExecuteExpression(ConsoleExpression expression)
        {
            ConsoleExpression.Result exprResult = null;
            try
            {
                exprResult = expression.Evaluate();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            var isVoid = exprResult.ReturnType == typeof(void);

            //Print a result message if there's anything to print.
            if (isVoid && exprResult.Message is not null || !isVoid)
            {
                var sep = exprResult.Message is not null && isVoid ? " | " : "";
                var returnVal = isVoid ? "" : $"Returned: {GetReturnValueString(exprResult)} ({exprResult.ReturnType.Name})";

                Debug.Log($"{ConsoleLogItem.CommandResultPrefix}{exprResult.Message ?? ""}{sep}{returnVal}");
            }
        }

        private static string GetReturnValueString(ConsoleExpression.Result exprResult)
        {
            return exprResult.Value.ConsoleToString();
        }
    }
}
