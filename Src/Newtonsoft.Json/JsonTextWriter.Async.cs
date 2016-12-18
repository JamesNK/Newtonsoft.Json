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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Globalization;
using System.Threading;
#if !PORTABLE || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    public partial class JsonTextWriter
    {
        private bool SafeAsync => GetType() == typeof(JsonTextWriter);

        /// <summary>
        /// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoFlushAsync(cancellationToken) : base.FlushAsync(cancellationToken);
        }

        internal Task DoFlushAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.CancelIfRequesedAsync() ?? _writer.FlushAsync();
        }

        /// <summary>
        /// Asynchronously writes the JSON value delimiter.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        protected override Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            return SafeAsync ? DoWriteValueDelimiterAsync(cancellationToken) : base.WriteValueDelimiterAsync(cancellationToken);
        }

        internal Task DoWriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(',', cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            return SafeAsync ? DoWriteEndAsync(token, cancellationToken) : base.WriteEndAsync(token, cancellationToken);
        }

        internal Task DoWriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    return _writer.WriteAsync('}', cancellationToken);
                case JsonToken.EndArray:
                    return _writer.WriteAsync(']', cancellationToken);
                case JsonToken.EndConstructor:
                    return _writer.WriteAsync(')', cancellationToken);
                default:
                    throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
            }
        }

        /// <summary>
        /// Asynchronously closes this writer.
        /// If <see cref="JsonWriter.CloseOutput"/> is set to <c>true</c>, the destination is also closed.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoCloseAsync(cancellationToken) : base.CloseAsync(cancellationToken);
        }

        internal async Task DoCloseAsync(CancellationToken cancellationToken)
        {
            while (Top > 0)
            {
                await WriteEndAsync(cancellationToken).ConfigureAwait(false);
            }

            if (_writeBuffer != null)
            {
                BufferUtils.ReturnBuffer(_arrayPool, _writeBuffer);
                _writeBuffer = null;
            }

            if (CloseOutput)
            {
#if !(DOTNET || PORTABLE40 || PORTABLE)
                _writer?.Close();
#else
                _writer?.Dispose();
#endif
            }
        }

        /// <summary>
        /// Asynchronously writes the end of the current JSON object or array.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteEndInternalAsync(cancellationToken) : base.WriteEndAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes indent characters.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        protected override Task WriteIndentAsync(CancellationToken cancellationToken)
        {
            return SafeAsync ? DoWriteIndentAsync(cancellationToken) : base.WriteIndentAsync(cancellationToken);
        }

        internal async Task DoWriteIndentAsync(CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync(cancellationToken).ConfigureAwait(false);

            // levels of indentation multiplied by the indent count
            int currentIndentCount = Top * _indentation;

            if (currentIndentCount > 0)
            {
                if (_indentChars == null)
                {
                    _indentChars = new string(_indentChar, 10).ToCharArray();
                }

                while (currentIndentCount > 0)
                {
                    int writeCount = Math.Min(currentIndentCount, 10);

                    await _writer.WriteAsync(_indentChars, 0, writeCount, cancellationToken).ConfigureAwait(false);

                    currentIndentCount -= writeCount;
                }
            }
        }

        private Task WriteValueInternalAsync(string value, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes an indent space.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        protected override Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            return SafeAsync ? DoWriteIndentSpaceAsync(cancellationToken) : base.WriteIndentSpaceAsync(cancellationToken);
        }

        internal Task DoWriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(' ', cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes raw JSON without changing the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteRawAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteRawAsync(json, cancellationToken) : base.WriteRawAsync(json, cancellationToken);
        }

        internal Task DoWriteRawAsync(string json, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(json, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a null value.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteNullAsync(cancellationToken) : base.WriteNullAsync(cancellationToken);
        }

        internal async Task DoWriteNullAsync(CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Null, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.Null, cancellationToken).ConfigureAwait(false);
        }

        private Task WriteDigitsAsync(ulong uvalue, CancellationToken cancellationToken)
        {
            if (uvalue <= 9)
            {
                return _writer.WriteAsync((char)('0' + uvalue), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return cancellationToken.CancelledAsync();

            EnsureWriteBuffer();

            int totalLength = MathUtils.IntLength(uvalue);
            int length = 0;

            do
            {
                _writeBuffer[totalLength - ++length] = (char)('0' + uvalue % 10);
                uvalue /= 10;
            } while (uvalue != 0);

            return _writer.WriteAsync(_writeBuffer, 0, length, cancellationToken);
        }

        internal async Task WriteIntegerValueAsync(long value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Integer, cancellationToken).ConfigureAwait(false);
            if (value < 0)
            {
                await _writer.WriteAsync('-', cancellationToken).ConfigureAwait(false);
            }

            await WriteDigitsAsync(value < 0 ? (ulong)-value : (ulong)value, cancellationToken).ConfigureAwait(false);
        }

        internal async Task WriteIntegerValueAsync(ulong uvalue, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Integer, cancellationToken).ConfigureAwait(false);
            await WriteDigitsAsync(uvalue, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteEscapedStringAsync(string value, bool quote, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureWriteBuffer();
            _writeBuffer = await JavaScriptUtils.WriteEscapedJavaScriptStringAsync(_writer, value, _quoteChar, quote, _charEscapeFlags, StringEscapeHandling, _arrayPool, _writeBuffer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the property name of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWritePropertyNameAsync(name, cancellationToken) : base.WritePropertyNameAsync(name, cancellationToken);
        }

        internal async Task DoWritePropertyNameAsync(string name, CancellationToken cancellationToken)
        {
            await InternalWritePropertyNameAsync(name, cancellationToken).ConfigureAwait(false);

            await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync(':', cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the property name of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWritePropertyNameAsync(name, escape, cancellationToken) : base.WritePropertyNameAsync(name, escape, cancellationToken);
        }

        internal async Task DoWritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken)
        {
            await InternalWritePropertyNameAsync(name, cancellationToken).ConfigureAwait(false);

            if (escape)
            {
                await WriteEscapedStringAsync(name, _quoteName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (_quoteName)
                {
                    await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
                }

                await _writer.WriteAsync(name, cancellationToken).ConfigureAwait(false);

                if (_quoteName)
                {
                    await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
                }
            }

            await _writer.WriteAsync(':', cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the beginning of a JSON array.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteStartArrayAsync(cancellationToken) : base.WriteStartArrayAsync(cancellationToken);
        }

        internal async Task DoWriteStartArrayAsync(CancellationToken cancellationToken)
        {
            await InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync('[', cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteWhitespaceAsync(ws, cancellationToken) : base.WriteWhitespaceAsync(ws, cancellationToken);
        }

        internal Task DoWriteWhitespaceAsync(string ws, CancellationToken cancellationToken)
        {
            InternalWriteWhitespace(ws);
            return _writer.WriteAsync(ws, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="bool"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(bool value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Boolean, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.ToString(value), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The <see cref="bool"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(bool? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="byte"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(byte? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/>[] value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(byte[] value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? (value == null ? WriteNullAsync(cancellationToken) : WriteValueNonNullAsync(value, cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task WriteValueNonNullAsync(byte[] value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Bytes, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
            await Base64Encoder.EncodeAsync(value, 0, value.Length, cancellationToken).ConfigureAwait(false);
            await Base64Encoder.FlushAsync(cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="char"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(char value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.ToString(value), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="char"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(char? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(DateTime value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(false);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                EnsureWriteBuffer();

                int pos = 0;
                _writeBuffer[pos++] = _quoteChar;
                pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, value, null, value.Kind, DateFormatHandling);
                _writeBuffer[pos++] = _quoteChar;

                await _writer.WriteAsync(_writeBuffer, 0, pos, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="DateTime"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(DateTime? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                EnsureWriteBuffer();

                int pos = 0;
                _writeBuffer[pos++] = _quoteChar;
                pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, DateFormatHandling == DateFormatHandling.IsoDateFormat ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, DateFormatHandling);
                _writeBuffer[pos++] = _quoteChar;

                await _writer.WriteAsync(_writeBuffer, 0, pos, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellationToken).ConfigureAwait(false);
                await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="decimal"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(decimal value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Float, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.ToString(value), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="decimal"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(decimal? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="double"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteValueAsync(value, false, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task WriteValueAsync(double value, bool nullable, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Float, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="double"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? (value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="float"/> value.
        /// </summary>
        /// <param name="value">The <see cref="float"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteValueAsync(value, false, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task WriteValueAsync(float value, bool nullable, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Float, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="float"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="float"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? (value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(Guid value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);

            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
#if !(DOTNET || PORTABLE40 || PORTABLE)
            await _writer.WriteAsync(value.ToString("D", CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
#else
            await _writer.WriteAsync(value.ToString("D"), cancellationToken).ConfigureAwait(false);
#endif
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Guid"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(Guid? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="int"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="int"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(int? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="long"/> value.
        /// </summary>
        /// <param name="value">The <see cref="long"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="long"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="long"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(long? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }


#if !(PORTABLE || PORTABLE40) || NETSTANDARD1_1
        internal async Task WriteValueAsync(BigInteger value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.Integer, cancellationToken).ConfigureAwait(false);
            await WriteValueInternalAsync(value.ToString(CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
        }
#endif

        /// <summary>
        /// Asynchronously writes a <see cref="object"/> value.
        /// </summary>
        /// <param name="value">The <see cref="object"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (SafeAsync)
            {
                if (value == null)
                {
                    return WriteNullAsync(cancellationToken);
                }
#if !(PORTABLE || PORTABLE40) || NETSTANDARD1_1
                if (value is BigInteger)
                {
                    return WriteValueAsync((BigInteger)value, cancellationToken);
                }
#endif

                return WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancellationToken);
            }

            return base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="sbyte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="sbyte"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="sbyte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="sbyte"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(sbyte? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="short"/> value.
        /// </summary>
        /// <param name="value">The <see cref="short"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="short"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="short"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(short? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(string value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);
            await (value == null ? WriteValueInternalAsync(JsonConvert.Null, cancellationToken) : WriteEscapedStringAsync(value, true, cancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task DoWriteValueAsync(TimeSpan value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(value.ToString(null, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(_quoteChar, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(TimeSpan? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="uint"/> value.
        /// </summary>
        /// <param name="value">The <see cref="uint"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="uint"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="uint"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(uint? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ulong"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="ulong"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="ulong"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(ulong? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteValueAsync(Uri value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? (value == null ? WriteNullAsync(cancellationToken) : WriteValueNotNullAsync(value, cancellationToken)) : base.WriteValueAsync(value, cancellationToken);
        }

        internal async Task WriteValueNotNullAsync(Uri value, CancellationToken cancellationToken)
        {
            await InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(false);
            await WriteEscapedStringAsync(value.OriginalString, true, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="ushort"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ushort"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? WriteIntegerValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="ushort"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="ushort"/> value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteValueAsync(value, cancellationToken) : base.WriteValueAsync(value, cancellationToken);
        }

        internal Task DoWriteValueAsync(ushort? value, CancellationToken cancellationToken)
        {
            return value == null ? DoWriteNullAsync(cancellationToken) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteCommentAsync(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteCommentAsync(text, cancellationToken) : base.WriteCommentAsync(text, cancellationToken);
        }

        internal async Task DoWriteCommentAsync(string text, CancellationToken cancellationToken)
        {
            await InternalWriteCommentAsync(cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync("/*", cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync(text, cancellationToken).ConfigureAwait(false);
            await _writer.WriteAsync("*/", cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the end of an array.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? InternalWriteEndAsync(JsonContainerType.Array, cancellationToken) : base.WriteEndArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the end of a constructor.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken) : base.WriteEndConstructorAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the end of a JSON object.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? InternalWriteEndAsync(JsonContainerType.Object, cancellationToken) : base.WriteEndObjectAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteRawValueAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync ? DoWriteRawValueAsync(json, cancellationToken) : base.WriteRawValueAsync(json, cancellationToken);
        }

        internal async Task DoWriteRawValueAsync(string json, CancellationToken cancellationToken)
        {
            UpdateScopeWithFinishedValue();
            await AutoCompleteAsync(JsonToken.Undefined, cancellationToken).ConfigureAwait(false);
            await WriteRawAsync(json, cancellationToken).ConfigureAwait(false);
        }
    }

    internal sealed partial class JsonTextWriterImpl
    {
        public override Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoFlushAsync(cancellationToken);
        }

        protected override Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
        {
            return DoWriteValueDelimiterAsync(cancellationToken);
        }

        protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            return DoWriteEndAsync(token, cancellationToken);
        }

        public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoCloseAsync(cancellationToken);
        }

        public override Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteEndInternalAsync(cancellationToken);
        }

        protected override Task WriteIndentAsync(CancellationToken cancellationToken)
        {
            return DoWriteIndentAsync(cancellationToken);
        }

        protected override Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
        {
            return DoWriteIndentSpaceAsync(cancellationToken);
        }

        public override Task WriteRawAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteRawAsync(json, cancellationToken);
        }

        public override Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteNullAsync(cancellationToken);
        }

        public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWritePropertyNameAsync(name, cancellationToken);
        }

        public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWritePropertyNameAsync(name, escape, cancellationToken);
        }

        public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteStartArrayAsync(cancellationToken);
        }

        public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteWhitespaceAsync(ws, cancellationToken);
        }

        public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(byte[] value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return value == null ? WriteNullAsync(cancellationToken) : WriteValueNonNullAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteValueAsync(value, false, cancellationToken);
        }

        public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken);
        }

        public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteValueAsync(value, false, cancellationToken);
        }

        public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellationToken) : WriteNullAsync(cancellationToken);
        }

        public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (value == null)
            {
                return WriteNullAsync(cancellationToken);
            }
#if !(PORTABLE || PORTABLE40) || NETSTANDARD1_1
            if (value is BigInteger)
            {
                return WriteValueAsync((BigInteger)value, cancellationToken);
            }
#endif

            return WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancellationToken);
        }

        public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(Uri value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return value == null ? WriteNullAsync(cancellationToken) : WriteValueNotNullAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteIntegerValueAsync(value, cancellationToken);
        }

        public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteValueAsync(value, cancellationToken);
        }

        public override Task WriteCommentAsync(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteCommentAsync(text, cancellationToken);
        }

        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return InternalWriteEndAsync(JsonContainerType.Array, cancellationToken);
        }

        public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken);
        }

        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return InternalWriteEndAsync(JsonContainerType.Object, cancellationToken);
        }

        public override Task WriteRawValueAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteRawValueAsync(json, cancellationToken);
        }
    }
}

#endif
