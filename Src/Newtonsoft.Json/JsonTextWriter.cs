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
using System.Globalization;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Text;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    public partial class JsonTextWriter : JsonWriter
    {
        private const int IndentCharBufferSize = 12;
        private readonly TextWriter _writer;
        private Base64Encoder _base64Encoder;
        private char _indentChar;
        private int _indentation;
        private char _quoteChar;
        private bool _quoteName;
        private bool[] _charEscapeFlags;
        private char[] _writeBuffer;
        private IArrayPool<char> _arrayPool;
        private char[] _indentChars;

        private Base64Encoder Base64Encoder
        {
            get
            {
                if (_base64Encoder == null)
                {
                    _base64Encoder = new Base64Encoder(_writer);
                }

                return _base64Encoder;
            }
        }

        /// <summary>
        /// Gets or sets the writer's character array pool.
        /// </summary>
        public IArrayPool<char> ArrayPool
        {
            get { return _arrayPool; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _arrayPool = value;
            }
        }

        /// <summary>
        /// Gets or sets how many <see cref="JsonTextWriter.IndentChar"/>s to write for each level in the hierarchy when <see cref="JsonWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
        /// </summary>
        public int Indentation
        {
            get { return _indentation; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Indentation value must be greater than 0.");
                }

                _indentation = value;
            }
        }

        /// <summary>
        /// Gets or sets which character to use to quote attribute values.
        /// </summary>
        public char QuoteChar
        {
            get { return _quoteChar; }
            set
            {
                if (value != '"' && value != '\'')
                {
                    throw new ArgumentException(@"Invalid JavaScript string quote character. Valid quote characters are ' and "".");
                }

                _quoteChar = value;
                UpdateCharEscapeFlags();
            }
        }

        /// <summary>
        /// Gets or sets which character to use for indenting when <see cref="JsonWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
        /// </summary>
        public char IndentChar
        {
            get { return _indentChar; }
            set
            {
                if (value != _indentChar)
                {
                    _indentChar = value;
                    _indentChars = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether object names will be surrounded with quotes.
        /// </summary>
        public bool QuoteName
        {
            get { return _quoteName; }
            set { _quoteName = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextWriter"/> class using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        public JsonTextWriter(TextWriter textWriter)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            _writer = textWriter;
            _quoteChar = '"';
            _quoteName = true;
            _indentChar = ' ';
            _indentation = 2;

            UpdateCharEscapeFlags();

#if HAVE_ASYNC
            _safeAsync = GetType() == typeof(JsonTextWriter);
#endif
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying <see cref="TextWriter"/> and also flushes the underlying <see cref="TextWriter"/>.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Closes this writer.
        /// If <see cref="JsonWriter.CloseOutput"/> is set to <c>true</c>, the underlying <see cref="TextWriter"/> is also closed.
        /// If <see cref="JsonWriter.AutoCompleteOnClose"/> is set to <c>true</c>, the JSON is auto-completed.
        /// </summary>
        public override void Close()
        {
            base.Close();

            CloseBufferAndWriter();
        }

        private void CloseBufferAndWriter()
        {
            if (_writeBuffer != null)
            {
                BufferUtils.ReturnBuffer(_arrayPool, _writeBuffer);
                _writeBuffer = null;
            }

            if (CloseOutput)
            {
#if HAVE_STREAM_READER_WRITER_CLOSE
                _writer?.Close();
#else
                _writer?.Dispose();
#endif
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public override void WriteStartObject()
        {
            InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

            _writer.Write('{');
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public override void WriteStartArray()
        {
            InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

            _writer.Write('[');
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public override void WriteStartConstructor(string name)
        {
            InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);

            _writer.Write("new ");
            _writer.Write(name);
            _writer.Write('(');
        }

        /// <summary>
        /// Writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        protected override void WriteEnd(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    _writer.Write('}');
                    break;
                case JsonToken.EndArray:
                    _writer.Write(']');
                    break;
                case JsonToken.EndConstructor:
                    _writer.Write(')');
                    break;
                default:
                    throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
            }
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);

            WriteEscapedString(name, _quoteName);

            _writer.Write(':');
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public override void WritePropertyName(string name, bool escape)
        {
            InternalWritePropertyName(name);

            if (escape)
            {
                WriteEscapedString(name, _quoteName);
            }
            else
            {
                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }

                _writer.Write(name);

                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }
            }

            _writer.Write(':');
        }

        internal override void OnStringEscapeHandlingChanged()
        {
            UpdateCharEscapeFlags();
        }

        private void UpdateCharEscapeFlags()
        {
            _charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(StringEscapeHandling, _quoteChar);
        }

        /// <summary>
        /// Writes indent characters.
        /// </summary>
        protected override void WriteIndent()
        {
            // levels of indentation multiplied by the indent count
            int currentIndentCount = Top * _indentation;

            int newLineLen = SetIndentChars();

            _writer.Write(_indentChars, 0, newLineLen + Math.Min(currentIndentCount, IndentCharBufferSize));

            while ((currentIndentCount -= IndentCharBufferSize) > 0)
            {
                _writer.Write(_indentChars, newLineLen, Math.Min(currentIndentCount, IndentCharBufferSize));
            }
        }

        private int SetIndentChars()
        {
            // Set _indentChars to be a newline followed by IndentCharBufferSize indent characters.
            string writerNewLine = _writer.NewLine;
            int newLineLen = writerNewLine.Length;
            bool match = _indentChars != null && _indentChars.Length == IndentCharBufferSize + newLineLen;
            if (match)
            {
                for (int i = 0; i != newLineLen; ++i)
                {
                    if (writerNewLine[i] != _indentChars[i])
                    {
                        match = false;
                        break;
                    }
                }
            }

            if (!match)
            {
                // If we're here, either _indentChars hasn't been set yet, or _writer.NewLine
                // has been changed, or _indentChar has been changed.
                _indentChars = (writerNewLine + new string(_indentChar, IndentCharBufferSize)).ToCharArray();
            }

            return newLineLen;
        }

        /// <summary>
        /// Writes the JSON value delimiter.
        /// </summary>
        protected override void WriteValueDelimiter()
        {
            _writer.Write(',');
        }

        /// <summary>
        /// Writes an indent space.
        /// </summary>
        protected override void WriteIndentSpace()
        {
            _writer.Write(' ');
        }

        private void WriteValueInternal(string value, JsonToken token)
        {
            _writer.Write(value);
        }

        #region WriteValue methods
        /// <summary>
        /// Writes a <see cref="Object"/> value.
        /// An error will raised if the value cannot be written as a single JSON token.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> value to write.</param>
        public override void WriteValue(object value)
        {
#if HAVE_BIG_INTEGER
            if (value is BigInteger)
            {
                InternalWriteValue(JsonToken.Integer);
                WriteValueInternal(((BigInteger)value).ToString(CultureInfo.InvariantCulture), JsonToken.String);
            }
            else
#endif
            {
                base.WriteValue(value);
            }
        }

        /// <summary>
        /// Writes a null value.
        /// </summary>
        public override void WriteNull()
        {
            InternalWriteValue(JsonToken.Null);
            WriteValueInternal(JsonConvert.Null, JsonToken.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            InternalWriteValue(JsonToken.Undefined);
            WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
        }

        /// <summary>
        /// Writes raw JSON.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            InternalWriteRaw();

            _writer.Write(json);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            InternalWriteValue(JsonToken.String);

            if (value == null)
            {
                WriteValueInternal(JsonConvert.Null, JsonToken.Null);
            }
            else
            {
                WriteEscapedString(value, true);
            }
        }

        private void WriteEscapedString(string value, bool quote)
        {
            EnsureWriteBuffer();
            JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, quote, _charEscapeFlags, StringEscapeHandling, _arrayPool, ref _writeBuffer);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value, false);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Single"/> value to write.</param>
        public override void WriteValue(float? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Double"/> value to write.</param>
        public override void WriteValue(double? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            InternalWriteValue(JsonToken.Boolean);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            InternalWriteValue(JsonToken.String);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            InternalWriteValue(JsonToken.Date);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                _writer.Write(_writeBuffer, 0, length);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }

        private int WriteValueToBuffer(DateTime value)
        {
            EnsureWriteBuffer();

            int pos = 0;
            _writeBuffer[pos++] = _quoteChar;
            pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, value, null, value.Kind, DateFormatHandling);
            _writeBuffer[pos++] = _quoteChar;
            return pos;
        }

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/>[] value to write.</param>
        public override void WriteValue(byte[] value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Bytes);
                _writer.Write(_quoteChar);
                Base64Encoder.Encode(value, 0, value.Length);
                Base64Encoder.Flush();
                _writer.Write(_quoteChar);
            }
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                int length = WriteValueToBuffer(value);

                _writer.Write(_writeBuffer, 0, length);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }

        private int WriteValueToBuffer(DateTimeOffset value)
        {
            EnsureWriteBuffer();

            int pos = 0;
            _writeBuffer[pos++] = _quoteChar;
            pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, (DateFormatHandling == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, DateFormatHandling);
            _writeBuffer[pos++] = _quoteChar;
            return pos;
        }
#endif

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            InternalWriteValue(JsonToken.String);

            string text = null;

#if HAVE_CHAR_TO_STRING_WITH_CULTURE
            text = value.ToString("D", CultureInfo.InvariantCulture);
#else
            text = value.ToString("D");
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            InternalWriteValue(JsonToken.String);

            string text;
#if !HAVE_TIME_SPAN_TO_STRING_WITH_CULTURE
            text = value.ToString();
#else
            text = value.ToString(null, CultureInfo.InvariantCulture);
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.String);
                WriteEscapedString(value.OriginalString, true);
            }
        }
        #endregion

        /// <summary>
        /// Writes a comment <c>/*...*/</c> containing the specified text. 
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            InternalWriteComment();

            _writer.Write("/*");
            _writer.Write(text);
            _writer.Write("*/");
        }

        /// <summary>
        /// Writes the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public override void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);

            _writer.Write(ws);
        }

        private void EnsureWriteBuffer()
        {
            if (_writeBuffer == null)
            {
                // maximum buffer sized used when writing iso date
                _writeBuffer = BufferUtils.RentBuffer(_arrayPool, 35);
            }
        }

        private void WriteIntegerValue(long value)
        {
            if (value >= 0 && value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                bool negative = value < 0;
                WriteIntegerValue(negative ? (ulong)-value : (ulong)value, negative);
            }
        }

        private void WriteIntegerValue(ulong uvalue, bool negative)
        {
            if (!negative & uvalue <= 9)
            {
                _writer.Write((char)('0' + uvalue));
            }
            else
            {
                int length = WriteNumberToBuffer(uvalue, negative);
                _writer.Write(_writeBuffer, 0, length);
            }
        }

        private int WriteNumberToBuffer(ulong value, bool negative)
        {
            EnsureWriteBuffer();

            int totalLength = MathUtils.IntLength(value);

            if (negative)
            {
                totalLength++;
                _writeBuffer[0] = '-';
            }

            int index = totalLength;

            do
            {
                _writeBuffer[--index] = (char)('0' + value % 10);
                value /= 10;
            } while (value != 0);

            return totalLength;
        }
    }
}