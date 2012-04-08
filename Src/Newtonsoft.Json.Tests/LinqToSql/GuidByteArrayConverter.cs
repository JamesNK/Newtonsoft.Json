using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.LinqToSql
{
  public class GuidByteArrayConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Guid guid = (Guid) value;
      writer.WriteValue(Convert.ToBase64String(guid.ToByteArray()));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      string encodedData = (string) reader.Value;
      byte[] data = Convert.FromBase64String(encodedData);
      return new Guid(data);
    }

    public override bool CanConvert(Type objectType)
    {
      return (objectType == typeof (Guid));
    }
  }
}