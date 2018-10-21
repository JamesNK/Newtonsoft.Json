namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies how byte arrays are formatted when writing JSON text.
  /// </summary>
  public enum ByteArrayFormatHandling
  {
    /// <summary>
    /// Byte arrays are written as Base64 encoded string, e.g. <c>"AQIDBAU="</c>.
    /// </summary>
    Base64EncodedString,

    /// <summary>
    /// Byte arrays are written as JSON array, e.g. <c>"[1,2,3,4,5]"</c>.
    /// </summary>
    Array
  }
}