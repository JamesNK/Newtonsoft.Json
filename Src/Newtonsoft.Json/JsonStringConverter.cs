using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Converts an object to and from a string.
    /// </summary>
    public abstract class JsonStringConverter : JsonConverter
    {
        /// <summary>
        /// Converts the object to its string representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public abstract override string ConvertToString(object value);

        /// <summary>
        /// Converts the string representation of an object to that object.
        /// </summary>
        /// <param name="value">The object's string representation.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object represented by its string representation.</returns>
        public abstract override object ConvertFromString(string value, Type objectType);

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values to strings. Used when serializing dictionary keys.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values to strings; otherwise, <c>false</c>.</value>
        public override bool CanConvertToString
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values from strings. Used when deserializing dictionary keys.
        /// </summary>
        /// <value></value>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values from strings; otherwise, <c>false</c>.</value>
        public override bool CanConvertFromString
        {
            get { return true; }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string valueAsString = ConvertToString(value);
            if (valueAsString == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(valueAsString);
            }
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
            if (reader.TokenType == JsonToken.String || reader.TokenType == JsonToken.Null)
            {
                return ConvertFromString(reader.Value?.ToString(), objectType);
            }
            throw JsonSerializationException.Create(reader, "Unexpected token or value when parsing. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
        }
    }
}
