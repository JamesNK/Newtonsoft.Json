#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests.Converters
{
  public class IsoDateTimeConverterTests : TestFixtureBase
  {
    [Test]
    public void PropertiesShouldBeSet()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter();
      Assert.AreEqual(CultureInfo.CurrentCulture, converter.Culture);
      Assert.AreEqual(string.Empty, converter.DateTimeFormat);
      Assert.AreEqual(DateTimeStyles.RoundtripKind, converter.DateTimeStyles);

      converter = new IsoDateTimeConverter()
      {
        DateTimeFormat = "F",
        Culture = CultureInfo.InvariantCulture,
        DateTimeStyles = DateTimeStyles.None
      };

      Assert.AreEqual(CultureInfo.InvariantCulture, converter.Culture);
      Assert.AreEqual("F", converter.DateTimeFormat);
      Assert.AreEqual(DateTimeStyles.None, converter.DateTimeStyles);
    }

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
    public void SerializeFormattedDateTimeInvariantCulture()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "F", Culture = CultureInfo.InvariantCulture };

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Utc);
      string result;

      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 22:11:03""", result);

      Assert.AreEqual(d, JavaScriptConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 22:11:03""", result);
    }

    [Test]
    public void SerializeCustomFormattedDateTime()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "dd/MM/yyyy" };

      string json = @"""09/12/2006""";

      DateTime d = JavaScriptConvert.DeserializeObject<DateTime>(json, converter);

      Assert.AreEqual(9, d.Day);
      Assert.AreEqual(12, d.Month);
      Assert.AreEqual(2006, d.Year);
    }

#if !SILVERLIGHT
    [Test]
    public void SerializeFormattedDateTimeNewZealandCulture()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "F", Culture = CultureInfo.GetCultureInfo("en-NZ") };

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Utc);
      string result;

      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);

      Assert.AreEqual(d, JavaScriptConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
      result = JavaScriptConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);
    }

    [Test]
    public void SerializeDateTimeCulture()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { Culture = CultureInfo.GetCultureInfo("en-NZ") };

      string json = @"""09/12/2006""";

      DateTime d = JavaScriptConvert.DeserializeObject<DateTime>(json, converter);

      Assert.AreEqual(9, d.Day);
      Assert.AreEqual(12, d.Month);
      Assert.AreEqual(2006, d.Year);
    }
#endif

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
      c.DateTimeOffsetField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12.0000000Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12.0000000+00:00"",""PostField"":""Post""}", json);

      //test the other edge case too
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
      c.DateTimeOffsetField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      json = JavaScriptConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01.0000000Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01.0000000+00:00"",""PostField"":""Post""}", json);
    }

    [Test]
    public void DeserializeUTC()
    {
      DateTimeTestClass c =
        JavaScriptConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
      Assert.AreEqual(new DateTimeOffset(2008, 12, 12, 12, 12, 12, 0, TimeSpan.Zero), c.DateTimeOffsetField);
      Assert.AreEqual("Pre", c.PreField);
      Assert.AreEqual("Post", c.PostField);

      DateTimeTestClass c2 =
       JavaScriptConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime(), c2.DateTimeField);
      Assert.AreEqual(new DateTimeOffset(2008, 1, 1, 1, 1, 1, 0, TimeSpan.Zero), c2.DateTimeOffsetField);
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