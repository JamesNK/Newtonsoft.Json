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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities
{
    internal static partial class JavaScriptUtils
    {
        public static async Task<char[]> WriteEscapedJavaScriptStringAsync(TextWriter writer, string s, char delimiter, bool appendDelimiters,
            bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, IArrayPool<char> bufferPool, char[] writeBuffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            // leading delimiter
            if (appendDelimiters)
            {
                await writer.WriteAsync(delimiter, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (s != null)
            {
                int lastWritePosition = 0;

                for (int i = 0; i < s.Length; i++)
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
                        int length = i - lastWritePosition + ((isEscapedUnicodeText) ? UnicodeTextLength : 0);
                        int start = (isEscapedUnicodeText) ? UnicodeTextLength : 0;

                        if (writeBuffer == null || writeBuffer.Length < length)
                        {
                            char[] newBuffer = BufferUtils.RentBuffer(bufferPool, length);

                            // the unicode text is already in the buffer
                            // copy it over when creating new buffer
                            if (isEscapedUnicodeText)
                            {
                                Array.Copy(writeBuffer, newBuffer, UnicodeTextLength);
                            }

                            BufferUtils.ReturnBuffer(bufferPool, writeBuffer);

                            writeBuffer = newBuffer;
                        }

                        s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                        cancellationToken.ThrowIfCancellationRequested();
                        // write unchanged chars before writing escaped text
                        await writer.WriteAsync(writeBuffer, start, length - start, cancellationToken).ConfigureAwait(false);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    lastWritePosition = i + 1;
                    if (!isEscapedUnicodeText)
                    {
                        await writer.WriteAsync(escapedValue, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await writer.WriteAsync(writeBuffer, 0, UnicodeTextLength, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (lastWritePosition == 0)
                {
                    // no escaped text, write entire string
                    await writer.WriteAsync(s, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    int length = s.Length - lastWritePosition;

                    if (writeBuffer == null || writeBuffer.Length < length)
                    {
                        writeBuffer = BufferUtils.EnsureBufferSize(bufferPool, length, writeBuffer);
                    }

                    s.CopyTo(lastWritePosition, writeBuffer, 0, length);

                    // write remaining text
                    await writer.WriteAsync(writeBuffer, 0, length, cancellationToken).ConfigureAwait(false);
                }
            }

            // trailing delimiter
            if (appendDelimiters)
            {
                await writer.WriteAsync(delimiter, cancellationToken).ConfigureAwait(false);
            }

            return writeBuffer;
        }
    }
}

#endif