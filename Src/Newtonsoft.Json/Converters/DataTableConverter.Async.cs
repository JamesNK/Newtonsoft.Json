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

#if !(NET20 || NET35 || PORTABLE40 || DOTNET || PORTABLE)

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class DataTableConverterImpl
    {
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteJsonAsync(writer, value, serializer, cancellationToken);
        }

        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }
    }
    public partial class DataTableConverter
    {
        private bool SafeAsync => GetType() == typeof(DataTableConverter);

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

        internal async Task DoWriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DataTable table = (DataTable)value;
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

            foreach (DataRow row in table.Rows)
            {
                await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
                foreach (DataColumn column in row.Table.Columns)
                {
                    object columnValue = row[column];

                    if (serializer.NullValueHandling == NullValueHandling.Ignore && (columnValue == null || columnValue == DBNull.Value))
                    {
                        continue;
                    }

                    await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName, cancellationToken).ConfigureAwait(false);
                    await serializer.SerializeAsync(writer, columnValue, cancellationToken).ConfigureAwait(false);
                }
                await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
            }

            await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
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
                ? DoReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken)
                : base.ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        internal Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            return reader.TokenType == JsonToken.Null
                ? cancellationToken.CancelledOrNullAsync()
                : ReadJsonNotNullAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        private async Task<object> ReadJsonNotNullAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DataTable dt = existingValue as DataTable ??
                (objectType == typeof(DataTable) ? new DataTable() : (DataTable)Activator.CreateInstance(objectType));

            // DataTable is inside a DataSet
            // populate the name from the property name
            if (reader.TokenType == JsonToken.PropertyName)
            {
                dt.TableName = (string)reader.Value;

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                if (reader.TokenType == JsonToken.Null)
                {
                    return dt;
                }
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw JsonSerializationException.Create(reader, "Unexpected JSON token when reading DataTable. Expected StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            while (reader.TokenType != JsonToken.EndArray)
            {
                await CreateRowAsync(reader, dt, serializer, cancellationToken).ConfigureAwait(false);

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            return dt;
        }
        private static async Task CreateRowAsync(JsonReader reader, DataTable dt, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            DataRow dr = dt.NewRow();

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string columnName = (string)reader.Value;

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                DataColumn column = dt.Columns[columnName];
                if (column == null)
                {
                    Type columnType = GetColumnDataType(reader);
                    column = new DataColumn(columnName, columnType);
                    dt.Columns.Add(column);
                }

                if (column.DataType == typeof(DataTable))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    DataTable nestedDt = new DataTable();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        await CreateRowAsync(reader, nestedDt, serializer, cancellationToken).ConfigureAwait(false);

                        await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    dr[columnName] = nestedDt;
                }
                else if (column.DataType.IsArray && column.DataType != typeof(byte[]))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    List<object> o = new List<object>();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        o.Add(reader.Value);
                        await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    Array destinationArray = Array.CreateInstance(column.DataType.GetElementType(), o.Count);
                    Array.Copy(o.ToArray(), destinationArray, o.Count);

                    dr[columnName] = destinationArray;
                }
                else
                {
                    object columnValue = reader.Value != null
                        ? serializer.Deserialize(reader, column.DataType) ?? DBNull.Value
                        : DBNull.Value;

                    dr[columnName] = columnValue;
                }

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            dr.EndEdit();
            dt.Rows.Add(dr);
        }
    }
}

#endif
