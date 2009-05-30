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
    protected enum State
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
    private JsonToken _token;
    private object _value;
    private Type _valueType;
    private char _quoteChar;
    private State _currentState;

    /// <summary>
    /// Gets the current reader state.
    /// </summary>
    /// <value>The current reader state.</value>
    protected State CurrentState
    {
      get { return _currentState; }
      private set { _currentState = value; }
    }

    private int _top;

    private readonly List<JTokenType> _stack;

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
      get { return _token; }
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
      get { return _valueType; }
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
    /// Initializes a new instance of the <see cref="JsonReader"/> class with the specified <see cref="TextReader"/>.
    /// </summary>
    public JsonReader()
    {
      //_testBuffer = new StringBuilder();
      _currentState = State.Start;
      _stack = new List<JTokenType>();
      _top = 0;
      Push(JTokenType.None);
    }

    private void Push(JTokenType value)
    {
      _stack.Add(value);
      _top++;
    }

    private JTokenType Pop()
    {
      JTokenType value = Peek();
      _stack.RemoveAt(_stack.Count - 1);
      _top--;

      return value;
    }

    private JTokenType Peek()
    {
      return _stack[_top - 1];
    }

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>true if the next token was read successfully; false if there are no more tokens to read.</returns>
    public abstract bool Read();

    /// <summary>
    /// Skips the children of the current token.
    /// </summary>
    public void Skip()
    {
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
    protected virtual void SetToken(JsonToken newToken, object value)
    {
      _token = newToken;

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
          _currentState = State.PostValue;
          break;
        case JsonToken.EndArray:
          ValidateEnd(JsonToken.EndArray);
          _currentState = State.PostValue;
          break;
        case JsonToken.EndConstructor:
          ValidateEnd(JsonToken.EndConstructor);
          _currentState = State.PostValue;
          break;
        case JsonToken.PropertyName:
          _currentState = State.Property;
          Push(JTokenType.Property);
          break;
        case JsonToken.Undefined:
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.Boolean:
        case JsonToken.Null:
        case JsonToken.Date:
        case JsonToken.String:
        case JsonToken.Raw:
          _currentState = State.PostValue;
          break;
      }

      JTokenType current = Peek();
      if (current == JTokenType.Property && _currentState == State.PostValue)
        Pop();

      if (value != null)
      {
        _value = value;
        _valueType = value.GetType();
      }
      else
      {
        _value = null;
        _valueType = null;
      }
    }

    private void ValidateEnd(JsonToken endToken)
    {
      JTokenType currentObject = Pop();

      if (GetTypeForCloseToken(endToken) != currentObject)
        throw new JsonReaderException("JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));
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
          throw new JsonReaderException("While setting the reader state back to current object an unexpected JsonType was encountered: " + currentObject);
      }
    }

    internal static bool IsStartToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.StartObject:
        case JsonToken.StartArray:
        case JsonToken.StartConstructor:
        case JsonToken.PropertyName:
          return true;
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
          throw new JsonReaderException("Not a valid close JsonToken: " + token);
      }
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
      _token = JsonToken.None;
      _value = null;
      _valueType = null;
    }
  }
}
