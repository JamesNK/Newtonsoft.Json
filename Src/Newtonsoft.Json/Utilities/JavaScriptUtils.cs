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
using System.IO;
#if HAVE_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Diagnostics;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Utilities
{
    internal static class BufferUtils
    {
        public static char[] RentBuffer(IArrayPool<char> bufferPool, int minSize)
        {
            if (bufferPool == null)
            {
                return new char[minSize];
            }

            char[] buffer = bufferPool.Rent(minSize);
            return buffer;
        }

        public static void ReturnBuffer(IArrayPool<char> bufferPool, char[] buffer)
        {
            bufferPool?.Return(buffer);
        }

        public static char[] EnsureBufferSize(IArrayPool<char> bufferPool, int size, char[] buffer)
        {
            if (bufferPool == null)
            {
                return new char[size];
            }

            if (buffer != null)
            {
                bufferPool.Return(buffer);
            }

            return bufferPool.Rent(size);
        }
    }

    internal static class JavaScriptUtils
    {
        internal static readonly bool[] SingleQuoteCharEscapeFlags = new bool[128];
        internal static readonly bool[] DoubleQuoteCharEscapeFlags = new bool[128];
        internal static readonly bool[] HtmlCharEscapeFlags = new bool[128];

        private const int UnicodeTextLength = 6;

        static JavaScriptUtils()
        {
            IList<char> escapeChars = new List<char>
            {
                '\n', '\r', '\t', '\\', '\f', '\b',
            };
            for (int i = 0; i < ' '; i++)
            {
                escapeChars.Add((char)i);
            }

            foreach (char escapeChar in escapeChars.Union(new[] { '\'' }))
            {
                SingleQuoteCharEscapeFlags[escapeChar] = true;
            }
            foreach (char escapeChar in escapeChars.Union(new[] { '"' }))
            {
                DoubleQuoteCharEscapeFlags[escapeChar] = true;
            }
            foreach (char escapeChar in escapeChars.Union(new[] { '"', '\'', '<', '>', '&' }))
            {
                HtmlCharEscapeFlags[escapeChar] = true;
            }
        }

        private const string EscapedUnicodeText = "!";

        public static bool[] GetCharEscapeFlags(StringEscapeHandling stringEscapeHandling, char quoteChar)
        {
            if (stringEscapeHandling == StringEscapeHandling.EscapeHtml)
            {
                return HtmlCharEscapeFlags;
            }

            if (quoteChar == '"')
            {
                return DoubleQuoteCharEscapeFlags;
            }

            return SingleQuoteCharEscapeFlags;
        }

        public static bool ShouldEscapeJavaScriptString(string s, bool[] charEscapeFlags)
        {
            if (s == null)
            {
                return false;
            }

            foreach (char c in s)
            {
                if (c >= charEscapeFlags.Length || charEscapeFlags[c])
                {
                    return true;
                }
            }

            return false;
        }

        public static void WriteEscapedJavaScriptString(TextWriter writer, string s, char delimiter, bool appendDelimiters,
            bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, IArrayPool<char> bufferPool, ref char[] writeBuffer)
        {
            // leading delimiter
            if (appendDelimiters)
            {
                writer.Write(delimiter);
            }

            if (!string.IsNullOrEmpty(s))
            {
                int lastWritePosition = FirstCharToEscape(s, charEscapeFlags, stringEscapeHandling);
                if (lastWritePosition == -1)
                {
                    writer.Write(s);
                }
                else
                {
                    if (lastWritePosition != 0)
                    {
                        if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
                        {
                            writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, lastWritePosition, writeBuffer);
                        }

                        // write unchanged chars at start of text.
                        s.CopyTo(0, writeBuffer, 0, lastWritePosition);
                        writer.Write(writeBuffer, 0, lastWritePosition);
                    }

                    int length;
                    for (int i = lastWritePosition; i < s.Length; i++)
                    {
                        char c = s[i];

                        if (c < charEscapeFlags.Length && !charEscapeFlags[c])
                        {
                            continue;
                        }

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
                            default:
                                if (c < charEscapeFlags.Length || stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
                                {
                                    if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                                    {
                                        escapedValue = @"\'";
                                    }
                                    else if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                                    {
                                        escapedValue = @"\""";
                                    }
                                    else
                                    {
                                        if (writeBuffer == null || writeBuffer.Length < UnicodeTextLength)
                                        {
                                            writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, UnicodeTextLength, writeBuffer);
                                        }

                                        StringUtils.ToCharAsUnicode(c, writeBuffer);

                                        // slightly hacky but it saves multiple conditions in if test
                                        escapedValue = EscapedUnicodeText;
                                    }
                                }
                                else
                                {
                                    escapedValue = null;
                                }
                                break;
                        }

                        if (escapedValue == null)
                        {
                            continue;
                        }

                        bool isEscapedUnicodeText = string.Equals(escapedValue, EscapedUnicodeText);

                        if (i > lastWritePosition)
                        {
                            length = i - lastWritePosition + ((isEscapedUnicodeText) ? UnicodeTextLength : 0);
                            int start = (isEscapedUnicodeText) ? UnicodeTextLength : 0;

                            if (writeBuffer == null || writeBuffer.Length < length)
                            {
                                char[] newBuffer = BufferUtils.RentBuffer(bufferPool, length);

                                // the unicode text is already in the buffer
                                // copy it over when creating new buffer
                                if (isEscapedUnicodeText)
                                {
                                    Debug.Assert(writeBuffer != null, "Write buffer should never be null because it is set when the escaped unicode text is encountered.");

                                    Array.Copy(writeBuffer, newBuffer, UnicodeTextLength);
                                }

                                BufferUtils.ReturnBuffer(bufferPool, writeBuffer);

                                writeBuffer = newBuffer;
                            }

                            s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                            // write unchanged chars before writing escaped text
                            writer.Write(writeBuffer, start, length - start);
                        }

                        lastWritePosition = i + 1;
                        if (!isEscapedUnicodeText)
                        {
                            writer.Write(escapedValue);
                        }
                        else
                        {
                            writer.Write(writeBuffer, 0, UnicodeTextLength);
                        }
                    }

                    Debug.Assert(lastWritePosition != 0);
                    length = s.Length - lastWritePosition;
                    if (length > 0)
                    {
                        if (writeBuffer == null || writeBuffer.Length < length)
                        {
                            writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, length, writeBuffer);
                        }

                        s.CopyTo(lastWritePosition, writeBuffer, 0, length);

                        // write remaining text
                        writer.Write(writeBuffer, 0, length);
                    }
                }
            }

            // trailing delimiter
            if (appendDelimiters)
            {
                writer.Write(delimiter);
            }
        }

        public static string ToEscapedJavaScriptString(string value, char delimiter, bool appendDelimiters, StringEscapeHandling stringEscapeHandling)
        {
            bool[] charEscapeFlags = GetCharEscapeFlags(stringEscapeHandling, delimiter);

            using (StringWriter w = StringUtils.CreateStringWriter(value?.Length ?? 16))
            {
                char[] buffer = null;
                WriteEscapedJavaScriptString(w, value, delimiter, appendDelimiters, charEscapeFlags, stringEscapeHandling, null, ref buffer);
                return w.ToString();
            }
        }
        
        private static int FirstCharToEscape(string s, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling)
        {
            for (int i = 0; i != s.Length; i++)
            {
                char c = s[i];

                if (c < charEscapeFlags.Length)
                {
                    if (charEscapeFlags[c])
                    {
                        return i;
                    }
                }
                else if (stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
                {
                    return i;
                }
                else
                {
                    switch (c)
                    {
                        case '\u0085':
                        case '\u2028':
                        case '\u2029':
                            return i;
                    }
                }
            }

            return -1;
        }

#if HAVE_ASYNC
        public static Task WriteEscapedJavaScriptStringAsync(TextWriter writer, string s, char delimiter, bool appendDelimiters, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return cancellationToken.FromCanceled();
            }

            if (appendDelimiters)
            {
                return WriteEscapedJavaScriptStringWithDelimitersAsync(writer, s, delimiter, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
            }

            if (string.IsNullOrEmpty(s))
            {
                return cancellationToken.CancelIfRequestedAsync() ?? AsyncUtils.CompletedTask;
            }

            return WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
        }

        private static Task WriteEscapedJavaScriptStringWithDelimitersAsync(TextWriter writer, string s, char delimiter,
            bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
        {
            Task task = writer.WriteAsync(delimiter, cancellationToken);
            if (!task.IsCompletedSucessfully())
            {
                return WriteEscapedJavaScriptStringWithDelimitersAsync(task, writer, s, delimiter, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
            }

            if (!string.IsNullOrEmpty(s))
            {
                task = WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
                if (task.IsCompletedSucessfully())
                {
                    return writer.WriteAsync(delimiter, cancellationToken);
                }
            }

            return WriteCharAsync(task, writer, delimiter, cancellationToken);
            
        }

        private static async Task WriteEscapedJavaScriptStringWithDelimitersAsync(Task task, TextWriter writer, string s, char delimiter,
            bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);

            if (!string.IsNullOrEmpty(s))
            {
                await WriteEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken).ConfigureAwait(false);
            }

            await writer.WriteAsync(delimiter).ConfigureAwait(false);
        }

        public static async Task WriteCharAsync(Task task, TextWriter writer, char c, CancellationToken cancellationToken)
        {
            await task.ConfigureAwait(false);
            await writer.WriteAsync(c, cancellationToken).ConfigureAwait(false);
        }

        private static Task WriteEscapedJavaScriptStringWithoutDelimitersAsync(
            TextWriter writer, string s, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling,
            JsonTextWriter client, char[] writeBuffer, CancellationToken cancellationToken)
        {
            int i = FirstCharToEscape(s, charEscapeFlags, stringEscapeHandling);
            return i == -1
                ? writer.WriteAsync(s, cancellationToken)
                : WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(writer, s, i, charEscapeFlags, stringEscapeHandling, client, writeBuffer, cancellationToken);
        }

        private static async Task WriteDefinitelyEscapedJavaScriptStringWithoutDelimitersAsync(
            TextWriter writer, string s, int lastWritePosition, bool[] charEscapeFlags,
            StringEscapeHandling stringEscapeHandling, JsonTextWriter client, char[] writeBuffer,
            CancellationToken cancellationToken)
        {
            if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
            {
                writeBuffer = client.EnsureWriteBuffer(lastWritePosition, UnicodeTextLength);
            }

            if (lastWritePosition != 0)
            {
                s.CopyTo(0, writeBuffer, 0, lastWritePosition);

                // write unchanged chars at start of text.
                await writer.WriteAsync(writeBuffer, 0, lastWritePosition, cancellationToken).ConfigureAwait(false);
            }

            int length;
            bool isEscapedUnicodeText = false;
            string escapedValue = null;

            for (int i = lastWritePosition; i < s.Length; i++)
            {
                char c = s[i];

                if (c < charEscapeFlags.Length && !charEscapeFlags[c])
                {
                    continue;
                }

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
                    default:
                        if (c < charEscapeFlags.Length || stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
                        {
                            if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                            {
                                escapedValue = @"\'";
                            }
                            else if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                            {
                                escapedValue = @"\""";
                            }
                            else
                            {
                                if (writeBuffer.Length < UnicodeTextLength)
                                {
                                    writeBuffer = client.EnsureWriteBuffer(UnicodeTextLength, 0);
                                }

                                StringUtils.ToCharAsUnicode(c, writeBuffer);

                                isEscapedUnicodeText = true;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        break;
                }

                if (i > lastWritePosition)
                {
                    length = i - lastWritePosition + (isEscapedUnicodeText ? UnicodeTextLength : 0);
                    int start = isEscapedUnicodeText ? UnicodeTextLength : 0;

                    if (writeBuffer.Length < length)
                    {
                        writeBuffer = client.EnsureWriteBuffer(length, UnicodeTextLength);
                    }

                    s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                    // write unchanged chars before writing escaped text
                    await writer.WriteAsync(writeBuffer, start, length - start, cancellationToken).ConfigureAwait(false);
                }

                lastWritePosition = i + 1;
                if (!isEscapedUnicodeText)
                {
                    await writer.WriteAsync(escapedValue, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(writeBuffer, 0, UnicodeTextLength, cancellationToken).ConfigureAwait(false);
                    isEscapedUnicodeText = false;
                }
            }

            length = s.Length - lastWritePosition;

            if (length != 0)
            {
                if (writeBuffer.Length < length)
                {
                    writeBuffer = client.EnsureWriteBuffer(length, 0);
                }

                s.CopyTo(lastWritePosition, writeBuffer, 0, length);

                // write remaining text
                await writer.WriteAsync(writeBuffer, 0, length, cancellationToken).ConfigureAwait(false);
            }
        }
#endif

        public static bool TryGetDateFromConstructorJson(JsonReader reader, out DateTime dateTime, out string errorMessage)
        {
            dateTime = default;
            errorMessage = null;

            if (!TryGetDateConstructorValue(reader, out long? t1, out errorMessage) || t1 == null)
            {
                errorMessage = errorMessage ?? "Date constructor has no arguments.";
                return false;
            }
            if (!TryGetDateConstructorValue(reader, out long? t2, out errorMessage))
            {
                return false;
            }
            else if (t2 != null)
            {
                // Only create a list when there is more than one argument
                List<long> dateArgs = new List<long>
                {
                    t1.Value,
                    t2.Value
                };
                while (true)
                {
                    if (!TryGetDateConstructorValue(reader, out long? integer, out errorMessage))
                    {
                        return false;
                    }
                    else if (integer != null)
                    {
                        dateArgs.Add(integer.Value);
                    }
                    else
                    {
                        break;
                    }
                }

                if (dateArgs.Count > 7)
                {
                    errorMessage = "Unexpected number of arguments when reading date constructor.";
                    return false;
                }

                // Pad args out to the number used by the ctor
                while (dateArgs.Count < 7)
                {
                    dateArgs.Add(0);
                }

                dateTime = new DateTime((int)dateArgs[0], (int)dateArgs[1] + 1, dateArgs[2] == 0 ? 1 : (int)dateArgs[2],
                    (int)dateArgs[3], (int)dateArgs[4], (int)dateArgs[5], (int)dateArgs[6]);
            }
            else
            {
                dateTime = DateTimeUtils.ConvertJavaScriptTicksToDateTime(t1.Value);
            }

            return true;
        }

        private static bool TryGetDateConstructorValue(JsonReader reader, out long? integer, out string errorMessage)
        {
            integer = null;
            errorMessage = null;

            if (!reader.Read())
            {
                errorMessage = "Unexpected end when reading date constructor.";
                return false;
            }
            if (reader.TokenType == JsonToken.EndConstructor)
            {
                return true;
            }
            if (reader.TokenType != JsonToken.Integer)
            {
                errorMessage = "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType;
                return false;
            }

            integer = (long)reader.Value;
            return true;
        }
    }
}