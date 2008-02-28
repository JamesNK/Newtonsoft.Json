using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public abstract class TestFixtureBase
  {
    public string GetOffset(DateTime value)
    {
      TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(value.ToLocalTime());

      return utcOffset.Hours.ToString("+00;-00") + utcOffset.Minutes.ToString("00;00");
    }
  }
}
