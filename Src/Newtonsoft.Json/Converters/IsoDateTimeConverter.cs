using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  public class IsoDateTimeConverter : JsonConverter
  {
    private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

    public DateTimeStyles DateTimeStyles
    {
      get { return _dateTimeStyles; }
      set { _dateTimeStyles = value; }
    }

    public override void WriteJson(JsonWriter writer, object value)
    {
      string text;

      if (value is DateTime)
      {
        DateTime dateTime = (DateTime)value;

        if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
          || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
          dateTime = dateTime.ToUniversalTime();

        text = dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
      }
      else
      {
        DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
        if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
          || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
          dateTimeOffset = dateTimeOffset.ToUniversalTime();

        text = dateTimeOffset.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
      }

      writer.WriteValue(text);
    }

    public override object ReadJson(JsonReader reader, Type objectType)
    {
      if (reader.TokenType != JsonToken.String)
        throw new Exception("Unexpected token parsing date. Expected String, got {0}.".FormatWith(reader.TokenType));

      string dateText = reader.Value.ToString();

      if (objectType == typeof(DateTimeOffset))
        return DateTimeOffset.Parse(dateText, CultureInfo.InvariantCulture, _dateTimeStyles);

      return DateTime.Parse(dateText, CultureInfo.InvariantCulture, _dateTimeStyles);
    }

    public override bool CanConvert(Type objectType)
    {
      return (typeof(DateTime).IsAssignableFrom(objectType)
        || typeof(DateTimeOffset).IsAssignableFrom(objectType));
    }
  }
}