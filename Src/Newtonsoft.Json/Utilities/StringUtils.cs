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
using System.IO;
using System.Text;
using System.Globalization;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
    internal static class StringUtils
    {
        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        public static string FormatWith(this string format, IFormatProvider provider, object arg0)
        {
            return format.FormatWith(provider, new[] { arg0 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object arg0, object arg1)
        {
            return format.FormatWith(provider, new[] { arg0, arg1 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object arg0, object arg1, object arg2)
        {
            return format.FormatWith(provider, new[] { arg0, arg1, arg2 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object arg0, object arg1, object arg2, object arg3)
        {
            return format.FormatWith(provider, new[] { arg0, arg1, arg2, arg3 });
        }

        private static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            // leave this a private to force code to use an explicit overload
            // avoids stack memory being reserved for the object array
            ValidationUtils.ArgumentNotNull(format, nameof(format));

            return string.Format(provider, format, args);
        }

        /// <summary>
        /// Determines whether the string is all white space. Empty string will return <c>false</c>.
        /// </summary>
        /// <param name="s">The string to test whether it is all white space.</param>
        /// <returns>
        /// 	<c>true</c> if the string is all white space; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWhiteSpace(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (s.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static StringWriter CreateStringWriter(int capacity)
        {
            StringBuilder sb = new StringBuilder(capacity);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            return sw;
        }

        public static void ToCharAsUnicode(char c, char[] buffer)
        {
            buffer[0] = '\\';
            buffer[1] = 'u';
            buffer[2] = MathUtils.IntToHex((c >> 12) & '\x000f');
            buffer[3] = MathUtils.IntToHex((c >> 8) & '\x000f');
            buffer[4] = MathUtils.IntToHex((c >> 4) & '\x000f');
            buffer[5] = MathUtils.IntToHex(c & '\x000f');
        }

        public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            IEnumerable<TSource> caseInsensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase));
            if (caseInsensitiveResults.Count() <= 1)
            {
                return caseInsensitiveResults.SingleOrDefault();
            }
            else
            {
                // multiple results returned. now filter using case sensitivity
                IEnumerable<TSource> caseSensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.Ordinal));
                return caseSensitiveResults.SingleOrDefault();
            }
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                char c;
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
                c = char.ToLower(chars[i], CultureInfo.InvariantCulture);
#else
                c = char.ToLowerInvariant(chars[i]);
#endif
                chars[i] = c;
            }

            return new string(chars);
        }

        internal enum SnakeCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        public static string ToSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder sb = new StringBuilder();
            SnakeCaseState state = SnakeCaseState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SnakeCaseState.Start)
                    {
                        state = SnakeCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != '_')
                                {
                                    sb.Append('_');
                                }
                            }
                            break;
                        case SnakeCaseState.Lower:
                        case SnakeCaseState.NewWord:
                            sb.Append('_');
                            break;
                    }

                    char c;
#if HAVE_CHAR_TO_LOWER_WITH_CULTURE
                    c = char.ToLower(s[i], CultureInfo.InvariantCulture);
#else
                    c = char.ToLowerInvariant(s[i]);
#endif
                    sb.Append(c);

                    state = SnakeCaseState.Upper;
                }
                else if (s[i] == '_')
                {
                    sb.Append('_');
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                    {
                        sb.Append('_');
                    }

                    sb.Append(s[i]);
                    state = SnakeCaseState.Lower;
                }
            }

            return sb.ToString();
        }

        public static bool IsHighSurrogate(char c)
        {
#if HAVE_UNICODE_SURROGATE_DETECTION
            return char.IsHighSurrogate(c);
#else
            return (c >= 55296 && c <= 56319);
#endif
        }

        public static bool IsLowSurrogate(char c)
        {
#if HAVE_UNICODE_SURROGATE_DETECTION
            return char.IsLowSurrogate(c);
#else
            return (c >= 56320 && c <= 57343);
#endif
        }

        public static bool StartsWith(this string source, char value)
        {
            return (source.Length > 0 && source[0] == value);
        }

        public static bool EndsWith(this string source, char value)
        {
            return (source.Length > 0 && source[source.Length - 1] == value);
        }

        public static string Trim(this string s, int start, int length)
        {
            // References: https://referencesource.microsoft.com/#mscorlib/system/string.cs,2691
            // https://referencesource.microsoft.com/#mscorlib/system/string.cs,1226
            if (s == null)
            {
                throw new ArgumentNullException();
            }
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            int end = start + length - 1;
            if (end >= s.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            for (; start < end; start++)
            {
                if (!char.IsWhiteSpace(s[start]))
                {
                    break;
                }
            }
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(s[end]))
                {
                    break;
                }
            }
            return s.Substring(start, end - start + 1);
        }
    }
}