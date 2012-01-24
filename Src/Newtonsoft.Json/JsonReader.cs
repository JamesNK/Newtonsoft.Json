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
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public abstract class JsonReader : IDisposable
  {
    /// <summary>
    /// Specifies the state of the reader.
    /// </summary>
    protected internal enum State
    {
      /// <summary>
      /// The Read method has not been called.
      /// </summary>
      Start,
      /// <summary>
      /// The end of the file has been reached successfully.
      /// </summary>
      Complete,
      /// <summary>
      /// Reader is at a property.
      /// </summary>
      Property,
      /// <summary>
      /// Reader is at the start of an object.
      /// </summary>
      ObjectStart,
      /// <summary>
      /// Reader is in an object.
      /// </summary>
      Object,
      /// <summary>
      /// Reader is at the start of an array.
      /// </summary>
      ArrayStart,
      /// <summary>
      /// Reader is in an array.
      /// </summary>
      Array,
      /// <summary>
      /// The Close method has been called.
      /// </summary>
      Closed,
      /// <summary>
      /// Reader has just read a value.
      /// </summary>
      PostValue,
      /// <summary>
      /// Reader is at the start of a constructor.
      /// </summary>
      ConstructorStart,
      /// <summary>
      /// Reader in a constructor.
      /// </summary>
      Constructor,
      /// <summary>
      /// An error occurred that prevents the read operation from continuing.
      /// </summary>
      Error,
      /// <summary>
      /// The end of the file has been reached successfully.
      /// </summary>
      Finished
    }

    // current Token data
    private JsonToken _tokenType;
    private object _value;
    private char _quoteChar;
    internal State _currentState;
    private JTokenType _currentTypeContext;
    private bool _serializerInArray;
    private CultureInfo _culture;

    internal void SetSerializeInArray(bool serializerInArray)
    {
      _serializerInArray = serializerInArray;
    }

    /// <summary>
    /// Gets the current reader state.
    /// </summary>
    /// <value>The current reader state.</value>
    protected State CurrentState
    {
      get { return _currentState; }
    }

    private int _top;

    private readonly List<JTokenType> _stack;

    /// <summary>
    /// Gets or sets a value indicating whether the underlying stream or
    /// <see cref="TextReader"/> should be closed when the reader is closed.
    /// </summary>
    /// <value>
    /// true to close the underlying stream or <see cref="TextReader"/> when
    /// the reader is closed; otherwise false. The default is true.
    /// </value>
    public bool CloseInput { get; set; }

    /// <summary>
    /// Gets the quotation mark character used to enclose the value of a string.
    /// </summary>
    public virtual char QuoteChar
    {
      get { return _quoteChar; }
      protected internal set { _quoteChar = value; }
    }

    /// <summary>
    /// Gets the type of the current Json token. 
    /// </summary>
    public virtual JsonToken TokenType
    {
      get { return _tokenType; }
    }

    /// <summary>
    /// Gets the text value of the current Json token.
    /// </summary>
    public virtual object Value
    {
      get { return _value; }
    }

    /// <summary>
    /// Gets The Common Language Runtime (CLR) type for the current Json token.
    /// </summary>
    public virtual Type ValueType
    {
      get { return (_value != null) ? _value.GetType() : null; }
    }

    /// <summary>
    /// Gets the depth of the current token in the JSON document.
    /// </summary>
    /// <value>The depth of the current token in the JSON document.</value>
    public virtual int Depth
    {
      get
      {
        int depth = _top - 1;
        if (IsStartToken(TokenType))
          return depth - 1;
        else
          return depth;
      }
    }

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
    protected JsonReader()
    {
      _currentState = State.Start;
      _stack = new List<JTokenType>();

      CloseInput = true;
      
      Push(JTokenType.None);
    }

    private void Push(JTokenType value)
    {
      _stack.Add(value);
      _top++;
      _currentTypeContext = value;
    }

    private JTokenType Pop()
    {
      JTokenType value = Peek();
      _stack.RemoveAt(_stack.Count - 1);
      _top--;
      _currentTypeContext = _stack[_top - 1];

      return value;
    }

    private JTokenType Peek()
    {
      return _currentTypeContext;
    }

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>true if the next token was read successfully; false if there are no more tokens to read.</returns>
    public abstract bool Read();

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Int32}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Int32}"/>.</returns>
    public abstract int? ReadAsInt32();

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
    /// </summary>
    /// <returns>A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null.</returns>
    public abstract byte[] ReadAsBytes();

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Decimal}"/>.</returns>
    public abstract decimal? ReadAsDecimal();

#if !NET20
    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{DateTimeOffset}"/>.</returns>
    public abstract DateTimeOffset? ReadAsDateTimeOffset();
#endif

    /// <summary>
    /// Skips the children of the current token.
    /// </summary>
    public void Skip()
    {
      if (TokenType == JsonToken.PropertyName)
        Read();

      if (IsStartToken(TokenType))
      {
        int depth = Depth;

        while (Read() && (depth < Depth))
        {
        }
      }
    }

    /// <summary>
    /// Sets the current token.
    /// </summary>
    /// <param name="newToken">The new token.</param>
    protected void SetToken(JsonToken newToken)
    {
      SetToken(newToken, null);
    }

    /// <summary>
    /// Sets the current token and value.
    /// </summary>
    /// <param name="newToken">The new token.</param>
    /// <param name="value">The value.</param>
    protected void SetToken(JsonToken newToken, object value)
    {
      _tokenType = newToken;

      switch (newToken)
      {
        case JsonToken.StartObject:
          _currentState = State.ObjectStart;
          Push(JTokenType.Object);
          break;
        case JsonToken.StartArray:
          _currentState = State.ArrayStart;
          Push(JTokenType.Array);
          break;
        case JsonToken.StartConstructor:
          _currentState = State.ConstructorStart;
          Push(JTokenType.Constructor);
          break;
        case JsonToken.EndObject:
          ValidateEnd(JsonToken.EndObject);
          break;
        case JsonToken.EndArray:
          ValidateEnd(JsonToken.EndArray);
          break;
        case JsonToken.EndConstructor:
          ValidateEnd(JsonToken.EndConstructor);
          break;
        case JsonToken.PropertyName:
          _currentState = State.Property;
          break;
        case JsonToken.Undefined:
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.Boolean:
        case JsonToken.Null:
        case JsonToken.Date:
        case JsonToken.String:
        case JsonToken.Raw:
        case JsonToken.Bytes:
          _currentState = (Peek() != JTokenType.None) ? State.PostValue : State.Finished;
          break;
      }

      _value = value;
    }

    private void ValidateEnd(JsonToken endToken)
    {
      JTokenType currentObject = Pop();

      if (GetTypeForCloseToken(endToken) != currentObject)
        throw new JsonReaderException("JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));

      _currentState = (Peek() != JTokenType.None) ? State.PostValue : State.Finished;
    }

    /// <summary>
    /// Sets the state based on current token type.
    /// </summary>
    protected void SetStateBasedOnCurrent()
    {
      JTokenType currentObject = Peek();

      switch (currentObject)
      {
        case JTokenType.Object:
          _currentState = State.Object;
          break;
        case JTokenType.Array:
          _currentState = State.Array;
          break;
        case JTokenType.Constructor:
          _currentState = State.Constructor;
          break;
        case JTokenType.None:
          _currentState = State.Finished;
          break;
        default:
          throw new JsonReaderException("While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, currentObject));
      }
    }

    internal static bool IsPrimitiveToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.String:
        case JsonToken.Boolean:
        case JsonToken.Undefined:
        case JsonToken.Null:
        case JsonToken.Date:
        case JsonToken.Bytes:
          return true;
        default:
          return false;
      }
    }

    internal static bool IsStartToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.StartObject:
        case JsonToken.StartArray:
        case JsonToken.StartConstructor:
          return true;
        case JsonToken.PropertyName:
        case JsonToken.None:
        case JsonToken.Comment:
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.String:
        case JsonToken.Boolean:
        case JsonToken.Null:
        case JsonToken.Undefined:
        case JsonToken.EndObject:
        case JsonToken.EndArray:
        case JsonToken.EndConstructor:
        case JsonToken.Date:
        case JsonToken.Raw:
        case JsonToken.Bytes:
          return false;
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected JsonToken value.");
      }
    }

    private JTokenType GetTypeForCloseToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.EndObject:
          return JTokenType.Object;
        case JsonToken.EndArray:
          return JTokenType.Array;
        case JsonToken.EndConstructor:
          return JTokenType.Constructor;
        default:
          throw new JsonReaderException("Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
      }
    }

    internal bool ReaderIsSerializerInArray()
    {
      return (TokenType == JsonToken.EndArray && _serializerInArray);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    void IDisposable.Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (_currentState != State.Closed && disposing)
        Close();
    }

    /// <summary>
    /// Changes the <see cref="State"/> to Closed. 
    /// </summary>
    public virtual void Close()
    {
      _currentState = State.Closed;
      _tokenType = JsonToken.None;
      _value = null;
    }

    internal JsonReaderException CreateReaderException(JsonReader reader, string message)
    {
      return CreateReaderException(reader, message, null);
    }

    internal JsonReaderException CreateReaderException(JsonReader reader, string message, Exception ex)
    {
      return CreateReaderException(reader as IJsonLineInfo, message, ex);
    }

    internal JsonReaderException CreateReaderException(IJsonLineInfo lineInfo, string message, Exception ex)
    {
      message = FormatExceptionMessage(lineInfo, message);

      int lineNumber;
      int linePosition;
      if (lineInfo != null && lineInfo.HasLineInfo())
      {
        lineNumber = lineInfo.LineNumber;
        linePosition = lineInfo.LinePosition;
      }
      else
      {
        lineNumber = 0;
        linePosition = 0;
      }

      return new JsonReaderException(message, ex, lineNumber, linePosition);
    }

    internal static string FormatExceptionMessage(IJsonLineInfo lineInfo, string message)
    {
      if (!message.EndsWith("."))
        message += ".";

      if (lineInfo != null && lineInfo.HasLineInfo())
        message += " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition);

      return message;
    }
  }
}
