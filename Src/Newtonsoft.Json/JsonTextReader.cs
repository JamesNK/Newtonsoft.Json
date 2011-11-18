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
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public class JsonTextReader : JsonReader, IJsonLineInfo
  {
    private enum ReadType
    {
      Read,
      ReadAsBytes,
      ReadAsDecimal,
#if !NET20
      ReadAsDateTimeOffset
#endif
    }

    private readonly TextReader _reader;
    private readonly StringBuffer _buffer;
    private char? _lastChar;
    private int _currentLinePosition;
    private int _currentLineNumber;
    private bool _end;
    private ReadType _readType;
    private CultureInfo _culture;

    /// <summary>
    /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public CultureInfo Culture
    {
      get { return _culture ?? CultureInfo.InvariantCulture; }
      set { _culture = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReader"/> class with the specified <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <c>TextReader</c> containing the XML data to read.</param>
    public JsonTextReader(TextReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      _reader = reader;
      _buffer = new StringBuffer(4096);
      _currentLineNumber = 1;
    }

    private void ParseString(char quote)
    {
      ReadStringIntoBuffer(quote);

      if (_readType == ReadType.ReadAsBytes)
      {
        byte[] data;
        if (_buffer.Position == 0)
        {
          data = new byte[0];
        }
        else
        {
          data = Convert.FromBase64CharArray(_buffer.GetInternalBuffer(), 0, _buffer.Position);
          _buffer.Position = 0;
        }

        SetToken(JsonToken.Bytes, data);
      }
      else
      {
        string text = _buffer.ToString();
        _buffer.Position = 0;

        if (text.StartsWith("/Date(", StringComparison.Ordinal) && text.EndsWith(")/", StringComparison.Ordinal))
        {
          ParseDate(text);
        }
        else
        {
          SetToken(JsonToken.String, text);
          QuoteChar = quote;
        } 
      }
    }

    private void ReadStringIntoBuffer(char quote)
    {
      while (true)
      {
        char currentChar = MoveNext();

        switch (currentChar)
        {
          case '\0':
            if (_end)
              throw CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", quote, _currentLineNumber, _currentLinePosition);

            _buffer.Append('\0');
            break;
          case '\\':
            if ((currentChar = MoveNext()) != '\0' || !_end)
            {
              switch (currentChar)
              {
                case 'b':
                  _buffer.Append('\b');
                  break;
                case 't':
                  _buffer.Append('\t');
                  break;
                case 'n':
                  _buffer.Append('\n');
                  break;
                case 'f':
                  _buffer.Append('\f');
                  break;
                case 'r':
                  _buffer.Append('\r');
                  break;
                case '\\':
                  _buffer.Append('\\');
                  break;
                case '"':
                case '\'':
                case '/':
                  _buffer.Append(currentChar);
                  break;
                case 'u':
                  char[] hexValues = new char[4];
                  for (int i = 0; i < hexValues.Length; i++)
                  {
                    if ((currentChar = MoveNext()) != '\0' || !_end)
                      hexValues[i] = currentChar;
                    else
                      throw CreateJsonReaderException("Unexpected end while parsing unicode character. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
                  }

                  char hexChar = Convert.ToChar(int.Parse(new string(hexValues), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));
                  _buffer.Append(hexChar);
                  break;
                default:
                  throw CreateJsonReaderException("Bad JSON escape sequence: {0}. Line {1}, position {2}.", @"\" + currentChar, _currentLineNumber, _currentLinePosition);
              }
            }
            else
            {
              throw CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", quote, _currentLineNumber, _currentLinePosition);
            }
            break;
          case '"':
          case '\'':
            if (currentChar == quote)
            {
              return;
            }
            else
            {
              _buffer.Append(currentChar);
            }
            break;
          default:
            _buffer.Append(currentChar);
            break;
        }
      }
    }

    private JsonReaderException CreateJsonReaderException(string format, params object[] args)
    {
      string message = format.FormatWith(CultureInfo.InvariantCulture, args);
      return new JsonReaderException(message, null, _currentLineNumber, _currentLinePosition);
    }

    private TimeSpan ReadOffset(string offsetText)
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

    private void ParseDate(string text)
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

        SetToken(JsonToken.Date, dateTime);
      }
    }

    private const int LineFeedValue = StringUtils.LineFeed;
    private const int CarriageReturnValue = StringUtils.CarriageReturn;

    private char MoveNext()
    {
      int value = _reader.Read();

      switch (value)
      {
        case -1:
          _end = true;
          return '\0';
        case CarriageReturnValue:
          if (_reader.Peek() == LineFeedValue)
            _reader.Read();

          _currentLineNumber++;
          _currentLinePosition = 0;
          break;
        case LineFeedValue:
          _currentLineNumber++;
          _currentLinePosition = 0;
          break;
        default:
          _currentLinePosition++;
          break;
      }

      return (char)value;
    }

    private bool HasNext()
    {
      return (_reader.Peek() != -1);
    }

    private int PeekNext()
    {
      return _reader.Peek();
    }

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>
    /// true if the next token was read successfully; false if there are no more tokens to read.
    /// </returns>
    public override bool Read()
    {
      _readType = ReadType.Read;
      return ReadInternal();
    }

    private bool IsWrappedInTypeObject()
    {
      _readType = ReadType.Read;

      if (TokenType == JsonToken.StartObject)
      {
        int startObjectLineNumber = _currentLineNumber;
        int startObjectLinePosition = _currentLinePosition;

        ReadInternal();
        if (Value.ToString() == "$type")
        {
          ReadInternal();
          if (Value != null && Value.ToString().StartsWith("System.Byte[]"))
          {
            ReadInternal();
            if (Value.ToString() == "$value")
            {
              return true;
            }
          }
        }

        throw CreateJsonReaderException("Unexpected token when reading bytes: {0}. Line {1}, position {2}.", JsonToken.StartObject, startObjectLineNumber, startObjectLinePosition);
      }

      return false;
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null.
    /// </returns>
    public override byte[] ReadAsBytes()
    {
      _readType = ReadType.ReadAsBytes;

      do
      {
        if (!ReadInternal())
          throw CreateJsonReaderException("Unexpected end when reading bytes: Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      } while (TokenType == JsonToken.Comment);

      if (IsWrappedInTypeObject())
      {
        byte[] data = ReadAsBytes();
        ReadInternal();
        SetToken(JsonToken.Bytes, data);
        return data;
      }

      if (TokenType == JsonToken.Null)
        return null;
      if (TokenType == JsonToken.Bytes)
        return (byte[]) Value;
      if (TokenType == JsonToken.StartArray)
      {
        List<byte> data = new List<byte>();

        while (ReadInternal())
        {
          switch (TokenType)
          {
            case JsonToken.Integer:
              data.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
              break;
            case JsonToken.EndArray:
              byte[] d = data.ToArray();
              SetToken(JsonToken.Bytes, d);
              return d;
            case JsonToken.Comment:
              // skip
              break;
            default:
              throw CreateJsonReaderException("Unexpected token when reading bytes: {0}. Line {1}, position {2}.", TokenType, _currentLineNumber, _currentLinePosition);
          }
        }

        throw CreateJsonReaderException("Unexpected end when reading bytes: Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }


      throw CreateJsonReaderException("Unexpected token when reading bytes: {0}. Line {1}, position {2}.", TokenType, _currentLineNumber, _currentLinePosition);
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Decimal}"/>.</returns>
    public override decimal? ReadAsDecimal()
    {
      _readType = ReadType.ReadAsDecimal;
      
      do
      {
        if (!ReadInternal())
          throw CreateJsonReaderException("Unexpected end when reading decimal: Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      } while (TokenType == JsonToken.Comment);
 
      if (TokenType == JsonToken.Null)
        return null;
      if (TokenType == JsonToken.Float)
        return (decimal?)Value;

      decimal d;
      if (TokenType == JsonToken.String && decimal.TryParse((string)Value, NumberStyles.Number, Culture, out d))
      {
        SetToken(JsonToken.Float, d);
        return d;
      }

      throw CreateJsonReaderException("Unexpected token when reading decimal: {0}. Line {1}, position {2}.", TokenType, _currentLineNumber, _currentLinePosition);
    }

#if !NET20
    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/>.</returns>
    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
      _readType = ReadType.ReadAsDateTimeOffset;

      do
      {
        if (!ReadInternal())
          throw CreateJsonReaderException("Unexpected end when reading date: Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      } while (TokenType == JsonToken.Comment);

      if (TokenType == JsonToken.Null)
        return null;
      if (TokenType == JsonToken.Date)
        return (DateTimeOffset)Value;

      DateTimeOffset dt;
      if (TokenType == JsonToken.String && DateTimeOffset.TryParse((string)Value, Culture, DateTimeStyles.None, out dt))
      {
        SetToken(JsonToken.Date, dt);
        return dt;
      }

      throw CreateJsonReaderException("Unexpected token when reading date: {0}. Line {1}, position {2}.", TokenType, _currentLineNumber, _currentLinePosition);
    }
#endif

    private bool ReadInternal()
    {
      while (true)
      {
        char currentChar;
        if (_lastChar != null)
        {
          currentChar = _lastChar.Value;
          _lastChar = null;
        }
        else
        {
          currentChar = MoveNext();
        }

        if (currentChar == '\0' && _end)
          return false;

        switch (CurrentState)
        {
          case State.Start:
          case State.Property:
          case State.Array:
          case State.ArrayStart:
          case State.Constructor:
          case State.ConstructorStart:
            return ParseValue(currentChar);
          case State.Complete:
            break;
          case State.Object:
          case State.ObjectStart:
            return ParseObject(currentChar);
          case State.PostValue:
            // returns true if it hits
            // end of object or array
            if (ParsePostValue(currentChar))
              return true;
            break;
          case State.Closed:
            break;
          case State.Error:
            break;
          default:
            throw CreateJsonReaderException("Unexpected state: {0}. Line {1}, position {2}.", CurrentState, _currentLineNumber, _currentLinePosition);
        }
      }
    }

    private bool ParsePostValue(char currentChar)
    {
      do
      {
        switch (currentChar)
        {
          case '}':
            SetToken(JsonToken.EndObject);
            return true;
          case ']':
            SetToken(JsonToken.EndArray);
            return true;
          case ')':
            SetToken(JsonToken.EndConstructor);
            return true;
          case '/':
            ParseComment();
            return true;
          case ',':
            // finished parsing
            SetStateBasedOnCurrent();
            return false;
          case ' ':
          case StringUtils.Tab:
          case StringUtils.LineFeed:
          case StringUtils.CarriageReturn:
            // eat
            break;
          default:
            if (char.IsWhiteSpace(currentChar))
            {
              // eat
            }
            else
            {
              throw CreateJsonReaderException("After parsing a value an unexpected character was encountered: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
            }
            break;
        }
      } while ((currentChar = MoveNext()) != '\0' || !_end);

      return false;
    }

    private bool ParseObject(char currentChar)
    {
      do
      {
        switch (currentChar)
        {
          case '}':
            SetToken(JsonToken.EndObject);
            return true;
          case '/':
            ParseComment();
            return true;
          case ' ':
          case StringUtils.Tab:
          case StringUtils.LineFeed:
          case StringUtils.CarriageReturn:
            // eat
            break;
          default:
            if (char.IsWhiteSpace(currentChar))
            {
              // eat
            }
            else
            {
              return ParseProperty(currentChar);
            }
            break;
        }
      } while ((currentChar = MoveNext()) != '\0' || !_end);

      return false;
    }

    private bool ParseProperty(char firstChar)
    {
      char currentChar = firstChar;
      char quoteChar;

      if (ValidIdentifierChar(currentChar))
      {
        quoteChar = '\0';
        currentChar = ParseUnquotedProperty(currentChar);
      }
      else if (currentChar == '"' || currentChar == '\'')
      {
        quoteChar = currentChar;
        ReadStringIntoBuffer(quoteChar);
        currentChar = MoveNext();
      }
      else
      {
        throw CreateJsonReaderException("Invalid property identifier character: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
      }

      if (currentChar != ':')
      {
        currentChar = MoveNext();

        // finished property. skip any whitespace and move to colon
        EatWhitespace(currentChar, false, out currentChar);

        if (currentChar != ':')
          throw CreateJsonReaderException("Invalid character after parsing property name. Expected ':' but got: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
      }

      SetToken(JsonToken.PropertyName, _buffer.ToString());
      QuoteChar = quoteChar;
      _buffer.Position = 0;

      return true;
    }

    private bool ValidIdentifierChar(char value)
    {
      return (char.IsLetterOrDigit(value) || value == '_' || value == '$');
    }

    private char ParseUnquotedProperty(char firstChar)
    {
      // parse unquoted property name until whitespace or colon
      _buffer.Append(firstChar);

      char currentChar;

      while ((currentChar = MoveNext()) != '\0' || !_end)
      {
        if (char.IsWhiteSpace(currentChar) || currentChar == ':')
        {
          return currentChar;
        }
        else if (ValidIdentifierChar(currentChar))
        {
          _buffer.Append(currentChar);
        }
        else
        {
          throw CreateJsonReaderException("Invalid JavaScript property identifier character: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
        }
      }

      throw CreateJsonReaderException("Unexpected end when parsing unquoted property name. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
    }

    private bool ParseValue(char currentChar)
    {
      do
      {
        switch (currentChar)
        {
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
            if (HasNext())
            {
              char next = (char)PeekNext();

              if (next == 'u')
                ParseNull();
              else if (next == 'e')
                ParseConstructor();
              else
                throw CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
            }
            else
            {
              throw CreateJsonReaderException("Unexpected end. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
            }
            return true;
          case 'N':
            ParseNumberNaN();
            return true;
          case 'I':
            ParseNumberPositiveInfinity();
            return true;
          case '-':
            if (PeekNext() == 'I')
              ParseNumberNegativeInfinity();
            else
              ParseNumber(currentChar);
            return true;
          case '/':
            ParseComment();
            return true;
          case 'u':
            ParseUndefined();
            return true;
          case '{':
            SetToken(JsonToken.StartObject);
            return true;
          case '[':
            SetToken(JsonToken.StartArray);
            return true;
          case '}':
            SetToken(JsonToken.EndObject);
            return true;
          case ']':
            SetToken(JsonToken.EndArray);
            return true;
          case ',':
            SetToken(JsonToken.Undefined);
            return true;
          case ')':
            SetToken(JsonToken.EndConstructor);
            return true;
          case ' ':
          case StringUtils.Tab:
          case StringUtils.LineFeed:
          case StringUtils.CarriageReturn:
            // eat
            break;
          default:
            if (char.IsWhiteSpace(currentChar))
            {
              // eat
            }
            else if (char.IsNumber(currentChar) || currentChar == '-' || currentChar == '.')
            {
              ParseNumber(currentChar);
              return true;
            }
            else
            {
              throw CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);
            }
            break;
        }
      } while ((currentChar = MoveNext()) != '\0' || !_end);

      return false;
    }

    private bool EatWhitespace(char initialChar, bool oneOrMore, out char finalChar)
    {
      bool whitespace = false;
      char currentChar = initialChar;
      while (currentChar == ' ' || char.IsWhiteSpace(currentChar))
      {
        whitespace = true;
        currentChar = MoveNext();
      }

      finalChar = currentChar;

      return (!oneOrMore || whitespace);
    }

    private void ParseConstructor()
    {
      if (MatchValue('n', "new", true))
      {
        char currentChar = MoveNext();

        if (EatWhitespace(currentChar, true, out currentChar))
        {
          while (char.IsLetter(currentChar))
          {
            _buffer.Append(currentChar);
            currentChar = MoveNext();
          }

          EatWhitespace(currentChar, false, out currentChar);

          if (currentChar != '(')
            throw CreateJsonReaderException("Unexpected character while parsing constructor: {0}. Line {1}, position {2}.", currentChar, _currentLineNumber, _currentLinePosition);

          string constructorName = _buffer.ToString();
          _buffer.Position = 0;

          SetToken(JsonToken.StartConstructor, constructorName);
        }
      }
    }

    private void ParseNumber(char firstChar)
    {
      char currentChar = firstChar;

      // parse until seperator character or end
      bool end = false;
      do
      {
        if (IsSeperator(currentChar))
        {
          end = true;
          _lastChar = currentChar;
        }
        else
        {
          _buffer.Append(currentChar);
        }

      } while (!end && ((currentChar = MoveNext()) != '\0' || !_end));

      string number = _buffer.ToString();
      object numberValue;
      JsonToken numberType;

      bool nonBase10 = (firstChar == '0' && !number.StartsWith("0.", StringComparison.OrdinalIgnoreCase));

      if (_readType == ReadType.ReadAsDecimal)
      {
        if (nonBase10)
        {
          // decimal.Parse doesn't support parsing hexadecimal values
          long integer = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? Convert.ToInt64(number, 16)
            : Convert.ToInt64(number, 8);

          numberValue = Convert.ToDecimal(integer);
        }
        else
        {
          numberValue = decimal.Parse(number, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
        }

        numberType = JsonToken.Float;
      }
      else
      {
        if (nonBase10)
        {
          numberValue = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? Convert.ToInt64(number, 16)
            : Convert.ToInt64(number, 8);
          numberType = JsonToken.Integer;
        }
        else if (number.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || number.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
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

      _buffer.Position = 0;

      SetToken(numberType, numberValue);
    }

    private void ParseComment()
    {
      // should have already parsed / character before reaching this method

      char currentChar = MoveNext();

      if (currentChar == '*')
      {
        while ((currentChar = MoveNext()) != '\0' || !_end)
        {
          if (currentChar == '*')
          {
            if ((currentChar = MoveNext()) != '\0' || !_end)
            {
              if (currentChar == '/')
              {
                break;
              }
              else
              {
                _buffer.Append('*');
                _buffer.Append(currentChar);
              }
            }
          }
          else
          {
            _buffer.Append(currentChar);
          }
        }
      }
      else
      {
        throw CreateJsonReaderException("Error parsing comment. Expected: *. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }

      SetToken(JsonToken.Comment, _buffer.ToString());

      _buffer.Position = 0;
    }

    private bool MatchValue(char firstChar, string value)
    {
      char currentChar = firstChar;

      int i = 0;
      do
      {
        if (currentChar != value[i])
        {
          break;
        }
        i++;
      }
      while (i < value.Length && ((currentChar = MoveNext()) != '\0' || !_end));

      return (i == value.Length);
    }

    private bool MatchValue(char firstChar, string value, bool noTrailingNonSeperatorCharacters)
    {
      // will match value and then move to the next character, checking that it is a seperator character
      bool match = MatchValue(firstChar, value);

      if (!noTrailingNonSeperatorCharacters)
      {
        return match;
      }
      else
      {
        int c = PeekNext();
        char next = (c != -1) ? (char) c : '\0';
        bool matchAndNoTrainingNonSeperatorCharacters = (match && (next == '\0' || IsSeperator(next)));

        return matchAndNoTrainingNonSeperatorCharacters;
      }
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
          return (HasNext() && PeekNext() == '*');
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
      if (MatchValue('t', JsonConvert.True, true))
      {
        SetToken(JsonToken.Boolean, true);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing boolean value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseNull()
    {
      if (MatchValue('n', JsonConvert.Null, true))
      {
        SetToken(JsonToken.Null);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing null value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseUndefined()
    {
      if (MatchValue('u', JsonConvert.Undefined, true))
      {
        SetToken(JsonToken.Undefined);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing undefined value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseFalse()
    {
      if (MatchValue('f', JsonConvert.False, true))
      {
        SetToken(JsonToken.Boolean, false);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing boolean value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseNumberNegativeInfinity()
    {
      if (MatchValue('-', JsonConvert.NegativeInfinity, true))
      {
        SetToken(JsonToken.Float, double.NegativeInfinity);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing negative infinity value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseNumberPositiveInfinity()
    {
      if (MatchValue('I', JsonConvert.PositiveInfinity, true))
      {
        SetToken(JsonToken.Float, double.PositiveInfinity);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing positive infinity value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }
    }

    private void ParseNumberNaN()
    {
      if (MatchValue('N', JsonConvert.NaN, true))
      {
        SetToken(JsonToken.Float, double.NaN);
      }
      else
      {
        throw CreateJsonReaderException("Error parsing NaN value. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
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
        if (CurrentState == State.Start)
          return 0;

        return _currentLineNumber;
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
      get { return _currentLinePosition; }
    }
  }
}