using System;
using System.Linq;
using Terasievert.AbonConsole.Interpreter.Expressions;
using UnityEditor;
using UnityEngine;

namespace Terasievert.AbonConsole.BuiltInCommands
{
    public static class CommonCommands
    {
        [ConsoleMember("listcmds", "Lists all available commands.", false, false)]
        public static void ListCommands()
        {
            Debug.Log("<u>All Commands:</u>");
            var commandRows = ConsoleMemberStore.MemberMap.Values.OrderBy(m => m.Name).Select(m => m.ToStringColumns).ToArray();
            TabularLogTools.Log(commandRows, " | ");
        }

        [ConsoleMember(new string[] { "", "qqq" }, "Quits the game")]
        public static void Quit()
        {
            Debug.Log("Bye!");

#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }

        [ConsoleMember("Returns the string representation of the object.")]
        public static string ToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.ToString();
        }

        [ConsoleMember("Logs message repeatCount times.")]
        public static void Repeat(object message, int repeatCount)
        {
            if (repeatCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeatCount));
            }

            for (int i = 0; i < repeatCount; i++)
            {
                Debug.Log(message);
            }
        }

        [ConsoleMember("Evaluates a nested expression.")]
        public static void EvaluateNestedExpression(ConsoleExpression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            CommandExecutor.ExecuteExpression(expression);
        }

        [ConsoleMember("Sets the game's time scale.", cheat: true)]
        public static float TimeScale { get => Time.timeScale ; set => Time.timeScale = value; }
    }
}
