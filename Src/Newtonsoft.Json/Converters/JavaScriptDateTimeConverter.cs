using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Newtonsoft.Json.Converters
{
  public class JavaScriptDateTimeConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value)
    {
      long ticks;

      if (value is DateTime)
      {
        DateTime dateTime = (DateTime)value;
        DateTime utcDateTime = dateTime.ToUniversalTime();
        ticks = JavaScriptConvert.ConvertDateTimeToJavaScriptTicks(utcDateTime);
      }
      else
      {
        DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
        DateTimeOffset utcDateTimeOffset = dateTimeOffset.ToUniversalTime();
        ticks = JavaScriptConvert.ConvertDateTimeToJavaScriptTicks(utcDateTimeOffset.UtcDateTime);
      }

      writer.WriteStartConstructor("Date");
      writer.WriteValue(ticks);
      writer.WriteEndConstructor();
    }

    public override object ReadJson(JsonReader reader, Type objectType)
    {
      if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
        throw new Exception(string.Format("Unexpected token or value when parsing date. Token: {0}, Value: {1}", reader.TokenType, reader.Value));

      reader.Read();

      if (reader.TokenType != JsonToken.Integer)
        throw new Exception(string.Format("Unexpected token parsing date. Expected Integer, got {0}.", reader.TokenType));

      long ticks = (long)reader.Value;

      DateTime d = JavaScriptConvert.ConvertJavaScriptTicksToDateTime(ticks);

      reader.Read();

      if (reader.TokenType != JsonToken.EndConstructor)
        throw new Exception(string.Format("Unexpected token parsing date. Expected EndConstructor, got {0}.", reader.TokenType));

      if (objectType == typeof(DateTimeOffset))
        return new DateTimeOffset(d);

      return d;
    }

    public override bool CanConvert(Type objectType)
    {
      return (typeof(DateTime).IsAssignableFrom(objectType)
        || typeof(DateTimeOffset).IsAssignableFrom(objectType));
    }
  }
}
