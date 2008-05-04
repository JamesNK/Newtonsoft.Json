using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="DateTime"/> to and from the ISO 8601 date format (e.g. 2008-04-12T12:53Z).
  /// </summary>
  public class IsoDateTimeConverter : JsonConverter
  {
    private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

    /// <summary>
    /// Gets or sets the date time styles used when converting a date to and from JSON.
    /// </summary>
    /// <value>The date time styles used when converting a date to and from JSON.</value>
    public DateTimeStyles DateTimeStyles
    {
      get { return _dateTimeStyles; }
      set { _dateTimeStyles = value; }
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
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

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType)
    {
      if (reader.TokenType != JsonToken.String)
        throw new Exception("Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      string dateText = reader.Value.ToString();

      if (objectType == typeof(DateTimeOffset))
        return DateTimeOffset.Parse(dateText, CultureInfo.InvariantCulture, _dateTimeStyles);

      return DateTime.Parse(dateText, CultureInfo.InvariantCulture, _dateTimeStyles);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
      return (typeof(DateTime).IsAssignableFrom(objectType)
        || typeof(DateTimeOffset).IsAssignableFrom(objectType));
    }
  }
}