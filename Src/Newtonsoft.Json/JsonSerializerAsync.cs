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
#if HAVE_ASYNC

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;

namespace Newtonsoft.Json
{
    public partial class JsonSerializer
    {


        /// <summary>
        /// Asynchronously populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> that contains the JSON structure to read values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        [DebuggerStepThrough]
        public Task PopulateAsync(TextReader reader, object target, CancellationToken cancellationToken = default)
        {
            return PopulateAsync(new JsonTextReader(reader), target, cancellationToken);
        }

        /// <summary>
        /// Asynchronously populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to read values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        [DebuggerStepThrough]
        public Task PopulateAsync(JsonReader reader, object target, CancellationToken cancellationToken = default)
        {
            return PopulateInternalAsync(reader, target, cancellationToken);
        }

        internal virtual async Task PopulateInternalAsync(JsonReader reader, object target, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            SetupReader(
                reader,
                out CultureInfo? previousCulture,
                out DateTimeZoneHandling? previousDateTimeZoneHandling,
                out DateParseHandling? previousDateParseHandling,
                out FloatParseHandling? previousFloatParseHandling,
                out int? previousMaxDepth,
                out string? previousDateFormatString);

            TraceJsonReader? traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            await serializerReader.PopulateAsync(traceJsonReader ?? reader, target, cancellationToken).ConfigureAwait(false);

            if (traceJsonReader != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }

        /// <summary>
        /// Asynchronously deserializes the JSON structure contained by the specified <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to deserialize.</param>
        /// <returns>The <see cref="Object"/> being deserialized.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        [DebuggerStepThrough]
        public Task<object?> DeserializeAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return DeserializeAsync(reader, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deserializes the JSON structure contained by the specified <see cref="TextReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        [DebuggerStepThrough]
        public Task<object?> DeserializeAsync(TextReader reader, Type objectType, CancellationToken cancellationToken = default)
        {
            return DeserializeAsync(new JsonTextReader(reader), objectType, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The instance of <typeparamref name="T"/> being deserialized.</returns>
        [DebuggerStepThrough]
        public async Task<T?> DeserializeAsync<T>(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return (T?)(await DeserializeAsync(reader, typeof(T), cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Asynchronously deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        [DebuggerStepThrough]
        public Task<object?> DeserializeAsync(JsonReader reader, Type? objectType, CancellationToken cancellationToken = default)
        {
            return DeserializeInternalAsync(reader, objectType, cancellationToken);
        }

        internal virtual async Task<object?> DeserializeInternalAsync(JsonReader reader, Type? objectType, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            SetupReader(
                reader,
                out CultureInfo? previousCulture,
                out DateTimeZoneHandling? previousDateTimeZoneHandling,
                out DateParseHandling? previousDateParseHandling,
                out FloatParseHandling? previousFloatParseHandling,
                out int? previousMaxDepth,
                out string? previousDateFormatString);

            TraceJsonReader? traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? CreateTraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            object? value = await serializerReader.DeserializeAsync(traceJsonReader ?? reader, objectType, CheckAdditionalContent, cancellationToken).ConfigureAwait(false);

            if (traceJsonReader != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>  
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public Task SerializeAsync(TextWriter textWriter, object? value, CancellationToken cancellationToken = default)
        {
            return SerializeAsync(new JsonTextWriter(textWriter), value, value?.GetType() ?? typeof(object), cancellationToken);
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
        /// Specifying the type is optional.
        /// </param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public Task SerializeAsync(TextWriter textWriter, object? value, Type objectType, CancellationToken cancellationToken = default)
        {
            return SerializeAsync(new JsonTextWriter(textWriter), value, objectType, cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="JsonSerializer.TypeNameHandling"/> is <see cref="Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public Task SerializeAsync(JsonWriter jsonWriter, object? value, Type? objectType, CancellationToken cancellationToken = default)
        {
            return SerializeInternalAsync(jsonWriter, value, objectType, cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public Task SerializeAsync(JsonWriter jsonWriter, object? value, CancellationToken cancellationToken = default)
        {
            return SerializeInternalAsync(jsonWriter, value, null, cancellationToken);
        }

        internal virtual async Task SerializeInternalAsync(JsonWriter jsonWriter, object? value, Type? objectType, CancellationToken cancellationToken)
        {
            Formatting? previousFormatting;
            DateFormatHandling? previousDateFormatHandling;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            FloatFormatHandling? previousFloatFormatHandling;
            StringEscapeHandling? previousStringEscapeHandling;
            CultureInfo? previousCulture;
            string? previousDateFormatString;
            TraceJsonWriter? traceJsonWriter;

            PreSerializeInternal(jsonWriter, out previousFormatting, out previousDateFormatHandling, out previousDateTimeZoneHandling, out previousFloatFormatHandling, out previousStringEscapeHandling, out previousCulture, out previousDateFormatString, out traceJsonWriter);

            JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
            await serializerWriter.SerializeAsync(traceJsonWriter ?? jsonWriter, value, objectType, cancellationToken).ConfigureAwait(false);

            if (traceJsonWriter != null)
            {
                TraceWriter!.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
            }

            PostSerializeInternal(jsonWriter, previousFormatting, previousDateFormatHandling, previousDateTimeZoneHandling, previousFloatFormatHandling, previousStringEscapeHandling, previousCulture, previousDateFormatString);
        }

        
    }
}
#endif