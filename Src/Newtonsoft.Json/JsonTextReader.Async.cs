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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    public partial class JsonTextReader
    {
        private bool SafeAsync => GetType() == typeof(JsonTextReader);

        /// <summary>
        /// Asynchronously reads the next JSON token from the source.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsync(cancellationToken) : base.ReadAsync(cancellationToken);
        }

        internal async Task<bool> DoReadAsync(CancellationToken cancellationToken)
        {
            EnsureBuffer();

            for (;;)
            {
                switch (_currentState)
                {
                    case State.Start:
                    case State.Property:
                    case State.Array:
                    case State.ArrayStart:
                    case State.Constructor:
                    case State.ConstructorStart:
                        return await ParseValueAsync(cancellationToken).ConfigureAwait(false);
                    case State.Object:
                    case State.ObjectStart:
                        return await ParseObjectAsync(cancellationToken).ConfigureAwait(false);
                    case State.PostValue:

                        // returns true if it hits
                        // end of object or array
                        if (await ParsePostValueAsync(cancellationToken).ConfigureAwait(false))
                        {
                            return true;
                        }

                        break;
                    case State.Finished:
                        if (await EnsureCharsAsync(0, false, cancellationToken).ConfigureAwait(false))
                        {
                            await EatWhitespaceAsync(false, cancellationToken).ConfigureAwait(false);
                            if (_isEndOfFile)
                            {
                                SetToken(JsonToken.None);
                                return false;
                            }

                            if (_chars[_charPos] == '/')
                            {
                                await ParseCommentAsync(true, cancellationToken).ConfigureAwait(false);
                                return true;
                            }

                            throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
                        }

                        SetToken(JsonToken.None);
                        return false;
                    default:
                        throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
                }
            }
        }

        private Task<int> ReadDataAsync(bool append, CancellationToken cancellationToken)
        {
            return ReadDataAsync(append, 0, cancellationToken);
        }

        private async Task<int> ReadDataAsync(bool append, int charsRequired, CancellationToken cancellationToken)
        {
            if (_isEndOfFile)
            {
                return 0;
            }

            // char buffer is full
            if (_charsUsed + charsRequired >= _chars.Length - 1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (append)
                {
                    // copy to new array either double the size of the current or big enough to fit required content
                    int newArrayLength = Math.Max(_chars.Length * 2, _charsUsed + charsRequired + 1);

                    // increase the size of the buffer
                    char[] dst = BufferUtils.RentBuffer(_arrayPool, newArrayLength);

                    BlockCopyChars(_chars, 0, dst, 0, _chars.Length);

                    BufferUtils.ReturnBuffer(_arrayPool, _chars);

                    _chars = dst;
                }
                else
                {
                    int remainingCharCount = _charsUsed - _charPos;

                    if (remainingCharCount + charsRequired + 1 >= _chars.Length)
                    {
                        // the remaining count plus the required is bigger than the current buffer size
                        char[] dst = BufferUtils.RentBuffer(_arrayPool, remainingCharCount + charsRequired + 1);

                        if (remainingCharCount > 0)
                        {
                            BlockCopyChars(_chars, _charPos, dst, 0, remainingCharCount);
                        }

                        BufferUtils.ReturnBuffer(_arrayPool, _chars);

                        _chars = dst;
                    }
                    else
                    {
                        // copy any remaining data to the beginning of the buffer if needed and reset positions
                        if (remainingCharCount > 0)
                        {
                            BlockCopyChars(_chars, _charPos, _chars, 0, remainingCharCount);
                        }
                    }

                    _lineStartPos -= _charPos;
                    _charPos = 0;
                    _charsUsed = remainingCharCount;
                }
            }

            int attemptCharReadCount = _chars.Length - _charsUsed - 1;

            int charsRead = await _reader.ReadAsync(_chars, _charsUsed, attemptCharReadCount, cancellationToken).ConfigureAwait(false);

            _charsUsed += charsRead;

            if (charsRead == 0)
            {
                _isEndOfFile = true;
            }

            _chars[_charsUsed] = '\0';
            return charsRead;
        }

        private async Task<bool> ParseValueAsync(CancellationToken cancellationToken)
        {
            for (;;)
            {
                char currentChar = _chars[_charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            _charPos++;
                        }

                        break;
                    case '"':
                    case '\'':
                        await ParseStringAsync(currentChar, ReadType.Read, cancellationToken).ConfigureAwait(false);
                        return true;
                    case 't':
                        await ParseTrueAsync(cancellationToken).ConfigureAwait(false);
                        return true;
                    case 'f':
                        await ParseFalseAsync(cancellationToken).ConfigureAwait(false);
                        return true;
                    case 'n':
                        if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false))
                        {
                            char next = _chars[_charPos + 1];

                            if (next == 'u')
                            {
                                await ParseNullAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else if (next == 'e')
                            {
                                await ParseConstructorAsync(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                throw CreateUnexpectedCharacterException(_chars[_charPos]);
                            }
                        }
                        else
                        {
                            _charPos++;
                            throw CreateUnexpectedEndException();
                        }

                        return true;
                    case 'N':
                        await ParseNumberNaNAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                        return true;
                    case 'I':
                        await ParseNumberPositiveInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                        return true;
                    case '-':
                        if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false) && _chars[_charPos + 1] == 'I')
                        {
                            await ParseNumberNegativeInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                        }
                        return true;
                    case '/':
                        await ParseCommentAsync(true, cancellationToken).ConfigureAwait(false);
                        return true;
                    case 'u':
                        await ParseUndefinedAsync(cancellationToken).ConfigureAwait(false);
                        return true;
                    case '{':
                        _charPos++;
                        SetToken(JsonToken.StartObject);
                        return true;
                    case '[':
                        _charPos++;
                        SetToken(JsonToken.StartArray);
                        return true;
                    case ']':
                        _charPos++;
                        SetToken(JsonToken.EndArray);
                        return true;
                    case ',':

                        // don't increment position, the next call to read will handle comma
                        // this is done to handle multiple empty comma values
                        SetToken(JsonToken.Undefined);
                        return true;
                    case ')':
                        _charPos++;
                        SetToken(JsonToken.EndConstructor);
                        return true;
                    case StringUtils.CarriageReturn:
                        await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                        break;
                    case StringUtils.LineFeed:
                        ProcessLineFeed();
                        break;
                    case ' ':
                    case StringUtils.Tab:

                        // eat
                        _charPos++;
                        break;
                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            _charPos++;
                            break;
                        }

                        if (char.IsNumber(currentChar) || currentChar == '-' || currentChar == '.')
                        {
                            ParseNumber(ReadType.Read);
                            return true;
                        }

                        throw CreateUnexpectedCharacterException(currentChar);
                }
            }
        }

        private async Task ReadStringIntoBufferAsync(char quote, CancellationToken cancellationToken)
        {
            int charPos = _charPos;
            int initialPosition = _charPos;
            int lastWritePosition = _charPos;
            _stringBuffer.Position = 0;

            for (;;)
            {
                switch (_chars[charPos++])
                {
                    case '\0':
                        if (_charsUsed == charPos - 1)
                        {
                            charPos--;

                            if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                _charPos = charPos;
                                throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
                            }
                        }

                        break;
                    case '\\':
                        _charPos = charPos;
                        if (!await EnsureCharsAsync(0, true, cancellationToken).ConfigureAwait(false))
                        {
                            throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
                        }

                        // start of escape sequence
                        int escapeStartPos = charPos - 1;

                        char currentChar = _chars[charPos];
                        charPos++;

                        char writeChar;

                        switch (currentChar)
                        {
                            case 'b':
                                writeChar = '\b';
                                break;
                            case 't':
                                writeChar = '\t';
                                break;
                            case 'n':
                                writeChar = '\n';
                                break;
                            case 'f':
                                writeChar = '\f';
                                break;
                            case 'r':
                                writeChar = '\r';
                                break;
                            case '\\':
                                writeChar = '\\';
                                break;
                            case '"':
                            case '\'':
                            case '/':
                                writeChar = currentChar;
                                break;
                            case 'u':
                                _charPos = charPos;
                                writeChar = await ParseUnicodeAsync(cancellationToken).ConfigureAwait(false);

                                if (StringUtils.IsLowSurrogate(writeChar))
                                {
                                    // low surrogate with no preceding high surrogate; this char is replaced
                                    writeChar = UnicodeReplacementChar;
                                }
                                else if (StringUtils.IsHighSurrogate(writeChar))
                                {
                                    bool anotherHighSurrogate;

                                    // loop for handling situations where there are multiple consecutive high surrogates
                                    do
                                    {
                                        anotherHighSurrogate = false;

                                        // potential start of a surrogate pair
                                        if (EnsureChars(2, true) && _chars[_charPos] == '\\' && _chars[_charPos + 1] == 'u')
                                        {
                                            char highSurrogate = writeChar;

                                            _charPos += 2;
                                            writeChar = await ParseUnicodeAsync(cancellationToken).ConfigureAwait(false);

                                            if (StringUtils.IsLowSurrogate(writeChar))
                                            {
                                                // a valid surrogate pair!
                                            }
                                            else if (StringUtils.IsHighSurrogate(writeChar))
                                            {
                                                // another high surrogate; replace current and start check over
                                                highSurrogate = UnicodeReplacementChar;
                                                anotherHighSurrogate = true;
                                            }
                                            else
                                            {
                                                // high surrogate not followed by low surrogate; original char is replaced
                                                highSurrogate = UnicodeReplacementChar;
                                            }

                                            EnsureBufferNotEmpty();

                                            WriteCharToBuffer(highSurrogate, lastWritePosition, escapeStartPos);
                                            lastWritePosition = _charPos;
                                        }
                                        else
                                        {
                                            // there are not enough remaining chars for the low surrogate or is not follow by unicode sequence
                                            // replace high surrogate and continue on as usual
                                            writeChar = UnicodeReplacementChar;
                                        }
                                    } while (anotherHighSurrogate);
                                }

                                charPos = _charPos;
                                break;
                            default:
                                _charPos = charPos;
                                throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, @"\" + currentChar));
                        }

                        EnsureBufferNotEmpty();
                        WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);

                        lastWritePosition = charPos;
                        break;
                    case StringUtils.CarriageReturn:
                        _charPos = charPos - 1;
                        await ProcessCarriageReturnAsync(true, cancellationToken).ConfigureAwait(false);
                        charPos = _charPos;
                        break;
                    case StringUtils.LineFeed:
                        _charPos = charPos - 1;
                        ProcessLineFeed();
                        charPos = _charPos;
                        break;
                    case '"':
                    case '\'':
                        if (_chars[charPos - 1] == quote)
                        {
                            charPos--;

                            if (initialPosition == lastWritePosition)
                            {
                                _stringReference = new StringReference(_chars, initialPosition, charPos - initialPosition);
                            }
                            else
                            {
                                EnsureBufferNotEmpty();

                                if (charPos > lastWritePosition)
                                {
                                    _stringBuffer.Append(_arrayPool, _chars, lastWritePosition, charPos - lastWritePosition);
                                }

                                _stringReference = new StringReference(_stringBuffer.InternalBuffer, 0, _stringBuffer.Position);
                            }

                            charPos++;
                            _charPos = charPos;
                            return;
                        }

                        break;
                }
            }
        }

        private async Task ProcessCarriageReturnAsync(bool append, CancellationToken cancellationToken)
        {
            _charPos++;

            if (await EnsureCharsAsync(1, append, cancellationToken).ConfigureAwait(false) && _chars[_charPos] == StringUtils.LineFeed)
            {
                _charPos++;
            }

            OnNewLine(_charPos);
        }

        private async Task<char> ParseUnicodeAsync(CancellationToken cancellationToken)
        {
            char writeChar;
            if (await EnsureCharsAsync(4, true, cancellationToken).ConfigureAwait(false))
            {
                char hexChar = Convert.ToChar(ConvertUtils.HexTextToInt(_chars, _charPos, _charPos + 4));
                writeChar = hexChar;

                _charPos += 4;
            }
            else
            {
                throw JsonReaderException.Create(this, "Unexpected end while parsing unicode character.");
            }

            return writeChar;
        }

        private Task<bool> EnsureCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
        {
            if (_charPos + relativePosition < _charsUsed)
            {
                return AsyncUtils.True;
            }

            if (_isEndOfFile)
            {
                return AsyncUtils.False;
            }

            return ReadCharsAsync(relativePosition, append, cancellationToken);
        }

        private async Task<bool> ReadCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
        {
            int charsRequired = _charPos + relativePosition - _charsUsed + 1;

            int totalCharsRead = 0;

            // it is possible that the TextReader doesn't return all data at once
            // repeat read until the required text is returned or the reader is out of content
            do
            {
                int charsRead = await ReadDataAsync(append, charsRequired - totalCharsRead, cancellationToken).ConfigureAwait(false);

                // no more content
                if (charsRead == 0)
                {
                    return false;
                }

                totalCharsRead += charsRead;
            } while (totalCharsRead < charsRequired);

            return true;
        }

        private async Task<bool> ParseObjectAsync(CancellationToken cancellationToken)
        {
            for (;;)
            {
                char currentChar = _chars[_charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            _charPos++;
                        }

                        break;
                    case '}':
                        SetToken(JsonToken.EndObject);
                        _charPos++;
                        return true;
                    case '/':
                        await ParseCommentAsync(true, cancellationToken).ConfigureAwait(false);
                        return true;
                    case StringUtils.CarriageReturn:
                        await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                        break;
                    case StringUtils.LineFeed:
                        ProcessLineFeed();
                        break;
                    case ' ':
                    case StringUtils.Tab:

                        // eat
                        _charPos++;
                        break;
                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            _charPos++;
                        }
                        else
                        {
                            return await ParsePropertyAsync(cancellationToken).ConfigureAwait(false);
                        }

                        break;
                }
            }
        }

        private async Task ParseCommentAsync(bool setToken, CancellationToken cancellationToken)
        {
            // should have already parsed / character before reaching this method
            _charPos++;

            if (!await EnsureCharsAsync(1, false, cancellationToken).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
            }

            bool singlelineComment;

            if (_chars[_charPos] == '*')
            {
                singlelineComment = false;
            }
            else if (_chars[_charPos] == '/')
            {
                singlelineComment = true;
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
            }

            _charPos++;

            int initialPosition = _charPos;

            for (;;)
            {
                switch (_chars[_charPos])
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                if (!singlelineComment)
                                {
                                    throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
                                }

                                EndComment(setToken, initialPosition, _charPos);
                                return;
                            }
                        }
                        else
                        {
                            _charPos++;
                        }

                        break;
                    case '*':
                        _charPos++;

                        if (!singlelineComment)
                        {
                            if (await EnsureCharsAsync(0, true, cancellationToken).ConfigureAwait(false))
                            {
                                if (_chars[_charPos] == '/')
                                {
                                    EndComment(setToken, initialPosition, _charPos - 1);

                                    _charPos++;
                                    return;
                                }
                            }
                        }

                        break;
                    case StringUtils.CarriageReturn:
                        if (singlelineComment)
                        {
                            EndComment(setToken, initialPosition, _charPos);
                            return;
                        }

                        await ProcessCarriageReturnAsync(true, cancellationToken).ConfigureAwait(false);
                        break;
                    case StringUtils.LineFeed:
                        if (singlelineComment)
                        {
                            EndComment(setToken, initialPosition, _charPos);
                            return;
                        }

                        ProcessLineFeed();
                        break;
                    default:
                        _charPos++;
                        break;
                }
            }
        }

        private async Task<bool> ParsePostValueAsync(CancellationToken cancellationToken)
        {
            for (;;)
            {
                char currentChar = _chars[_charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                _currentState = State.Finished;
                                return false;
                            }
                        }
                        else
                        {
                            _charPos++;
                        }

                        break;
                    case '}':
                        _charPos++;
                        SetToken(JsonToken.EndObject);
                        return true;
                    case ']':
                        _charPos++;
                        SetToken(JsonToken.EndArray);
                        return true;
                    case ')':
                        _charPos++;
                        SetToken(JsonToken.EndConstructor);
                        return true;
                    case '/':
                        await ParseCommentAsync(true, cancellationToken).ConfigureAwait(false);
                        return true;
                    case ',':
                        _charPos++;

                        // finished parsing
                        SetStateBasedOnCurrent();
                        return false;
                    case ' ':
                    case StringUtils.Tab:

                        // eat
                        _charPos++;
                        break;
                    case StringUtils.CarriageReturn:
                        await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                        break;
                    case StringUtils.LineFeed:
                        ProcessLineFeed();
                        break;
                    default:
                        if (char.IsWhiteSpace(currentChar))
                        {
                            // eat
                            _charPos++;
                        }
                        else
                        {
                            throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                        }

                        break;
                }
            }
        }

        private async Task<bool> EatWhitespaceAsync(bool oneOrMore, CancellationToken cancellationToken)
        {
            bool finished = false;
            bool ateWhitespace = false;
            while (!finished)
            {
                char currentChar = _chars[_charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                finished = true;
                            }
                        }
                        else
                        {
                            _charPos++;
                        }
                        break;
                    case StringUtils.CarriageReturn:
                        await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                        break;
                    case StringUtils.LineFeed:
                        ProcessLineFeed();
                        break;
                    default:
                        if (currentChar == ' ' || char.IsWhiteSpace(currentChar))
                        {
                            ateWhitespace = true;
                            _charPos++;
                        }
                        else
                        {
                            finished = true;
                        }
                        break;
                }
            }

            return !oneOrMore || ateWhitespace;
        }

        private async Task ParseStringAsync(char quote, ReadType readType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _charPos++;

            ShiftBufferIfNeeded();
            await ReadStringIntoBufferAsync(quote, cancellationToken).ConfigureAwait(false);
            SetPostValueState(true);

            switch (readType)
            {
                case ReadType.ReadAsBytes:
                    Guid g;
                    byte[] data;
                    if (_stringReference.Length == 0)
                    {
                        data = new byte[0];
                    }
                    else if (_stringReference.Length == 36 && ConvertUtils.TryConvertGuid(_stringReference.ToString(), out g))
                    {
                        data = g.ToByteArray();
                    }
                    else
                    {
                        data = Convert.FromBase64CharArray(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length);
                    }

                    SetToken(JsonToken.Bytes, data, false);
                    break;
                case ReadType.ReadAsString:
                    string text = _stringReference.ToString();

                    SetToken(JsonToken.String, text, false);
                    _quoteChar = quote;
                    break;
                case ReadType.ReadAsInt32:
                case ReadType.ReadAsDecimal:
                case ReadType.ReadAsBoolean:

                    // caller will convert result
                    break;
                default:
                    if (_dateParseHandling != DateParseHandling.None)
                    {
                        DateParseHandling dateParseHandling;
                        if (readType == ReadType.ReadAsDateTime)
                        {
                            dateParseHandling = DateParseHandling.DateTime;
                        }
#if !NET20
                        else if (readType == ReadType.ReadAsDateTimeOffset)
                        {
                            dateParseHandling = DateParseHandling.DateTimeOffset;
                        }
#endif
                        else
                        {
                            dateParseHandling = _dateParseHandling;
                        }

                        if (dateParseHandling == DateParseHandling.DateTime)
                        {
                            DateTime dt;
                            if (DateTimeUtils.TryParseDateTime(_stringReference, DateTimeZoneHandling, DateFormatString, Culture, out dt))
                            {
                                SetToken(JsonToken.Date, dt, false);
                                return;
                            }
                        }
#if !NET20
                        else
                        {
                            DateTimeOffset dt;
                            if (DateTimeUtils.TryParseDateTimeOffset(_stringReference, DateFormatString, Culture, out dt))
                            {
                                SetToken(JsonToken.Date, dt, false);
                                return;
                            }
                        }
#endif
                    }

                    SetToken(JsonToken.String, _stringReference.ToString(), false);
                    _quoteChar = quote;
                    break;
            }
        }

        private async Task<bool> MatchValueAsync(string value, CancellationToken cancellationToken)
        {
            if (!await EnsureCharsAsync(value.Length - 1, true, cancellationToken).ConfigureAwait(false))
            {
                _charPos = _charsUsed;
                throw CreateUnexpectedEndException();
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (_chars[_charPos + i] != value[i])
                {
                    _charPos += i;
                    return false;
                }
            }

            _charPos += value.Length;

            return true;
        }

        private async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, CancellationToken cancellationToken)
        {
            // will match value and then move to the next character, checking that it is a separator character
            if (!await MatchValueAsync(value, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            if (!await EnsureCharsAsync(0, false, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            return IsSeparator(_chars[_charPos]) || _chars[_charPos] == '\0';
        }

        private async Task MatchAndSetAsync(string value, JsonToken newToken, object tokenValue, CancellationToken cancellationToken)
        {
            if (await MatchValueWithTrailingSeparatorAsync(value, cancellationToken).ConfigureAwait(false))
            {
                SetToken(newToken, tokenValue);
            }
            else
            {
                throw JsonReaderException.Create(this, "Error parsing " + newToken.ToString().ToLowerInvariant() + " value.");
            }
        }

        private Task ParseTrueAsync(CancellationToken cancellationToken)
        {
            return MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancellationToken);
        }

        private Task ParseFalseAsync(CancellationToken cancellationToken)
        {
            return MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancellationToken);
        }

        private Task ParseNullAsync(CancellationToken cancellationToken)
        {
            return MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancellationToken);
        }

        private async Task ParseConstructorAsync(CancellationToken cancellationToken)
        {
            if (await MatchValueWithTrailingSeparatorAsync("new", cancellationToken).ConfigureAwait(false))
            {
                await EatWhitespaceAsync(false, cancellationToken).ConfigureAwait(false);

                int initialPosition = _charPos;
                int endPosition;

                for (;;)
                {
                    char currentChar = _chars[_charPos];
                    if (currentChar == '\0')
                    {
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
                            }
                        }
                        else
                        {
                            endPosition = _charPos;
                            _charPos++;
                            break;
                        }
                    }
                    else if (char.IsLetterOrDigit(currentChar))
                    {
                        _charPos++;
                    }
                    else if (currentChar == StringUtils.CarriageReturn)
                    {
                        endPosition = _charPos;
                        await ProcessCarriageReturnAsync(true, cancellationToken).ConfigureAwait(false);
                        break;
                    }
                    else if (currentChar == StringUtils.LineFeed)
                    {
                        endPosition = _charPos;
                        ProcessLineFeed();
                        break;
                    }
                    else if (char.IsWhiteSpace(currentChar))
                    {
                        endPosition = _charPos;
                        _charPos++;
                        break;
                    }
                    else if (currentChar == '(')
                    {
                        endPosition = _charPos;
                        break;
                    }
                    else
                    {
                        throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                    }
                }

                _stringReference = new StringReference(_chars, initialPosition, endPosition - initialPosition);
                string constructorName = _stringReference.ToString();

                EatWhitespace(false);

                if (_chars[_charPos] != '(')
                {
                    throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
                }

                _charPos++;

                ClearRecentString();

                SetToken(JsonToken.StartConstructor, constructorName);
            }
            else
            {
                throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
            }
        }

        private async Task<object> ParseNumberNaNAsync(ReadType readType, CancellationToken cancellationToken)
        {
            if (await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellationToken).ConfigureAwait(false))
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (_floatParseHandling == FloatParseHandling.Double)
                        {
                            SetToken(JsonToken.Float, double.NaN);
                            return double.NaN;
                        }

                        break;
                    case ReadType.ReadAsString:
                        SetToken(JsonToken.String, JsonConvert.NaN);
                        return JsonConvert.NaN;
                }

                throw JsonReaderException.Create(this, "Cannot read NaN value.");
            }

            throw JsonReaderException.Create(this, "Error parsing NaN value.");
        }

        private async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, CancellationToken cancellationToken)
        {
            if (await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellationToken).ConfigureAwait(false))
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (_floatParseHandling == FloatParseHandling.Double)
                        {
                            SetToken(JsonToken.Float, double.PositiveInfinity);
                            return double.PositiveInfinity;
                        }

                        break;
                    case ReadType.ReadAsString:
                        SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
                        return JsonConvert.PositiveInfinity;
                }

                throw JsonReaderException.Create(this, "Cannot read Infinity value.");
            }

            throw JsonReaderException.Create(this, "Error parsing Infinity value.");
        }

        private async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, CancellationToken cancellationToken)
        {
            if (await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellationToken).ConfigureAwait(false))
            {
                switch (readType)
                {
                    case ReadType.Read:
                    case ReadType.ReadAsDouble:
                        if (_floatParseHandling == FloatParseHandling.Double)
                        {
                            SetToken(JsonToken.Float, double.NegativeInfinity);
                            return double.NegativeInfinity;
                        }

                        break;
                    case ReadType.ReadAsString:
                        SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
                        return JsonConvert.NegativeInfinity;
                }

                throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
            }

            throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
        }

        private async Task ParseNumberAsync(ReadType readType, CancellationToken cancellationToken)
        {
            ShiftBufferIfNeeded();

            char firstChar = _chars[_charPos];
            int initialPosition = _charPos;

            await ReadNumberIntoBufferAsync(cancellationToken).ConfigureAwait(false);

            // set state to PostValue now so that if there is an error parsing the number then the reader can continue
            SetPostValueState(true);

            _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);

            object numberValue;
            JsonToken numberType;

            bool singleDigit = char.IsDigit(firstChar) && _stringReference.Length == 1;
            bool nonBase10 = firstChar == '0' && _stringReference.Length > 1 && _stringReference.Chars[_stringReference.StartIndex + 1] != '.' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'e' && _stringReference.Chars[_stringReference.StartIndex + 1] != 'E';

            if (readType == ReadType.ReadAsString)
            {
                string number = _stringReference.ToString();

                // validate that the string is a valid number
                if (nonBase10)
                {
                    try
                    {
                        if (number.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            Convert.ToInt64(number, 16);
                        }
                        else
                        {
                            Convert.ToInt64(number, 8);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    double value;
                    if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                    }
                }

                numberType = JsonToken.String;
                numberValue = number;
            }
            else if (readType == ReadType.ReadAsInt32)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = _stringReference.ToString();

                    try
                    {
                        int integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(number, 16) : Convert.ToInt32(number, 8);

                        numberValue = integer;
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    int value;
                    ParseResult parseResult = ConvertUtils.Int32TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
                        throw ThrowReaderError("JSON integer {0} is too large or small for an Int32.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                    }
                    else
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Integer;
            }
            else if (readType == ReadType.ReadAsDecimal)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (decimal)firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = _stringReference.ToString();

                    try
                    {
                        // decimal.Parse doesn't support parsing hexadecimal values
                        long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDecimal(integer);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    string number = _stringReference.ToString();

                    decimal value;
                    if (decimal.TryParse(number, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value))
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Float;
            }
            else if (readType == ReadType.ReadAsDouble)
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (double)firstChar - 48;
                }
                else if (nonBase10)
                {
                    string number = _stringReference.ToString();

                    try
                    {
                        // double.Parse doesn't support parsing hexadecimal values
                        long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);

                        numberValue = Convert.ToDouble(integer);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }
                }
                else
                {
                    double value;
                    ParseResult parseResult = ConvertUtils.DoubleTryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                    }
                    else
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                    }
                }

                numberType = JsonToken.Float;
            }
            else
            {
                if (singleDigit)
                {
                    // digit char values start at 48
                    numberValue = (long)firstChar - 48;
                    numberType = JsonToken.Integer;
                }
                else if (nonBase10)
                {
                    string number = _stringReference.ToString();

                    try
                    {
                        numberValue = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(number, 16) : Convert.ToInt64(number, 8);
                    }
                    catch (Exception ex)
                    {
                        throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, number), ex);
                    }

                    numberType = JsonToken.Integer;
                }
                else
                {
                    long value;
                    ParseResult parseResult = ConvertUtils.Int64TryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out value);
                    if (parseResult == ParseResult.Success)
                    {
                        numberValue = value;
                        numberType = JsonToken.Integer;
                    }
                    else if (parseResult == ParseResult.Overflow)
                    {
#if !(PORTABLE40 || PORTABLE) || NETSTANDARD1_1
                        string number = _stringReference.ToString();

                        if (number.Length > MaximumJavascriptIntegerCharacterLength)
                        {
                            throw ThrowReaderError("JSON integer {0} is too large to parse.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
                        }

                        numberValue = BigIntegerParse(number, CultureInfo.InvariantCulture);
                        numberType = JsonToken.Integer;
#else
                        throw ThrowReaderError("JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, _stringReference.ToString()));
#endif
                    }
                    else
                    {
                        string number = _stringReference.ToString();

                        if (_floatParseHandling == FloatParseHandling.Decimal)
                        {
                            decimal d;
                            if (decimal.TryParse(number, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out d))
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, number));
                            }
                        }
                        else
                        {
                            double d;
                            parseResult = ConvertUtils.DoubleTryParse(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length, out d);
                            if (parseResult == ParseResult.Success)
                            {
                                numberValue = d;
                            }
                            else
                            {
                                throw ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, number));
                            }
                        }

                        numberType = JsonToken.Float;
                    }
                }
            }

            ClearRecentString();

            // index has already been updated
            SetToken(numberType, numberValue, false);
        }

        private Task ParseUndefinedAsync(CancellationToken cancellationToken)
        {
            return MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellationToken);
        }

        private async Task<bool> ParsePropertyAsync(CancellationToken cancellationToken)
        {
            char firstChar = _chars[_charPos];
            char quoteChar;

            if (firstChar == '"' || firstChar == '\'')
            {
                _charPos++;
                quoteChar = firstChar;
                ShiftBufferIfNeeded();
                await ReadStringIntoBufferAsync(quoteChar, cancellationToken).ConfigureAwait(false);
            }
            else if (ValidIdentifierChar(firstChar))
            {
                quoteChar = '\0';
                ShiftBufferIfNeeded();
                await ParseUnquotedPropertyAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
            }

            string propertyName;

            if (NameTable != null)
            {
                propertyName = NameTable.Get(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length)
                    // no match in name table
                    ?? _stringReference.ToString();
            }
            else
            {
                propertyName = _stringReference.ToString();
            }

            await EatWhitespaceAsync(false, cancellationToken).ConfigureAwait(false);

            if (_chars[_charPos] != ':')
            {
                throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
            }

            _charPos++;

            SetToken(JsonToken.PropertyName, propertyName);
            _quoteChar = quoteChar;
            ClearRecentString();

            return true;
        }

        private async Task ReadNumberIntoBufferAsync(CancellationToken cancellationToken)
        {
            int charPos = _charPos;

            for (;;)
            {
                switch (_chars[charPos])
                {
                    case '\0':
                        _charPos = charPos;

                        if (_charsUsed == charPos)
                        {
                            if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }

                        break;
                    case '-':
                    case '+':
                    case 'a':
                    case 'A':
                    case 'b':
                    case 'B':
                    case 'c':
                    case 'C':
                    case 'd':
                    case 'D':
                    case 'e':
                    case 'E':
                    case 'f':
                    case 'F':
                    case 'x':
                    case 'X':
                    case '.':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        charPos++;
                        break;
                    default:
                        _charPos = charPos;

                        char currentChar = _chars[_charPos];
                        if (char.IsWhiteSpace(currentChar) || currentChar == ',' || currentChar == '}' || currentChar == ']' || currentChar == ')' || currentChar == '/')
                        {
                            return;
                        }

                        throw JsonReaderException.Create(this, "Unexpected character encountered while parsing number: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                }
            }
        }

        private async Task ParseUnquotedPropertyAsync(CancellationToken cancellationToken)
        {
            int initialPosition = _charPos;

            // parse unquoted property name until whitespace or colon
            for (;;)
            {
                switch (_chars[_charPos])
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
                            }

                            break;
                        }

                        _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
                        return;
                    default:
                        char currentChar = _chars[_charPos];

                        if (ValidIdentifierChar(currentChar))
                        {
                            _charPos++;
                            break;
                        }
                        else if (char.IsWhiteSpace(currentChar) || currentChar == ':')
                        {
                            _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
                            return;
                        }

                        throw JsonReaderException.Create(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                }
            }
        }

        private async Task<bool> ReadNullCharAsync(CancellationToken cancellationToken)
        {
            if (_charsUsed == _charPos)
            {
                if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                {
                    _isEndOfFile = true;
                    return true;
                }
            }
            else
            {
                _charPos++;
            }

            return false;
        }

        private async Task HandleNullAsync(CancellationToken cancellationToken)
        {
            if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false))
            {
                char next = _chars[_charPos + 1];

                if (next == 'u')
                {
                    await ParseNullAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }

                _charPos += 2;
                throw CreateUnexpectedCharacterException(_chars[_charPos - 1]);
            }

            _charPos = _charsUsed;
            throw CreateUnexpectedEndException();
        }

        private async Task ReadFinishedAsync(CancellationToken cancellationToken)
        {
            if (await EnsureCharsAsync(0, false, cancellationToken).ConfigureAwait(false))
            {
                await EatWhitespaceAsync(false, cancellationToken).ConfigureAwait(false);
                if (_isEndOfFile)
                {
                    return;
                }

                if (_chars[_charPos] == '/')
                {
                    await ParseCommentAsync(false, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
                }
            }

            SetToken(JsonToken.None);
        }

        private async Task<object> ReadStringValueAsync(ReadType readType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureBuffer();

            switch (_currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                case State.PostValue:
                    for (;;)
                    {
                        char currentChar = _chars[_charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    SetToken(JsonToken.None, null, false);
                                    return null;
                                }

                                break;
                            case '"':
                            case '\'':
                                await ParseStringAsync(currentChar, readType, cancellationToken).ConfigureAwait(false);
                                switch (readType)
                                {
                                    case ReadType.ReadAsBytes:
                                        return Value;
                                    case ReadType.ReadAsString:
                                        return Value;
                                    case ReadType.ReadAsDateTime:
                                        if (Value is DateTime)
                                        {
                                            return (DateTime)Value;
                                        }

                                        return ReadDateTimeString((string)Value);
#if !NET20
                                    case ReadType.ReadAsDateTimeOffset:
                                        if (Value is DateTimeOffset)
                                        {
                                            return (DateTimeOffset)Value;
                                        }

                                        return ReadDateTimeOffsetString((string)Value);
#endif
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(readType));
                                }
                            case '-':
                                if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false) && _chars[_charPos + 1] == 'I')
                                {
                                    return ParseNumberNegativeInfinity(readType);
                                }
                                else
                                {
                                    await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(false);
                                    return Value;
                                }
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                if (readType != ReadType.ReadAsString)
                                {
                                    _charPos++;
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                await ParseNumberAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(false);
                                return Value;
                            case 't':
                            case 'f':
                                if (readType != ReadType.ReadAsString)
                                {
                                    _charPos++;
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                string expected = currentChar == 't' ? JsonConvert.True : JsonConvert.False;
                                if (!await MatchValueWithTrailingSeparatorAsync(expected, cancellationToken).ConfigureAwait(false))
                                {
                                    throw CreateUnexpectedCharacterException(_chars[_charPos]);
                                }

                                SetToken(JsonToken.String, expected);
                                return expected;
                            case 'I':
                                return await ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(false);
                            case 'N':
                                return await ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(false);
                            case 'n':
                                await HandleNullAsync(cancellationToken).ConfigureAwait(false);
                                return null;
                            case '/':
                                await ParseCommentAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case ',':
                                ProcessValueComma();
                                break;
                            case ']':
                                _charPos++;
                                if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
                                {
                                    SetToken(JsonToken.EndArray);
                                    return null;
                                }

                                throw CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case StringUtils.LineFeed:
                                ProcessLineFeed();
                                break;
                            case ' ':
                            case StringUtils.Tab:

                                // eat
                                _charPos++;
                                break;
                            default:
                                _charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    await ReadFinishedAsync(cancellationToken).ConfigureAwait(false);
                    return null;
                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
            }
        }

        private async Task<object> ReadNumberValueAsync(ReadType readType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureBuffer();

            switch (_currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                case State.PostValue:
                    for (;;)
                    {
                        char currentChar = _chars[_charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    SetToken(JsonToken.None, null, false);
                                    return null;
                                }

                                break;
                            case '"':
                            case '\'':
                                await ParseStringAsync(currentChar, readType, cancellationToken).ConfigureAwait(false);
                                switch (readType)
                                {
                                    case ReadType.ReadAsInt32:
                                        return ReadInt32String(_stringReference.ToString());
                                    case ReadType.ReadAsDecimal:
                                        return ReadDecimalString(_stringReference.ToString());
                                    case ReadType.ReadAsDouble:
                                        return ReadDoubleString(_stringReference.ToString());
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(readType));
                                }
                            case 'n':
                                await HandleNullAsync(cancellationToken).ConfigureAwait(false);
                                return null;
                            case 'N':
                                return await ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(false);
                            case 'I':
                                return await ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(false);
                            case '-':
                                if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false) && _chars[_charPos + 1] == 'I')
                                {
                                    return await ParseNumberNegativeInfinityAsync(readType, cancellationToken).ConfigureAwait(false);
                                }
                                else
                                {
                                    await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(false);
                                    return Value;
                                }
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                await ParseNumberAsync(readType, cancellationToken).ConfigureAwait(false);
                                return Value;
                            case '/':
                                await ParseCommentAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case ',':
                                ProcessValueComma();
                                break;
                            case ']':
                                _charPos++;
                                if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
                                {
                                    SetToken(JsonToken.EndArray);
                                    return null;
                                }

                                throw CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case StringUtils.LineFeed:
                                ProcessLineFeed();
                                break;
                            case ' ':
                            case StringUtils.Tab:

                                // eat
                                _charPos++;
                                break;
                            default:
                                _charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    await ReadFinishedAsync(cancellationToken).ConfigureAwait(false);
                    return null;
                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
            }
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="bool"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="bool"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsBooleanAsync(cancellationToken) : base.ReadAsBooleanAsync(cancellationToken);
        }

        internal async Task<bool?> DoReadAsBooleanAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureBuffer();

            switch (_currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                case State.PostValue:
                    while (true)
                    {
                        char currentChar = _chars[_charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    SetToken(JsonToken.None, null, false);
                                    return null;
                                }

                                break;
                            case '"':
                            case '\'':
                                await ParseStringAsync(currentChar, ReadType.Read, cancellationToken).ConfigureAwait(false);
                                return ReadBooleanString(_stringReference.ToString());
                            case 'n':
                                await HandleNullAsync(cancellationToken).ConfigureAwait(false);
                                return null;
                            case '-':
                            case '.':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                                bool b;
#if !(PORTABLE40 || PORTABLE) || NETSTANDARD1_1
                                if (Value is BigInteger)
                                {
                                    b = (BigInteger)Value != 0;
                                }
                                else
#endif
                                {
                                    b = Convert.ToBoolean(Value, CultureInfo.InvariantCulture);
                                }
                                SetToken(JsonToken.Boolean, b, false);
                                return b;
                            case 't':
                            case 'f':
                                bool isTrue = currentChar == 't';
                                string expected = isTrue ? JsonConvert.True : JsonConvert.False;

                                if (!await MatchValueWithTrailingSeparatorAsync(expected, cancellationToken).ConfigureAwait(false))
                                {
                                    throw CreateUnexpectedCharacterException(_chars[_charPos]);
                                }

                                SetToken(JsonToken.Boolean, isTrue);
                                return isTrue;
                            case '/':
                                await ParseCommentAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case ',':
                                ProcessValueComma();
                                break;
                            case ']':
                                _charPos++;
                                if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
                                {
                                    SetToken(JsonToken.EndArray);
                                    return null;
                                }

                                throw CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case StringUtils.LineFeed:
                                ProcessLineFeed();
                                break;
                            case ' ':
                            case StringUtils.Tab:

                                // eat
                                _charPos++;
                                break;
                            default:
                                _charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    await ReadFinishedAsync(cancellationToken).ConfigureAwait(false);
                    return null;
                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
            }
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="byte"/>[].
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="byte"/>[]. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsBytesAsync(cancellationToken) : base.ReadAsBytesAsync(cancellationToken);
        }

        internal async Task<byte[]> DoReadAsBytesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureBuffer();
            bool isWrapped = false;

            switch (_currentState)
            {
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
                case State.PostValue:
                    for (;;)
                    {
                        char currentChar = _chars[_charPos];

                        switch (currentChar)
                        {
                            case '\0':
                                if (await ReadNullCharAsync(cancellationToken).ConfigureAwait(false))
                                {
                                    SetToken(JsonToken.None, null, false);
                                    return null;
                                }

                                break;
                            case '"':
                            case '\'':
                                await ParseStringAsync(currentChar, ReadType.ReadAsBytes, cancellationToken).ConfigureAwait(false);
                                byte[] data = (byte[])Value;
                                if (isWrapped)
                                {
                                    await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                                    if (TokenType != JsonToken.EndObject)
                                    {
                                        throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
                                    }

                                    SetToken(JsonToken.Bytes, data, false);
                                }

                                return data;
                            case '{':
                                _charPos++;
                                SetToken(JsonToken.StartObject);
                                await ReadIntoWrappedTypeObjectAsync(cancellationToken).ConfigureAwait(false);
                                isWrapped = true;
                                break;
                            case '[':
                                _charPos++;
                                SetToken(JsonToken.StartArray);
                                return await ReadArrayIntoByteArrayAsync(cancellationToken).ConfigureAwait(false);
                            case 'n':
                                await HandleNullAsync(cancellationToken);
                                return null;
                            case '/':
                                await ParseCommentAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case ',':
                                ProcessValueComma();
                                break;
                            case ']':
                                _charPos++;
                                if (_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.PostValue)
                                {
                                    SetToken(JsonToken.EndArray);
                                    return null;
                                }

                                throw CreateUnexpectedCharacterException(currentChar);
                            case StringUtils.CarriageReturn:
                                await ProcessCarriageReturnAsync(false, cancellationToken).ConfigureAwait(false);
                                break;
                            case StringUtils.LineFeed:
                                ProcessLineFeed();
                                break;
                            case ' ':
                            case StringUtils.Tab:

                                // eat
                                _charPos++;
                                break;
                            default:
                                _charPos++;

                                if (!char.IsWhiteSpace(currentChar))
                                {
                                    throw CreateUnexpectedCharacterException(currentChar);
                                }

                                // eat
                                break;
                        }
                    }
                case State.Finished:
                    await ReadFinishedAsync(cancellationToken).ConfigureAwait(false);
                    return null;
                default:
                    throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
            }
        }

        private async Task ReadIntoWrappedTypeObjectAsync(CancellationToken cancellationToken)
        {
            await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            if (Value != null && Value.ToString() == JsonTypeReflector.TypePropertyName)
            {
                await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                if (Value != null && Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
                {
                    await ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    if (Value.ToString() == JsonTypeReflector.ValuePropertyName)
                    {
                        return;
                    }
                }
            }

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
        }

        private async Task<byte[]> ReadArrayIntoByteArrayAsync(CancellationToken cancellationToken)
        {
            List<byte> buffer = new List<byte>();

            for (;;)
            {
                if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    SetToken(JsonToken.None);
                }

                switch (TokenType)
                {
                    case JsonToken.None:
                        throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
                    case JsonToken.Integer:
                        buffer.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.EndArray:
                        byte[] d = buffer.ToArray();
                        SetToken(JsonToken.Bytes, d, false);
                        return d;
                    case JsonToken.Comment:
                        continue;
                    default:
                        throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsDateTimeAsync(cancellationToken) : base.ReadAsDateTimeAsync(cancellationToken);
        }

        internal async Task<DateTime?> DoReadAsDateTimeAsync(CancellationToken cancellationToken)
        {
            return (DateTime?)await ReadStringValueAsync(ReadType.ReadAsDateTime, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsDateTimeOffsetAsync(cancellationToken) : base.ReadAsDateTimeOffsetAsync(cancellationToken);
        }

        internal async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(CancellationToken cancellationToken)
        {
            return (DateTimeOffset?)await ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="decimal"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="decimal"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsDecimalAsync(cancellationToken) : base.ReadAsDecimalAsync(cancellationToken);
        }

        internal async Task<decimal?> DoReadAsDecimalAsync(CancellationToken cancellationToken)
        {
            return (decimal?)await ReadNumberValueAsync(ReadType.ReadAsDecimal, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="double"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="double"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsDoubleAsync(cancellationToken) : base.ReadAsDoubleAsync(cancellationToken);
        }

        internal async Task<double?> DoReadAsDoubleAsync(CancellationToken cancellationToken)
        {
            return (double?)await ReadNumberValueAsync(ReadType.ReadAsDouble, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="int"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="int"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsInt32Async(cancellationToken) : base.ReadAsInt32Async(cancellationToken);
        }

        internal async Task<int?> DoReadAsInt32Async(CancellationToken cancellationToken)
        {
            return (int?)await ReadNumberValueAsync(ReadType.ReadAsInt32, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="string"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="string"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoReadAsStringAsync(cancellationToken) : base.ReadAsStringAsync(cancellationToken);
        }

        internal async Task<string> DoReadAsStringAsync(CancellationToken cancellationToken)
        {
            return (string)await ReadStringValueAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(false);
        }
    }

    internal sealed partial class JsonTextReaderImpl
    {
        public override Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsync(cancellationToken);
        }

        public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsBooleanAsync(cancellationToken);
        }

        public override Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsBytesAsync(cancellationToken);
        }

        public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsDateTimeAsync(cancellationToken);
        }

        public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsDateTimeOffsetAsync(cancellationToken);
        }

        public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsDecimalAsync(cancellationToken);
        }

        public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsDoubleAsync(cancellationToken);
        }

        public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsInt32Async(cancellationToken);
        }

        public override Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadAsStringAsync(cancellationToken);
        }
    }
}

#endif
