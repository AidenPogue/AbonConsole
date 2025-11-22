using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KeenIdeas.Tools
{
    /// <summary>
    /// Provides utilities for getting custom attributes.
    /// </summary>
    public static class CustomAttributeTool
    {
        private static Dictionary<Type, List<CustomAttributeMemberInfoPair<Attribute>>> attributes;
        private static readonly int listCapacity = 16;

        /// <summary>
        /// Returns all attributes of type T across all loaded assembies
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CustomAttributeMemberInfoPair<T>> GetCustomAttributesOfType<T>() where T : Attribute
        {
            //if (typeof(Attribute).IsAssignableFrom(type)) throw new ArgumentException("Type was not an attribute", nameof(type));
            return attributes[typeof(T)].Select(pair => new CustomAttributeMemberInfoPair<T>(pair.MemberInfo, pair.Attribute as T)); //Generic hell moment
        }

        static CustomAttributeTool()
        {
            attributes = new Dictionary<Type, List<CustomAttributeMemberInfoPair<Attribute>>>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (MemberInfo member in type.GetMembers())
                {
                    foreach (Attribute attribute in member.GetCustomAttributes())
                    {
                        var attributeType = attribute.GetType();
                        if (!attributes.ContainsKey(attributeType)) attributes.Add(attributeType, new List<CustomAttributeMemberInfoPair<Attribute>>(listCapacity));
                        attributes[attributeType].Add(new CustomAttributeMemberInfoPair<Attribute>(member, attribute));
                    }
                }
            }
        }
    }

    /// <summary>
    /// A custom attribute and the member it's attached to.
    /// </summary>
    public class CustomAttributeMemberInfoPair<AttributeT> where AttributeT : Attribute
    {
        public readonly MemberInfo MemberInfo;
        public readonly AttributeT Attribute;

        public CustomAttributeMemberInfoPair(MemberInfo memberInfo, AttributeT attribute)
        {
            MemberInfo = memberInfo;
            Attribute = attribute;
        }
    }

}
