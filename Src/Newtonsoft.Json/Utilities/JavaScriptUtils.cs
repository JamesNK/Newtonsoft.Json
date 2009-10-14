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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Newtonsoft.Json.Utilities
{
  internal static class JavaScriptUtils
  {
    public static void WriteEscapedJavaScriptChar(TextWriter writer, char c, char delimiter)
    {
      switch (c)
      {
        case '\t':
          writer.Write(@"\t");
          break;
        case '\n':
          writer.Write(@"\n");
          break;
        case '\r':
          writer.Write(@"\r");
          break;
        case '\f':
          writer.Write(@"\f");
          break;
        case '\b':
          writer.Write(@"\b");
          break;
        case '\\':
          writer.Write(@"\\");
          break;
        case '\u0085': // Next Line
          writer.Write(@"\u0085");
          break;
        case '\u2028': // Line Separator
          writer.Write(@"\u2028");
          break;
        case '\u2029': // Paragraph Separator
          writer.Write(@"\u2029");
          break;
        case '\'':
          // only escape if this charater is being used as the delimiter
          writer.Write((delimiter == '\'') ? @"\'" : @"'");
          break;
        case '"':
          // only escape if this charater is being used as the delimiter
          writer.Write((delimiter == '"') ? "\\\"" : @"""");
          break;
        default:
          if (c > '\u001f')
            writer.Write(c);
          else
            StringUtils.WriteCharAsUnicode(writer, c);
          break;
      }
    }

    public static void WriteEscapedJavaScriptString(TextWriter writer, string value, char delimiter, bool appendDelimiters)
    {
      // leading delimiter
      if (appendDelimiters)
        writer.Write(delimiter);

      if (value != null)
      {
        for (int i = 0; i < value.Length; i++)
        {
          WriteEscapedJavaScriptChar(writer, value[i], delimiter);
        }
      }

      // trailing delimiter
      if (appendDelimiters)
        writer.Write(delimiter);
    }

    public static string ToEscapedJavaScriptString(string value)
    {
      return ToEscapedJavaScriptString(value, '"', true);
    }

    public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters)
    {
      using (StringWriter w = StringUtils.CreateStringWriter(StringUtils.GetLength(value) ?? 16))
      {
        WriteEscapedJavaScriptString(w, value, delimiter, appendDelimiters);
        return w.ToString();
      }
    }
  }
}