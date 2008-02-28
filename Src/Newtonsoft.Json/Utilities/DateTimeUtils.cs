using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Newtonsoft.Json.Utilities
{
  public static class DateTimeUtils
  {
    public static string GetLocalOffset(this DateTime d)
    {
      TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(d);

      return utcOffset.Hours.ToString("+00;-00") + ":" + utcOffset.Minutes.ToString("00;00");
    }

    public static XmlDateTimeSerializationMode ToSerializationMode(DateTimeKind kind)
    {
      switch (kind)
      {
        case DateTimeKind.Local:
          return XmlDateTimeSerializationMode.Local;
        case DateTimeKind.Unspecified:
          return XmlDateTimeSerializationMode.Unspecified;
        case DateTimeKind.Utc:
          return XmlDateTimeSerializationMode.Utc;
        default:
          throw new ArgumentOutOfRangeException("kind", kind, "Unexpected DateTimeKind value.");
      }
    }
  }
}
