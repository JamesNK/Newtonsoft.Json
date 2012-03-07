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
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  internal enum ReadType
  {
    Read,
    ReadAsInt32,
    ReadAsBytes,
    ReadAsString,
    ReadAsDecimal,
    ReadAsDateTime,
#if !NET20
    ReadAsDateTimeOffset
#endif
  }

  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to JSON text data.
  /// </summary>
  public class JsonTextReader : JsonReader, IJsonLineInfo
  {
    private readonly TextReader _reader;

    private char[] _chars;
    private int _charsUsed;
    private int _charPos;
    private int _lineStartPos;
    private int _lineNumber;
    private bool _isEndOfFile;
    private StringBuffer _buffer;
    private StringReference _stringReference;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReader"/> class with the specified <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <c>TextReader</c> containing the XML data to read.</param>
    public JsonTextReader(TextReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      _reader = reader;
      _lineNumber = 1;

      _chars = new char[4097];
    }

    internal void SetCharBuffer(char[] chars)
    {
      _chars = chars;
    }

    private StringBuffer GetBuffer()
    {
      if (_buffer == null)
      {
        _buffer = new StringBuffer(4096);
      }
      else
      {
        _buffer.Position = 0;
      }

      return _buffer;
    }

    private void OnNewLine(int pos)
    {
      _lineNumber++;
      _lineStartPos = pos - 1;
    }

    private void ParseString(char quote)
    {
      _charPos++;

      ShiftBufferIfNeeded();
      ReadStringIntoBuffer(quote);

      if (_readType == ReadType.ReadAsBytes)
      {
        byte[] data;
        if (_stringReference.Length == 0)
        {
          data = new byte[0];
        }
        else
        {
          data = Convert.FromBase64CharArray(_stringReference.Chars, _stringReference.StartIndex, _stringReference.Length);
        }

        SetToken(JsonToken.Bytes, data);
      }
      else if (_readType == ReadType.ReadAsString)
      {
        string text = _stringReference.ToString();

        SetToken(JsonToken.String, text);
        QuoteChar = quote;
      }
      else
      {
        string text = _stringReference.ToString();

        if (text.Length > 0)
        {
          if (text[0] == '/')
          {
            if (text.StartsWith("/Date(", StringComparison.Ordinal) && text.EndsWith(")/", StringComparison.Ordinal))
            {
              ParseDateMicrosoft(text);
              return;
            }
          }
          else if (char.IsDigit(text[0]) && text.Length >= 19 && text.Length <= 40)
          {
            if (ParseDateIso(text))
              return;
          }
        }

        SetToken(JsonToken.String, text);
        QuoteChar = quote;
      }
    }

    private bool ParseDateIso(string text)
    {
      const string isoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

#if !NET20
      if (_readType == ReadType.ReadAsDateTimeOffset)
      {
        DateTimeOffset dateTimeOffset;
        if (DateTimeOffset.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
        {
          SetToken(JsonToken.Date, dateTimeOffset);
          return true;
        }
      }
      else
#endif
      {
        DateTime dateTime;
        if (DateTime.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
        {
          dateTime = JsonConvert.EnsureDateTime(dateTime, DateTimeZoneHandling);

          SetToken(JsonToken.Date, dateTime);
          return true;
        }
      }

      return false;
    }

    private void ParseDateMicrosoft(string text)
    {
      string value = text.Substring(6, text.Length - 8);
      DateTimeKind kind = DateTimeKind.Utc;

      int index = value.IndexOf('+', 1);

      if (index == -1)
        index = value.IndexOf('-', 1);

      TimeSpan offset = TimeSpan.Zero;

      if (index != -1)
      {
        kind = DateTimeKind.Local;
        offset = ReadOffset(value.Substring(index));
        value = value.Substring(0, index);
      }

      long javaScriptTicks = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

      DateTime utcDateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);

#if !NET20
      if (_readType == ReadType.ReadAsDateTimeOffset)
      {
        SetToken(JsonToken.Date, new DateTimeOffset(utcDateTime.Add(offset).Ticks, offset));
      }
      else
#endif
      {
        DateTime dateTime;

        switch (kind)
        {
          case DateTimeKind.Unspecified:
            dateTime = DateTime.SpecifyKind(utcDateTime.ToLocalTime(), DateTimeKind.Unspecified);
            break;
          case DateTimeKind.Local:
            dateTime = utcDateTime.ToLocalTime();
            break;
          default:
            dateTime = utcDateTime;
            break;
        }

        dateTime = JsonConvert.EnsureDateTime(dateTime, DateTimeZoneHandling);

        SetToken(JsonToken.Date, dateTime);
      }
    }

    private static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
    {
      const int charByteCount = 2;

      Buffer.BlockCopy(src, srcOffset * charByteCount, dst, dstOffset * charByteCount, count * charByteCount);
    }

    private void ShiftBufferIfNeeded()
    {
      // once in the last 10% of the buffer shift the remainling content to the start to avoid
      // unnessesarly increasing the buffer size when reading numbers/strings
      int length = _chars.Length;
      if (length - _charPos <= length * 0.1)
      {
        int count = _charsUsed - _charPos;
        if (count > 0)
          BlockCopyChars(_chars, _charPos, _chars, 0, count);

        _lineStartPos -= _charPos;
        _charPos = 0;
        _charsUsed = count;
        _chars[_charsUsed] = '\0';
      }
    }

    private int ReadData(bool append)
    {
      return ReadData(append, 0);
    }

    private int ReadData(bool append, int charsRequired)
    {
      if (_isEndOfFile)
        return 0;

      // char buffer is full
      if (_charsUsed + charsRequired >= _chars.Length - 1)
      {
        if (append)
        {
          // copy to new array either double the size of the current or big enough to fit required content
          int newArrayLength = Math.Max(_chars.Length * 2, _charsUsed + charsRequired + 1);

          // increase the size of the buffer
          char[] dst = new char[newArrayLength];

          BlockCopyChars(_chars, 0, dst, 0, _chars.Length);

          _chars = dst;
        }
        else
        {
          int remainingCharCount = _charsUsed - _charPos;

          if (remainingCharCount + charsRequired + 1 >= _chars.Length)
          {
            // the remaining count plus the required is bigger than the current buffer size
            char[] dst = new char[remainingCharCount + charsRequired + 1];

            if (remainingCharCount > 0)
              BlockCopyChars(_chars, _charPos, dst, 0, remainingCharCount);

            _chars = dst;
          }
          else
          {
            // copy any remaining data to the beginning of the buffer if needed and reset positions
            if (remainingCharCount > 0)
              BlockCopyChars(_chars, _charPos, _chars, 0, remainingCharCount);
          }

          _lineStartPos -= _charPos;
          _charPos = 0;
          _charsUsed = remainingCharCount;
        }
      }

      int attemptCharReadCount = _chars.Length - _charsUsed - 1;

      int charsRead = _reader.Read(_chars, _charsUsed, attemptCharReadCount);
      _charsUsed += charsRead;

      if (charsRead == 0)
        _isEndOfFile = true;

      _chars[_charsUsed] = '\0';
      return charsRead;
    }

    private bool EnsureChars(int relativePosition, bool append)
    {
      if (_charPos + relativePosition >= _charsUsed)
        return ReadChars(relativePosition, append);

      return true;
    }

    private bool ReadChars(int relativePosition, bool append)
    {
      if (_isEndOfFile)
        return false;

      int charsRequired = _charPos + relativePosition - _charsUsed + 1;

      int charsRead = ReadData(append, charsRequired);

      if (charsRead < charsRequired)
        return false;
      return true;
    }

    private static TimeSpan ReadOffset(string offsetText)
    {
      bool negative = (offsetText[0] == '-');

      int hours = int.Parse(offsetText.Substring(1, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);
      int minutes = 0;
      if (offsetText.Length >= 5)
        minutes = int.Parse(offsetText.Substring(3, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);

      TimeSpan offset = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
      if (negative)
        offset = offset.Negate();

      return offset;
    }

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>
    /// true if the next token was read successfully; false if there are no more tokens to read.
    /// </returns>
    [DebuggerStepThrough]
    public override bool Read()
    {
      _readType = ReadType.Read;
      if (!ReadInternal())
      {
        SetToken(JsonToken.None);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null. This method will return <c>null</c> at the end of an array.
    /// </returns>
    public override byte[] ReadAsBytes()
    {
      return ReadAsBytesInternal();
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Decimal}"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override decimal? ReadAsDecimal()
    {
      return ReadAsDecimalInternal();
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Int32}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Int32}"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override int? ReadAsInt32()
    {
      return ReadAsInt32Internal();
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="String"/>.
    /// </summary>
    /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override string ReadAsString()
    {
      return ReadAsStringInternal();
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTime}"/>.
    /// </summary>
    /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override DateTime? ReadAsDateTime()
    {
      return ReadAsDateTimeInternal();
    }

#if !NET20
    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/>. This method will return <c>null</c> at the end of an array.</returns>
    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
      return ReadAsDateTimeOffsetInternal();
    }
#endif

    internal override bool ReadInternal()
    {
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
            return ParseValue();
          case State.Complete:
            break;
          case State.Object:
          case State.ObjectStart:
            return ParseObject();
          case State.PostValue:
            // returns true if it hits
            // end of object or array
            if (ParsePostValue())
              return true;
            break;
          case State.Finished:
            if (EnsureChars(0, false))
            {
              EatWhitespace(false);
              if (_isEndOfFile)
              {
                return false;
              }
              if (_chars[_charPos] == '/')
              {
                ParseComment();
                return true;
              }
              else
              {
                throw CreateReaderException(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
              }
            }
            return false;
          case State.Closed:
            break;
          case State.Error:
            break;
          default:
            throw CreateReaderException(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, CurrentState));
        }
      }
    }

    private void ReadStringIntoBuffer(char quote)
    {
      int charPos = _charPos;
      int initialPosition = _charPos;
      int lastWritePosition = _charPos;
      StringBuffer buffer = null;

      while (true)
      {
        switch (_chars[charPos++])
        {
          case '\0':
            if (_charsUsed == charPos - 1)
            {
              charPos--;

              if (ReadData(true) == 0)
              {
                _charPos = charPos;
                throw CreateReaderException(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
              }
            }
            break;
          case '\\':
            _charPos = charPos;
            if (!EnsureChars(0, true))
            {
              _charPos = charPos;
              throw CreateReaderException(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
            }

            // start of escape sequence
            int escapeStartPos = charPos - 1;

            char currentChar = _chars[charPos];

            char writeChar;

            switch (currentChar)
            {
              case 'b':
                charPos++;
                writeChar = '\b';
                break;
              case 't':
                charPos++;
                writeChar = '\t';
                break;
              case 'n':
                charPos++;
                writeChar = '\n';
                break;
              case 'f':
                charPos++;
                writeChar = '\f';
                break;
              case 'r':
                charPos++;
                writeChar = '\r';
                break;
              case '\\':
                charPos++;
                writeChar = '\\';
                break;
              case '"':
              case '\'':
              case '/':
                writeChar = currentChar;
                charPos++;
                break;
              case 'u':
                charPos++;
                _charPos = charPos;
                if (EnsureChars(4, true))
                {
                  string hexValues = new string(_chars, charPos, 4);
                  char hexChar = Convert.ToChar(int.Parse(hexValues, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
                  writeChar = hexChar;

                  charPos += 4;
                }
                else
                {
                  _charPos = charPos;
                  throw CreateReaderException(this, "Unexpected end while parsing unicode character.");
                }
                break;
              default:
                charPos++;
                _charPos = charPos;
                throw CreateReaderException(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, @"\" + currentChar));
            }

            if (buffer == null)
              buffer = GetBuffer();

            if (escapeStartPos > lastWritePosition)
            {
              buffer.Append(_chars, lastWritePosition, escapeStartPos - lastWritePosition);
            }

            buffer.Append(writeChar);

            lastWritePosition = charPos;
            break;
          case StringUtils.CarriageReturn:
            _charPos = charPos - 1;
            ProcessCarriageReturn(true);
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
                if (buffer == null)
                  buffer = GetBuffer();

                if (charPos > lastWritePosition)
                  buffer.Append(_chars, lastWritePosition, charPos - lastWritePosition);

                _stringReference = new StringReference(buffer.GetInternalBuffer(), 0, buffer.Position);
              }

              charPos++;
              _charPos = charPos;
              return;
            }
            break;
        }
      }
    }

    private void ReadNumberIntoBuffer()
    {
      int charPos = _charPos;

      while (true)
      {
        switch (_chars[charPos++])
        {
          case '\0':
            if (_charsUsed == charPos - 1)
            {
              charPos--;
              _charPos = charPos;
              if (ReadData(true) == 0)
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
            break;
          default:
            _charPos = charPos - 1;
            return;
        }
      }
    }

    private void ClearRecentString()
    {
      if (_buffer != null)
        _buffer.Position = 0;

      _stringReference = new StringReference();
    }

    private bool ParsePostValue()
    {
      while (true)
      {
        char currentChar = _chars[_charPos];

        switch (currentChar)
        {
          case '\0':
            if (_charsUsed == _charPos)
            {
              if (ReadData(false) == 0)
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
            ParseComment();
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
            ProcessCarriageReturn(false);
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
              throw CreateReaderException(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
            }
            break;
        }
      }
    }

    private bool ParseObject()
    {
      while (true)
      {
        char currentChar = _chars[_charPos];

        switch (currentChar)
        {
          case '\0':
            if (_charsUsed == _charPos)
            {
              if (ReadData(false) == 0)
                return false;
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
            ParseComment();
            return true;
          case StringUtils.CarriageReturn:
            ProcessCarriageReturn(false);
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
              return ParseProperty();
            }
            break;
        }
      }
    }

    private bool ParseProperty()
    {
      char firstChar = _chars[_charPos];
      char quoteChar;

      if (firstChar == '"' || firstChar == '\'')
      {
        _charPos++;
        quoteChar = firstChar;
        ShiftBufferIfNeeded();
        ReadStringIntoBuffer(quoteChar);
      }
      else if (ValidIdentifierChar(firstChar))
      {
        quoteChar = '\0';
        ShiftBufferIfNeeded();
        ParseUnquotedProperty();
      }
      else
      {
        throw CreateReaderException(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
      }

      string propertyName = _stringReference.ToString();

      EatWhitespace(false);

      if (_chars[_charPos] != ':')
        throw CreateReaderException(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));

      _charPos++;

      SetToken(JsonToken.PropertyName, propertyName);
      QuoteChar = quoteChar;
      ClearRecentString();

      return true;
    }

    private bool ValidIdentifierChar(char value)
    {
      return (char.IsLetterOrDigit(value) || value == '_' || value == '$');
    }

    private void ParseUnquotedProperty()
    {
      int initialPosition = _charPos;

      // parse unquoted property name until whitespace or colon
      while (true)
      {
        switch (_chars[_charPos])
        {
          case '\0':
            if (_charsUsed == _charPos)
            {
              if (ReadData(true) == 0)
                throw CreateReaderException(this, "Unexpected end while parsing unquoted property name.");

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

            throw CreateReaderException(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
        }
      }
    }

    private bool ParseValue()
    {
      while (true)
      {
        char currentChar = _chars[_charPos];

        switch (currentChar)
        {
          case '\0':
            if (_charsUsed == _charPos)
            {
              if (ReadData(false) == 0)
                return false;
            }
            else
            {
              _charPos++;
            }
            break;
          case '"':
          case '\'':
            ParseString(currentChar);
            return true;
          case 't':
            ParseTrue();
            return true;
          case 'f':
            ParseFalse();
            return true;
          case 'n':
            if (EnsureChars(1, true))
            {
              char next = _chars[_charPos + 1];

              if (next == 'u')
                ParseNull();
              else if (next == 'e')
                ParseConstructor();
              else
                throw CreateReaderException(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
            }
            else
            {
              throw CreateReaderException(this, "Unexpected end.");
            }
            return true;
          case 'N':
            ParseNumberNaN();
            return true;
          case 'I':
            ParseNumberPositiveInfinity();
            return true;
          case '-':
            if (EnsureChars(1, true) && _chars[_charPos + 1] == 'I')
              ParseNumberNegativeInfinity();
            else
              ParseNumber();
            return true;
          case '/':
            ParseComment();
            return true;
          case 'u':
            ParseUndefined();
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
            ProcessCarriageReturn(false);
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
            else if (char.IsNumber(currentChar) || currentChar == '-' || currentChar == '.')
            {
              ParseNumber();
              return true;
            }
            else
            {
              throw CreateReaderException(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
            }
        }
      }
    }

    private void ProcessLineFeed()
    {
      _charPos++;
      OnNewLine(_charPos);
    }

    private void ProcessCarriageReturn(bool append)
    {
      _charPos++;

      if (EnsureChars(1, append) && _chars[_charPos] == StringUtils.LineFeed)
        _charPos++;

      OnNewLine(_charPos);
    }

    private bool EatWhitespace(bool oneOrMore)
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
              if (ReadData(false) == 0)
                finished = true;
            }
            else
            {
              _charPos++;
            }
            break;
          case StringUtils.CarriageReturn:
            ProcessCarriageReturn(false);
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

      return (!oneOrMore || ateWhitespace);
    }

    private void ParseConstructor()
    {
      if (MatchValueWithTrailingSeperator("new"))
      {
        EatWhitespace(false);

        int initialPosition = _charPos;
        int endPosition;

        while (true)
        {
          char currentChar = _chars[_charPos];
          if (currentChar == '\0')
          {
            if (_charsUsed == _charPos)
            {
              if (ReadData(true) == 0)
                throw CreateReaderException(this, "Unexpected end while parsing constructor.");
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
            ProcessCarriageReturn(true);
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
            throw CreateReaderException(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
          }
        }

        _stringReference = new StringReference(_chars, initialPosition, endPosition - initialPosition);
        string constructorName = _stringReference.ToString();

        EatWhitespace(false);

        if (_chars[_charPos] != '(')
          throw CreateReaderException(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));

        _charPos++;

        ClearRecentString();

        SetToken(JsonToken.StartConstructor, constructorName);
      }
    }

    private void ParseNumber()
    {
      ShiftBufferIfNeeded();

      char firstChar = _chars[_charPos];
      int initialPosition = _charPos;

      ReadNumberIntoBuffer();

      _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition);

      object numberValue;
      JsonToken numberType;

      bool singleDigit = (char.IsDigit(firstChar) && _stringReference.Length == 1);
      bool nonBase10 = (firstChar == '0' && _stringReference.Length > 1
        && _stringReference.Chars[_stringReference.StartIndex + 1] != '.'
        && _stringReference.Chars[_stringReference.StartIndex + 1] != 'e'
        && _stringReference.Chars[_stringReference.StartIndex + 1] != 'E');

      if (_readType == ReadType.ReadAsInt32)
      {
        if (singleDigit)
        {
          // digit char values start at 48
          numberValue = firstChar - 48;
        }
        else if (nonBase10)
        {
          string number = _stringReference.ToString();

          // decimal.Parse doesn't support parsing hexadecimal values
          int integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                           ? Convert.ToInt32(number, 16)
                           : Convert.ToInt32(number, 8);

          numberValue = integer;
        }
        else
        {
          string number = _stringReference.ToString();

          numberValue = Convert.ToInt32(number, CultureInfo.InvariantCulture);
        }

        numberType = JsonToken.Integer;
      }
      else if (_readType == ReadType.ReadAsDecimal)
      {
        if (singleDigit)
        {
          // digit char values start at 48
          numberValue = (decimal)firstChar - 48;
        }
        else if (nonBase10)
        {
          string number = _stringReference.ToString();

          // decimal.Parse doesn't support parsing hexadecimal values
          long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                           ? Convert.ToInt64(number, 16)
                           : Convert.ToInt64(number, 8);

          numberValue = Convert.ToDecimal(integer);
        }
        else
        {
          string number = _stringReference.ToString();

          numberValue = decimal.Parse(number, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
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

          numberValue = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                          ? Convert.ToInt64(number, 16)
                          : Convert.ToInt64(number, 8);
          numberType = JsonToken.Integer;
        }
        else
        {
          string number = _stringReference.ToString();

          // it's faster to do 3 indexof with single characters than an indexofany
          if (number.IndexOf('.') != -1 || number.IndexOf('E') != -1 || number.IndexOf('e') != -1)
          {
            numberValue = Convert.ToDouble(number, CultureInfo.InvariantCulture);
            numberType = JsonToken.Float;
          }
          else
          {
            try
            {
              numberValue = Convert.ToInt64(number, CultureInfo.InvariantCulture);
            }
            catch (OverflowException ex)
            {
              throw new JsonReaderException("JSON integer {0} is too large or small for an Int64.".FormatWith(CultureInfo.InvariantCulture, number), ex);
            }

            numberType = JsonToken.Integer;
          }
        }
      }

      ClearRecentString();

      SetToken(numberType, numberValue);
    }

    private void ParseComment()
    {
      // should have already parsed / character before reaching this method
      _charPos++;

      if (!EnsureChars(1, false) || _chars[_charPos] != '*')
        throw CreateReaderException(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, _chars[_charPos]));
      else
        _charPos++;

      int initialPosition = _charPos;

      bool commentFinished = false;

      while (!commentFinished)
      {
        switch (_chars[_charPos])
        {
          case '\0':
            if (_charsUsed == _charPos)
            {
              if (ReadData(true) == 0)
                throw CreateReaderException(this, "Unexpected end while parsing comment.");
            }
            else
            {
              _charPos++;
            }
            break;
          case '*':
            _charPos++;

            if (EnsureChars(0, true))
            {
              if (_chars[_charPos] == '/')
              {
                _stringReference = new StringReference(_chars, initialPosition, _charPos - initialPosition - 1);

                _charPos++;
                commentFinished = true;
              }
            }
            break;
          case StringUtils.CarriageReturn:
            ProcessCarriageReturn(true);
            break;
          case StringUtils.LineFeed:
            ProcessLineFeed();
            break;
          default:
            _charPos++;
            break;
        }
      }

      SetToken(JsonToken.Comment, _stringReference.ToString());

      ClearRecentString();
    }

    private bool MatchValue(string value)
    {
      if (!EnsureChars(value.Length - 1, true))
        return false;

      for (int i = 0; i < value.Length; i++)
      {
        if (_chars[_charPos + i] != value[i])
        {
          return false;
        }
      }

      _charPos += value.Length;

      return true;
    }

    private bool MatchValueWithTrailingSeperator(string value)
    {
      // will match value and then move to the next character, checking that it is a seperator character
      bool match = MatchValue(value);

      if (!match)
        return false;

      if (!EnsureChars(0, false))
        return true;

      return IsSeperator(_chars[_charPos]) || _chars[_charPos] == '\0';
    }

    private bool IsSeperator(char c)
    {
      switch (c)
      {
        case '}':
        case ']':
        case ',':
          return true;
        case '/':
          // check next character to see if start of a comment
          if (!EnsureChars(1, false))
            return false;

          return (_chars[_charPos + 1] == '*');
        case ')':
          if (CurrentState == State.Constructor || CurrentState == State.ConstructorStart)
            return true;
          break;
        case ' ':
        case StringUtils.Tab:
        case StringUtils.LineFeed:
        case StringUtils.CarriageReturn:
          return true;
        default:
          if (char.IsWhiteSpace(c))
            return true;
          break;
      }

      return false;
    }

    private void ParseTrue()
    {
      // check characters equal 'true'
      // and that it is followed by either a seperator character
      // or the text ends
      if (MatchValueWithTrailingSeperator(JsonConvert.True))
      {
        SetToken(JsonToken.Boolean, true);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing boolean value.");
      }
    }

    private void ParseNull()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.Null))
      {
        SetToken(JsonToken.Null);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing null value.");
      }
    }

    private void ParseUndefined()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.Undefined))
      {
        SetToken(JsonToken.Undefined);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing undefined value.");
      }
    }

    private void ParseFalse()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.False))
      {
        SetToken(JsonToken.Boolean, false);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing boolean value.");
      }
    }

    private void ParseNumberNegativeInfinity()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.NegativeInfinity))
      {
        SetToken(JsonToken.Float, double.NegativeInfinity);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing negative infinity value.");
      }
    }

    private void ParseNumberPositiveInfinity()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.PositiveInfinity))
      {
        SetToken(JsonToken.Float, double.PositiveInfinity);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing positive infinity value.");
      }
    }

    private void ParseNumberNaN()
    {
      if (MatchValueWithTrailingSeperator(JsonConvert.NaN))
      {
        SetToken(JsonToken.Float, double.NaN);
      }
      else
      {
        throw CreateReaderException(this, "Error parsing NaN value.");
      }
    }

    /// <summary>
    /// Changes the state to closed. 
    /// </summary>
    public override void Close()
    {
      base.Close();

      if (CloseInput && _reader != null)
        _reader.Close();

      if (_buffer != null)
        _buffer.Clear();
    }

    /// <summary>
    /// Gets a value indicating whether the class can return line information.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if LineNumber and LinePosition can be provided; otherwise, <c>false</c>.
    /// </returns>
    public bool HasLineInfo()
    {
      return true;
    }

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    /// <value>
    /// The current line number or 0 if no line information is available (for example, HasLineInfo returns false).
    /// </value>
    public int LineNumber
    {
      get
      {
        if (CurrentState == State.Start && LinePosition == 0)
          return 0;

        return _lineNumber;
      }
    }

    /// <summary>
    /// Gets the current line position.
    /// </summary>
    /// <value>
    /// The current line position or 0 if no line information is available (for example, HasLineInfo returns false).
    /// </value>
    public int LinePosition
    {
      get { return _charPos - _lineStartPos; }
    }
  }
}