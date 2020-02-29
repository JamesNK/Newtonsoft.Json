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

#if HAVE_ADO_NET
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System;
using System.Data;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="DataTable"/> to and from JSON.
    /// </summary>
    public class DataTableConverter : JsonConverter
    {
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			DataTable table = (DataTable)value;
			DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

			writer.WriteStartObject();

			writer.WritePropertyName("Columns");
			serializer.Serialize(writer, GetColumnDataTypes(table));

			writer.WritePropertyName("Rows");
			writer.WriteStartArray();

			foreach (DataRow row in table.Rows)
			{
				serializer.Serialize(writer, row.ItemArray);
			}

			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			DataTable dataTable = existingValue as DataTable;

			if (dataTable == null)
			{
				// handle typed datasets
				dataTable = (objectType == typeof(DataTable))
						? new DataTable()
						: (DataTable)Activator.CreateInstance(objectType);
			}

			// DataTable is inside a DataSet
			// populate the name from the property name
			if (reader.TokenType == JsonToken.PropertyName)
			{
				dataTable.TableName = (string)reader.Value;

				reader.Read();

				if (reader.TokenType == JsonToken.Null)
				{
					return dataTable;
				}
			}

			if (reader.TokenType == JsonToken.StartObject)
			{
				reader.Read();
				if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "Columns")
				{
					reader.Read();

					Dictionary<string, string> columnTypes = new Dictionary<string, string>();
					columnTypes = serializer.Deserialize<Dictionary<string, string>>(reader);

					foreach (KeyValuePair<string, string> column in columnTypes)
					{
						dataTable.Columns.Add(column.Key, Type.GetType(column.Value));
					}
				}
				reader.Read();
				reader.Read();
			}

			if (reader.TokenType != JsonToken.StartArray)
			{
				throw new JsonSerializationException($"Unexpected JSON token when reading DataTable. Expected StartArray, got {reader.TokenType}.");
			}

			reader.Read();

			while (reader.TokenType != JsonToken.EndArray)
			{
				DataRow dr = dataTable.NewRow();
				dr.ItemArray = serializer.Deserialize<System.Object[]>(reader);
				dataTable.Rows.Add(dr);

				reader.Read();
			}

			reader.Read();

			return dataTable;
		}

		private static Dictionary<string, string> GetColumnDataTypes(DataTable dt)
		{
			Dictionary<string, string> columnTypes = new Dictionary<string, string>();
			foreach (DataColumn column in dt.Columns)
				columnTypes.Add(column.ColumnName, column.DataType.FullName);

			return columnTypes;
		}

		/// <summary>
		/// Determines whether this instance can convert the specified value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type valueType)
		{
			return typeof(DataTable).IsAssignableFrom(valueType);
		}
    }
}

#endif
