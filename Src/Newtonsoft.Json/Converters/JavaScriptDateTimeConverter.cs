using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="DateTime"/> to and from a JavaScript date constructor (e.g. new Date(52231943)).
  /// </summary>
  public class JavaScriptDateTimeConverter : JsonConverter
  {
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
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

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType)
    {
      if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
        throw new Exception("Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));

      reader.Read();

      if (reader.TokenType != JsonToken.Integer)
        throw new Exception("Unexpected token parsing date. Expected Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      long ticks = (long)reader.Value;

      DateTime d = JavaScriptConvert.ConvertJavaScriptTicksToDateTime(ticks);

      reader.Read();

      if (reader.TokenType != JsonToken.EndConstructor)
        throw new Exception("Unexpected token parsing date. Expected EndConstructor, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      if (objectType == typeof(DateTimeOffset))
        return new DateTimeOffset(d);

      return d;
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
      if (typeof(DateTime).IsAssignableFrom(objectType))
        return true;
      if (typeof(DateTimeOffset).IsAssignableFrom(objectType))
        return true;

      return false;
    }
  }
}