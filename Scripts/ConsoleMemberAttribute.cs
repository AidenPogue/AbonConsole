using System;

namespace Terasievert.AbonConsole
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class ConsoleMemberAttribute : Attribute
    {
        public readonly string[] Commands = null;
        public readonly string Description;
        public readonly bool IsCheat;
        public readonly bool AllowPrefix;

        public ConsoleMemberAttribute(string[] commands, string description, bool cheat = false, bool allowPrefix = true)
        {
            Commands = commands;
            Description = description;
            IsCheat = cheat;
            AllowPrefix = allowPrefix;
        }
        public ConsoleMemberAttribute(string command, string description, bool cheat = false, bool allowPrefix = true) : this(new string[] { command }, description, cheat, allowPrefix) { }
        public ConsoleMemberAttribute(string description, bool cheat = false, bool allowPrefix = true) : this(new string[] { "" }, description, cheat, allowPrefix) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConsoleMemberPrefixAttribute : Attribute //Makes all commands in a class start with "<prefix>_" if allowPrefix is true.
    {
        public readonly string Prefix;
        public ConsoleMemberPrefixAttribute(string prefix = "") => Prefix = prefix;
    }
}