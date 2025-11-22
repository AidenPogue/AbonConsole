using System;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    /// <summary>
    /// Base class for all expressions that can be interpreted from a console command. An expression does some operation when Evaluate() is called, and may return a value.
    /// </summary>
    /// <remarks>
    /// Void method calls and assignments have ReturnType void and return null from Evaluate
    /// </remarks>
    public abstract class ConsoleExpression
    {
        private static Func<bool> cheatsEnabled;

        /// <summary>
        /// If set, this delegate will be used do determine if cheat expressions can be used.
        /// </summary>
        public static Func<bool> CheatsEnabledDelegate
        {
            get => cheatsEnabled;
            set
            {
                if (cheatsEnabled is not null)
                {
                    throw new InvalidOperationException(nameof(CheatsEnabledDelegate) + " already has a value. It should only be set once.");
                }
                cheatsEnabled = value;
            }
        }

        /// <summary>
        /// Are cheats enabled?
        /// </summary>
        public static bool CheatsEnabled => CheatsEnabledDelegate?.Invoke() ?? true;

        public class Result
        {
            /// <summary>
            /// The value of the evaluation, if the return type isn't null.
            /// </summary>
            public readonly object Value;
            /// <summary>
            /// Message associated with the result, to be displayed to the user.
            /// </summary>
            public readonly string Message;
            public readonly Type ReturnType;

            public Result(object value, string message, Type returnType)
            {
                Value = value;
                Message = message;
                ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            }
        }


        public abstract Type ReturnType { get; }

        /// <summary>
        /// Executes the operation associated with this expression.
        /// </summary>
        /// <returns>Only the resulting value (if it exists). </returns>
        public abstract object EvaluateValueOnly();

        /// <summary>
        /// Executes the operation associated with this expression.
        /// </summary>
        /// <returns>A <see cref="Result"></see> containing information about the evaluation.</returns>
        /// <remarks>The default implementation gets a result value from EvaluateValueOnly() and does not assign a message.</remarks>
        public virtual Result Evaluate()
        {
            return new Result(EvaluateValueOnly(), null, ReturnType);
        }
    }
}
