using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    /// <summary>
    /// Represents an expression that invokes a method or constructor with a set of parameters.
    /// </summary>
    public class MethodInvocationExpression : ConsoleExpression
    {
        protected readonly MethodBase method;
        protected readonly IList<ConsoleExpression> parameters;

        public MethodInvocationExpression(MethodBase method, IList<ConsoleExpression> parameters)
        {
            this.method = method ?? throw new ArgumentNullException(nameof(method));
            this.parameters = parameters ?? new ConsoleExpression[0];
        }

        public override Type ReturnType => (method as MethodInfo)?.ReturnType ?? (method as ConstructorInfo)?.DeclaringType ?? null;

        public override object EvaluateValueOnly()
        {
            var evaluatedParams = parameters.Select(p => p.EvaluateValueOnly()).ToArray();
            return method.Invoke(null, evaluatedParams);
        }

        protected string GetParameterListString()
        {
            return "(" + string.Join(", ", parameters) + ")";
        }

        public override string ToString()
        {
            return $"{nameof(MethodInvocationExpression)}: {method.DeclaringType.Name}.{method.Name}{GetParameterListString()}";
        }
    }
}
