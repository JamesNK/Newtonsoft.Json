using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts an <see cref="Enum"/> to and from its name string value.
  /// </summary>
  public class StringEnumConverter : JsonConverter
  {

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    public override void WriteJson(JsonWriter writer, object value)
    {
      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      Enum e = (Enum) value;
      string enumName = e.ToString("G");

      if (char.IsNumber(enumName[0]) || enumName[0] == '-')
        writer.WriteValue(value);
      else
        writer.WriteValue(enumName);
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

      if (reader.TokenType == JsonToken.String)
        return Enum.Parse(t, reader.Value.ToString(), true);
      
      if (reader.TokenType == JsonToken.Integer)
        return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, t);
      
      throw new Exception("Unexpected token when parsing enum. Expected String or Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
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

      return t.IsEnum;
    }
  }
}