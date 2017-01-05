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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    public partial class JsonSerializer
    {
        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SerializeAsync(TextWriter textWriter, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SerializeAsync(textWriter, value, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifing the type is optional.
        /// </param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SerializeAsync(TextWriter textWriter, object value, Type objectType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SerializeAsync(new JsonTextWriterImpl(textWriter), value, objectType, cancellationToken);
        }


        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SerializeAsync(JsonWriter jsonWriter, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SerializeInternalAsync(jsonWriter, value, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifing the type is optional.
        /// </param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task SerializeAsync(JsonWriter jsonWriter, object value, Type objectType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SerializeInternalAsync(jsonWriter, value, objectType, cancellationToken);
        }

        internal virtual async Task SerializeInternalAsync(JsonWriter jsonWriter, object value, Type objectType, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(jsonWriter, nameof(jsonWriter));

            cancellationToken.ThrowIfCancellationRequested();

            // set serialization options onto writer
            Formatting? previousFormatting = null;
            if (_formatting != null && jsonWriter.Formatting != _formatting)
            {
                previousFormatting = jsonWriter.Formatting;
                jsonWriter.Formatting = _formatting.GetValueOrDefault();
            }

            DateFormatHandling? previousDateFormatHandling = null;
            if (_dateFormatHandling != null && jsonWriter.DateFormatHandling != _dateFormatHandling)
            {
                previousDateFormatHandling = jsonWriter.DateFormatHandling;
                jsonWriter.DateFormatHandling = _dateFormatHandling.GetValueOrDefault();
            }

            DateTimeZoneHandling? previousDateTimeZoneHandling = null;
            if (_dateTimeZoneHandling != null && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
                jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
            }

            FloatFormatHandling? previousFloatFormatHandling = null;
            if (_floatFormatHandling != null && jsonWriter.FloatFormatHandling != _floatFormatHandling)
            {
                previousFloatFormatHandling = jsonWriter.FloatFormatHandling;
                jsonWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
            }

            StringEscapeHandling? previousStringEscapeHandling = null;
            if (_stringEscapeHandling != null && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
            {
                previousStringEscapeHandling = jsonWriter.StringEscapeHandling;
                jsonWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
            }

            CultureInfo previousCulture = null;
            if (_culture != null && !_culture.Equals(jsonWriter.Culture))
            {
                previousCulture = jsonWriter.Culture;
                jsonWriter.Culture = _culture;
            }

            string previousDateFormatString = null;
            if (_dateFormatStringSet && jsonWriter.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = jsonWriter.DateFormatString;
                jsonWriter.DateFormatString = _dateFormatString;
            }

            TraceJsonWriter traceJsonWriter = TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose ? new TraceJsonWriter(jsonWriter) : null;

            JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
            await serializerWriter.SerializeAsync(traceJsonWriter ?? jsonWriter, value, objectType, cancellationToken).ConfigureAwait(false);

            if (traceJsonWriter != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
            }

            // reset writer back to previous options
            if (previousFormatting != null)
            {
                jsonWriter.Formatting = previousFormatting.GetValueOrDefault();
            }
            if (previousDateFormatHandling != null)
            {
                jsonWriter.DateFormatHandling = previousDateFormatHandling.GetValueOrDefault();
            }
            if (previousDateTimeZoneHandling != null)
            {
                jsonWriter.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousFloatFormatHandling != null)
            {
                jsonWriter.FloatFormatHandling = previousFloatFormatHandling.GetValueOrDefault();
            }
            if (previousStringEscapeHandling != null)
            {
                jsonWriter.StringEscapeHandling = previousStringEscapeHandling.GetValueOrDefault();
            }
            if (_dateFormatStringSet)
            {
                jsonWriter.DateFormatString = previousDateFormatString;
            }
            if (previousCulture != null)
            {
                jsonWriter.Culture = previousCulture;
            }
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The <see cref="Task{TResult}.Result"/>
        /// property returns the instance of <typeparamref name="T"/> being deserialized.</returns>
        public async Task<T> DeserializeAsync<T>(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (T)await DeserializeAsync(reader, typeof(T), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="object"/>  being deserialized.</returns>
        public Task<object> DeserializeAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeserializeInternalAsync(reader, null, cancellationToken);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The <see cref="Task{TResult}.Result"/>
        /// property returns the instance of <paramref name="objectType"/> being deserialized.</returns>
        public Task<object> DeserializeAsync(JsonReader reader, Type objectType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeserializeInternalAsync(reader, objectType, cancellationToken);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The <see cref="Task{TResult}.Result"/>
        /// property returns the instance of <paramref name="objectType"/> being deserialized.</returns>
        public Task<object> DeserializeAsync(TextReader reader, Type objectType, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeserializeAsync(new JsonTextReaderImpl(reader), objectType, cancellationToken);
        }

        internal virtual async Task<object> DeserializeInternalAsync(JsonReader reader, Type objectType, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            cancellationToken.ThrowIfCancellationRequested();

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose ? new TraceJsonReader(reader) : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            object value = await serializerReader.DeserializeAsync(traceJsonReader ?? reader, objectType, CheckAdditionalContent, cancellationToken).ConfigureAwait(false);

            if (traceJsonReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        /// <summary>
        /// Asynchronously populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task PopulateAsync(TextReader reader, object target, CancellationToken cancellationToken = default(CancellationToken))
        {
            return PopulateAsync(new JsonTextReaderImpl(reader), target, cancellationToken);
        }

        /// <summary>
        /// Asynchronously populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task PopulateAsync(JsonReader reader, object target, CancellationToken cancellationToken = default(CancellationToken))
        {
            return PopulateInternalAsync(reader, target, cancellationToken);
        }

        internal virtual async Task PopulateInternalAsync(JsonReader reader, object target, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose ? new TraceJsonReader(reader) : null;

            await new JsonSerializerInternalReader(this).PopulateAsync(traceJsonReader ?? reader, target, cancellationToken).ConfigureAwait(false);

            if (traceJsonReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }
    }
}

#endif
