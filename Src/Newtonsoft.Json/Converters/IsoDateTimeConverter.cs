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
    private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

    private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
    private string _dateTimeFormat;
    private CultureInfo _culture;

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
    /// Gets or sets the date time format used when converting a date to and from JSON.
    /// </summary>
    /// <value>The date time format used when converting a date to and from JSON.</value>
    public string DateTimeFormat
    {
      get { return _dateTimeFormat ?? string.Empty; }
      set { _dateTimeFormat = StringUtils.NullEmptyString(value); }
    }

    /// <summary>
    /// Gets or sets the culture used when converting a date to and from JSON.
    /// </summary>
    /// <value>The culture used when converting a date to and from JSON.</value>
    public CultureInfo Culture
    {
      get { return _culture ?? CultureInfo.CurrentCulture; }
      set { _culture = value; }
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

        text = dateTime.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);
      }
      else if (value is DateTimeOffset)
      {
        DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
        if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
          || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
          dateTimeOffset = dateTimeOffset.ToUniversalTime();

        text = dateTimeOffset.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);
      }
      else
      {
        throw new Exception("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.".FormatWith(CultureInfo.InvariantCulture, ReflectionUtils.GetObjectType(value)));
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
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (reader.TokenType == JsonToken.Null)
      {
        if (!ReflectionUtils.IsNullableType(objectType))
          throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
 
        return null;
      }

      if (reader.TokenType != JsonToken.String)
        throw new Exception("Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      string dateText = reader.Value.ToString();

      if (t == typeof(DateTimeOffset))
      {
        if (!string.IsNullOrEmpty(_dateTimeFormat))
          return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
        else
          return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
      }

      if (!string.IsNullOrEmpty(_dateTimeFormat))
        return DateTime.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
      else
        return DateTime.Parse(dateText, Culture, _dateTimeStyles);
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
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (typeof(DateTime).IsAssignableFrom(t))
        return true;
      if (typeof(DateTimeOffset).IsAssignableFrom(t))
        return true;

      return false;
    }
  }
}