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

#if HAVE_ADO_NET
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters
{
    public partial class DataSetConverter : JsonConverter
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

            DataSet dataSet = (DataSet)value;
            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            DataTableConverter converter = new DataTableConverter();

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);

            foreach (DataTable table in dataSet.Tables)
            {
                await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(table.TableName) : table.TableName, cancellationToken).ConfigureAwait(false);

                await converter.WriteJsonAsync(writer, table, serializer, cancellationToken).ConfigureAwait(false);
            }

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
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            // handle typed datasets
            DataSet ds = (objectType == typeof(DataSet))
                ? new DataSet()
                : (DataSet)Activator.CreateInstance(objectType)!;

            DataTableConverter converter = new DataTableConverter();

            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                DataTable? dt = ds.Tables[(string)reader.Value!];
                bool exists = (dt != null);

                dt = (DataTable)(await converter.ReadJsonAsync(reader, typeof(DataTable), dt, serializer, cancellationToken).ConfigureAwait(false))!;

                if (!exists)
                {
                    ds.Tables.Add(dt);
                }

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            return ds;
        }

    }
}

#endif
#endif