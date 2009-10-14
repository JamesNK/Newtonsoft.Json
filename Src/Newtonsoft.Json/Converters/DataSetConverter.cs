#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Converters
{
  public class DataSetConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      DataSet dataSet = (DataSet)value;

      DataTableConverter converter = new DataTableConverter();

      writer.WriteStartObject();

      foreach (DataTable table in dataSet.Tables)
      {
        writer.WritePropertyName(table.TableName);
        
        converter.WriteJson(writer, table, serializer);
      }

      writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      DataSet ds = new DataSet();

      DataTableConverter converter = new DataTableConverter();

      reader.Read();

      while (reader.TokenType == JsonToken.PropertyName)
      {
        DataTable dt = (DataTable)converter.ReadJson(reader, typeof (DataTable), serializer);
        ds.Tables.Add(dt);
      }

      reader.Read();

      return ds;
    }

    public override bool CanConvert(Type valueType)
    {
      return typeof(DataSet).IsAssignableFrom(valueType);
    }
  }
}
#endif