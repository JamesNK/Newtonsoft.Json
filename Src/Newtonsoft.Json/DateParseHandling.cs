namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON text.
  /// </summary>
  public enum DateParseHandling
  {
    /// <summary>
    /// Date formatted strings are not parsed to a date type and are read as strings.
    /// </summary>
    None,
    /// <summary>
    /// Date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed to <see cref="DateTime"/>.
    /// </summary>
    DateTime,
#if !NET20
    /// <summary>
    /// Date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed to <see cref="DateTimeOffset"/>.
    /// </summary>
    DateTimeOffset
#endif
  }
}