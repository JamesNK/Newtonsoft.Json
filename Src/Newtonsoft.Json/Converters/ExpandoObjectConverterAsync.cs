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
#if HAVE_DYNAMIC

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    public partial class ExpandoObjectConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public override Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            // can write is set to false
            return AsyncUtils.CompletedTask;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The object value.</returns>
        public override Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            return ReadValueAsync(reader, cancellationToken);
        }

        private async Task<object?> ReadValueAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            if (!await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false))
            {
                throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return await ReadObjectAsync(reader, cancellationToken).ConfigureAwait(false);
                case JsonToken.StartArray:
                    return await ReadListAsync(reader, cancellationToken).ConfigureAwait(false);
                default:
                    if (JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                    {
                        return reader.Value;
                    }

                    throw JsonSerializationException.Create(reader, "Unexpected token when converting ExpandoObject: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private async Task<object> ReadListAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            IList<object?> list = new List<object?>();

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        object? v = await ReadValueAsync(reader, cancellationToken).ConfigureAwait(false);

                        list.Add(v);
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
        }

        private async Task<object> ReadObjectAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            IDictionary<string, object?> expandoObject = new ExpandoObject();

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value!.ToString()!;

                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
                        }

                        object? v = await ReadValueAsync(reader, cancellationToken).ConfigureAwait(false);

                        expandoObject[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return expandoObject;
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
        }
    }
}

#endif
#endif