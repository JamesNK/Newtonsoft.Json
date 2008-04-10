using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Specifies whether a DateTime object represents a local time, a Coordinated Universal Time (UTC), or is not specified as either local time or UTC.
  /// </summary>
  public enum JsonDateTimeSerializationMode
  {
    /// <summary>
    /// The time represented is local time.
    /// </summary>
    Local,
    /// <summary>
    /// The time represented is UTC.
    /// </summary>
    Utc,
    /// <summary>
    /// The time represented is not specified as either local time or Coordinated Universal Time (UTC).
    /// </summary>
    Unspecified,
    /// <summary>
    /// Preserves the DateTimeKind field of a date when a DateTime object is converted to a string and the string is then converted back to a DateTime object.
    /// </summary>
    RoundtripKind
  }
}