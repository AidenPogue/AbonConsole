using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    /// <summary>
    /// Builds a dictionary of ConsoleMembers by searching for attributes.
    /// </summary>
    public static class ConsoleMemberStore
    {
        private const string NotStaticError = "not static";
        private const string NotPublicError = "not public";

        private static readonly Regex pascalToSnakeCaseRegex = new("(?<=[a-z])[A-Z]", RegexOptions.Compiled);

        /// <summary>
        /// Maps all member names to their ConsoleMember object.
        /// </summary>
        public static ReadOnlyDictionary<string, ConsoleMember> MemberMap { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            var dict = new Dictionary<string, ConsoleMember>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (MemberInfo member in type.GetMembers())
                {
                    var attribute = member.GetCustomAttribute<ConsoleMemberAttribute>();

                    if (attribute is not null)
                    {
                        var errReason = GetErrorReasonFromAttributeMember(member);

                        if (errReason is not null)
                        {
                            PrintInvalidAttributeCommandMessage(member, errReason);
                            continue;
                        }

                        foreach (var name in attribute.Commands)
                        {
                            var prefixAttribute = member.DeclaringType.GetCustomAttribute<ConsoleMemberPrefixAttribute>();
                            //Get prefix if it's not null, and set prefix to the name of the class if no name is provided.
                            string prefix = prefixAttribute != null ? (prefixAttribute.Prefix ?? member.DeclaringType.Name) + "_" : "";
                            //Set command to the name of the member if a name was not provided and adds the prefix if allowed.
                            string finalCommand = MemberNameTransformer($"{(attribute.AllowPrefix ? prefix : "")}{(string.IsNullOrEmpty(name) ? member.Name : name)}");

                            var consoleMember = new ConsoleMember(member, finalCommand, attribute.Description, attribute.IsCheat);

                            if (!dict.ContainsKey(finalCommand))
                            {
                                dict.Add(finalCommand, consoleMember);
                            }
                            else
                            {
                                Debug.LogWarning($"Tried to add console command {finalCommand}, but it already exists. It will be ignored");
                            }
                        }
                    }
                }
            }

            MemberMap = new ReadOnlyDictionary<string, ConsoleMember>(dict);

            Debug.Log($"Initialized console member store, found {MemberMap.Count} attribute commands.");
        }

        private static string MemberNameTransformer(string memberName)
        {
            var matches = pascalToSnakeCaseRegex.Matches(memberName);
            var insertions = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                memberName = memberName.Insert(matches[i].Index + insertions++, "_");
            }

            return memberName.ToLower();
        }

        private static string GetErrorReasonFromAttributeMember(MemberInfo member)
        {
            if (member is FieldInfo f)
            {
                if (!f.IsStatic)
                {
                    return NotStaticError;
                }
                else if (f.IsLiteral)
                {
                    return "a constant field";
                }
                else if (!f.IsPublic)
                {
                    return NotPublicError;
                }
            }
            else if (member is PropertyInfo p)
            {
                if (!p.GetMethod.IsStatic)
                {
                    return NotStaticError;
                }
            }
            else if (member is MethodInfo m)
            {
                if (!m.IsStatic)
                {
                    return NotStaticError;
                }
                else if (!m.IsPublic)
                {
                    return NotPublicError;
                }
            }

            return null;
        }

        private static void PrintInvalidAttributeCommandMessage(MemberInfo member, string errorReason)
        {
            Debug.LogError($"The member {member.DeclaringType.Name}.{member.Name} has a {nameof(ConsoleMemberAttribute)} attribute, but cannot be used because it is {errorReason}. It will be ignored.");
        }
    }
}
