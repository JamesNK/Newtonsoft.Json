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

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE40 || PORTABLE)
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

      writer.WriteStartArray();

      foreach (DataRow row in table.Rows)
      {
        writer.WriteStartObject();
        foreach (DataColumn column in row.Table.Columns)
        {
          if (serializer.NullValueHandling == NullValueHandling.Ignore && (row[column] == null || row[column] == DBNull.Value))
            continue;

          writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName);
          serializer.Serialize(writer, row[column]);
        }
        writer.WriteEndObject();
      }

      writer.WriteEndArray();
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
      DataTable dt = existingValue as DataTable;

      if (dt == null)
      {
        // handle typed datasets
        dt = (objectType == typeof(DataTable))
               ? new DataTable()
               : (DataTable)Activator.CreateInstance(objectType);
      }

      if (reader.TokenType == JsonToken.PropertyName)
      {
        dt.TableName = (string)reader.Value;

        reader.Read();
      }

      reader.Read();

      while (reader.TokenType == JsonToken.StartObject)
      {
        DataRow dr = dt.NewRow();
        reader.Read();

        while (reader.TokenType == JsonToken.PropertyName)
        {
          string columnName = (string)reader.Value;

          reader.Read();

          if (!dt.Columns.Contains(columnName))
          {
            Type columnType = GetColumnDataType(reader.TokenType);
            dt.Columns.Add(new DataColumn(columnName, columnType));
          }

          dr[columnName] = reader.Value ?? DBNull.Value;
          reader.Read();
        }

        dr.EndEdit();
        dt.Rows.Add(dr);

        reader.Read();
      }

      return dt;
    }

    private static Type GetColumnDataType(JsonToken tokenType)
    {
      switch (tokenType)
      {
        case JsonToken.Integer:
          return typeof (long);
        case JsonToken.Float:
          return typeof (double);
        case JsonToken.String:
        case JsonToken.Null:
        case JsonToken.Undefined:
          return typeof (string);
        case JsonToken.Boolean:
          return typeof (bool);
        case JsonToken.Date:
          return typeof (DateTime);
        default:
          throw new ArgumentOutOfRangeException();
      }
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