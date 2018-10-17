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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
    internal static class EnumUtils
    {
        private const char EnumSeparatorChar = ',';
        private const string EnumSeparatorString = ", ";

        private static readonly ThreadSafeStore<StructMultiKey<Type, NamingStrategy>, EnumInfo> ValuesAndNamesPerEnum = new ThreadSafeStore<StructMultiKey<Type, NamingStrategy>, EnumInfo>(InitializeValuesAndNames);

        private static EnumInfo InitializeValuesAndNames(StructMultiKey<Type, NamingStrategy> key)
        {
            Type enumType = key.Value1;
            string[] names = Enum.GetNames(enumType);
            string[] resolvedNames = new string[names.Length];
            ulong[] values = new ulong[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                FieldInfo f = enumType.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
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

                resolvedNames[i] = key.Value2 != null
                    ? key.Value2.GetPropertyName(resolvedName, false)
                    : resolvedName;
            }

            bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);

            return new EnumInfo(isFlags, values, names, resolvedNames);
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
            EnumInfo enumNameValues = GetEnumValuesAndNames(enumType);
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
                selectedFlagsValues.Add(default);
            }

            return selectedFlagsValues;
        }

        public static bool TryToString(Type enumType, object value, NamingStrategy namingStrategy, out string name)
        {
            EnumInfo enumInfo = ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, namingStrategy));
            ulong v = ToUInt64(value);

            if (!enumInfo.IsFlags)
            {
                int index = Array.BinarySearch(enumInfo.Values, v);
                if (index >= 0)
                {
                    name = enumInfo.ResolvedNames[index];
                    return true;
                }

                // is number value
                name = null;
                return false;
            }
            else // These are flags OR'ed together (We treat everything as unsigned types)
            {
                name = InternalFlagsFormat(enumInfo, v);
                return name != null;
            }
        }

        private static string InternalFlagsFormat(EnumInfo entry, ulong result)
        {
            string[] resolvedNames = entry.ResolvedNames;
            ulong[] values = entry.Values;

            int index = values.Length - 1;
            StringBuilder sb = new StringBuilder();
            bool firstTime = true;
            ulong saveResult = result;

            // We will not optimize this code further to keep it maintainable. There are some boundary checks that can be applied
            // to minimize the comparsions required. This code works the same for the best/worst case. In general the number of
            // items in an enum are sufficiently small and not worth the optimization.
            while (index >= 0)
            {
                if (index == 0 && values[index] == 0)
                {
                    break;
                }

                if ((result & values[index]) == values[index])
                {
                    result -= values[index];
                    if (!firstTime)
                    {
                        sb.Insert(0, EnumSeparatorString);
                    }

                    string resolvedName = resolvedNames[index];
                    sb.Insert(0, resolvedName);
                    firstTime = false;
                }

                index--;
            }

            string returnString;
            if (result != 0)
            {
                // We were unable to represent this number as a bitwise or of valid flags
                returnString = null; // return null so the caller knows to .ToString() the input
            }
            else if (saveResult == 0)
            {
                // For the cases when we have zero
                if (values.Length > 0 && values[0] == 0)
                {
                    returnString = resolvedNames[0]; // Zero was one of the enum values.
                }
                else
                {
                    returnString = null;
                }
            }
            else
            {
                returnString = sb.ToString(); // Return the string representation
            }

            return returnString;
        }

        public static EnumInfo GetEnumValuesAndNames(Type enumType)
        {
            return ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, null));
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

        public static object ParseEnum(Type enumType, NamingStrategy namingStrategy, string value, bool disallowNumber)
        {
            ValidationUtils.ArgumentNotNull(enumType, nameof(enumType));
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (!enumType.IsEnum())
            {
                throw new ArgumentException("Type provided must be an Enum.", nameof(enumType));
            }

            EnumInfo entry = ValuesAndNamesPerEnum.Get(new StructMultiKey<Type, NamingStrategy>(enumType, namingStrategy));
            string[] enumNames = entry.Names;
            string[] resolvedNames = entry.ResolvedNames;
            ulong[] enumValues = entry.Values;

            // first check if the entire text (including commas) matches a resolved name
            int? matchingIndex = FindIndexByName(resolvedNames, value, 0, value.Length, StringComparison.Ordinal);
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

            // check whether string is a number and parse as a number value
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

                // match with case sensitivity
                matchingIndex = MatchName(value, enumNames, resolvedNames, valueIndex, valueSubstringLength, StringComparison.Ordinal);

                // if no match found, attempt case insensitive search
                if (matchingIndex == null)
                {
                    matchingIndex = MatchName(value, enumNames, resolvedNames, valueIndex, valueSubstringLength, StringComparison.OrdinalIgnoreCase);
                }

                if (matchingIndex == null)
                {
                    // still can't find a match
                    // before we throw an error, check whether the entire string has a case insensitive match against resolve names
                    matchingIndex = FindIndexByName(resolvedNames, value, 0, value.Length, StringComparison.OrdinalIgnoreCase);
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
            int? matchingIndex = FindIndexByName(resolvedNames, value, valueIndex, valueSubstringLength, comparison);
            if (matchingIndex == null)
            {
                matchingIndex = FindIndexByName(enumNames, value, valueIndex, valueSubstringLength, comparison);
            }

            return matchingIndex;
        }

        private static int? FindIndexByName(string[] enumNames, string value, int valueIndex, int valueSubstringLength, StringComparison comparison)
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
}