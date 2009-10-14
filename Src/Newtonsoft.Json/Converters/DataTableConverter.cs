#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Newtonsoft.Json.Converters
{
  public class DataTableConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      DataTable table = (DataTable)value;

      writer.WriteStartArray();

      foreach (DataRow row in table.Rows)
      {
        writer.WriteStartObject();
        foreach (DataColumn column in row.Table.Columns)
        {
          writer.WritePropertyName(column.ColumnName);
          serializer.Serialize(writer, row[column]);
        }
        writer.WriteEndObject();
      }

      writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      DataTable dt;

      if (reader.TokenType == JsonToken.PropertyName)
      {
        dt = new DataTable((string)reader.Value);
        reader.Read();
      }
      else
      {
        dt = new DataTable();
      }

      reader.Read();

      while (reader.TokenType == JsonToken.StartObject)
      {
        DataRow dr = dt.NewRow();
        reader.Read();

        while (reader.TokenType == JsonToken.PropertyName)
        {
          string columnName = (string) reader.Value;

          reader.Read();

          if (!dt.Columns.Contains(columnName))
          {
            Type columnType = GetColumnDataType(reader.TokenType);
            dt.Columns.Add(new DataColumn(columnName, columnType));
          }

          dr[columnName] = reader.Value;
          reader.Read();
        }

        dr.EndEdit();
        dt.Rows.Add(dr);

        reader.Read();
      }

      reader.Read();

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

    public override bool CanConvert(Type valueType)
    {
      return typeof(DataTable).IsAssignableFrom(valueType);
    }
  }
}
#endif