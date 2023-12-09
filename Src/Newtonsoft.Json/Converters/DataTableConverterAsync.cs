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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System;
using System.Data;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Converters
{
    public partial class DataTableConverter : JsonConverter
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

            DataTable table = (DataTable)value;
            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

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

                    await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName, cancellationToken).ConfigureAwait(false);
                    await serializer.SerializeAsync(writer, columnValue, columnValue?.GetType() ?? typeof(object), cancellationToken).ConfigureAwait(false);
                }
                await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
            }

            await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
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

            if (!(existingValue is DataTable dt))
            {
                // handle typed datasets
                dt = (objectType == typeof(DataTable))
                    ? new DataTable()
                    : (DataTable)Activator.CreateInstance(objectType)!;
            }

            // DataTable is inside a DataSet
            // populate the name from the property name
            if (reader.TokenType == JsonToken.PropertyName)
            {
                dt.TableName = (string)reader.Value!;

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                if (reader.TokenType == JsonToken.Null)
                {
                    return dt;
                }
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw JsonSerializationException.Create(reader, "Unexpected JSON token when reading DataTable. Expected StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            while (reader.TokenType != JsonToken.EndArray)
            {
                await CreateRowAsync(reader, dt, serializer, cancellationToken).ConfigureAwait(false);

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            return dt;
        }

        private static async Task CreateRowAsync(JsonReader reader, DataTable dt, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            DataRow dr = dt.NewRow();
            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string columnName = (string)reader.Value!;

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                DataColumn? column = dt.Columns[columnName];
                if (column == null)
                {
                    Type columnType = await GetColumnDataTypeAsync(reader, cancellationToken).ConfigureAwait(false);
                    column = new DataColumn(columnName, columnType);
                    dt.Columns.Add(column);
                }

                if (column.DataType == typeof(DataTable))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    DataTable nestedDt = new DataTable();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        CreateRow(reader, nestedDt, serializer);

                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    dr[columnName] = nestedDt;
                }
                else if (column.DataType.IsArray && column.DataType != typeof(byte[]))
                {
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    List<object?> o = new List<object?>();

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        o.Add(reader.Value);
                        await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    }

                    Array destinationArray = Array.CreateInstance(column.DataType.GetElementType()!, o.Count);
                    ((IList)o).CopyTo(destinationArray, 0);

                    dr[columnName] = destinationArray;
                }
                else
                {
                    object columnValue = (reader.Value != null)
                        ? serializer.Deserialize(reader, column.DataType) ?? DBNull.Value
                        : DBNull.Value;

                    dr[columnName] = columnValue;
                }

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            dr.EndEdit();
            dt.Rows.Add(dr);
        }

        private static async Task<Type> GetColumnDataTypeAsync(JsonReader reader, CancellationToken cancellationToken)
        {
            JsonToken tokenType = reader.TokenType;

            switch (tokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Boolean:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.ValueType!;
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.EndArray:
                    return typeof(string);
                case JsonToken.StartArray:
                    await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        return typeof(DataTable); // nested datatable
                    }

                    Type arrayType = await GetColumnDataTypeAsync(reader, cancellationToken).ConfigureAwait(false);
                    return arrayType.MakeArrayType();
                default:
                    throw JsonSerializationException.Create(reader, "Unexpected JSON token when reading DataTable: {0}".FormatWith(CultureInfo.InvariantCulture, tokenType));
            }
        }

    }
}

#endif
#endif