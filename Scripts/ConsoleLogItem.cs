using System;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    public class ConsoleLogItem
    {
        public const string
            UserEntryPrefix = "!>>",
            CommandResultPrefix = "!<<";

        public enum SpecialLogType
        {
            None,
            UserEntry,
            CommandResult
        }

        /// <summary>
        /// Returns the special type of the log string based on the prefix and places the string with the prefix removed in newValue.
        /// </summary>
        public static AbonLogType GetSpecialTypeFromString(string value, AbonLogType logType, out string newValue)
        {
            newValue = value;
            if (string.IsNullOrEmpty(value))
            {
                return logType;
            }
            else if (value.StartsWith(UserEntryPrefix))
            {
                newValue = newValue.Substring(UserEntryPrefix.Length);
                return AbonLogType.UserEntry;
            }
            else if (value.StartsWith(CommandResultPrefix))
            {
                newValue = newValue.Substring(CommandResultPrefix.Length);
                return AbonLogType.CommandResult;
            }
            else
            {
                return logType;
            }
        }

        public static AbonLogType GetAbonLogTypeFromLogType(LogType logType)
        {
            return (AbonLogType)logType;
        }

        public readonly string CachedToString;

        public readonly object Content;
        public readonly AbonLogType LogLevel;
        public readonly string Trace;
        public readonly DateTime Time;
        public readonly int SortIndex;


        public ConsoleLogItem(object log, AbonLogType logType, string trace, DateTime time, int sortIndex)
        {
            Content = log;
            LogLevel = GetSpecialTypeFromString(log?.ToString() ?? null, logType, out CachedToString);
            Trace = trace;
            Time = time;
            SortIndex = sortIndex;
        }

        public ConsoleLogItem(object log, LogType logType, string trace, DateTime time, int sortIndex) : this(log, GetAbonLogTypeFromLogType(logType), trace, time, sortIndex) { }
    }
}