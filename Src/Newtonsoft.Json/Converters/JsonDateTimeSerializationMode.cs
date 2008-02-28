using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Converters
{
  public enum JsonDateTimeSerializationMode
  {
    Local,
    Utc,
    Unspecified,
    RoundtripKind
  }
}