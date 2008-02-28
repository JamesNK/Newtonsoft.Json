using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests
{
  public class IsoDateTimeConverterTests : TestFixtureBase
  {
    [Test]
    public void SerializeDateTime()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter();

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);
      string result;

      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.0550000Z""", result);

      Assert.AreEqual(d, JavaScriptConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Local);
      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.0550000" + d.GetLocalOffset() + @"""", result);
    }

    [Test]
    public void SerializeDateTimeOffset()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter();

      DateTimeOffset d = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);
      string result;

      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.0550000+00:00""", result);

      Assert.AreEqual(d, JavaScriptConvert.DeserializeObject<DateTimeOffset>(result, converter));
    }

    [Test]
    public void SerializeUTC()
    {
      DateTimeTestClass c = new DateTimeTestClass();
      c.DateTimeField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12.0000000Z"",""PostField"":""Post""}", json);

      //test the other edge case too
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      json = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01.0000000Z"",""PostField"":""Post""}", json);
    }

    [Test]
    public void DeserializeUTC()
    {
      DateTimeTestClass c =
        JavaScriptConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
      Assert.AreEqual("Pre", c.PreField);
      Assert.AreEqual("Post", c.PostField);

      DateTimeTestClass c2 =
       JavaScriptConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime(), c2.DateTimeField);
      Assert.AreEqual("Pre", c2.PreField);
      Assert.AreEqual("Post", c2.PostField);
    }

    [Test]
    public void SerializeShouldChangeNonUTCDates()
    {
      DateTimeTestClass c = new DateTimeTestClass();
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal }); //note that this fails without the Utc converter...
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
      string json2 = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreNotEqual(json, json2);
    }
  }
}