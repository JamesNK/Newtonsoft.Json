#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
    internal static class EnumUtils
    {
        private const char EnumSeparatorChar = ',';

        private static readonly ThreadSafeStore<Type, TypeValuesAndNames> ValuesAndNamesPerEnum = new ThreadSafeStore<Type, TypeValuesAndNames>(InitializeValuesAndNames);

        private static TypeValuesAndNames InitializeValuesAndNames(Type enumType)
        {
            string[] names = Enum.GetNames(enumType);
            string[] resolvedNames = new string[names.Length];
            ulong[] values = new ulong[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                FieldInfo f = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
                values[i] = ToUInt64(f.GetValue(null));

                string resolvedName;
#if HAVE_DATA_CONTRACTS
                resolvedName = f.GetCustomAttributes(typeof(EnumMemberAttribute), true)
                         .Cast<EnumMemberAttribute>()
                         .Select(a => a.Value)
                         .SingleOrDefault() ?? f.Name;

                if (Array.IndexOf(resolvedNames, resolvedName, 0, i) != -1)
                {
                    throw new InvalidOperationException("Enum name '{0}' already exists on enum '{1}'.".FormatWith(CultureInfo.InvariantCulture, resolvedName, enumType.Name));
                }
#else
                resolvedName = name;
#endif

                resolvedNames[i] = resolvedName;
            }

            return new TypeValuesAndNames(values, names, resolvedNames);
        }

        public static IList<T> GetFlagsValues<T>(T value) where T : struct
        {
            Type enumType = typeof(T);

            if (!enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                throw new ArgumentException("Enum type {0} is not a set of flags.".FormatWith(CultureInfo.InvariantCulture, enumType));
            }

            Type underlyingType = Enum.GetUnderlyingType(value.GetType());

            ulong num = ToUInt64(value);
            TypeValuesAndNames enumNameValues = GetEnumValuesAndNames(enumType);
            IList<T> selectedFlagsValues = new List<T>();

            for (int i = 0; i < enumNameValues.Values.Length; i++)
            {
                ulong v = enumNameValues.Values[i];

                if ((num & v) == v && v != 0)
                {
                    selectedFlagsValues.Add((T)Convert.ChangeType(v, underlyingType, CultureInfo.CurrentCulture));
                }
            }

            if (selectedFlagsValues.Count == 0 && enumNameValues.Values.Any(v => v == 0))
            {
                selectedFlagsValues.Add(default(T));
            }

            return selectedFlagsValues;
        }

        public static object ParseEnumName(string enumText, bool isNullable, bool disallowValue, Type t)
        {
            if (enumText == string.Empty && isNullable)
            {
                return null;
            }

            return ParseEnum(t, enumText, disallowValue);
        }

        public static string ToEnumName(Type enumType, string enumText, bool camelCaseText)
        {
            TypeValuesAndNames enumValuesAndNames = ValuesAndNamesPerEnum.Get(enumType);

            string[] names = enumText.Split(',');
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i].Trim();

                int? matchingIndex = FindIndexByName(enumValuesAndNames.Names, name, name.Length, 0, StringComparison.Ordinal);
                string resolvedEnumName = matchingIndex != null
                    ? enumValuesAndNames.ResolvedNames[matchingIndex.Value]
                    : name;

                if (camelCaseText)
                {
                    resolvedEnumName = StringUtils.ToCamelCase(resolvedEnumName);
                }

                names[i] = resolvedEnumName;
            }

            string finalName = string.Join(", ", names);

            return finalName;
        }

        public static TypeValuesAndNames GetEnumValuesAndNames(Type enumType)
        {
            return ValuesAndNamesPerEnum.Get(enumType);
        }

        private static ulong ToUInt64(object value)
        {
            PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(value.GetType(), out bool _);

            switch (typeCode)
            {
                case PrimitiveTypeCode.SByte:
                    return (ulong)(sbyte)value;
                case PrimitiveTypeCode.Byte:
                    return (byte)value;
                case PrimitiveTypeCode.Boolean:
                    // direct cast from bool to byte is not allowed
                    return Convert.ToByte((bool)value);
                case PrimitiveTypeCode.Int16:
                    return (ulong)(short)value;
                case PrimitiveTypeCode.UInt16:
                    return (ushort)value;
                case PrimitiveTypeCode.Char:
                    return (char)value;
                case PrimitiveTypeCode.UInt32:
                    return (uint)value;
                case PrimitiveTypeCode.Int32:
                    return (ulong)(int)value;
                case PrimitiveTypeCode.UInt64:
                    return (ulong)value;
                case PrimitiveTypeCode.Int64:
                    return (ulong)(long)value;
                // All unsigned types will be directly cast
                default:
                    throw new InvalidOperationException("Unknown enum type.");
            }
        }

        private static object ParseEnum(Type enumType, string value, bool disallowNumber)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (!enumType.IsEnum())
            {
                throw new ArgumentException("Type provided must be an Enum.", nameof(enumType));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            TypeValuesAndNames entry = ValuesAndNamesPerEnum.Get(enumType);
            string[] enumNames = entry.Names;
            string[] resolvedNames = entry.ResolvedNames;
            ulong[] enumValues = entry.Values;

            // first check if the entire text (including commas) matches a resolved name
            int? matchingIndex = MatchName(value, enumNames, resolvedNames, 0, value.Length, StringComparison.Ordinal);
            if (matchingIndex != null)
            {
                return Enum.ToObject(enumType, enumValues[matchingIndex.Value]);
            }

            int firstNonWhitespaceIndex = -1;
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    firstNonWhitespaceIndex = i;
                    break;
                }
            }
            if (firstNonWhitespaceIndex == -1)
            {
                throw new ArgumentException("Must specify valid information for parsing in the string.");
            }

            // first check whether string is a number
            char firstNonWhitespaceChar = value[firstNonWhitespaceIndex];
            if (char.IsDigit(firstNonWhitespaceChar) || firstNonWhitespaceChar == '-' || firstNonWhitespaceChar == '+')
            {
                Type underlyingType = Enum.GetUnderlyingType(enumType);

                value = value.Trim();
                object temp = null;

                try
                {
                    temp = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    // We need to Parse this as a String instead. There are cases
                    // when you tlbimp enums that can have values of the form "3D".
                    // Don't fix this code.
                }

                if (temp != null)
                {
                    if (disallowNumber)
                    {
                        throw new FormatException("Integer string '{0}' is not allowed.".FormatWith(CultureInfo.InvariantCulture, value));
                    }

                    return Enum.ToObject(enumType, temp);
                }
            }

            ulong result = 0;

            int valueIndex = firstNonWhitespaceIndex;
            while (valueIndex <= value.Length) // '=' is to handle invalid case of an ending comma
            {
                // Find the next separator, if there is one, otherwise the end of the string.
                int endIndex = value.IndexOf(EnumSeparatorChar, valueIndex);
                if (endIndex == -1)
                {
                    endIndex = value.Length;
                }

                // Shift the starting and ending indices to eliminate whitespace
                int endIndexNoWhitespace = endIndex;
                while (valueIndex < endIndex && char.IsWhiteSpace(value[valueIndex]))
                {
                    valueIndex++;
                }

                while (endIndexNoWhitespace > valueIndex && char.IsWhiteSpace(value[endIndexNoWhitespace - 1]))
                {
                    endIndexNoWhitespace--;
                }
                int valueSubstringLength = endIndexNoWhitespace - valueIndex;

                matchingIndex = MatchName(value, enumNames, resolvedNames, valueIndex, valueSubstringLength, StringComparison.Ordinal);

                // if no match found, attempt case insensitive search
                if (matchingIndex == null)
                {
                    matchingIndex = MatchName(value, enumNames, resolvedNames, valueIndex, valueSubstringLength, StringComparison.OrdinalIgnoreCase);
                }

                // If we couldn't find a match
                if (matchingIndex == null)
                {
                    // before we throw an error, check whether the entire string has a case insensitive match against resolve names
                    matchingIndex = MatchName(value, enumNames, resolvedNames, 0, value.Length, StringComparison.OrdinalIgnoreCase);
                    if (matchingIndex != null)
                    {
                        return Enum.ToObject(enumType, enumValues[matchingIndex.Value]);
                    }

                    // no match so error
                    throw new ArgumentException("Requested value '{0}' was not found.".FormatWith(CultureInfo.InvariantCulture, value));
                }

                result |= enumValues[matchingIndex.Value];

                // Move our pointer to the ending index to go again.
                valueIndex = endIndex + 1;
            }

            return Enum.ToObject(enumType, result);
        }

        private static int? MatchName(string value, string[] enumNames, string[] resolvedNames, int valueIndex, int valueSubstringLength, StringComparison comparison)
        {
            int? matchingIndex = FindIndexByName(resolvedNames, value, valueSubstringLength, valueIndex, comparison);
            if (matchingIndex == null)
            {
                matchingIndex = FindIndexByName(enumNames, value, valueSubstringLength, valueIndex, comparison);
            }

            return matchingIndex;
        }

        private static int? FindIndexByName(string[] enumNames, string value, int valueSubstringLength, int valueIndex, StringComparison comparison)
        {
            for (int i = 0; i < enumNames.Length; i++)
            {
                if (enumNames[i].Length == valueSubstringLength &&
                    string.Compare(enumNames[i], 0, value, valueIndex, valueSubstringLength, comparison) == 0)
                {
                    return i;
                }
            }

            return null;
        }
    }

    internal class TypeValuesAndNames
    {
        public TypeValuesAndNames(ulong[] values, string[] names, string[] resolvedNames)
        {
            Values = values;
            Names = names;
            ResolvedNames = resolvedNames;
        }

        public readonly ulong[] Values;
        public readonly string[] Names;
        public readonly string[] ResolvedNames;
    }
}