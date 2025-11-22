using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Terasievert.AbonConsole
{
    public static class AbonConsoleTools
    {
        //Thanks chat gpt!
        /// <summary>
        /// Maps numeric types to a set of all other numeric types they can be implicitly converted to.
        /// </summary>
        private static readonly Dictionary<Type, HashSet<Type>> implicitNumericConversions = new()
        {
            [typeof(sbyte)] = new() { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) },
            [typeof(byte)] = new() { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
            [typeof(short)] = new() { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) },
            [typeof(ushort)] = new() { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
            [typeof(int)] = new() { typeof(long), typeof(float), typeof(double), typeof(decimal) },
            [typeof(uint)] = new() { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
            [typeof(long)] = new() { typeof(float), typeof(double), typeof(decimal) },
            [typeof(ulong)] = new() { typeof(float), typeof(double), typeof(decimal) },
            [typeof(char)] = new() { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) },
            [typeof(float)] = new() { typeof(double) },
        };

        public static string RemoveQuoteCharacters(this string s)
        {
            return s.Replace("\"", "").Replace("'", "");
        }

        /// <summary>
        /// Returns the string that should be used to display obj in the console.
        /// </summary>
        /// <remarks>For example, it will add quote characters to strings and chars.</remarks>
        public static string ConsoleToString(this object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var type = obj.GetType();

            if (type == typeof(string))
            {
                return "\"" + obj + "\"";
            }
            if (type == typeof(char))
            {
                return "'" + obj + "'";
            }

            return obj.ToString();
        }

        /// <summary>
        /// Returns if a field or property can be written to.
        /// </summary>
        public static bool IsReadOnly(this MemberInfo memberInfo)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                return (fieldInfo.IsInitOnly || fieldInfo.IsLiteral);
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return !propertyInfo.CanWrite | (!propertyInfo.SetMethod?.IsPublic ?? true);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the type of an object, or object if it's null.
        /// </summary>
        /// <returns></returns>
        public static Type GetTypeNullSafe(this object obj)
        {
            return obj == null ? typeof(object) : obj.GetType();
        }

        /// <summary>
        /// Checks if a variable of type <paramref name="type"/> could be assigned a value of type <paramref name="assignFrom"/>, including implicit conversions. The implicit operator method will be returned if it exists.
        /// </summary>
        /// <param name="implicitOperator">The MethodInfo of the implicit operator on <paramref name="assignFrom"/>, if it exists.</param>
        public static bool IsAssignableOrCastableFrom(this Type type, Type assignFrom, out MethodInfo implicitOperator)
        {
            implicitOperator = null;
            //TODO:(?) Check for implicit operators.
            if (type.IsAssignableFrom(assignFrom))
            {
                return true;
            }
            else if (implicitNumericConversions.TryGetValue(assignFrom, out var conversions))
            {
                return conversions.Contains(type);
            }
            /*
            else
            {
                implicitOperator = assignFrom.GetAllMembers().FirstOrDefault(member =>
                {
                    if (member is MethodInfo method && method.Name == "op_Implicit" && type.IsAssignableOrCastableFrom(method.ReturnType))
                    {
                        var p = method.GetParameters().FirstOrDefault();

                        return p is not null && p.ParameterType.IsAssignableOrCastableFrom(assignFrom);
                    }

                    return false;
                }) as MethodInfo;

                if (implicitOperator is not null)
                {
                    return true;
                }
            }
            */

            return false;
        }

        /// <summary>
        /// Checks if a variable of type <paramref name="type"/> could be assigned a value of type <paramref name="assignFrom"/>, including implicit conversions.
        /// </summary>
        public static bool IsAssignableOrCastableFrom(this Type type, Type assignFrom)
        {
            return IsAssignableOrCastableFrom(type, assignFrom, out var _);
        }

        /// <summary>
        /// Returns whether a variable of type <paramref name="type"/> can be assigned a null value.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return !(type.IsValueType || type.IsEnum || type.IsPrimitive);
        }

        public static string Unescape(string str, char delimiter)
        {
            if (str is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            //Regex method obviously doesn't escape the delimiter
            str = str.Replace("\\" + delimiter, delimiter.ToString());
            try
            {
                return Regex.Unescape(str);
            }
            catch (Exception e)
            {
                throw new FormatException($"Unrecognized escape sequence in string/char literal: {delimiter}{str}{delimiter}", e);
            }
        }

        public static char Unescape(char chr, char delimiter)
        {
            return Unescape(chr.ToString(), delimiter)[0];
        }

        /// <summary>
        /// Gets the return type of a method or constructor, or the type of a field or property.
        /// </summary>
        /// <exception cref="ArgumentException">If the member is not one of the above supported types.</exception>
        public static Type GetReturnType(this MemberInfo member)
        {
            if (member is MethodInfo method)
            {
                return method.ReturnType;
            }
            else if (member is FieldInfo field)
            {
                return field.FieldType;
            }
            else if (member is PropertyInfo property)
            {
                return property.PropertyType;
            }
            else if (member is ConstructorInfo constructor)
            {
                return constructor.DeclaringType;
            }
            else
            {
                throw new ArgumentException("The member is not one of the supported types.", nameof(member));
            }
        }

        public static IList<MemberInfo> GetAllMembers(this Type type)
        {
            var res = new List<MemberInfo>(50);

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var currentType = type;

            while (currentType is not null)
            {
                res.AddRange(currentType.GetMembers());

                currentType = currentType.BaseType;
            }

            return res;
        }

        /// <summary>
        /// Returns a string in the form "Param1Type param1Name, Param2Type param2Name, ..."
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetParametersSignature(this MethodBase method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        }
    }

}