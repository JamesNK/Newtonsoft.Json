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
    public static void WriteEscapedJavaScriptString(TextWriter writer, string value, char delimiter, bool appendDelimiters)
    {
      // leading delimiter
      if (appendDelimiters)
        writer.Write(delimiter);

      if (value != null)
      {
        int lastWritePosition = 0;
        int skipped = 0;
        char[] chars = null;

        for (int i = 0; i < value.Length; i++)
        {
          char c = value[i];
          string escapedValue;

          switch (c)
          {
            case '\t':
              escapedValue = @"\t";
              break;
            case '\n':
              escapedValue = @"\n";
              break;
            case '\r':
              escapedValue = @"\r";
              break;
            case '\f':
              escapedValue = @"\f";
              break;
            case '\b':
              escapedValue = @"\b";
              break;
            case '\\':
              escapedValue = @"\\";
              break;
            case '\u0085': // Next Line
              escapedValue = @"\u0085";
              break;
            case '\u2028': // Line Separator
              escapedValue = @"\u2028";
              break;
            case '\u2029': // Paragraph Separator
              escapedValue = @"\u2029";
              break;
            case '\'':
              // only escape if this charater is being used as the delimiter
              escapedValue = (delimiter == '\'') ? @"\'" : null;
              break;
            case '"':
              // only escape if this charater is being used as the delimiter
              escapedValue = (delimiter == '"') ? "\\\"" : null;
              break;
            default:
              escapedValue = (c <= '\u001f') ? StringUtils.ToCharAsUnicode(c) : null;
              break;
          }

          if (escapedValue != null)
          {
            if (chars == null)
              chars = value.ToCharArray();

            // write skipped text
            if (skipped > 0)
            {
              writer.Write(chars, lastWritePosition, skipped);
              skipped = 0;
            }

            // write escaped value and note position
            writer.Write(escapedValue);
            lastWritePosition = i + 1;
          }
          else
          {
            skipped++;
          }
        }

        // write any remaining skipped text
        if (skipped > 0)
        {
          if (lastWritePosition == 0)
            writer.Write(value);
          else
            writer.Write(chars, lastWritePosition, skipped);
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