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
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    public abstract partial class JsonConverter
    {
        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <remarks>
        /// The default implementation simply calls the synchronous <see cref="WriteJson(JsonWriter, object?, JsonSerializer)"/> method.
        /// Sub classes must override this method to implement true async functionality.
        /// </remarks>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public virtual Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            WriteJson(writer, value, serializer);
            return AsyncUtils.CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <remarks>
        /// The default implementation simply calls the synchronous <see cref="ReadJson(JsonReader, Type, object?, JsonSerializer)"/> method.
        /// Sub classes must override this method to implement true async functionality.
        /// </remarks>
        /// <returns>The object value.</returns>
        public virtual Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadJson(reader, objectType, existingValue, serializer));
        }
    }

    public abstract partial class JsonConverter<T> : JsonConverter
    {
        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public sealed override Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            if (!(value != null ? value is T : ReflectionUtils.IsNullable(typeof(T))))
            {
                throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
            }
            return WriteJsonAsync(writer, (T?)value, serializer);
        }

        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <remarks>
        /// The default implementation simply calls the synchronous <see cref="WriteJson(JsonWriter, object?, JsonSerializer)"/> method.
        /// Sub classes must override this method to impelement true async functionality.
        /// </remarks>
        public virtual Task WriteJsonAsync(JsonWriter writer, T? value, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            WriteJson(writer, value, serializer);
            return AsyncUtils.CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The object value.</returns>
        public sealed override async Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            bool existingIsNull = existingValue == null;
            if (!(existingIsNull || existingValue is T))
            {
                throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
            }
            return await ReadJsonAsync(reader, objectType, existingIsNull ? default : (T?)existingValue, !existingIsNull, serializer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <remarks>
        /// The default implementation simply calls the synchronous <see cref="ReadJson(JsonReader, Type, T?, bool, JsonSerializer)"/> method.
        /// Sub classes must override this method to implement true async functionality.
        /// </remarks>
        /// <returns>The object value.</returns>
        public virtual Task<T?> ReadJsonAsync(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadJson(reader, objectType, existingValue, hasExistingValue, serializer));
        }
    }
}
#endif