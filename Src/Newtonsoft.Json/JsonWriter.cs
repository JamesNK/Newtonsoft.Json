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
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using Newtonsoft.Json.Utilities;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
    /// </summary>
    public abstract class JsonWriter : IDisposable
    {
        internal enum State
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
        private static readonly State[][] StateArray;

        internal static readonly State[][] StateArrayTempate = new[]
        {
            //                                      Start                    PropertyName            ObjectStart         Object            ArrayStart              Array                   ConstructorStart        Constructor             Closed          Error
            //
            /* None                        */new[] { State.Error,            State.Error,            State.Error,        State.Error,      State.Error,            State.Error,            State.Error,            State.Error,            State.Error,    State.Error },
            /* StartObject                 */new[] { State.ObjectStart,      State.ObjectStart,      State.Error,        State.Error,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.ObjectStart,      State.Error,    State.Error },
            /* StartArray                  */new[] { State.ArrayStart,       State.ArrayStart,       State.Error,        State.Error,      State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.ArrayStart,       State.Error,    State.Error },
            /* StartConstructor            */new[] { State.ConstructorStart, State.ConstructorStart, State.Error,        State.Error,      State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.ConstructorStart, State.Error,    State.Error },
            /* Property                    */new[] { State.Property,         State.Error,            State.Property,     State.Property,   State.Error,            State.Error,            State.Error,            State.Error,            State.Error,    State.Error },
            /* Comment                     */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
            /* Raw                         */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.ArrayStart,       State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error },
            /* Value (this will be copied) */new[] { State.Start,            State.Object,           State.Error,        State.Error,      State.Array,            State.Array,            State.Constructor,      State.Constructor,      State.Error,    State.Error }
        };

        internal static State[][] BuildStateArray()
        {
            var allStates = StateArrayTempate.ToList();
            var errorStates = StateArrayTempate[0];
            var valueStates = StateArrayTempate[7];

            foreach (JsonToken valueToken in EnumUtils.GetValues(typeof(JsonToken)))
            {
                if (allStates.Count <= (int)valueToken)
                {
                    switch (valueToken)
                    {
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.String:
                        case JsonToken.Boolean:
                        case JsonToken.Null:
                        case JsonToken.Undefined:
                        case JsonToken.Date:
                        case JsonToken.Bytes:
                            allStates.Add(valueStates);
                            break;
                        default:
                            allStates.Add(errorStates);
                            break;
                    }
                }
            }

            return allStates.ToArray();
        }

        static JsonWriter()
        {
            StateArray = BuildStateArray();
        }

        private readonly List<JsonPosition> _stack;
        private JsonPosition _currentPosition;
        private State _currentState;
        private Formatting _formatting;

        /// <summary>
        /// Gets or sets a value indicating whether the underlying stream or
        /// <see cref="TextReader"/> should be closed when the writer is closed.
        /// </summary>
        /// <value>
        /// true to close the underlying stream or <see cref="TextReader"/> when
        /// the writer is closed; otherwise false. The default is true.
        /// </value>
        public bool CloseOutput { get; set; }

        /// <summary>
        /// Gets the top.
        /// </summary>
        /// <value>The top.</value>
        protected internal int Top
        {
            get
            {
                int depth = _stack.Count;
                if (Peek() != JsonContainerType.None)
                    depth++;

                return depth;
            }
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
                        throw JsonWriterException.Create(this, "Invalid state: " + _currentState, null);
                }
            }
        }

        internal string ContainerPath
        {
            get
            {
                if (_currentPosition.Type == JsonContainerType.None)
                    return string.Empty;

                return JsonPosition.BuildPath(_stack);
            }
        }

        /// <summary>
        /// Gets the path of the writer. 
        /// </summary>
        public string Path
        {
            get
            {
                if (_currentPosition.Type == JsonContainerType.None)
                    return string.Empty;

                bool insideContainer = (_currentState != State.ArrayStart
                                        && _currentState != State.ConstructorStart
                                        && _currentState != State.ObjectStart);

                IEnumerable<JsonPosition> positions = (!insideContainer)
                    ? _stack
                    : _stack.Concat(new[] { _currentPosition });

                return JsonPosition.BuildPath(positions);
            }
        }

        private DateFormatHandling _dateFormatHandling;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private StringEscapeHandling _stringEscapeHandling;
        private FloatFormatHandling _floatFormatHandling;
        private string _dateFormatString;
        private CultureInfo _culture;

        /// <summary>
        /// Indicates how JSON text output is formatted.
        /// </summary>
        public Formatting Formatting
        {
            get { return _formatting; }
            set { _formatting = value; }
        }

        /// <summary>
        /// Get or set how dates are written to JSON text.
        /// </summary>
        public DateFormatHandling DateFormatHandling
        {
            get { return _dateFormatHandling; }
            set { _dateFormatHandling = value; }
        }

        /// <summary>
        /// Get or set how <see cref="DateTime"/> time zones are handling when writing JSON text.
        /// </summary>
        public DateTimeZoneHandling DateTimeZoneHandling
        {
            get { return _dateTimeZoneHandling; }
            set { _dateTimeZoneHandling = value; }
        }

        /// <summary>
        /// Get or set how strings are escaped when writing JSON text.
        /// </summary>
        public StringEscapeHandling StringEscapeHandling
        {
            get { return _stringEscapeHandling; }
            set
            {
                _stringEscapeHandling = value;
                OnStringEscapeHandlingChanged();
            }
        }

        internal virtual void OnStringEscapeHandlingChanged()
        {
            // hacky but there is a calculated value that relies on StringEscapeHandling
        }

        /// <summary>
        /// Get or set how special floating point numbers, e.g. <see cref="F:System.Double.NaN"/>,
        /// <see cref="F:System.Double.PositiveInfinity"/> and <see cref="F:System.Double.NegativeInfinity"/>,
        /// are written to JSON text.
        /// </summary>
        public FloatFormatHandling FloatFormatHandling
        {
            get { return _floatFormatHandling; }
            set { _floatFormatHandling = value; }
        }

        /// <summary>
        /// Get or set how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatting when writing JSON text.
        /// </summary>
        public string DateFormatString
        {
            get { return _dateFormatString; }
            set { _dateFormatString = value; }
        }

        /// <summary>
        /// Gets or sets the culture used when writing JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? CultureInfo.InvariantCulture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Creates an instance of the <c>JsonWriter</c> class. 
        /// </summary>
        protected JsonWriter()
        {
            _stack = new List<JsonPosition>(4);
            _currentState = State.Start;
            _formatting = Formatting.None;
            _dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

            CloseOutput = true;
        }

        internal void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex)
                _currentPosition.Position++;
        }

        private void Push(JsonContainerType value)
        {
            if (_currentPosition.Type != JsonContainerType.None)
                _stack.Add(_currentPosition);

            _currentPosition = new JsonPosition(value);
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition = _currentPosition;

            if (_stack.Count > 0)
            {
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            }
            else
            {
                _currentPosition = new JsonPosition();
            }

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return _currentPosition.Type;
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
            InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
        }

        /// <summary>
        /// Writes the end of a Json object.
        /// </summary>
        public virtual void WriteEndObject()
        {
            InternalWriteEnd(JsonContainerType.Object);
        }

        /// <summary>
        /// Writes the beginning of a Json array.
        /// </summary>
        public virtual void WriteStartArray()
        {
            InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
        }

        /// <summary>
        /// Writes the end of an array.
        /// </summary>
        public virtual void WriteEndArray()
        {
            InternalWriteEnd(JsonContainerType.Array);
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public virtual void WriteStartConstructor(string name)
        {
            InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
        }

        /// <summary>
        /// Writes the end constructor.
        /// </summary>
        public virtual void WriteEndConstructor()
        {
            InternalWriteEnd(JsonContainerType.Constructor);
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public virtual void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public virtual void WritePropertyName(string name, bool escape)
        {
            WritePropertyName(name);
        }

        /// <summary>
        /// Writes the end of the current Json object or array.
        /// </summary>
        public virtual void WriteEnd()
        {
            WriteEnd(Peek());
        }

        /// <summary>
        /// Writes the current <see cref="JsonReader"/> token and its children.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the token from.</param>
        public void WriteToken(JsonReader reader)
        {
            WriteToken(reader, true, true);
        }

        /// <summary>
        /// Writes the current <see cref="JsonReader"/> token.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the token from.</param>
        /// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
        public void WriteToken(JsonReader reader, bool writeChildren)
        {
            ValidationUtils.ArgumentNotNull(reader, "reader");

            WriteToken(reader, writeChildren, true);
        }

        internal void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate)
        {
            int initialDepth;

            if (reader.TokenType == JsonToken.None)
                initialDepth = -1;
            else if (!IsStartToken(reader.TokenType))
                initialDepth = reader.Depth + 1;
            else
                initialDepth = reader.Depth;

            WriteToken(reader, initialDepth, writeChildren, writeDateConstructorAsDate);
        }

        internal void WriteToken(JsonReader reader, int initialDepth, bool writeChildren, bool writeDateConstructorAsDate)
        {
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
                        if (writeDateConstructorAsDate && string.Equals(constructorName, "Date", StringComparison.Ordinal))
                            WriteConstructorDate(reader);
                        else
                            WriteStartConstructor(reader.Value.ToString());
                        break;
                    case JsonToken.PropertyName:
                        WritePropertyName(reader.Value.ToString());
                        break;
                    case JsonToken.Comment:
                        WriteComment((reader.Value != null) ? reader.Value.ToString() : null);
                        break;
                    case JsonToken.Integer:
#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
                        if (reader.Value is BigInteger)
                        {
                            WriteValue((BigInteger)reader.Value);
                        }
                        else
#endif
                        {
                            WriteValue(Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture));
                        }
                        break;
                    case JsonToken.Float:
                        object value = reader.Value;

                        if (value is decimal)
                            WriteValue((decimal)value);
                        else if (value is double)
                            WriteValue((double)value);
                        else if (value is float)
                            WriteValue((float)value);
                        else
                            WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.String:
                        WriteValue(reader.Value.ToString());
                        break;
                    case JsonToken.Boolean:
                        WriteValue(Convert.ToBoolean(reader.Value, CultureInfo.InvariantCulture));
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
#if !NET20
                        if (reader.Value is DateTimeOffset)
                            WriteValue((DateTimeOffset)reader.Value);
                        else
#endif
                            WriteValue(Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.Raw:
                        WriteRawValue((reader.Value != null) ? reader.Value.ToString() : null);
                        break;
                    case JsonToken.Bytes:
                        if (reader.Value is Guid)
                            WriteValue((Guid)reader.Value);
                        else
                            WriteValue((byte[])reader.Value);
                        break;
                    default:
                        throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", reader.TokenType, "Unexpected token type.");
                }
            } while (
                // stop if we have reached the end of the token being read
                initialDepth - 1 < reader.Depth - (IsEndToken(reader.TokenType) ? 1 : 0)
                && writeChildren
                && reader.Read());
        }

        private void WriteConstructorDate(JsonReader reader)
        {
            if (!reader.Read())
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            if (reader.TokenType != JsonToken.Integer)
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType, null);

            long ticks = (long)reader.Value;
            DateTime date = DateTimeUtils.ConvertJavaScriptTicksToDateTime(ticks);

            if (!reader.Read())
                throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
            if (reader.TokenType != JsonToken.EndConstructor)
                throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected EndConstructor, got " + reader.TokenType, null);

            WriteValue(date);
        }

        internal static bool IsEndToken(JsonToken token)
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

        internal static bool IsStartToken(JsonToken token)
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

        private void WriteEnd(JsonContainerType type)
        {
            switch (type)
            {
                case JsonContainerType.Object:
                    WriteEndObject();
                    break;
                case JsonContainerType.Array:
                    WriteEndArray();
                    break;
                case JsonContainerType.Constructor:
                    WriteEndConstructor();
                    break;
                default:
                    throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
            }
        }

        private void AutoCompleteAll()
        {
            while (Top > 0)
            {
                WriteEnd();
            }
        }

        private JsonToken GetCloseTokenForType(JsonContainerType type)
        {
            switch (type)
            {
                case JsonContainerType.Object:
                    return JsonToken.EndObject;
                case JsonContainerType.Array:
                    return JsonToken.EndArray;
                case JsonContainerType.Constructor:
                    return JsonToken.EndConstructor;
                default:
                    throw JsonWriterException.Create(this, "No close token for type: " + type, null);
            }
        }

        private void AutoCompleteClose(JsonContainerType type)
        {
            // write closing symbol and calculate new state
            int levelsToComplete = 0;

            if (_currentPosition.Type == type)
            {
                levelsToComplete = 1;
            }
            else
            {
                int top = Top - 2;
                for (int i = top; i >= 0; i--)
                {
                    int currentLevel = top - i;

                    if (_stack[currentLevel].Type == type)
                    {
                        levelsToComplete = i + 2;
                        break;
                    }
                }
            }

            if (levelsToComplete == 0)
                throw JsonWriterException.Create(this, "No token to close.", null);

            for (int i = 0; i < levelsToComplete; i++)
            {
                JsonToken token = GetCloseTokenForType(Pop());

                if (_currentState == State.Property)
                    WriteNull();

                if (_formatting == Formatting.Indented)
                {
                    if (_currentState != State.ObjectStart && _currentState != State.ArrayStart)
                        WriteIndent();
                }

                WriteEnd(token);

                JsonContainerType currentLevelType = Peek();

                switch (currentLevelType)
                {
                    case JsonContainerType.Object:
                        _currentState = State.Object;
                        break;
                    case JsonContainerType.Array:
                        _currentState = State.Array;
                        break;
                    case JsonContainerType.Constructor:
                        _currentState = State.Array;
                        break;
                    case JsonContainerType.None:
                        _currentState = State.Start;
                        break;
                    default:
                        throw JsonWriterException.Create(this, "Unknown JsonType: " + currentLevelType, null);
                }
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

        internal void AutoComplete(JsonToken tokenBeingWritten)
        {
            // gets new state based on the current state and what is being written
            State newState = StateArray[(int)tokenBeingWritten][(int)_currentState];

            if (newState == State.Error)
                throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), _currentState.ToString()), null);

            if ((_currentState == State.Object || _currentState == State.Array || _currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment)
            {
                WriteValueDelimiter();
            }

            if (_formatting == Formatting.Indented)
            {
                if (_currentState == State.Property)
                    WriteIndentSpace();

                // don't indent a property when it is the first token to be written (i.e. at the start)
                if ((_currentState == State.Array || _currentState == State.ArrayStart || _currentState == State.Constructor || _currentState == State.ConstructorStart)
                    || (tokenBeingWritten == JsonToken.PropertyName && _currentState != State.Start))
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
            InternalWriteValue(JsonToken.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public virtual void WriteUndefined()
        {
            InternalWriteValue(JsonToken.Undefined);
        }

        /// <summary>
        /// Writes raw JSON without changing the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public virtual void WriteRaw(string json)
        {
            InternalWriteRaw();
        }

        /// <summary>
        /// Writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public virtual void WriteRawValue(string json)
        {
            // hack. want writer to change state as if a value had been written
            UpdateScopeWithFinishedValue();
            AutoComplete(JsonToken.Undefined);
            WriteRaw(json);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public virtual void WriteValue(string value)
        {
            InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public virtual void WriteValue(int value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(uint value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public virtual void WriteValue(long value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ulong value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public virtual void WriteValue(float value)
        {
            InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public virtual void WriteValue(double value)
        {
            InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public virtual void WriteValue(bool value)
        {
            InternalWriteValue(JsonToken.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public virtual void WriteValue(short value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ushort value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public virtual void WriteValue(char value)
        {
            InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public virtual void WriteValue(byte value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(sbyte value)
        {
            InternalWriteValue(JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public virtual void WriteValue(decimal value)
        {
            InternalWriteValue(JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public virtual void WriteValue(DateTime value)
        {
            InternalWriteValue(JsonToken.Date);
        }

#if !NET20
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public virtual void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);
        }
#endif

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public virtual void WriteValue(Guid value)
        {
            InternalWriteValue(JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public virtual void WriteValue(TimeSpan value)
        {
            InternalWriteValue(JsonToken.String);
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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
        [CLSCompliant(false)]
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

#if !NET20
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
#endif

        /// <summary>
        /// Writes a <see cref="Nullable{Guid}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Guid}"/> value to write.</param>
        public virtual void WriteValue(Guid? value)
        {
            if (value == null)
                WriteNull();
            else
                WriteValue(value.Value);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{TimeSpan}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{TimeSpan}"/> value to write.</param>
        public virtual void WriteValue(TimeSpan? value)
        {
            if (value == null)
                WriteNull();
            else
                WriteValue(value.Value);
        }

        /// <summary>
        /// Writes a <see cref="T:Byte[]"/> value.
        /// </summary>
        /// <param name="value">The <see cref="T:Byte[]"/> value to write.</param>
        public virtual void WriteValue(byte[] value)
        {
            if (value == null)
                WriteNull();
            else
                InternalWriteValue(JsonToken.Bytes);
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public virtual void WriteValue(Uri value)
        {
            if (value == null)
                WriteNull();
            else
                InternalWriteValue(JsonToken.String);
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
            }
            else
            {
#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
                // this is here because adding a WriteValue(BigInteger) to JsonWriter will
                // mean the user has to add a reference to System.Numerics.dll
                if (value is BigInteger)
                    throw CreateUnsupportedTypeException(this, value);
#endif

                WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
            }
        }
        #endregion

        /// <summary>
        /// Writes out a comment <code>/*...*/</code> containing the specified text. 
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public virtual void WriteComment(string text)
        {
            InternalWriteComment();
        }

        /// <summary>
        /// Writes out the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public virtual void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_currentState != State.Closed)
                Close();
        }

        internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Char:
                    writer.WriteValue((char)value);
                    break;
                case PrimitiveTypeCode.CharNullable:
                    writer.WriteValue((value == null) ? (char?)null : (char)value);
                    break;
                case PrimitiveTypeCode.Boolean:
                    writer.WriteValue((bool)value);
                    break;
                case PrimitiveTypeCode.BooleanNullable:
                    writer.WriteValue((value == null) ? (bool?)null : (bool)value);
                    break;
                case PrimitiveTypeCode.SByte:
                    writer.WriteValue((sbyte)value);
                    break;
                case PrimitiveTypeCode.SByteNullable:
                    writer.WriteValue((value == null) ? (sbyte?)null : (sbyte)value);
                    break;
                case PrimitiveTypeCode.Int16:
                    writer.WriteValue((short)value);
                    break;
                case PrimitiveTypeCode.Int16Nullable:
                    writer.WriteValue((value == null) ? (short?)null : (short)value);
                    break;
                case PrimitiveTypeCode.UInt16:
                    writer.WriteValue((ushort)value);
                    break;
                case PrimitiveTypeCode.UInt16Nullable:
                    writer.WriteValue((value == null) ? (ushort?)null : (ushort)value);
                    break;
                case PrimitiveTypeCode.Int32:
                    writer.WriteValue((int)value);
                    break;
                case PrimitiveTypeCode.Int32Nullable:
                    writer.WriteValue((value == null) ? (int?)null : (int)value);
                    break;
                case PrimitiveTypeCode.Byte:
                    writer.WriteValue((byte)value);
                    break;
                case PrimitiveTypeCode.ByteNullable:
                    writer.WriteValue((value == null) ? (byte?)null : (byte)value);
                    break;
                case PrimitiveTypeCode.UInt32:
                    writer.WriteValue((uint)value);
                    break;
                case PrimitiveTypeCode.UInt32Nullable:
                    writer.WriteValue((value == null) ? (uint?)null : (uint)value);
                    break;
                case PrimitiveTypeCode.Int64:
                    writer.WriteValue((long)value);
                    break;
                case PrimitiveTypeCode.Int64Nullable:
                    writer.WriteValue((value == null) ? (long?)null : (long)value);
                    break;
                case PrimitiveTypeCode.UInt64:
                    writer.WriteValue((ulong)value);
                    break;
                case PrimitiveTypeCode.UInt64Nullable:
                    writer.WriteValue((value == null) ? (ulong?)null : (ulong)value);
                    break;
                case PrimitiveTypeCode.Single:
                    writer.WriteValue((float)value);
                    break;
                case PrimitiveTypeCode.SingleNullable:
                    writer.WriteValue((value == null) ? (float?)null : (float)value);
                    break;
                case PrimitiveTypeCode.Double:
                    writer.WriteValue((double)value);
                    break;
                case PrimitiveTypeCode.DoubleNullable:
                    writer.WriteValue((value == null) ? (double?)null : (double)value);
                    break;
                case PrimitiveTypeCode.DateTime:
                    writer.WriteValue((DateTime)value);
                    break;
                case PrimitiveTypeCode.DateTimeNullable:
                    writer.WriteValue((value == null) ? (DateTime?)null : (DateTime)value);
                    break;
#if !NET20
                case PrimitiveTypeCode.DateTimeOffset:
                    writer.WriteValue((DateTimeOffset)value);
                    break;
                case PrimitiveTypeCode.DateTimeOffsetNullable:
                    writer.WriteValue((value == null) ? (DateTimeOffset?)null : (DateTimeOffset)value);
                    break;
#endif
                case PrimitiveTypeCode.Decimal:
                    writer.WriteValue((decimal)value);
                    break;
                case PrimitiveTypeCode.DecimalNullable:
                    writer.WriteValue((value == null) ? (decimal?)null : (decimal)value);
                    break;
                case PrimitiveTypeCode.Guid:
                    writer.WriteValue((Guid)value);
                    break;
                case PrimitiveTypeCode.GuidNullable:
                    writer.WriteValue((value == null) ? (Guid?)null : (Guid)value);
                    break;
                case PrimitiveTypeCode.TimeSpan:
                    writer.WriteValue((TimeSpan)value);
                    break;
                case PrimitiveTypeCode.TimeSpanNullable:
                    writer.WriteValue((value == null) ? (TimeSpan?)null : (TimeSpan)value);
                    break;
#if !(PORTABLE || PORTABLE40 || NET35 || NET20)
                case PrimitiveTypeCode.BigInteger:
                    // this will call to WriteValue(object)
                    writer.WriteValue((BigInteger)value);
                    break;
                case PrimitiveTypeCode.BigIntegerNullable:
                    // this will call to WriteValue(object)
                    writer.WriteValue((value == null) ? (BigInteger?)null : (BigInteger)value);
                    break;
#endif
                case PrimitiveTypeCode.Uri:
                    writer.WriteValue((Uri)value);
                    break;
                case PrimitiveTypeCode.String:
                    writer.WriteValue((string)value);
                    break;
                case PrimitiveTypeCode.Bytes:
                    writer.WriteValue((byte[])value);
                    break;
#if !(PORTABLE || NETFX_CORE)
                case PrimitiveTypeCode.DBNull:
                    writer.WriteNull();
                    break;
#endif
                default:
#if !(PORTABLE || NETFX_CORE)
                    if (value is IConvertible)
                    {
                        // the value is a non-standard IConvertible
                        // convert to the underlying value and retry
                        IConvertible convertable = (IConvertible)value;

                        TypeInformation typeInformation = ConvertUtils.GetTypeInformation(convertable);

                        // if convertable has an underlying typecode of Object then attempt to convert it to a string
                        PrimitiveTypeCode resolvedTypeCode = (typeInformation.TypeCode == PrimitiveTypeCode.Object) ? PrimitiveTypeCode.String : typeInformation.TypeCode;
                        Type resolvedType = (typeInformation.TypeCode == PrimitiveTypeCode.Object) ? typeof(string) : typeInformation.Type;

                        object convertedValue = convertable.ToType(resolvedType, CultureInfo.InvariantCulture);

                        WriteValue(writer, resolvedTypeCode, convertedValue);
                        break;
                    }
                    else
#endif
                    {
                        throw CreateUnsupportedTypeException(writer, value);
                    }
            }
        }

        private static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value)
        {
            return JsonWriterException.Create(writer, "Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
        }

        /// <summary>
        /// Sets the state of the JsonWriter,
        /// </summary>
        /// <param name="token">The JsonToken being written.</param>
        /// <param name="value">The value being written.</param>
        protected void SetWriteState(JsonToken token, object value)
        {
            switch (token)
            {
                case JsonToken.StartObject:
                    InternalWriteStart(token, JsonContainerType.Object);
                    break;
                case JsonToken.StartArray:
                    InternalWriteStart(token, JsonContainerType.Array);
                    break;
                case JsonToken.StartConstructor:
                    InternalWriteStart(token, JsonContainerType.Constructor);
                    break;
                case JsonToken.PropertyName:
                    if (!(value is string))
                        throw new ArgumentException("A name is required when setting property name state.", "value");

                    InternalWritePropertyName((string)value);
                    break;
                case JsonToken.Comment:
                    InternalWriteComment();
                    break;
                case JsonToken.Raw:
                    InternalWriteRaw();
                    break;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Date:
                case JsonToken.Bytes:
                case JsonToken.Null:
                case JsonToken.Undefined:
                    InternalWriteValue(token);
                    break;
                case JsonToken.EndObject:
                    InternalWriteEnd(JsonContainerType.Object);
                    break;
                case JsonToken.EndArray:
                    InternalWriteEnd(JsonContainerType.Array);
                    break;
                case JsonToken.EndConstructor:
                    InternalWriteEnd(JsonContainerType.Constructor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("token");
            }
        }

        internal void InternalWriteEnd(JsonContainerType container)
        {
            AutoCompleteClose(container);
        }

        internal void InternalWritePropertyName(string name)
        {
            _currentPosition.PropertyName = name;
            AutoComplete(JsonToken.PropertyName);
        }

        internal void InternalWriteRaw()
        {
        }

        internal void InternalWriteStart(JsonToken token, JsonContainerType container)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
            Push(container);
        }

        internal void InternalWriteValue(JsonToken token)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
        }

        internal void InternalWriteWhitespace(string ws)
        {
            if (ws != null)
            {
                if (!StringUtils.IsWhiteSpace(ws))
                    throw JsonWriterException.Create(this, "Only white space characters should be used.", null);
            }
        }

        internal void InternalWriteComment()
        {
            AutoComplete(JsonToken.Comment);
        }
    }
}