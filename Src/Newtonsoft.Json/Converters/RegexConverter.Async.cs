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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class RegexConverterImpl
    {
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadJsonAsync(reader, serializer, cancellationToken);
        }

        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteJsonAsync(writer, value, serializer, cancellationToken);
        }
    }

    public partial class RegexConverter
    {
        private bool SafeAsync => GetType() == typeof(RegexConverter);

        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync
                ? DoWriteJsonAsync(writer, value, serializer, cancellationToken)
                : base.WriteJsonAsync(writer, value, serializer, cancellationToken);
        }

        internal Task DoWriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            Regex regex = (Regex)value;

            BsonWriter bsonWriter = writer as BsonWriter;
            if (bsonWriter != null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return cancellationToken.CancelledAsync();
                }

                WriteBson(bsonWriter, regex);
                return AsyncUtils.CompletedTask;
            }

            return WriteJsonAsync(writer, regex, serializer, cancellationToken);
        }

        private static async Task WriteJsonAsync(JsonWriter writer, Regex regex, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(PatternName) : PatternName, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(regex.ToString(), cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(OptionsName) : OptionsName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, regex.Options, cancellationToken).ConfigureAwait(false);
            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the Result property is the object read.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync
                ? DoReadJsonAsync(reader, serializer, cancellationToken) 
                : base.ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        internal Task<object> DoReadJsonAsync(JsonReader reader, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return cancellationToken.CancelledAsync<object>();

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadRegexObjectAsync(reader, serializer, cancellationToken);
                case JsonToken.String:
                    return Task.FromResult(ReadRegexString(reader));
                case JsonToken.Null:
                    return Task.FromResult<object>(null);
            }

            throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex.");
        }

        private static async Task<object> ReadRegexObjectAsync(JsonReader reader, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            string pattern = null;
            RegexOptions? options = null;

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                        }

                        if (string.Equals(propertyName, PatternName, StringComparison.OrdinalIgnoreCase))
                        {
                            pattern = (string)reader.Value;
                        }
                        else if (string.Equals(propertyName, OptionsName, StringComparison.OrdinalIgnoreCase))
                        {
                            options = await serializer.DeserializeAsync<RegexOptions>(reader, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        if (pattern == null)
                        {
                            throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
                        }

                        return new Regex(pattern, options ?? RegexOptions.None);
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
        }
    }
}

#endif
