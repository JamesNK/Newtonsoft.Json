using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal static class DateTimeUtils
  {
    public static string GetLocalOffset(this DateTime d)
    {
      TimeSpan utcOffset;
#if PocketPC
      utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(d);
#else
      utcOffset = TimeZoneInfo.Local.GetUtcOffset(d);
#endif

      return utcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + utcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
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
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("kind", kind, "Unexpected DateTimeKind value.");
      }
    }
  }
}
