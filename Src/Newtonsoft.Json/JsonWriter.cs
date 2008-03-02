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
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies the state of the <see cref="JsonWriter"/>.
  /// </summary>
  public enum WriteState
  {
    /// <summary>
    /// An exception has been thrown, which has left the <see cref="JsonWriter"/> in an invalid state.
    /// You may call the <see cref="JsonWriter.Close"/> method to put the <see cref="JsonWriter"/> in the <c>Closed</c> state.
    /// Any other <see cref="JsonWriter"/> method calls results in an <see cref="InvalidOperationException"/> being thrown. 
    /// </summary>
    Error,
    /// <summary>
    /// The <see cref="JsonWriter.Close"/> method has been called. 
    /// </summary>
    Closed,
    /// <summary>
    /// An object is being written. 
    /// </summary>
    Object,
    /// <summary>
    /// A array is being written.
    /// </summary>
    Array,
    /// <summary>
    /// A constructor is being written.
    /// </summary>
    Constructor,
    /// <summary>
    /// A property is being written.
    /// </summary>
    Property,
    /// <summary>
    /// A write method has not been called.
    /// </summary>
    Start
  }

  /// <summary>
  /// Specifies formatting options for the <see cref="JsonWriter"/>.
  /// </summary>
  public enum Formatting
  {
    /// <summary>
    /// No special formatting is applied. This is the default.
    /// </summary>
    None,
    /// <summary>
    /// Causes child objects to be indented according to the <see cref="JsonWriter.Indentation"/> and <see cref="JsonWriter.IndentChar"/> settings.
    /// </summary>
    Indented
  }

  /// <summary>
  /// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
  /// </summary>
  public abstract class JsonWriter : IDisposable
  {
    private enum State
    {
      Start,
      Property,
      ObjectStart,
      Object,
      ArrayStart,
      Array,
      ConstructorStart,
      Constructor,
      Closed,
      Error
    }

    // array that gives a new state based on the current state an the token being written
    private static readonly State[,] stateArray = {
//                      Start                   PropertyName            ObjectStart         Object            ArrayStart              Array                   ConstructorStart        Constructor             Closed          Error
//                        
/* None             */{ State.Error,            State.Error,            State.Error,        State.Error,      State.Error,            State.Error,            State.Error,            State.Error,            State.Error,    State.Error },
/* StartObject      */{ State.ObjectStart,      State.ObjectStart,      State.Error,        State.Error,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.Error,    State.Error },
/* StartArray       */{ State.ArrayStart,       State.ArrayStart,       State.Error,        State.Error,      State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.Error,    State.Error },
/* StartConstructor */{ State.ConstructorStart, State.ConstructorStart, State.Error,        State.Error,      State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.Error,    State.Error },
/* StartProperty    */{ State.Property,         State.Error,            State.Property,     State.Property,   State.Error,            State.Error,            State.Error,            State.Error,            State.Error,    State.Error },
/* Comment          */{ State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
/* Value            */{ State.Start,            State.Object,           State.Error,        State.Error,      State.Array,            State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
		};

    private int _top;

    private List<JsonTokenType> _stack;
    private List<object> _serializeStack;
    private State _currentState;
    private Formatting _formatting;

    internal List<object> SerializeStack
    {
      get
      {
        if (_serializeStack == null)
          _serializeStack = new List<object>();

        return _serializeStack;
      }
    }

    protected int Top
    {
      get { return _top; }
    }
    
    /// <summary>
    /// Gets the state of the writer.
    /// </summary>
    public WriteState WriteState
    {
      get
      {
        switch (_currentState)
        {
          case State.Error:
            return WriteState.Error;
          case State.Closed:
            return WriteState.Closed;
          case State.Object:
          case State.ObjectStart:
            return WriteState.Object;
          case State.Array:
          case State.ArrayStart:
            return WriteState.Array;
          case State.Constructor:
          case State.ConstructorStart:
            return WriteState.Constructor;
          case State.Property:
            return WriteState.Property;
          case State.Start:
            return WriteState.Start;
          default:
            throw new JsonWriterException("Invalid state: " + _currentState);
        }
      }
    }

    /// <summary>
    /// Indicates how the output is formatted.
    /// </summary>
    public Formatting Formatting
    {
      get { return _formatting; }
      set { _formatting = value; }
    }

    /// <summary>
    /// Creates an instance of the <c>JsonWriter</c> class using the specified <see cref="TextWriter"/>. 
    /// </summary>
    /// <param name="textWriter">The <c>TextWriter</c> to write to.</param>
    public JsonWriter()
    {
      _stack = new List<JsonTokenType>(1);
      _stack.Add(JsonTokenType.None);
      _currentState = State.Start;
      _formatting = Formatting.None;
    }

    private void Push(JsonTokenType value)
    {
      _top++;
      if (_stack.Count <= _top)
        _stack.Add(value);
      else
        _stack[_top] = value;
    }

    private JsonTokenType Pop()
    {
      JsonTokenType value = Peek();
      _top--;

      return value;
    }

    private JsonTokenType Peek()
    {
      return _stack[_top];
    }

    /// <summary>
    /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
    /// </summary>
    public abstract void Flush();

    /// <summary>
    /// Closes this stream and the underlying stream.
    /// </summary>
    public virtual void Close()
    {
      AutoCompleteAll();
    }

    /// <summary>
    /// Writes the beginning of a Json object.
    /// </summary>
    public virtual void WriteStartObject()
    {
      AutoComplete(JsonToken.StartObject);
      Push(JsonTokenType.Object);
    }

    /// <summary>
    /// Writes the end of a Json object.
    /// </summary>
    public void WriteEndObject()
    {
      AutoCompleteClose(JsonToken.EndObject);
    }

    /// <summary>
    /// Writes the beginning of a Json array.
    /// </summary>
    public virtual void WriteStartArray()
    {
      AutoComplete(JsonToken.StartArray);
      Push(JsonTokenType.Array);
    }

    /// <summary>
    /// Writes the end of an array.
    /// </summary>
    public void WriteEndArray()
    {
      AutoCompleteClose(JsonToken.EndArray);
    }

    public virtual void WriteStartConstructor(string name)
    {
      AutoComplete(JsonToken.StartConstructor);
      Push(JsonTokenType.Constructor);
    }

    public void WriteEndConstructor()
    {
      AutoCompleteClose(JsonToken.EndConstructor);
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a Json object.
    /// </summary>
    /// <param name="name"></param>
    public virtual void WritePropertyName(string name)
    {
      AutoComplete(JsonToken.PropertyName);
    }

    /// <summary>
    /// Writes the end of the current Json object or array.
    /// </summary>
    public void WriteEnd()
    {
      WriteEnd(Peek());
    }

    public void WriteToken(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      int currentDepth = (reader.TokenType == JsonToken.None) ? -1 : reader.Depth;
      bool continue1;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.None:
            // read to next
            break;
          case JsonToken.StartObject:
            WriteStartObject();
            break;
          case JsonToken.StartArray:
            WriteStartArray();
            break;
          case JsonToken.StartConstructor:
            WriteStartConstructor(reader.Value.ToString());
            break;
          case JsonToken.PropertyName:
            WritePropertyName(reader.Value.ToString());
            break;
          case JsonToken.Comment:
            WriteComment(reader.Value.ToString());
            break;
          case JsonToken.Integer:
            WriteValue((long)reader.Value);
            break;
          case JsonToken.Float:
            WriteValue((double)reader.Value);
            break;
          case JsonToken.String:
            WriteValue(reader.Value.ToString());
            break;
          case JsonToken.Boolean:
            WriteValue((bool)reader.Value);
            break;
          case JsonToken.Null:
            WriteNull();
            break;
          case JsonToken.Undefined:
            WriteUndefined();
            break;
          case JsonToken.EndObject:
            WriteEndObject();
            break;
          case JsonToken.EndArray:
            WriteEndArray();
            break;
          case JsonToken.EndConstructor:
            WriteEndConstructor();
            break;
          case JsonToken.Date:
            WriteValue((DateTime)reader.Value);
            break;
          default:
            throw new ArgumentOutOfRangeException("TokenType", reader.TokenType, "Unexpected token type.");
        }
      }
      while (reader.Read() && (currentDepth - 1 < reader.Depth || (currentDepth == reader.Depth && !IsEndToken(reader.TokenType))));
    }

    private bool IsEndToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.EndObject:
        case JsonToken.EndArray:
        case JsonToken.EndConstructor:
          return true;
        default:
          return false;
      }
    }

    private void WriteEnd(JsonTokenType type)
    {
      switch (type)
      {
        case JsonTokenType.Object:
          WriteEndObject();
          break;
        case JsonTokenType.Array:
          WriteEndArray();
          break;
        case JsonTokenType.Constructor:
          WriteEndConstructor();
          break;
        default:
          throw new JsonWriterException("Unexpected type when writing end: " + type);
      }
    }

    private void AutoCompleteAll()
    {
      while (_top > 0)
      {
        WriteEnd();
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
          throw new JsonWriterException("No type for token: " + token);
      }
    }

    private JsonToken GetCloseTokenForType(JsonTokenType type)
    {
      switch (type)
      {
        case JsonTokenType.Object:
          return JsonToken.EndObject;
        case JsonTokenType.Array:
          return JsonToken.EndArray;
        case JsonTokenType.Constructor:
          return JsonToken.EndConstructor;
        default:
          throw new JsonWriterException("No close token for type: " + type);
      }
    }

    private void AutoCompleteClose(JsonToken tokenBeingClosed)
    {
      // write closing symbol and calculate new state

      int levelsToComplete = 0;

      for (int i = 0; i < _top; i++)
      {
        int currentLevel = _top - i;

        if (_stack[currentLevel] == GetTypeForCloseToken(tokenBeingClosed))
        {
          levelsToComplete = i + 1;
          break;
        }
      }

      if (levelsToComplete == 0)
        throw new JsonWriterException("No token to close.");

      for (int i = 0; i < levelsToComplete; i++)
      {
        JsonToken token = GetCloseTokenForType(Pop());

        if (_currentState != State.ObjectStart && _currentState != State.ArrayStart)
          WriteIndent();

        WriteEnd(token);
      }

      JsonTokenType currentLevelType = Peek();

      switch (currentLevelType)
      {
        case JsonTokenType.Object:
          _currentState = State.Object;
          break;
        case JsonTokenType.Array:
          _currentState = State.Array;
          break;
        case JsonTokenType.None:
          _currentState = State.Start;
          break;
        default:
          throw new JsonWriterException("Unknown JsonType: " + currentLevelType);
      }
    }

    protected virtual void WriteEnd(JsonToken token)
    {
    }

    protected virtual void WriteIndent()
    {
    }

    protected virtual void WriteValueDelimiter()
    {
    }

    protected virtual void WriteIndentSpace()
    {
    }

    private void AutoComplete(JsonToken tokenBeingWritten)
    {
      int token;

      switch (tokenBeingWritten)
      {
        default:
          token = (int)tokenBeingWritten;
          break;
        case JsonToken.Integer:
        case JsonToken.Float:
        case JsonToken.String:
        case JsonToken.Boolean:
        case JsonToken.Null:
        case JsonToken.Undefined:
        case JsonToken.Date:
          // a value is being written
          token = 6;
          break;
      }

      // gets new state based on the current state and what is being written
      State newState = stateArray[token, (int)_currentState];

      if (newState == State.Error)
        throw new JsonWriterException(string.Format("Token {0} in state {1} would result in an invalid JavaScript object.", tokenBeingWritten.ToString(), _currentState.ToString()));

      if ((_currentState == State.Object || _currentState == State.Array || _currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment)
      {
        WriteValueDelimiter();
      }
      else if (_currentState == State.Property)
      {
        if (_formatting == Formatting.Indented)
          WriteIndentSpace();
      }

      // don't indent a property when it is the first token to be written (i.e. at the start)
      if ((tokenBeingWritten == JsonToken.PropertyName && WriteState != WriteState.Start) ||
        WriteState == WriteState.Array || WriteState == WriteState.Constructor)
      {
        WriteIndent();
      }

      _currentState = newState;
    }

    #region WriteValue methods
    /// <summary>
    /// Writes a null value.
    /// </summary>
    public virtual void WriteNull()
    {
      AutoComplete(JsonToken.Null);
    }

    /// <summary>
    /// Writes an undefined value.
    /// </summary>
    public virtual void WriteUndefined()
    {
      AutoComplete(JsonToken.Undefined);
    }

    /// <summary>
    /// Writes raw JavaScript manually.
    /// </summary>
    /// <param name="javaScript">The raw JavaScript to write.</param>
    public virtual void WriteRaw(string javaScript)
    {
      // hack. some 'raw' or 'other' token perhaps?
      AutoComplete(JsonToken.Undefined);
    }

    /// <summary>
    /// Writes a <see cref="String"/> value.
    /// </summary>
    /// <param name="value">The <see cref="String"/> value to write.</param>
    public virtual void WriteValue(string value)
    {
      AutoComplete(JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Int32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int32"/> value to write.</param>
    public virtual void WriteValue(int value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt32"/> value to write.</param>
    public virtual void WriteValue(uint value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int64"/> value to write.</param>
    public virtual void WriteValue(long value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt64"/> value to write.</param>
    public virtual void WriteValue(ulong value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Single"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Single"/> value to write.</param>
    public virtual void WriteValue(float value)
    {
      AutoComplete(JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Double"/> value to write.</param>
    public virtual void WriteValue(double value)
    {
      AutoComplete(JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Boolean"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Boolean"/> value to write.</param>
    public virtual void WriteValue(bool value)
    {
      AutoComplete(JsonToken.Boolean);
    }

    /// <summary>
    /// Writes a <see cref="Int16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int16"/> value to write.</param>
    public virtual void WriteValue(short value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt16"/> value to write.</param>
    public virtual void WriteValue(ushort value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Char"/> value to write.</param>
    public virtual void WriteValue(char value)
    {
      AutoComplete(JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Byte"/> value to write.</param>
    public virtual void WriteValue(byte value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="SByte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="SByte"/> value to write.</param>
    public virtual void WriteValue(sbyte value)
    {
      AutoComplete(JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Decimal"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Decimal"/> value to write.</param>
    public virtual void WriteValue(decimal value)
    {
      AutoComplete(JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    public virtual void WriteValue(DateTime value)
    {
      AutoComplete(JsonToken.Date);
    }

    /// <summary>
    /// Writes a <see cref="DateTimeOffset"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
    public virtual void WriteValue(DateTimeOffset value)
    {
      AutoComplete(JsonToken.Date);
    }
    #endregion

    /// <summary>
    /// Writes out a comment <code>/*...*/</code> containing the specified text. 
    /// </summary>
    /// <param name="text">Text to place inside the comment.</param>
    public virtual void WriteComment(string text)
    {
      AutoComplete(JsonToken.Comment);
    }

    /// <summary>
    /// Writes out the given white space.
    /// </summary>
    /// <param name="ws">The string of white space characters.</param>
    public virtual void WriteWhitespace(string ws)
    {
      if (ws != null)
      {
        if (!StringUtils.IsWhiteSpace(ws))
          throw new JsonWriterException("Only white space characters should be used.");
      }
    }


    void IDisposable.Dispose()
    {
      Dispose(true);
    }

    private void Dispose(bool disposing)
    {
      if (WriteState != WriteState.Closed)
        Close();
    }
  }
}