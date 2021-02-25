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

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    public partial class JConstructor
    {
        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/> asynchronously.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
        public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            await writer.WriteStartConstructorAsync(_name ?? string.Empty, cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < _values.Count; i++)
            {
                await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(false);
            }

            await writer.WriteEndConstructorAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that represents the asynchronous load. The <see cref="Task{TResult}.Result"/>
        /// property returns a <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static Task<JConstructor> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
        /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that represents the asynchronous load. The <see cref="Task{TResult}.Result"/>
        /// property returns a <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static async Task<JConstructor> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
                }
            }

            await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.StartConstructor)
            {
                throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JConstructor c = new JConstructor((string)reader.Value!);
            c.SetLineInfo(reader as IJsonLineInfo, settings);

            await c.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            return c;
        }
    }
}

#endif
