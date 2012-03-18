using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a raw JSON string.
  /// </summary>
  public class JRaw : JValue
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="JRaw"/> class from another <see cref="JRaw"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JRaw"/> object to copy from.</param>
    public JRaw(JRaw other)
      : base(other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JRaw"/> class.
    /// </summary>
    /// <param name="rawJson">The raw json.</param>
    public JRaw(object rawJson)
      : base(rawJson, JTokenType.Raw)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="JRaw"/> with the content of the reader's current token.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>An instance of <see cref="JRaw"/> with the content of the reader's current token.</returns>
    public static JRaw Create(JsonReader reader)
    {
      using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteToken(reader);

        return new JRaw(sw.ToString());
      }
    }

    internal override JToken CloneToken()
    {
      return new JRaw(this);
    }
  }
}