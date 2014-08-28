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
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

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
        internal char _quoteChar;
        internal State _currentState;
        internal ReadType _readType;
        private JsonPosition _currentPosition;
        private CultureInfo _culture;
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private int? _maxDepth;
        private bool _hasExceededMaxDepth;
        internal DateParseHandling _dateParseHandling;
        internal FloatParseHandling _floatParseHandling;
        private string _dateFormatString;
        private readonly List<JsonPosition> _stack;

        /// <summary>
        /// Gets the current reader state.
        /// </summary>
        /// <value>The current reader state.</value>
        protected State CurrentState
        {
            get { return _currentState; }
        }

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
        /// Gets or sets a value indicating whether multiple pieces of JSON content can
        /// be read from a continuous stream without erroring.
        /// </summary>
        /// <value>
        /// true to support reading multiple pieces of JSON content; otherwise false. The default is false.
        /// </value>
        public bool SupportMultipleContent { get; set; }

        /// <summary>
        /// Gets the quotation mark character used to enclose the value of a string.
        /// </summary>
        public virtual char QuoteChar
        {
            get { return _quoteChar; }
            protected internal set { _quoteChar = value; }
        }

        /// <summary>
        /// Get or set how <see cref="DateTime"/> time zones are handling when reading JSON.
        /// </summary>
        public DateTimeZoneHandling DateTimeZoneHandling
        {
            get { return _dateTimeZoneHandling; }
            set { _dateTimeZoneHandling = value; }
        }

        /// <summary>
        /// Get or set how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON.
        /// </summary>
        public DateParseHandling DateParseHandling
        {
            get { return _dateParseHandling; }
            set { _dateParseHandling = value; }
        }

        /// <summary>
        /// Get or set how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public FloatParseHandling FloatParseHandling
        {
            get { return _floatParseHandling; }
            set { _floatParseHandling = value; }
        }

        /// <summary>
        /// Get or set how custom date formatted strings are parsed when reading JSON.
        /// </summary>
        public string DateFormatString
        {
            get { return _dateFormatString; }
            set { _dateFormatString = value; }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException"/>.
        /// </summary>
        public int? MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive.", "value");

                _maxDepth = value;
            }
        }

        /// <summary>
        /// Gets the type of the current JSON token. 
        /// </summary>
        public virtual JsonToken TokenType
        {
            get { return _tokenType; }
        }

        /// <summary>
        /// Gets the text value of the current JSON token.
        /// </summary>
        public virtual object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets The Common Language Runtime (CLR) type for the current JSON token.
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
                int depth = _stack.Count;
                if (IsStartToken(TokenType) || _currentPosition.Type == JsonContainerType.None)
                    return depth;
                else
                    return depth + 1;
            }
        }

        /// <summary>
        /// Gets the path of the current JSON token. 
        /// </summary>
        public virtual string Path
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

        /// <summary>
        /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? CultureInfo.InvariantCulture; }
            set { _culture = value; }
        }

        internal JsonPosition GetPosition(int depth)
        {
            if (depth < _stack.Count)
                return _stack[depth];

            return _currentPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReader"/> class with the specified <see cref="TextReader"/>.
        /// </summary>
        protected JsonReader()
        {
            _currentState = State.Start;
            _stack = new List<JsonPosition>(4);
            _dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            _dateParseHandling = DateParseHandling.DateTime;
            _floatParseHandling = FloatParseHandling.Double;

            CloseInput = true;
        }

        private void Push(JsonContainerType value)
        {
            UpdateScopeWithFinishedValue();

            if (_currentPosition.Type == JsonContainerType.None)
            {
                _currentPosition = new JsonPosition(value);
            }
            else
            {
                _stack.Add(_currentPosition);
                _currentPosition = new JsonPosition(value);

                // this is a little hacky because Depth increases when first property/value is written but only testing here is faster/simpler
                if (_maxDepth != null && Depth + 1 > _maxDepth && !_hasExceededMaxDepth)
                {
                    _hasExceededMaxDepth = true;
                    throw JsonReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, _maxDepth));
                }
            }
        }

        private JsonContainerType Pop()
        {
            JsonPosition oldPosition;
            if (_stack.Count > 0)
            {
                oldPosition = _currentPosition;
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            }
            else
            {
                oldPosition = _currentPosition;
                _currentPosition = new JsonPosition();
            }

            if (_maxDepth != null && Depth <= _maxDepth)
                _hasExceededMaxDepth = false;

            return oldPosition.Type;
        }

        private JsonContainerType Peek()
        {
            return _currentPosition.Type;
        }

        /// <summary>
        /// Reads the next JSON token from the stream.
        /// </summary>
        /// <returns>true if the next token was read successfully; false if there are no more tokens to read.</returns>
        public abstract bool Read();

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{Int32}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{Int32}"/>. This method will return <c>null</c> at the end of an array.</returns>
        public abstract int? ReadAsInt32();

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public abstract string ReadAsString();

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
        /// </summary>
        /// <returns>A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
        public abstract byte[] ReadAsBytes();

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{Decimal}"/>. This method will return <c>null</c> at the end of an array.</returns>
        public abstract decimal? ReadAsDecimal();

        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTime}"/>.
        /// </summary>
        /// <returns>A <see cref="String"/>. This method will return <c>null</c> at the end of an array.</returns>
        public abstract DateTime? ReadAsDateTime();

#if !NET20
        /// <summary>
        /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
        /// </summary>
        /// <returns>A <see cref="Nullable{DateTimeOffset}"/>. This method will return <c>null</c> at the end of an array.</returns>
        public abstract DateTimeOffset? ReadAsDateTimeOffset();
#endif

        internal virtual bool ReadInternal()
        {
            throw new NotImplementedException();
        }

#if !NET20
        internal DateTimeOffset? ReadAsDateTimeOffsetInternal()
        {
            _readType = ReadType.ReadAsDateTimeOffset;

            JsonToken t;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            if (t == JsonToken.Date)
            {
                if (Value is DateTime)
                    SetToken(JsonToken.Date, new DateTimeOffset((DateTime)Value), false);

                return (DateTimeOffset)Value;
            }

            if (t == JsonToken.Null)
                return null;

            if (t == JsonToken.String)
            {
                string s = (string)Value;
                if (string.IsNullOrEmpty(s))
                {
                    SetToken(JsonToken.Null);
                    return null;
                }

                object temp;
                DateTimeOffset dt;
                if (DateTimeUtils.TryParseDateTime(s, DateParseHandling.DateTimeOffset, DateTimeZoneHandling, _dateFormatString, Culture, out temp))
                {
                    dt = (DateTimeOffset)temp;
                    SetToken(JsonToken.Date, dt, false);
                    return dt;
                }

                if (DateTimeOffset.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
                {
                    SetToken(JsonToken.Date, dt, false);
                    return dt;
                }
                
                throw JsonReaderException.Create(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
            }

            if (t == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }
#endif

        internal byte[] ReadAsBytesInternal()
        {
            _readType = ReadType.ReadAsBytes;

            JsonToken t;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            if (IsWrappedInTypeObject())
            {
                byte[] data = ReadAsBytes();
                ReadInternal();
                SetToken(JsonToken.Bytes, data, false);
                return data;
            }

            // attempt to convert possible base 64 string to bytes
            if (t == JsonToken.String)
            {
                string s = (string)Value;

                byte[] data;

                Guid g;
                if (s.Length == 0)
                {
                    data = new byte[0];
                }
                else if (ConvertUtils.TryConvertGuid(s, out g))
                {
                    data = g.ToByteArray();
                }
                else
                {
                    data = Convert.FromBase64String(s);
                }

                SetToken(JsonToken.Bytes, data, false);
                return data;
            }

            if (t == JsonToken.Null)
                return null;

            if (t == JsonToken.Bytes)
            {
                if (ValueType == typeof(Guid))
                {
                    byte[] data = ((Guid)Value).ToByteArray();
                    SetToken(JsonToken.Bytes, data, false);
                    return data;
                }

                return (byte[])Value;
            }

            if (t == JsonToken.StartArray)
            {
                List<byte> data = new List<byte>();

                while (ReadInternal())
                {
                    t = TokenType;
                    switch (t)
                    {
                        case JsonToken.Integer:
                            data.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
                            break;
                        case JsonToken.EndArray:
                            byte[] d = data.ToArray();
                            SetToken(JsonToken.Bytes, d, false);
                            return d;
                        case JsonToken.Comment:
                            // skip
                            break;
                        default:
                            throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
                    }
                }

                throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
            }

            if (t == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal decimal? ReadAsDecimalInternal()
        {
            _readType = ReadType.ReadAsDecimal;

            JsonToken t;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            if (t == JsonToken.Integer || t == JsonToken.Float)
            {
                if (!(Value is decimal))
                    SetToken(JsonToken.Float, Convert.ToDecimal(Value, CultureInfo.InvariantCulture), false);

                return (decimal)Value;
            }

            if (t == JsonToken.Null)
                return null;

            if (t == JsonToken.String)
            {
                string s = (string)Value;
                if (string.IsNullOrEmpty(s))
                {
                    SetToken(JsonToken.Null);
                    return null;
                }

                decimal d;
                if (decimal.TryParse(s, NumberStyles.Number, Culture, out d))
                {
                    SetToken(JsonToken.Float, d, false);
                    return d;
                }
                else
                {
                    throw JsonReaderException.Create(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
                }
            }

            if (t == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading decimal. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal int? ReadAsInt32Internal()
        {
            _readType = ReadType.ReadAsInt32;

            JsonToken t;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            if (t == JsonToken.Integer || t == JsonToken.Float)
            {
                if (!(Value is int))
                    SetToken(JsonToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture), false);

                return (int)Value;
            }

            if (t == JsonToken.Null)
                return null;

            int i;
            if (t == JsonToken.String)
            {
                string s = (string)Value;
                if (string.IsNullOrEmpty(s))
                {
                    SetToken(JsonToken.Null);
                    return null;
                }

                if (int.TryParse(s, NumberStyles.Integer, Culture, out i))
                {
                    SetToken(JsonToken.Integer, i, false);
                    return i;
                }
                else
                {
                    throw JsonReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
                }
            }

            if (t == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
        }

        internal string ReadAsStringInternal()
        {
            _readType = ReadType.ReadAsString;

            JsonToken t;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == JsonToken.Comment);

            if (t == JsonToken.String)
                return (string)Value;

            if (t == JsonToken.Null)
                return null;

            if (IsPrimitiveToken(t))
            {
                if (Value != null)
                {
                    string s;
                    if (Value is IFormattable)
                        s = ((IFormattable)Value).ToString(null, Culture);
                    else
                        s = Value.ToString();

                    SetToken(JsonToken.String, s, false);
                    return s;
                }
            }

            if (t == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal DateTime? ReadAsDateTimeInternal()
        {
            _readType = ReadType.ReadAsDateTime;

            do
            {
                if (!ReadInternal())
                {
                    SetToken(JsonToken.None);
                    return null;
                }
            } while (TokenType == JsonToken.Comment);

            if (TokenType == JsonToken.Date)
                return (DateTime)Value;

            if (TokenType == JsonToken.Null)
                return null;

            if (TokenType == JsonToken.String)
            {
                string s = (string)Value;
                if (string.IsNullOrEmpty(s))
                {
                    SetToken(JsonToken.Null);
                    return null;
                }

                DateTime dt;
                object temp;
                if (DateTimeUtils.TryParseDateTime(s, DateParseHandling.DateTime, DateTimeZoneHandling, _dateFormatString, Culture, out temp))
                {
                    dt = (DateTime)temp;
                    dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
                    SetToken(JsonToken.Date, dt, false);
                    return dt;
                }

                if (DateTime.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
                {
                    dt = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
                    SetToken(JsonToken.Date, dt, false);
                    return dt;
                }

                throw JsonReaderException.Create(this, "Could not convert string to DateTime: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
            }

            if (TokenType == JsonToken.EndArray)
                return null;

            throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
        }

        private bool IsWrappedInTypeObject()
        {
            _readType = ReadType.Read;

            if (TokenType == JsonToken.StartObject)
            {
                if (!ReadInternal())
                    throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");

                if (Value.ToString() == JsonTypeReflector.TypePropertyName)
                {
                    ReadInternal();
                    if (Value != null && Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
                    {
                        ReadInternal();
                        if (Value.ToString() == JsonTypeReflector.ValuePropertyName)
                        {
                            return true;
                        }
                    }
                }

                throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
            }

            return false;
        }

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
            SetToken(newToken, null, true);
        }

        /// <summary>
        /// Sets the current token and value.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <param name="value">The value.</param>
        protected void SetToken(JsonToken newToken, object value)
        {
            SetToken(newToken, value, true);
        }

        internal void SetToken(JsonToken newToken, object value, bool updateIndex)
        {
            _tokenType = newToken;
            _value = value;

            switch (newToken)
            {
                case JsonToken.StartObject:
                    _currentState = State.ObjectStart;
                    Push(JsonContainerType.Object);
                    break;
                case JsonToken.StartArray:
                    _currentState = State.ArrayStart;
                    Push(JsonContainerType.Array);
                    break;
                case JsonToken.StartConstructor:
                    _currentState = State.ConstructorStart;
                    Push(JsonContainerType.Constructor);
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

                    _currentPosition.PropertyName = (string)value;
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
                    SetPostValueState(updateIndex);
                    break;
            }
        }

        internal void SetPostValueState(bool updateIndex)
        {
            if (Peek() != JsonContainerType.None)
                _currentState = State.PostValue;
            else
                SetFinished();

            if (updateIndex)
                UpdateScopeWithFinishedValue();
        }

        private void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex)
                _currentPosition.Position++;
        }

        private void ValidateEnd(JsonToken endToken)
        {
            JsonContainerType currentObject = Pop();

            if (GetTypeForCloseToken(endToken) != currentObject)
                throw JsonReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));

            if (Peek() != JsonContainerType.None)
                _currentState = State.PostValue;
            else
                SetFinished();
        }

        /// <summary>
        /// Sets the state based on current token type.
        /// </summary>
        protected void SetStateBasedOnCurrent()
        {
            JsonContainerType currentObject = Peek();

            switch (currentObject)
            {
                case JsonContainerType.Object:
                    _currentState = State.Object;
                    break;
                case JsonContainerType.Array:
                    _currentState = State.Array;
                    break;
                case JsonContainerType.Constructor:
                    _currentState = State.Constructor;
                    break;
                case JsonContainerType.None:
                    SetFinished();
                    break;
                default:
                    throw JsonReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, currentObject));
            }
        }

        private void SetFinished()
        {
            if (SupportMultipleContent)
                _currentState = State.Start;
            else
                _currentState = State.Finished;
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
                default:
                    return false;
            }
        }

        private JsonContainerType GetTypeForCloseToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    return JsonContainerType.Object;
                case JsonToken.EndArray:
                    return JsonContainerType.Array;
                case JsonToken.EndConstructor:
                    return JsonContainerType.Constructor;
                default:
                    throw JsonReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
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
            _tokenType = JsonToken.None;
            _value = null;
        }
    }
}