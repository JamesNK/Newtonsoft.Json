using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using System.Globalization;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="Regex"/> to and from JSON and BSON.
  /// </summary>
  public class RegexConverter : JsonConverter
  {
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Regex regex = (Regex) value;

      BsonWriter bsonWriter = writer as BsonWriter;
      if (bsonWriter != null)
        WriteBson(bsonWriter, regex);
      else
        WriteJson(writer, regex);
    }

    private bool HasFlag(RegexOptions options, RegexOptions flag)
    {
      return ((options & flag) == flag);
    }

    private void WriteBson(BsonWriter writer, Regex regex)
    {
      // Regular expression - The first cstring is the regex pattern, the second
      // is the regex options string. Options are identified by characters, which 
      // must be stored in alphabetical order. Valid options are 'i' for case 
      // insensitive matching, 'm' for multiline matching, 'x' for verbose mode, 
      // 'l' to make \w, \W, etc. locale dependent, 's' for dotall mode 
      // ('.' matches everything), and 'u' to make \w, \W, etc. match unicode.

      string options = null;

      if (HasFlag(regex.Options, RegexOptions.IgnoreCase))
        options += "i";

      if (HasFlag(regex.Options, RegexOptions.Multiline))
        options += "m";

      if (HasFlag(regex.Options, RegexOptions.Singleline))
        options += "s";

      options += "u";

      if (HasFlag(regex.Options, RegexOptions.ExplicitCapture))
        options += "x";

      writer.WriteRegex(regex.ToString(), options);
    }

    private void WriteJson(JsonWriter writer, Regex regex)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("Pattern");
      writer.WriteValue(regex.ToString());
      writer.WritePropertyName("Options");
      writer.WriteValue(regex.Options);
      writer.WriteEndObject();
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
      BsonReader bsonReader = reader as BsonReader;

      if (bsonReader != null)
        return ReadBson(bsonReader);
      else
        return ReadJson(reader);
    }

    private object ReadBson(BsonReader reader)
    {
      string regexText = (string)reader.Value;
      int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

      string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
      string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

      RegexOptions options = RegexOptions.None;
      foreach (char c in optionsText)
      {
        switch (c)
        {
          case 'i':
            options |= RegexOptions.IgnoreCase;
            break;
          case 'm':
            options |= RegexOptions.Multiline;
            break;
          case 's':
            options |= RegexOptions.Singleline;
            break;
          case 'x':
            options |= RegexOptions.ExplicitCapture;
            break;
        }
      }

      return new Regex(patternText, options);
    }

    private Regex ReadJson(JsonReader reader)
    {
      reader.Read();
      reader.Read();
      string pattern = (string)reader.Value;

      reader.Read();
      reader.Read();
      int options = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);

      reader.Read();

      return new Regex(pattern, (RegexOptions) options);
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
      return (objectType == typeof (Regex));
    }
  }
}