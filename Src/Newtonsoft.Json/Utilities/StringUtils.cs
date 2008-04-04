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
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal static class StringUtils
  {
    public const string CarriageReturnLineFeed = "\r\n";
    public const string Empty = "";
    public const char CarriageReturn = '\r';
    public const char LineFeed = '\n';
    public const char Tab = '\t';

    //public static string FormatWith(this string format, params object[] args)
    //{
    //  return FormatWith(format, null, args);
    //}

    public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
    {
      ValidationUtils.ArgumentNotNull(format, "format");

      return string.Format(provider, format, args);
    }

    /// <summary>
    /// Determines whether the string contains white space.
    /// </summary>
    /// <param name="s">The string to test for white space.</param>
    /// <returns>
    /// 	<c>true</c> if the string contains white space; otherwise, <c>false</c>.
    /// </returns>
    public static bool ContainsWhiteSpace(string s)
    {
      if (s == null)
        throw new ArgumentNullException("s");

      for (int i = 0; i < s.Length; i++)
      {
        if (char.IsWhiteSpace(s[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether the string is all white space. Empty string will return false.
    /// </summary>
    /// <param name="s">The string to test whether it is all white space.</param>
    /// <returns>
    /// 	<c>true</c> if the string is all white space; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsWhiteSpace(string s)
    {
      if (s == null)
        throw new ArgumentNullException("s");

      if (s.Length == 0)
        return false;

      for (int i = 0; i < s.Length; i++)
      {
        if (!char.IsWhiteSpace(s[i]))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Ensures the target string ends with the specified string.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    /// <returns>The target string with the value string at the end.</returns>
    public static string EnsureEndsWith(string target, string value)
    {
      if (target == null)
        throw new ArgumentNullException("target");

      if (value == null)
        throw new ArgumentNullException("value");

      if (target.Length >= value.Length)
      {
        if (string.Compare(target, target.Length - value.Length, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) ==
                        0)
          return target;

        string trimmedString = target.TrimEnd(null);

        if (string.Compare(trimmedString, trimmedString.Length - value.Length, value, 0, value.Length,
                        StringComparison.OrdinalIgnoreCase) == 0)
          return target;
      }

      return target + value;
    }

    /// <summary>
    /// Determines whether the SqlString is null or empty.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>
    /// 	<c>true</c> if the SqlString is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(SqlString s)
    {
      if (s.IsNull)
        return true;
      else
        return string.IsNullOrEmpty(s.Value);
    }

    public static bool IsNullOrEmptyOrWhiteSpace(string s)
    {
      if (string.IsNullOrEmpty(s))
        return true;
      else if (IsWhiteSpace(s))
        return true;
      else
        return false;
    }

    /// <summary>
    /// Perform an action if the string is not null or empty.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="action">The action to perform.</param>
    public static void IfNotNullOrEmpty(string value, Action<string> action)
    {
      IfNotNullOrEmpty(value, action, null);
    }

    private static void IfNotNullOrEmpty(string value, Action<string> trueAction, Action<string> falseAction)
    {
      if (!string.IsNullOrEmpty(value))
      {
        if (trueAction != null)
          trueAction(value);
      }
      else
      {
        if (falseAction != null)
          falseAction(value);
      }
    }

    /// <summary>
    /// Indents the specified string.
    /// </summary>
    /// <param name="s">The string to indent.</param>
    /// <param name="indentation">The number of characters to indent by.</param>
    /// <returns></returns>
    public static string Indent(string s, int indentation)
    {
      return Indent(s, indentation, ' ');
    }

    /// <summary>
    /// Indents the specified string.
    /// </summary>
    /// <param name="s">The string to indent.</param>
    /// <param name="indentation">The number of characters to indent by.</param>
    /// <param name="indentChar">The indent character.</param>
    /// <returns></returns>
    public static string Indent(string s, int indentation, char indentChar)
    {
      if (s == null)
        throw new ArgumentNullException("s");

      if (indentation <= 0)
        throw new ArgumentException("Must be greater than zero.", "indentation");

      StringReader sr = new StringReader(s);
      StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

      ActionTextReaderLine(sr, sw, delegate(TextWriter tw, string line)
      {
        tw.Write(new string(indentChar, indentation));
        tw.Write(line);
      });

      return sw.ToString();
    }

    private delegate void ActionLine(TextWriter textWriter, string line);

    private static void ActionTextReaderLine(TextReader textReader, TextWriter textWriter, ActionLine lineAction)
    {
      string line;
      bool firstLine = true;
      while ((line = textReader.ReadLine()) != null)
      {
        if (!firstLine)
          textWriter.WriteLine();
        else
          firstLine = false;

        lineAction(textWriter, line);
      }
    }

    /// <summary>
    /// Numbers the lines.
    /// </summary>
    /// <param name="s">The string to number.</param>
    /// <returns></returns>
    public static string NumberLines(string s)
    {
      if (s == null)
        throw new ArgumentNullException("s");

      StringReader sr = new StringReader(s);
      StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

      int lineNumber = 1;

      ActionTextReaderLine(sr, sw, delegate(TextWriter tw, string line)
      {
        tw.Write(lineNumber.ToString(CultureInfo.InvariantCulture).PadLeft(4));
        tw.Write(". ");
        tw.Write(line);

        lineNumber++;
      });

      return sw.ToString();
    }

    /// <summary>
    /// Nulls an empty string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>Null if the string was null, otherwise the string unchanged.</returns>
    public static string NullEmptyString(string s)
    {
      return (string.IsNullOrEmpty(s)) ? null : s;
    }

    public static string ReplaceNewLines(string s, string replacement)
    {
      StringReader sr = new StringReader(s);
      StringBuilder sb = new StringBuilder();

      bool first = true;

      string line;
      while ((line = sr.ReadLine()) != null)
      {
        if (first)
          first = false;
        else
          sb.Append(replacement);

        sb.Append(line);
      }

      return sb.ToString();
    }

    public static string RemoveHtml(string s)
    {
      return RemoveHtmlInternal(s, null);
    }

    public static string RemoveHtml(string s, IList<string> removeTags)
    {
      if (removeTags == null)
        throw new ArgumentNullException("removeTags");

      return RemoveHtmlInternal(s, removeTags);
    }

    private static string RemoveHtmlInternal(string s, IList<string> removeTags)
    {
      List<string> removeTagsUpper = null;

      if (removeTags != null)
      {
        removeTagsUpper = new List<string>(removeTags.Count);

        foreach (string tag in removeTags)
        {
          removeTagsUpper.Add(tag.ToUpperInvariant());
        }
      }

      Regex anyTag = new Regex(@"<[/]{0,1}\s*(?<tag>\w*)\s*(?<attr>.*?=['""].*?[""'])*?\s*[/]{0,1}>", RegexOptions.Compiled);

      return anyTag.Replace(s, delegate(Match match)
      {
        string tag = match.Groups["tag"].Value.ToUpperInvariant();

        if (removeTagsUpper == null)
          return string.Empty;
        else if (removeTagsUpper.Contains(tag))
          return string.Empty;
        else
          return match.Value;
      });
    }

    public static string Truncate(string s, int maximumLength)
    {
      return Truncate(s, maximumLength, "...");
    }

    public static string Truncate(string s, int maximumLength, string suffix)
    {
      if (suffix == null)
        throw new ArgumentNullException("suffix");

      if (maximumLength <= 0)
        throw new ArgumentException("Maximum length must be greater than zero.", "maximumLength");

      int subStringLength = maximumLength - suffix.Length;

      if (subStringLength <= 0)
        throw new ArgumentException("Length of suffix string is greater or equal to maximumLength");

      if (s != null && s.Length > maximumLength)
      {
        string truncatedString = s.Substring(0, subStringLength);
        // incase the last character is a space
        truncatedString = truncatedString.Trim();
        truncatedString += suffix;

        return truncatedString;
      }
      else
      {
        return s;
      }
    }

    public static StringWriter CreateStringWriter(int capacity)
    {
      StringBuilder sb = new StringBuilder(capacity);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

      return sw;
    }

    public static int? GetLength(string value)
    {
      if (value == null)
        return null;
      else
        return value.Length;
    }

    public static string ToCharAsUnicode(char c)
    {
      using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture))
      {
        WriteCharAsUnicode(w, c);
        return w.ToString();
      }
    }

    public static void WriteCharAsUnicode(TextWriter writer, char c)
    {
      ValidationUtils.ArgumentNotNull(writer, "writer");

      char h1 = MathUtils.IntToHex((c >> 12) & '\x000f');
      char h2 = MathUtils.IntToHex((c >> 8) & '\x000f');
      char h3 = MathUtils.IntToHex((c >> 4) & '\x000f');
      char h4 = MathUtils.IntToHex(c & '\x000f');

      writer.Write('\\');
      writer.Write('u');
      writer.Write(h1);
      writer.Write(h2);
      writer.Write(h3);
      writer.Write(h4);
    }
  }
}