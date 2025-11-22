using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terasievert.AbonConsole.AntlrGenerated;
using Terasievert.AbonConsole.Interpreter.Expressions;

namespace Terasievert.AbonConsole.Interpreter
{
    /// <summary>
    /// Antlr visitor that returns console expressions.
    /// </summary>
    public class ConsoleExpressionVisitor : ConsoleParserBaseVisitor<ConsoleExpression>
    {
        private readonly IDictionary<string, ConsoleMember> memberMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleExpressionVisitor"/> class.
        /// </summary>
        /// <param name="memberMap">A dictionary that maps member names to their corresponding <see cref="ConsoleMember"/> objects. This map is
        /// used to resolve and process console expressions.</param>
        public ConsoleExpressionVisitor(IDictionary<string, ConsoleMember> memberMap)
        {
            this.memberMap = memberMap ?? throw new ArgumentNullException(nameof(memberMap));
        }

        public override ConsoleExpression VisitHelpStatement([NotNull] ConsoleParser.HelpStatementContext context)
        {
            var member = GetMemberFromID(context.memberName, context);
            return new HelpStatement(member);
        }

        public override ConsoleExpression VisitCall([NotNull] ConsoleParser.CallContext context)
        {
            var member = GetMemberFromID(context.methodName, context.Parent as ParserRuleContext);
            var args = HandleArgList(context.args);

            //Handle assignment, which can be formatted like a call.
            if (args?.Count == 1 && (member.MemberInfo.MemberType == MemberTypes.Field || member.MemberInfo.MemberType == MemberTypes.Property))
            {
                ValidateAssignment(member, args[0], context, context.args._args[0]);
                return new ConsoleMemberWriteExpression(member, args[0]);
            }

            ValidateMethodInvocation(member, args, context.args, context);
            return new ConsoleMethodInvocationExpression(member, args);
        }

        public override ConsoleExpression VisitDotCall([NotNull] ConsoleParser.DotCallContext context)
        {
            var member = GetMemberFromID(context.methodName, context);
            var dotExpression = Visit(context.dotExpr);
            var args = HandleArgList(context.args, dotExpression);

            ValidateMethodInvocation(member, args, context.argList(), context);

            return new ConsoleMethodInvocationExpression(member, args);
        }

        public override ConsoleExpression VisitNumLiteral([NotNull] ConsoleParser.NumLiteralContext context)
        {
            var lit = context.literal;

            var text = lit.Text;
            //For double and long literals that have a d or l character at the end
            var textNoLastChar = text[..^1];

            switch (lit.Type)
            {
                //We aren't doing any checks for these because float/double parse won't throw an overflow, and if we get a format exception something is wrong with the parser.
                case ConsoleParser.FLOAT:
                    return new ConstantValueExpression(float.Parse(text), typeof(float));

                case ConsoleParser.DOUBLE:
                    return new ConstantValueExpression(double.Parse(textNoLastChar), typeof(double));

                case ConsoleParser.INT:
                    if (int.TryParse(text, out var iVal))
                    {
                        return new ConstantValueExpression(iVal, typeof(int));
                    }
                    else
                    {
                        throw new OverflowException($"The value {text} is too large for an int");
                    }

                case ConsoleParser.LONG:
                    if (long.TryParse(textNoLastChar, out var lVal))
                    {
                        return new ConstantValueExpression(lVal, typeof(long));
                    }
                    else
                    {
                        throw new OverflowException($"The value {text} is too large for a long");
                    }

                default:
                    throw new InvalidOperationException("Tried to visit NumLiteral that didn't have any number tokens.");
            }
        }

        public override ConsoleExpression VisitTextLiteral([NotNull] ConsoleParser.TextLiteralContext context)
        {
            var lit = context.literal;

            var text = lit.Text;

            //We'll have at least the two delimiters, unless the parser broke.
            if (text.Length < 2)
            {
                throw new InvalidOperationException("Parser gave a text literal with less than 2 chars.");
            }

            var unQuotedText = text[1..^1];

            if (lit.Type == ConsoleParser.STR)
            {
                return new ConstantValueExpression(AbonConsoleTools.Unescape(unQuotedText, '"'), typeof(string));
            }
            else if (lit.Type == ConsoleParser.CHAR)
            {
                var escaped = AbonConsoleTools.Unescape(unQuotedText, '\'');

                if (escaped.Length != 1)
                {
                    throw new FormatException($"The char literal {text} has {unQuotedText.Length} characters. Char literals must have exactly 1 character.");
                }
                return new ConstantValueExpression(escaped[0], typeof(char));
            }

            throw new InvalidOperationException("Tried to visit TextLiteral that didn't have any text tokens.");
        }

        public override ConsoleExpression VisitBoolLiteral([NotNull] ConsoleParser.BoolLiteralContext context)
        {
            return new ConstantValueExpression(context.literal.Type == ConsoleParser.TRUE, typeof(bool));
        }

        public override ConsoleExpression VisitNullExpr([NotNull] ConsoleParser.NullExprContext context)
        {
            return new ConstantValueExpression(null, typeof(ConsoleNullValue));
        }

        public override ConsoleExpression VisitNestedStatementExpr([NotNull] ConsoleParser.NestedStatementExprContext context)
        {
            return new ConstantValueExpression(Visit(context.statement()), typeof(ConsoleExpression));
        }

        public override ConsoleExpression VisitLoneId([NotNull] ConsoleParser.LoneIdContext context)
        {
            var consoleMember = GetMemberFromID(context.ID(), context);

            //A lone ID will be a zero arg call or field/property read.
            if (consoleMember.MemberInfo is MethodBase)
            {
                ValidateMethodInvocation(consoleMember, null, null, context);
                return new ConsoleMethodInvocationExpression(consoleMember, null);
            }
            else
            {
                return new ConsoleMemberReadExpression(consoleMember);
            }
        }

        public override ConsoleExpression VisitAssignment([NotNull] ConsoleParser.AssignmentContext context)
        {
            var member = GetMemberFromID(context.lhs, context);

            var toWrite = Visit(context.rhs);

            ValidateAssignment(member, toWrite, context, context.rhs);

            return new ConsoleMemberWriteExpression(member, toWrite);
        }

        public override ConsoleExpression VisitParenExpr([NotNull] ConsoleParser.ParenExprContext context)
        {
            return Visit(context.expr());
        }

        public override ConsoleExpression VisitUnaryOp([NotNull] ConsoleParser.UnaryOpContext context)
        {
            var expr = Visit(context.value);
            switch (context.@operator.Type)
            {
                case ConsoleParser.EXCLAIM:
                    ValidateAssignableFrom(typeof(bool), expr.ReturnType, context, context.value);
                    return new BooleanNotExpression(expr);

                default:
                    throw new InvalidOperationException($"The unary operator {context.@operator.Text} is not supported.");
            }
        }

        /*
        public override ConsoleExpression VisitBinaryOp([NotNull] ConsoleParser.BinaryOpContext context)
        {
            throw new NotImplementedException();
        }
        */

        #region Helpers

        /// <summary>
        /// Builds and returns a list of the arguments in the ArgListContext, with the optional dotExpression being inserted as the first.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dotExpression"></param>
        /// <returns>A list of expressions from the arg list, with the dotExpression at index 0 if not null. If both are null, returns null.</returns>
        private IList<ConsoleExpression> HandleArgList(ConsoleParser.ArgListContext context, ConsoleExpression dotExpression = null)
        {
            if (context is null && dotExpression is null)
            {
                return null;
            }

            var argList = new List<ConsoleExpression>((context?._args.Count ?? 0) + (dotExpression is null ? 0 : 1));

            if (dotExpression is not null)
            {
                argList.Add(dotExpression);
            }

            if (context is not null)
            {
                argList.AddRange(context._args.Select(e => Visit(e)));
            }

            return argList;
        }

        private ConsoleMember GetMemberFromID(ITerminalNode idNode, ParserRuleContext context)
        {
            if (idNode is null)
            {
                throw new ArgumentNullException(nameof(idNode));
            }

            return GetMemberFromID(idNode.GetText(), context);
        }

        private ConsoleMember GetMemberFromID(IToken idToken, ParserRuleContext context)
        {
            if (idToken is null)
            {
                throw new ArgumentNullException(nameof(idToken));
            }

            return GetMemberFromID(idToken.Text, context);
        }

        private ConsoleMember GetMemberFromID(string memberName, ParserRuleContext context)
        {
            if (memberMap.TryGetValue(memberName, out var member))
            {
                return member;
            }
            else
            {
                ThrowMemberNotFound(memberName, context);
                return null;
            }
        }

        private void ThrowMemberNotFound(string name, ParserRuleContext context)
        {
            throw new ConsoleInterpreterException(context, $"The console member with name '{name}' does not exist.");
        }

        #endregion

        #region Validators 

        /// <summary>
        /// Throws an appropriate exception if the method invocation info provided is invalid. Otherwise does nothing.
        /// </summary>
        /// <param name="consoleMember">The console member accociated with the id</param>
        /// <param name="arguments">Parsed arguments. May be null if the method takes 0 args.</param>
        /// <param name="argList">The arg list context, if there is one. For more descriptive error messages.</param>
        /// <param name="mainContext">The parent context of the expression representing the call. Must be a DotCallContext, CallContext, or LoneIdContext</param>
        /// 
        /// <exception cref="ConsoleInterpreterException">Thrown if the number of parameters is mismatched, or the console member isn't a method.</exception>
        private void ValidateMethodInvocation(ConsoleMember consoleMember, IList<ConsoleExpression> arguments, ConsoleParser.ArgListContext argList, ConsoleParser.ExprContext mainContext)
        {
            if (consoleMember is null)
            {
                throw new ArgumentNullException(nameof(consoleMember));
            }
            if (mainContext is null)
            {
                throw new ArgumentNullException(nameof(mainContext));
            }

            ConsoleParser.DotCallContext dotCallContext = mainContext as ConsoleParser.DotCallContext;
            ConsoleParser.CallContext callContext = mainContext as ConsoleParser.CallContext;
            ConsoleParser.LoneIdContext loneIdContext = mainContext as ConsoleParser.LoneIdContext;

            if (dotCallContext is null && callContext is null && loneIdContext is null)
            {
                throw new ArgumentException("mainContext must be a DotCallContext, CallContext, or LoneIdContext", nameof(mainContext));
            }

            if (consoleMember.MemberInfo is MethodBase methodBase)
            {
                var parameters = methodBase.GetParameters();

                int argLength = arguments?.Count ?? 0, paramLength = parameters.Length;

                if (argLength != paramLength)
                {
                    throw new ConsoleInterpreterException(mainContext, $"The method {consoleMember.Name} expects {parameters.Length} arguments, got {argLength}");
                }

                for (int i = 0; i < argLength; i++)
                {
                    //Get the expr corresponding to the current arg, which may be the dot call expr.
                    var isDotCall = dotCallContext is not null;
                    var argExpr = isDotCall && i == 0 ? dotCallContext.dotExpr : argList?._args[isDotCall ? i - 1 : i];

                    ValidateAssignableFrom(parameters[i].ParameterType, arguments[i].ReturnType, mainContext, argExpr ?? mainContext);
                }
            }
            else
            {
                throw new ConsoleInterpreterException(mainContext, $"The console member {consoleMember.Name} is a {consoleMember.MemberInfo.MemberType.ToString().ToLower()}, and cannot be called like a method.");
            }
        }

        private void ValidateAssignment(ConsoleMember consoleMember, ConsoleExpression valueToAssign, ParserRuleContext mainContext, ParserRuleContext valueToAssignContext)
        {
            if (consoleMember is null)
            {
                throw new ArgumentNullException(nameof(consoleMember));
            }
            if (valueToAssign is null)
            {
                throw new ArgumentNullException(nameof(valueToAssign));
            }
            if (mainContext is null)
            {
                throw new ArgumentNullException(nameof(mainContext));
            }

            if (consoleMember.MemberInfo.MemberType != MemberTypes.Field && consoleMember.MemberInfo.MemberType != MemberTypes.Property)
            {
                throw new ConsoleInterpreterException(mainContext, $"The console member {consoleMember.Name} is a {consoleMember.MemberInfo.MemberType.ToString().ToLower()}, and cannot be assigned like a field or property.");
            }
            if (consoleMember.IsReadOnly)
            {
                throw new ConsoleInterpreterException(mainContext, $"The console member {consoleMember.Name} is read only, and cannot be assigned to.");
            }

            ValidateAssignableFrom(consoleMember.MemberInfo.GetReturnType(), valueToAssign.ReturnType, mainContext, valueToAssignContext);
        }

        /// <summary>
        /// Throws an appropriate exception if an object of type <paramref name="assignFrom"/> cannot be assigned to a value of type <paramref name="type"/>. Otherwise does nothing.
        /// </summary>
        /// <param name="type">Type to be assigned to</param>
        /// <param name="assignFrom">Type we're trying to assign.</param>
        /// <param name="context">Context of the expression containing this operation.</param>
        /// <param name="tokenInterval">The interval of the token who has type <paramref name="assignFrom"/></param>
        /// <exception cref="ConsoleInterpreterException">If the types could not be assigned.</exception>
        private void ValidateAssignableFrom(Type type, Type assignFrom, ParserRuleContext context, ISyntaxTree assignFromTree)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Handle special null
            if (assignFrom == typeof(ConsoleNullValue))
            {
                if (!type.IsNullable())
                {
                    throw new ConsoleInterpreterException(context, assignFromTree, $"Null cannot be assigned to a value of type {type.Name}.");
                }

                //If the type is nullable we don't need to do any further checks.
                return;
            }
            else if (!AbonConsoleTools.IsAssignableOrCastableFrom(type, assignFrom))
            {
                throw new ConsoleInterpreterException(context, assignFromTree, $"Type {assignFrom.Name} cannot be assigned to a value of type {type.Name}.");
            }
        }

        #endregion
    }
}
