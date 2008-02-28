using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests
{
  public class JavaScriptDateTimeConverterTests : TestFixtureBase
  {
    [Test]
    public void SerializeDateTime()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);
      string result;

      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual("new Date(976918263055)", result);
    }

    [Test]
    public void SerializeDateTimeOffset()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTimeOffset now = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);
      string result;

      result = JavaScriptConvert.SerializeObject(now, converter);
      Assert.AreEqual("new Date(976918263055)", result);
    }

    [Test]
    public void DeserializeDateTime()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTime result = JavaScriptConvert.DeserializeObject<DateTime>("new Date(976918263055)", converter);
      Assert.AreEqual(new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc), result);
    }

    [Test]
    public void DeserializeDateTimeOffset()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();
      DateTimeOffset start = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

      string json = JavaScriptConvert.SerializeObject(start, converter);

      DateTimeOffset result = JavaScriptConvert.DeserializeObject<DateTimeOffset>(json, converter);
      Assert.AreEqual(new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero), result);
    }
  }
}