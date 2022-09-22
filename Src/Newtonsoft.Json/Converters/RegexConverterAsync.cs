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
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Threading;
using System.Threading.Tasks;


namespace Newtonsoft.Json.Converters
{
    public partial class RegexConverter : JsonConverter
    {

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public override async Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            Regex regex = (Regex)value;

#pragma warning disable 618
            if (writer is BsonWriter bsonWriter)
            {
                await WriteBsonAsync(bsonWriter, regex, cancellationToken).ConfigureAwait(false);
            }
#pragma warning restore 618
            else
            {
                await WriteJsonAsync(writer, regex, serializer, cancellationToken).ConfigureAwait(false);
            }
        }


#pragma warning disable 618
        private async Task WriteBsonAsync(BsonWriter writer, Regex regex, CancellationToken cancellationToken)
        {
            // Regular expression - The first cstring is the regex pattern, the second
            // is the regex options string. Options are identified by characters, which 
            // must be stored in alphabetical order. Valid options are 'i' for case 
            // insensitive matching, 'm' for multiline matching, 'x' for verbose mode, 
            // 'l' to make \w, \W, etc. locale dependent, 's' for dotall mode 
            // ('.' matches everything), and 'u' to make \w, \W, etc. match unicode.

            string? options = null;

            if (HasFlag(regex.Options, RegexOptions.IgnoreCase))
            {
                options += "i";
            }

            if (HasFlag(regex.Options, RegexOptions.Multiline))
            {
                options += "m";
            }

            if (HasFlag(regex.Options, RegexOptions.Singleline))
            {
                options += "s";
            }

            options += "u";

            if (HasFlag(regex.Options, RegexOptions.ExplicitCapture))
            {
                options += "x";
            }

            await writer.WriteRegexAsync(regex.ToString(), options, cancellationToken).ConfigureAwait(false);
        }
#pragma warning restore 618

        private async Task WriteJsonAsync(JsonWriter writer, Regex regex, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(PatternName) : PatternName, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(regex.ToString(), cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(OptionsName) : OptionsName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, regex.Options, typeof(RegexOptions), cancellationToken).ConfigureAwait(false);
            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
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
        public override async Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return await ReadRegexObjectAsync(reader, serializer, cancellationToken).ConfigureAwait(false);
                case JsonToken.String:
                    return ReadRegexString(reader);
                case JsonToken.Null:
                    return null;
            }

            throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex.");
        }


        private async Task<Regex> ReadRegexObjectAsync(JsonReader reader, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            string? pattern = null;
            RegexOptions? options = null;

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value!.ToString()!;

                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                        }

                        if (string.Equals(propertyName, PatternName, StringComparison.OrdinalIgnoreCase))
                        {
                            pattern = (string?)reader.Value;
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