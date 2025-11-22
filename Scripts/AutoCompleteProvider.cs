using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terasievert.AbonConsole.AntlrGenerated;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    /// <summary>
    /// Provides auto complete results for console commands.
    /// </summary>
    public static class AutoCompleteProvider
    {
        private static readonly HashSet<AutoCompletion> autoCompleteItems = new(50);
        private static readonly int[] keywordTokens =
        {
            ConsoleParser.FALSE,
            ConsoleParser.TRUE,
            ConsoleParser.NEW,
            ConsoleParser.HELP,
            ConsoleParser.NULL,
        };

        /// <summary>
        /// Maps lexer token types to auto completion types allowed for that token. If a token type is not in here, it will not be auto completed.
        /// </summary>
        private static readonly Dictionary<int, AutoCompleteType> tokenToCompletionType = new()
        {
            [ConsoleParser.ID] = AutoCompleteType.MemberName | AutoCompleteType.Keyword,
            [ConsoleParser.STR] = AutoCompleteType.File | AutoCompleteType.Addressable,
        };

        static AutoCompleteProvider()
        {
            AddAutoCompletions(ConsoleMemberStore.MemberMap.Keys, AutoCompleteType.MemberName);
            AddAutoCompletions(keywordTokens.Select(t => ConsoleParser.DefaultVocabulary.GetLiteralName(t).Trim('\'')), AutoCompleteType.Keyword);
        }

        public static void AddAutoCompletions(IEnumerable<AutoCompletion> items)
        {
            foreach (var item in items)
            {
                autoCompleteItems.Add(item);
            }
        }

        public static void AddAutoCompletions(IEnumerable<string> tokens, AutoCompleteType type)
        {
            AddAutoCompletions(tokens.Select(t => new AutoCompletion(t, type)));
        }

        public static IList<AutoCompletion> GetAutoCompletions(string token, int count, AutoCompleteType types = AutoCompleteType.Any)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return autoCompleteItems.Where(c => c.Token.StartsWith(token, StringComparison.OrdinalIgnoreCase) || c.Token.Contains(token, StringComparison.OrdinalIgnoreCase) && (c.Type & types) != 0)
                                    .Take(count)
                                    //Should probably order by edit distance or something, but this works.
                                    .OrderBy(c => c.Token.StartsWith(token) ? 0 : 1)
                                    .ToArray();
        }
    }

    public readonly struct AutoCompletion
    {
        public readonly string Token;
        public readonly AutoCompleteType Type;

        public AutoCompletion(string token, AutoCompleteType type)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Type = type;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Token.GetHashCode(), Type.GetHashCode());
        }
    }

    [Flags]
    public enum AutoCompleteType
    {
        Any = ~0,
        MemberName = 1 << 0,
        Keyword = 1 << 1,
        File = 1 << 2,
        Addressable = 1 << 3,
    }
}
