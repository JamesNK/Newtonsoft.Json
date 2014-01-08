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
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  internal enum JsonContainerType
  {
    None,
    Object,
    Array,
    Constructor
  }

  internal struct JsonPosition
  {
    internal JsonContainerType Type;
    internal int Position;
    internal string PropertyName;
    internal bool HasIndex;

    public JsonPosition(JsonContainerType type)
    {
      Type = type;
      HasIndex = TypeHasIndex(type);
      Position = -1;
      PropertyName = null;
    }

    internal void WriteTo(StringBuilder sb)
    {
      switch (Type)
      {
        case JsonContainerType.Object:
          if (sb.Length > 0)
            sb.Append(".");
          sb.Append(PropertyName);
          break;
        case JsonContainerType.Array:
        case JsonContainerType.Constructor:
          sb.Append("[");
          sb.Append(Position);
          sb.Append("]");
          break;
      }
    }

    internal static bool TypeHasIndex(JsonContainerType type)
    {
      return (type == JsonContainerType.Array || type == JsonContainerType.Constructor);
    }

    internal static string BuildPath(IEnumerable<JsonPosition> positions)
    {
      StringBuilder sb = new StringBuilder();

      foreach (JsonPosition state in positions)
      {
        state.WriteTo(sb);
      }

      return sb.ToString();
    }

    internal static string FormatMessage(IJsonLineInfo lineInfo, string path, string message)
    {
      return FormatMessage(lineInfo, path, new StringBuilder(message)).ToString();
    }

    internal static StringBuilder FormatMessage(IJsonLineInfo lineInfo, string path, StringBuilder message)
    {
      // don't add a fullstop and space when message ends with a new line
      if (!message.EndsWith(Environment.NewLine))
      {
        message = message.Trim();

        if (!message.EndsWith("."))
          message.Append(".");

        message.Append(" ");
      }

      message.AppendFormat(CultureInfo.InvariantCulture, "Path '{0}'", path);

      if (lineInfo != null && lineInfo.HasLineInfo())
        message.AppendFormat(CultureInfo.InvariantCulture, ", line {0}, position {1}", lineInfo.LineNumber, lineInfo.LinePosition);

      message.Append(".");

      return message;
    }
  }

  internal static class StringBuilderExtensions
  {
    /// <summary>
    /// Determines whether the end of this <see cref="System.Text.StringBuilder"/> instance matches the specified string.
    /// </summary>
    /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to compare.</param>
    /// <param name="value">The string to compare to the substring at the end of this instance.</param>
    /// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
    /// <returns>
    /// true if the <paramref name="value"/> parameter matches the beginning of this string; otherwise, false.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool EndsWith(this StringBuilder sb, string value, bool ignoreCase = false)
    {
      if (value == null)
        throw new ArgumentNullException("value cannot be null.");

      int length = value.Length;
      int maxSBIndex = sb.Length - 1;
      int maxValueIndex = length - 1;
      if (length > sb.Length)
        return false;

      if (ignoreCase == false)
      {
        for (int i = 0; i < length; i++)
        {
          if (sb[maxSBIndex - i] != value[maxValueIndex - i])
          {
            return false;
          }
        }
      }
      else
      {
        for (int j = length - 1; j >= 0; j--)
        {
          if (char.ToLower(sb[maxSBIndex - j]) != char.ToLower(value[maxValueIndex - j]))
          {
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Removes all leading and trailing white-space characters from the current <see cref="System.Text.StringBuilder"/> object.
    /// </summary>
    /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to remove from.</param>
    /// <returns>
    /// The <see cref="System.Text.StringBuilder"/> object that contains a list of characters 
    /// that remains after all white-space characters are removed 
    /// from the start and end of the current StringBuilder.
    /// </returns>
    public static StringBuilder Trim(this StringBuilder sb)
    {
      return sb.TrimHelper(2);
    }

    private static bool IsBOMWhitespace(char c)
    {
      return false;
    }

    private static StringBuilder TrimHelper(this StringBuilder sb, int trimType)
    {
      int end = sb.Length - 1;
      int start = 0;
      if (trimType != 1)
      {
        start = 0;
        while (start < sb.Length)
        {
          if (!char.IsWhiteSpace(sb[start]) && !IsBOMWhitespace(sb[start]))
          {
            break;
          }
          start++;
        }
      }
      if (trimType != 0)
      {
        end = sb.Length - 1;
        while (end >= start)
        {
          if (!char.IsWhiteSpace(sb[end]) && !IsBOMWhitespace(sb[start]))
          {
            break;
          }
          end--;
        }
      }
      return sb.CreateTrimmedString(start, end);
    }

    private static StringBuilder CreateTrimmedString(this StringBuilder sb, int start, int end)
    {
      int length = (end - start) + 1;
      if (length == sb.Length)
      {
        return sb;
      }
      if (length == 0)
      {
        sb.Length = 0;
        return sb;
      }
      return sb.InternalSubString(start, end);
    }

    private static StringBuilder InternalSubString(this StringBuilder sb, int startIndex, int end)
    {
      sb.Length = end + 1;
      sb.Remove(0, startIndex);
      return sb;
    }
  }
}