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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public abstract class JsonReader : IDisposable
  {
    protected enum State
    {
      Start,
      Complete,
      Property,
      ObjectStart,
      Object,
      ArrayStart,
      Array,
      Closed,
      PostValue,
      ConstructorStart,
      Constructor,
      Error,
      Finished
    }

    // current Token data
    private JsonToken _token;
    private object _value;
    private Type _valueType;
    private char _quoteChar;
    private State _currentState;

    protected State CurrentState
    {
      get { return _currentState; }
      private set { _currentState = value; }
    }

    private int _top;

    private List<JsonTokenType> _stack;

    /// <summary>
    /// Gets the quotation mark character used to enclose the value of a string.
    /// </summary>
    public char QuoteChar
    {
      get { return _quoteChar; }
      protected set { _quoteChar = value; }
    }

    /// <summary>
    /// Gets the type of the current Json token. 
    /// </summary>
    public JsonToken TokenType
    {
      get { return _token; }
    }

    /// <summary>
    /// Gets the text value of the current Json token.
    /// </summary>
    public object Value
    {
      get { return _value; }
    }

    /// <summary>
    /// Gets The Common Language Runtime (CLR) type for the current Json token.
    /// </summary>
    public Type ValueType
    {
      get { return _valueType; }
    }

    public int Depth
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
      _stack = new List<JsonTokenType>();
      _top = 0;
      Push(JsonTokenType.None);
    }

    private void Push(JsonTokenType value)
    {
      _stack.Add(value);
      _top++;
    }

    private JsonTokenType Pop()
    {
      JsonTokenType value = Peek();
      _stack.RemoveAt(_stack.Count - 1);
      _top--;

      return value;
    }

    private JsonTokenType Peek()
    {
      return _stack[_top - 1];
    }

    /// <summary>
    /// Reads the next Json token from the stream.
    /// </summary>
    /// <returns></returns>
    public abstract bool Read();

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

    protected void SetToken(JsonToken newToken)
    {
      SetToken(newToken, null);
    }

    protected virtual void SetToken(JsonToken newToken, object value)
    {
      _token = newToken;

      switch (newToken)
      {
        case JsonToken.StartObject:
          _currentState = State.ObjectStart;
          Push(JsonTokenType.Object);
          break;
        case JsonToken.StartArray:
          _currentState = State.ArrayStart;
          Push(JsonTokenType.Array);
          break;
        case JsonToken.StartConstructor:
          _currentState = State.ConstructorStart;
          Push(JsonTokenType.Constructor);
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
          Push(JsonTokenType.Property);
          break;
        case JsonToken.Undefined:
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.Boolean:
        case JsonToken.Null:
        case JsonToken.Date:
        case JsonToken.String:
          _currentState = State.PostValue;
          break;
      }

      JsonTokenType current = Peek();
      if (current == JsonTokenType.Property && _currentState == State.PostValue)
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
      JsonTokenType currentObject = Pop();

      if (GetTypeForCloseToken(endToken) != currentObject)
        throw new JsonReaderException("JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));
    }

    protected void SetStateBasedOnCurrent()
    {
      JsonTokenType currentObject = Peek();

      switch (currentObject)
      {
        case JsonTokenType.Object:
          _currentState = State.Object;
          break;
        case JsonTokenType.Array:
          _currentState = State.Array;
          break;
        case JsonTokenType.Constructor:
          _currentState = State.Constructor;
          break;
        case JsonTokenType.None:
          _currentState = State.Finished;
          break;
        default:
          throw new JsonReaderException("While setting the reader state back to current object an unexpected JsonType was encountered: " + currentObject);
      }
    }

    private bool IsStartToken(JsonToken token)
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
          return false;
        default:
          throw new ArgumentOutOfRangeException("token", token, "Unexpected JsonToken value.");
      }
    }

    private JsonTokenType GetTypeForCloseToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.EndObject:
          return JsonTokenType.Object;
        case JsonToken.EndArray:
          return JsonTokenType.Array;
        case JsonToken.EndConstructor:
          return JsonTokenType.Constructor;
        default:
          throw new JsonReaderException("Not a valid close JsonToken: " + token);
      }
    }

    void IDisposable.Dispose()
    {
      Dispose(true);
    }

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
