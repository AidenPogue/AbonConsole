using Sirenix.Utilities;
using System.Reflection;

namespace Terasievert.AbonConsole
{
    public class ConsoleMember
    {
        public readonly MemberInfo MemberInfo;
        public readonly string Name;
        public readonly string Description;
        public readonly bool IsCheat, IsReadOnly;

        public readonly string[] ToStringColumns;

        private readonly string cachedToString;

        public ConsoleMember(MemberInfo memberInfo, string name, string description, bool isCheat)
        {
            MemberInfo = memberInfo;
            Name = name;
            Description = description;
            IsCheat = isCheat;

            IsReadOnly = memberInfo.IsReadOnly();

            var firstColumn = name;
            if (MemberInfo is MethodInfo methodInfo)
            {
                firstColumn = $"{Name}({methodInfo.GetParametersSignature()})";
            }

            var memberTypeColumn = $"{(IsCheat ? "CHEAT " : "")}{(IsReadOnly ? "read-only " : "")}{memberInfo.GetReturnType().Name} {memberInfo.MemberType}";

            ToStringColumns = new string[] { firstColumn, memberTypeColumn, description };
            cachedToString = TabularLogTools.Format(new[] { ToStringColumns }, " | ")[0];
        }

        public override string ToString()
        {
            return cachedToString;
        }
    }
}
