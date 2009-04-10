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
using System.Globalization;

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
  /// Specifies formatting options for the <see cref="JsonTextWriter"/>.
  /// </summary>
  public enum Formatting
  {
    /// <summary>
    /// No special formatting is applied. This is the default.
    /// </summary>
    None,
    /// <summary>
    /// Causes child objects to be indented according to the <see cref="JsonTextWriter.Indentation"/> and <see cref="JsonTextWriter.IndentChar"/> settings.
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
/* Raw              */{ State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
/* Value            */{ State.Start,            State.Object,           State.Error,        State.Error,      State.Array,            State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
		};

    private int _top;

    private List<JTokenType> _stack;
    private State _currentState;
    private Formatting _formatting;
    private List<object> _serializeStack;

    internal List<object> SerializeStack
    {
      get
      {
        if (_serializeStack == null)
          _serializeStack = new List<object>();

        return _serializeStack;
      }
    }

    /// <summary>
    /// Gets the top.
    /// </summary>
    /// <value>The top.</value>
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
    /// Creates an instance of the <c>JsonWriter</c> class. 
    /// </summary>
    public JsonWriter()
    {
      _stack = new List<JTokenType>(8);
      _stack.Add(JTokenType.None);
      _currentState = State.Start;
      _formatting = Formatting.None;
    }

    private void Push(JTokenType value)
    {
      _top++;
      if (_stack.Count <= _top)
        _stack.Add(value);
      else
        _stack[_top] = value;
    }

    private JTokenType Pop()
    {
      JTokenType value = Peek();
      _top--;

      return value;
    }

    private JTokenType Peek()
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
      Push(JTokenType.Object);
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
      Push(JTokenType.Array);
    }

    /// <summary>
    /// Writes the end of an array.
    /// </summary>
    public void WriteEndArray()
    {
      AutoCompleteClose(JsonToken.EndArray);
    }

    /// <summary>
    /// Writes the start of a constructor with the given name.
    /// </summary>
    /// <param name="name">The name of the constructor.</param>
    public virtual void WriteStartConstructor(string name)
    {
      AutoComplete(JsonToken.StartConstructor);
      Push(JTokenType.Constructor);
    }

    /// <summary>
    /// Writes the end constructor.
    /// </summary>
    public void WriteEndConstructor()
    {
      AutoCompleteClose(JsonToken.EndConstructor);
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a Json object.
    /// </summary>
    /// <param name="name">The name of the property.</param>
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

    /// <summary>
    /// Writes the current <see cref="JsonReader"/> token.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read the token from.</param>
    public void WriteToken(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      int currentDepth;

      if (reader.TokenType == JsonToken.None)
        currentDepth = -1;
      else if (!IsStartToken(reader.TokenType))
        currentDepth = reader.Depth + 1;
      else
        currentDepth = reader.Depth;

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
            string constructorName = reader.Value.ToString();
            // write a JValue date when the constructor is for a date
            if (string.Compare(constructorName, "Date", StringComparison.Ordinal) == 0)
              WriteConstructorDate(reader);
            else
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
          case JsonToken.Raw:
            WriteRawValue((string)reader.Value);
            break;
          default:
            throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", reader.TokenType, "Unexpected token type.");
        }
      }
      while (
        // stop if we have reached the end of the token being read
        currentDepth - 1 < reader.Depth - (IsEndToken(reader.TokenType) ? 1 : 0)
        && reader.Read());
    }

    private void WriteConstructorDate(JsonReader reader)
    {
      if (!reader.Read())
        throw new Exception("Unexpected end while reading date constructor.");
      if (reader.TokenType != JsonToken.Integer)
        throw new Exception("Unexpected token while reading date constructor. Expected Integer, got " + reader.TokenType);

      long ticks = (long)reader.Value;
      DateTime date = JsonConvert.ConvertJavaScriptTicksToDateTime(ticks);

      if (!reader.Read())
        throw new Exception("Unexpected end while reading date constructor.");
      if (reader.TokenType != JsonToken.EndConstructor)
        throw new Exception("Unexpected token while reading date constructor. Expected EndConstructor, got " + reader.TokenType);

      WriteValue(date);
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

    private bool IsStartToken(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.StartObject:
        case JsonToken.StartArray:
        case JsonToken.StartConstructor:
          return true;
        default:
          return false;
      }
    }

    private void WriteEnd(JTokenType type)
    {
      switch (type)
      {
        case JTokenType.Object:
          WriteEndObject();
          break;
        case JTokenType.Array:
          WriteEndArray();
          break;
        case JTokenType.Constructor:
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
          throw new JsonWriterException("No type for token: " + token);
      }
    }

    private JsonToken GetCloseTokenForType(JTokenType type)
    {
      switch (type)
      {
        case JTokenType.Object:
          return JsonToken.EndObject;
        case JTokenType.Array:
          return JsonToken.EndArray;
        case JTokenType.Constructor:
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

      JTokenType currentLevelType = Peek();

      switch (currentLevelType)
      {
        case JTokenType.Object:
          _currentState = State.Object;
          break;
        case JTokenType.Array:
          _currentState = State.Array;
          break;
        case JTokenType.Constructor:
          _currentState = State.Array;
          break;
        case JTokenType.None:
          _currentState = State.Start;
          break;
        default:
          throw new JsonWriterException("Unknown JsonType: " + currentLevelType);
      }
    }

    /// <summary>
    /// Writes the specified end token.
    /// </summary>
    /// <param name="token">The end token to write.</param>
    protected virtual void WriteEnd(JsonToken token)
    {
    }

    /// <summary>
    /// Writes indent characters.
    /// </summary>
    protected virtual void WriteIndent()
    {
    }

    /// <summary>
    /// Writes the JSON value delimiter.
    /// </summary>
    protected virtual void WriteValueDelimiter()
    {
    }

    /// <summary>
    /// Writes an indent space.
    /// </summary>
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
          token = 7;
          break;
      }

      // gets new state based on the current state and what is being written
      State newState = stateArray[token, (int)_currentState];

      if (newState == State.Error)
        throw new JsonWriterException("Token {0} in state {1} would result in an invalid JavaScript object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), _currentState.ToString()));

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
    /// Writes raw JSON without changing the writer's state.
    /// </summary>
    /// <param name="json">The raw JSON to write.</param>
    public virtual void WriteRaw(string json)
    {
    }

    /// <summary>
    /// Writes raw JSON where a value is expected and updates the writer's state.
    /// </summary>
    /// <param name="json">The raw JSON to write.</param>
    public virtual void WriteRawValue(string json)
    {
      // hack. want writer to change state as if a value had been written
      AutoComplete(JsonToken.Undefined);
      WriteRaw(json);
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

    /// <summary>
    /// Writes a <see cref="Nullable{Int32}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Int32}"/> value to write.</param>
    public virtual void WriteValue(int? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{UInt32}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{UInt32}"/> value to write.</param>
    public virtual void WriteValue(uint? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Int64}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Int64}"/> value to write.</param>
    public virtual void WriteValue(long? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{UInt64}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{UInt64}"/> value to write.</param>
    public virtual void WriteValue(ulong? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Single}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Single}"/> value to write.</param>
    public virtual void WriteValue(float? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Double}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Double}"/> value to write.</param>
    public virtual void WriteValue(double? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Boolean}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Boolean}"/> value to write.</param>
    public virtual void WriteValue(bool? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Int16}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Int16}"/> value to write.</param>
    public virtual void WriteValue(short? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{UInt16}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{UInt16}"/> value to write.</param>
    public virtual void WriteValue(ushort? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Char}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Char}"/> value to write.</param>
    public virtual void WriteValue(char? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Byte}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Byte}"/> value to write.</param>
    public virtual void WriteValue(byte? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{SByte}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{SByte}"/> value to write.</param>
    public virtual void WriteValue(sbyte? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Decimal}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Decimal}"/> value to write.</param>
    public virtual void WriteValue(decimal? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{DateTime}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{DateTime}"/> value to write.</param>
    public virtual void WriteValue(DateTime? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{DateTimeOffset}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{DateTimeOffset}"/> value to write.</param>
    public virtual void WriteValue(DateTimeOffset? value)
    {
      if (value == null)
        WriteNull();
      else
        WriteValue(value.Value);
    }

    /// <summary>
    /// Writes a <see cref="Object"/> value.
    /// An error will raised if the value cannot be written as a single JSON token.
    /// </summary>
    /// <param name="value">The <see cref="Object"/> value to write.</param>
    public virtual void WriteValue(object value)
    {
      if (value == null)
      {
        WriteNull();
        return;
      }
      else if (value is IConvertible)
      {
        IConvertible convertible = value as IConvertible;

        switch (convertible.GetTypeCode())
        {
          case TypeCode.String:
            WriteValue(convertible.ToString(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Char:
            WriteValue(convertible.ToChar(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Boolean:
            WriteValue(convertible.ToBoolean(CultureInfo.InvariantCulture));
            return;
          case TypeCode.SByte:
            WriteValue(convertible.ToSByte(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Int16:
            WriteValue(convertible.ToInt16(CultureInfo.InvariantCulture));
            return;
          case TypeCode.UInt16:
            WriteValue(convertible.ToUInt16(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Int32:
            WriteValue(convertible.ToInt32(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Byte:
            WriteValue(convertible.ToByte(CultureInfo.InvariantCulture));
            return;
          case TypeCode.UInt32:
            WriteValue(convertible.ToUInt32(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Int64:
            WriteValue(convertible.ToInt64(CultureInfo.InvariantCulture));
            return;
          case TypeCode.UInt64:
            WriteValue(convertible.ToUInt64(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Single:
            WriteValue(convertible.ToSingle(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Double:
            WriteValue(convertible.ToDouble(CultureInfo.InvariantCulture));
            return;
          case TypeCode.DateTime:
            WriteValue(convertible.ToDateTime(CultureInfo.InvariantCulture));
            return;
          case TypeCode.Decimal:
            WriteValue(convertible.ToDecimal(CultureInfo.InvariantCulture));
            return;
          case TypeCode.DBNull:
            WriteNull();
            return;
        }
      }
      else if (value is DateTimeOffset)
      {
        WriteValue((DateTimeOffset)value);
        return;
      }

      throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
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