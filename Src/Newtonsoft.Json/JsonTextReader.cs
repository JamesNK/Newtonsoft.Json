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
    private readonly TextReader _reader;
    private readonly StringBuffer _buffer;
    private char _currentChar;
    private int _currentLinePosition;
    private int _currentLineNumber;
    private bool _currentCharCarriageReturn;

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
      bool stringTerminated = false;
      bool hexNumber = false;
      int hexCount = 0;

      while (!stringTerminated && MoveNext())
      {
        if (hexNumber)
          hexCount++;

        switch (_currentChar)
        {
          case '\\':
            if (MoveNext())
            {
              switch (_currentChar)
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
                case 'u':
                  // note the start of a hex character
                  hexNumber = true;
                  break;
                default:
                  _buffer.Append(_currentChar);
                  break;
              }
            }
            else
            {
              throw CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", quote, _currentLineNumber, _currentLinePosition);
            }
            break;
          case '"':
          case '\'':
            if (_currentChar == quote)
              stringTerminated = true;
            else
              goto default;
            break;
          default:
            _buffer.Append(_currentChar);
            break;
        }

        if (hexCount == 4)
        {
          // remove hex characters from buffer, convert to char and then add
          string hexString = _buffer.ToString(_buffer.Position - 4, 4);
          char hexChar = Convert.ToChar(int.Parse(hexString, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo));

          _buffer.Position = _buffer.Position - 4;
          _buffer.Append(hexChar);

          hexNumber = false;
          hexCount = 0;
        }
      }

      if (!stringTerminated)
        throw CreateJsonReaderException("Unterminated string. Expected delimiter: {0}. Line {1}, position {2}.", quote, _currentLineNumber, _currentLinePosition);

      ClearCurrentChar();
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

    private JsonReaderException CreateJsonReaderException(string format, params object[] args)
    {
      string message = format.FormatWith(CultureInfo.InvariantCulture, args);
      return new JsonReaderException(message, null, _currentLineNumber, _currentLinePosition);
    }

    /// <summary>
    /// Sets the current token and value.
    /// </summary>
    /// <param name="newToken">The new token.</param>
    /// <param name="value">The value.</param>
    protected override void SetToken(JsonToken newToken, object value)
    {
      base.SetToken(newToken, value);

      switch (newToken)
      {
        case JsonToken.StartObject:
          ClearCurrentChar();
          break;
        case JsonToken.StartArray:
          ClearCurrentChar();
          break;
        case JsonToken.StartConstructor:
          ClearCurrentChar();
          break;
        case JsonToken.EndObject:
          ClearCurrentChar();
          break;
        case JsonToken.EndArray:
          ClearCurrentChar();
          break;
        case JsonToken.EndConstructor:
          ClearCurrentChar();
          break;
        case JsonToken.PropertyName:
          ClearCurrentChar();
          break;
      }
    }

    private void ParseDate(string text)
    {
      string value = text.Substring(6, text.Length - 8);
      DateTimeKind kind = DateTimeKind.Utc;

      int index = value.IndexOf('+', 1);

      if (index == -1)
        index = value.IndexOf('-', 1);

      if (index != -1)
      {
        kind = DateTimeKind.Local;
        value = value.Substring(0, index);
      }

      long javaScriptTicks = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
      DateTime utcDateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
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

    private const int LineFeedValue = StringUtils.LineFeed;
    private const int CarriageReturnValue = StringUtils.CarriageReturn;

    private bool MoveNext()
    {
      int value = _reader.Read();

      switch (value)
      {
        case -1:
          return false;
        case CarriageReturnValue:
          _currentLineNumber++;
          _currentLinePosition = 0;
          _currentCharCarriageReturn = true;
          break;
        case LineFeedValue:
          if (!_currentCharCarriageReturn)
            _currentLineNumber++;

          _currentLinePosition = 0;
          _currentCharCarriageReturn = false;
          break;
        default:
          _currentLinePosition++;
          _currentCharCarriageReturn = false;
          break;
      }

      _currentChar = (char)value;
      return true;
    }

    private bool HasNext()
    {
      return (_reader.Peek() != -1);
    }

    private char PeekNext()
    {
      return (char)_reader.Peek();
    }

    private void ClearCurrentChar()
    {
      _currentChar = '\0';
    }

    /// <summary>
    /// Reads the next Json token from the stream.
    /// </summary>
    /// <returns>
    /// true if the next token was read successfully; false if there are no more tokens to read.
    /// </returns>
    public override bool Read()
    {
      while (true)
      {
        if (_currentChar == '\0')
        {
          if (!MoveNext())
            return false;
        }

        switch (CurrentState)
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
          case State.Closed:
            break;
          case State.Error:
            break;
          default:
            throw CreateJsonReaderException("Unexpected state: {0}. Line {1}, position {2}.", CurrentState, _currentLineNumber, _currentLinePosition);
        }
      }
    }

    private bool ParsePostValue()
    {
      do
      {
        switch (_currentChar)
        {
          case '}':
            SetToken(JsonToken.EndObject);
            ClearCurrentChar();
            return true;
          case ']':
            SetToken(JsonToken.EndArray);
            ClearCurrentChar();
            return true;
          case ')':
            SetToken(JsonToken.EndConstructor);
            ClearCurrentChar();
            return true;
          case '/':
            ParseComment();
            return true;
          case ',':
            // finished paring
            SetStateBasedOnCurrent();
            ClearCurrentChar();
            return false;
          default:
            if (char.IsWhiteSpace(_currentChar))
            {
              // eat whitespace
              ClearCurrentChar();
            }
            else
            {
              throw CreateJsonReaderException("After parsing a value an unexpected character was encoutered: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
            }
            break;
        }
      } while (MoveNext());

      return false;
    }

    private bool ParseObject()
    {
      do
      {
        switch (_currentChar)
        {
          case '}':
            SetToken(JsonToken.EndObject);
            return true;
          case '/':
            ParseComment();
            return true;
          case ',':
            SetToken(JsonToken.Undefined);
            return true;
          default:
            if (char.IsWhiteSpace(_currentChar))
            {
              // eat
            }
            else
            {
              return ParseProperty();
            }
            break;
        }
      } while (MoveNext());

      return false;
    }

    private bool ParseProperty()
    {
      char quoteChar;

      if (ValidIdentifierChar(_currentChar))
      {
        quoteChar = '\0';
        ParseUnquotedProperty();
      }
      else if (_currentChar == '"' || _currentChar == '\'')
      {
        quoteChar = _currentChar;
        ParseQuotedProperty(_currentChar);
      }
      else
      {
        throw CreateJsonReaderException("Invalid property identifier character: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
      }

      if (_currentChar != ':')
      {
        MoveNext();

        // finished property. skip any whitespace and move to colon
        EatWhitespace(false);

        if (_currentChar != ':')
          throw CreateJsonReaderException("Invalid character after parsing property name. Expected ':' but got: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
      }

      SetToken(JsonToken.PropertyName, _buffer.ToString());
      QuoteChar = quoteChar;
      _buffer.Position = 0;

      return true;
    }

    private void ParseQuotedProperty(char quoteChar)
    {
      // parse property name until quoted char is hit
      while (MoveNext())
      {
        if (_currentChar == quoteChar)
        {
          return;
        }
        else
        {
          _buffer.Append(_currentChar);
        }
      }

      throw CreateJsonReaderException("Unclosed quoted property. Expected: {0}. Line {1}, position {2}.", quoteChar, _currentLineNumber, _currentLinePosition);
    }

    private bool ValidIdentifierChar(char value)
    {
      return (char.IsLetterOrDigit(_currentChar) || _currentChar == '_' || _currentChar == '$');
    }

    private void ParseUnquotedProperty()
    {
      // parse unquoted property name until whitespace or colon
      _buffer.Append(_currentChar);

      while (MoveNext())
      {
        if (char.IsWhiteSpace(_currentChar) || _currentChar == ':')
        {
          break;
        }
        else if (ValidIdentifierChar(_currentChar))
        {
          _buffer.Append(_currentChar);
        }
        else
        {
          throw CreateJsonReaderException("Invalid JavaScript property identifier character: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
        }
      }
    }

    private bool ParseValue()
    {
      do
      {
        switch (_currentChar)
        {
          case '"':
          case '\'':
            ParseString(_currentChar);
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
              char next = PeekNext();

              if (next == 'u')
                ParseNull();
              else if (next == 'e')
                ParseConstructor();
              else
                throw CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
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
              ParseNumber();
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
            //ClearCurrentChar();
            return true;
          case ')':
            SetToken(JsonToken.EndConstructor);
            return true;
          default:
            if (char.IsWhiteSpace(_currentChar))
            {
              // eat
            }
            else if (char.IsNumber(_currentChar) || _currentChar == '-' || _currentChar == '.')
            {
              ParseNumber();
              return true;
            }
            else
            {
              throw CreateJsonReaderException("Unexpected character encountered while parsing value: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);
            }
            break;
        }
      } while (MoveNext());

      return false;
    }

    private bool EatWhitespace(bool oneOrMore)
    {
      bool whitespace = false;
      while (char.IsWhiteSpace(_currentChar))
      {
        whitespace = true;
        MoveNext();
      }

      return (!oneOrMore || whitespace);
    }

    private void ParseConstructor()
    {
      if (MatchValue("new", true))
      {
        if (EatWhitespace(true))
        {
          while (char.IsLetter(_currentChar))
          {
            _buffer.Append(_currentChar);
            MoveNext();
          }

          EatWhitespace(false);

          if (_currentChar != '(')
            throw CreateJsonReaderException("Unexpected character while parsing constructor: {0}. Line {1}, position {2}.", _currentChar, _currentLineNumber, _currentLinePosition);

          string constructorName = _buffer.ToString();
          _buffer.Position = 0;

          SetToken(JsonToken.StartConstructor, constructorName);
        }
      }
    }

    private void ParseNumber()
    {
      // parse until seperator character or end
      bool end = false;
      do
      {
        if (CurrentIsSeperator())
          end = true;
        else
          _buffer.Append(_currentChar);

      } while (!end && MoveNext());

      // hit the end of the reader before the number ended. clear the last number value
      if (!end)
        ClearCurrentChar();

      string number = _buffer.ToString();
      object numberValue;
      JsonToken numberType;

      if (number.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1
        && number.IndexOf("e", StringComparison.OrdinalIgnoreCase) == -1)
      {
        numberValue = Convert.ToInt64(_buffer.ToString(), CultureInfo.InvariantCulture);
        numberType = JsonToken.Integer;
      }
      else
      {
        numberValue = Convert.ToDouble(_buffer.ToString(), CultureInfo.InvariantCulture);
        numberType = JsonToken.Float;
      }

      _buffer.Position = 0;

      SetToken(numberType, numberValue);
    }

    private void ParseComment()
    {
      // should have already parsed / character before reaching this method

      MoveNext();

      if (_currentChar == '*')
      {
        while (MoveNext())
        {
          if (_currentChar == '*')
          {
            if (MoveNext())
            {
              if (_currentChar == '/')
              {
                break;
              }
              else
              {
                _buffer.Append('*');
                _buffer.Append(_currentChar);
              }
            }
          }
          else
          {
            _buffer.Append(_currentChar);
          }
        }
      }
      else
      {
        throw CreateJsonReaderException("Error parsing comment. Expected: *. Line {0}, position {1}.", _currentLineNumber, _currentLinePosition);
      }

      SetToken(JsonToken.Comment, _buffer.ToString());

      _buffer.Position = 0;

      ClearCurrentChar();
    }

    private bool MatchValue(string value)
    {
      int i = 0;
      do
      {
        if (_currentChar != value[i])
        {
          break;
        }
        i++;
      }
      while (i < value.Length && MoveNext());

      return (i == value.Length);
    }

    private bool MatchValue(string value, bool noTrailingNonSeperatorCharacters)
    {
      // will match value and then move to the next character, checking that it is a seperator character
      bool match = MatchValue(value);

      if (!noTrailingNonSeperatorCharacters)
      {
        return match;
      }
      else
      {
        bool matchAndNoTrainingNonSeperatorCharacters = (match && (!MoveNext() || CurrentIsSeperator()));
        if (!CurrentIsSeperator())
          ClearCurrentChar();

        return matchAndNoTrainingNonSeperatorCharacters;
      }
    }

    private bool CurrentIsSeperator()
    {
      switch (_currentChar)
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
        default:
          if (char.IsWhiteSpace(_currentChar))
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
      if (MatchValue(JsonConvert.True, true))
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
      if (MatchValue(JsonConvert.Null, true))
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
      if (MatchValue(JsonConvert.Undefined, true))
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
      if (MatchValue(JsonConvert.False, true))
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
      if (MatchValue(JsonConvert.NegativeInfinity, true))
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
      if (MatchValue(JsonConvert.PositiveInfinity, true))
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
      if (MatchValue(JsonConvert.NaN, true))
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

      if (_reader != null)
        _reader.Close();

      if (_buffer != null)
        _buffer.Clear();
    }

    public bool HasLineInfo()
    {
      return true;
    }

    public int LineNumber
    {
      get
      {
        if (CurrentState == State.Start)
          return 0;

        return _currentLineNumber;
      }
    }

    public int LinePosition
    {
      get { return _currentLinePosition; }
    }
  }
}
