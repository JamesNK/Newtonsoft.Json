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

#if HAVE_ASYNC

using System;
using System.Globalization;
using System.Threading;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Diagnostics;

namespace Newtonsoft.Json
{
    public partial class JsonTextReader
    {
        // It's not safe to perform the async methods here in a derived class as if the synchronous equivalent
        // has been overriden then the asychronous method will no longer be doing the same operation
#if HAVE_ASYNC // Double-check this isn't included inappropriately.
        private readonly bool _safeAsync;
#endif

        /// <summary>
        /// Asynchronously reads the next JSON token from the source.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsync(cancellationToken) : base.ReadAsync(cancellationToken);
        }

        internal Task<bool> DoReadAsync(CancellationToken cancellationToken)
        {
            EnsureBuffer();

            while (true)
            {
                switch (_currentState)
                {
                    case State.Start:
                    case State.Property:
                    case State.Array:
                    case State.ArrayStart:
                    case State.Constructor:
                    case State.ConstructorStart:
                        return ParseValueAsync(cancellationToken);
                    case State.Object:
                    case State.ObjectStart:
                        return ParseObjectAsync(cancellationToken);
                    case State.PostValue:
                        Task<bool> task = ParsePostValueAsync(false, cancellationToken);
                        if (task.IsCompletedSucessfully())
                        {
                            if (task.Result)
                            {
                                return AsyncUtils.True;
                            }
                        }
                        else
                        {
                            return DoReadAsync(task, cancellationToken);
                        }
                        break;
                    case State.Finished:
                        return ReadFromFinishedAsync(cancellationToken);
                    default:
                        throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
                }
            }
        }

        private async Task<bool> DoReadAsync(Task<bool> task, CancellationToken cancellationToken)
        {
            bool result = await task.ConfigureAwait(false);
            if (result)
            {
                return true;
            }
            return await DoReadAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> ParsePostValueAsync(bool ignoreComments, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            while (true)
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
                        await ParseCommentAsync(!ignoreComments, cancellationToken).ConfigureAwait(false);
                        if (!ignoreComments)
                        {
                            return true;
                        }
                        break;
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
                            // handle multiple content without comma delimiter
                            if (SupportMultipleContent && Depth == 0)
                            {
                                SetStateBasedOnCurrent();
                                return false;
                            }

                            throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
                        }

                        break;
                }
            }
        }

        private async Task<bool> ReadFromFinishedAsync(CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            if (await EnsureCharsAsync(0, false, cancellationToken).ConfigureAwait(false))
            {
                await EatWhitespaceAsync(cancellationToken).ConfigureAwait(false);
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
        }

        private Task<int> ReadDataAsync(bool append, CancellationToken cancellationToken)
        {
            return ReadDataAsync(append, 0, cancellationToken);
        }

        private async Task<int> ReadDataAsync(bool append, int charsRequired, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            if (_isEndOfFile)
            {
                return 0;
            }

            PrepareBufferForReadData(append, charsRequired);

            int charsRead = await _reader.ReadAsync(_chars, _charsUsed, _chars.Length - _charsUsed - 1, cancellationToken).ConfigureAwait(false);

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
            MiscellaneousUtils.Assert(_chars != null);

            while (true)
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
                            switch (_chars[_charPos + 1])
                            {
                                case 'u':
                                    await ParseNullAsync(cancellationToken).ConfigureAwait(false);
                                    break;
                                case 'e':
                                    await ParseConstructorAsync(cancellationToken).ConfigureAwait(false);
                                    break;
                                default:
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
                            await ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(false);
                            return true;
                        }

                        throw CreateUnexpectedCharacterException(currentChar);
                }
            }
        }

        private async Task ReadStringIntoBufferAsync(char quote, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            int charPos = _charPos;
            int initialPosition = _charPos;
            int lastWritePosition = _charPos;
            _stringBuffer.Position = 0;

            while (true)
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
                                        if (await EnsureCharsAsync(2, true, cancellationToken).ConfigureAwait(false) && _chars[_charPos] == '\\' && _chars[_charPos + 1] == 'u')
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
                            FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
                            return;
                        }

                        break;
                }
            }
        }

        private Task ProcessCarriageReturnAsync(bool append, CancellationToken cancellationToken)
        {
            _charPos++;

            Task<bool> task = EnsureCharsAsync(1, append, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                SetNewLine(task.Result);
                return AsyncUtils.CompletedTask;
            }

            return ProcessCarriageReturnAsync(task);
        }

        private async Task ProcessCarriageReturnAsync(Task<bool> task)
        {
            SetNewLine(await task.ConfigureAwait(false));
        }

        private async Task<char> ParseUnicodeAsync(CancellationToken cancellationToken)
        {
            return ConvertUnicode(await EnsureCharsAsync(4, true, cancellationToken).ConfigureAwait(false));
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

            // it is possible that the TextReader doesn't return all data at once
            // repeat read until the required text is returned or the reader is out of content
            do
            {
                int charsRead = await ReadDataAsync(append, charsRequired, cancellationToken).ConfigureAwait(false);

                // no more content
                if (charsRead == 0)
                {
                    return false;
                }

                charsRequired -= charsRead;
            } while (charsRequired > 0);

            return true;
        }

        private async Task<bool> ParseObjectAsync(CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            while (true)
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
            MiscellaneousUtils.Assert(_chars != null);

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

            while (true)
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

        private async Task EatWhitespaceAsync(CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            while (true)
            {
                char currentChar = _chars[_charPos];

                switch (currentChar)
                {
                    case '\0':
                        if (_charsUsed == _charPos)
                        {
                            if (await ReadDataAsync(false, cancellationToken).ConfigureAwait(false) == 0)
                            {
                                return;
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
                            _charPos++;
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
            }
        }

        private async Task ParseStringAsync(char quote, ReadType readType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _charPos++;

            ShiftBufferIfNeeded();
            await ReadStringIntoBufferAsync(quote, cancellationToken).ConfigureAwait(false);
            ParseReadString(quote, readType);
        }

        private async Task<bool> MatchValueAsync(string value, CancellationToken cancellationToken)
        {
            return MatchValue(await EnsureCharsAsync(value.Length - 1, true, cancellationToken).ConfigureAwait(false), value);
        }

        private async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

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

        private async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, CancellationToken cancellationToken)
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
            MiscellaneousUtils.Assert(_chars != null);

            if (await MatchValueWithTrailingSeparatorAsync("new", cancellationToken).ConfigureAwait(false))
            {
                await EatWhitespaceAsync(cancellationToken).ConfigureAwait(false);

                int initialPosition = _charPos;
                int endPosition;

                while (true)
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

                await EatWhitespaceAsync(cancellationToken).ConfigureAwait(false);

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
            return ParseNumberNaN(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellationToken).ConfigureAwait(false));
        }

        private async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, CancellationToken cancellationToken)
        {
            return ParseNumberPositiveInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellationToken).ConfigureAwait(false));
        }

        private async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, CancellationToken cancellationToken)
        {
            return ParseNumberNegativeInfinity(readType, await MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellationToken).ConfigureAwait(false));
        }

        private async Task ParseNumberAsync(ReadType readType, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            ShiftBufferIfNeeded();

            char firstChar = _chars[_charPos];
            int initialPosition = _charPos;

            await ReadNumberIntoBufferAsync(cancellationToken).ConfigureAwait(false);

            ParseReadNumber(readType, firstChar, initialPosition);
        }

        private Task ParseUndefinedAsync(CancellationToken cancellationToken)
        {
            return MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellationToken);
        }

        private async Task<bool> ParsePropertyAsync(CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

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

            if (PropertyNameTable != null)
            {
                propertyName = PropertyNameTable.Get(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length)
                    // no match in name table
                    ?? _stringReference.ToString();
            }
            else
            {
                propertyName = _stringReference.ToString();
            }

            await EatWhitespaceAsync(cancellationToken).ConfigureAwait(false);

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
            MiscellaneousUtils.Assert(_chars != null);

            int charPos = _charPos;

            while (true)
            {
                char currentChar = _chars[charPos];
                if (currentChar == '\0')
                {
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
                }
                else if (ReadNumberCharIntoBuffer(currentChar, charPos))
                {
                    return;
                }
                else
                {
                    charPos++;
                }
            }
        }

        private async Task ParseUnquotedPropertyAsync(CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(_chars != null);

            int initialPosition = _charPos;

            // parse unquoted property name until whitespace or colon
            while (true)
            {
                char currentChar = _chars[_charPos];
                if (currentChar == '\0')
                {
                    if (_charsUsed == _charPos)
                    {
                        if (await ReadDataAsync(true, cancellationToken).ConfigureAwait(false) == 0)
                        {
                            throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
                        }

                        continue;
                    }

                    _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);
                    return;
                }

                if (ReadUnquotedPropertyReportIfDone(currentChar, initialPosition))
                {
                    return;
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
            MiscellaneousUtils.Assert(_chars != null);

            if (await EnsureCharsAsync(1, true, cancellationToken).ConfigureAwait(false))
            {
                if (_chars[_charPos + 1] == 'u')
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
            MiscellaneousUtils.Assert(_chars != null);

            if (await EnsureCharsAsync(0, false, cancellationToken).ConfigureAwait(false))
            {
                await EatWhitespaceAsync(cancellationToken).ConfigureAwait(false);
                if (_isEndOfFile)
                {
                    SetToken(JsonToken.None);
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

        private async Task<object?> ReadStringValueAsync(ReadType readType, CancellationToken cancellationToken)
        {
            EnsureBuffer();
            MiscellaneousUtils.Assert(_chars != null);

            switch (_currentState)
            {
                case State.PostValue:
                    if (await ParsePostValueAsync(true, cancellationToken).ConfigureAwait(false))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
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
                                await ParseStringAsync(currentChar, readType, cancellationToken).ConfigureAwait(false);
                                return FinishReadQuotedStringValue(readType);
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

        private async Task<object?> ReadNumberValueAsync(ReadType readType, CancellationToken cancellationToken)
        {
            EnsureBuffer();
            MiscellaneousUtils.Assert(_chars != null);

            switch (_currentState)
            {
                case State.PostValue:
                    if (await ParsePostValueAsync(true, cancellationToken).ConfigureAwait(false))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
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
                                await ParseStringAsync(currentChar, readType, cancellationToken).ConfigureAwait(false);
                                return FinishReadQuotedNumber(readType);
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
        public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsBooleanAsync(cancellationToken) : base.ReadAsBooleanAsync(cancellationToken);
        }

        internal async Task<bool?> DoReadAsBooleanAsync(CancellationToken cancellationToken)
        {
            EnsureBuffer();
            MiscellaneousUtils.Assert(_chars != null);

            switch (_currentState)
            {
                case State.PostValue:
                    if (await ParsePostValueAsync(true, cancellationToken).ConfigureAwait(false))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
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
#if HAVE_BIG_INTEGER
                                if (Value is BigInteger i)
                                {
                                    b = i != 0;
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
                                if (!await MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancellationToken).ConfigureAwait(false))
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
        public override Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsBytesAsync(cancellationToken) : base.ReadAsBytesAsync(cancellationToken);
        }

        internal async Task<byte[]?> DoReadAsBytesAsync(CancellationToken cancellationToken)
        {
            EnsureBuffer();
            MiscellaneousUtils.Assert(_chars != null);

            bool isWrapped = false;

            switch (_currentState)
            {
                case State.PostValue:
                    if (await ParsePostValueAsync(true, cancellationToken).ConfigureAwait(false))
                    {
                        return null;
                    }
                    goto case State.Start;
                case State.Start:
                case State.Property:
                case State.Array:
                case State.ArrayStart:
                case State.Constructor:
                case State.ConstructorStart:
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
                                await ParseStringAsync(currentChar, ReadType.ReadAsBytes, cancellationToken).ConfigureAwait(false);
                                byte[]? data = (byte[]?)Value;
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

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsDateTimeAsync(cancellationToken) : base.ReadAsDateTimeAsync(cancellationToken);
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
        public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsDateTimeOffsetAsync(cancellationToken) : base.ReadAsDateTimeOffsetAsync(cancellationToken);
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
        public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsDecimalAsync(cancellationToken) : base.ReadAsDecimalAsync(cancellationToken);
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
        public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsDoubleAsync(cancellationToken) : base.ReadAsDoubleAsync(cancellationToken);
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
        public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsInt32Async(cancellationToken) : base.ReadAsInt32Async(cancellationToken);
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
        public override Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return _safeAsync ? DoReadAsStringAsync(cancellationToken) : base.ReadAsStringAsync(cancellationToken);
        }

        internal async Task<string?> DoReadAsStringAsync(CancellationToken cancellationToken)
        {
            return (string?)await ReadStringValueAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(false);
        }
    }
}

#endif
