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
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Xml;

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

      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.055Z""", result);

      Assert.AreEqual(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Local);
      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.055" + d.GetLocalOffset() + @"""", result);
    }

    [Test]
    public void SerializeFormattedDateTimeInvariantCulture()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "F", Culture = CultureInfo.InvariantCulture };

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Utc);
      string result;

      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 22:11:03""", result);

      Assert.AreEqual(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 22:11:03""", result);
    }

    [Test]
    public void SerializeCustomFormattedDateTime()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "dd/MM/yyyy" };

      string json = @"""09/12/2006""";

      DateTime d = JsonConvert.DeserializeObject<DateTime>(json, converter);

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

      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);

      Assert.AreEqual(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

      d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);
    }

    [Test]
    public void SerializeDateTimeCulture()
    {
      IsoDateTimeConverter converter = new IsoDateTimeConverter() { Culture = CultureInfo.GetCultureInfo("en-NZ") };

      string json = @"""09/12/2006""";

      DateTime d = JsonConvert.DeserializeObject<DateTime>(json, converter);

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

      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual(@"""2000-12-15T22:11:03.055+00:00""", result);

      Assert.AreEqual(d, JsonConvert.DeserializeObject<DateTimeOffset>(result, converter));
    }

    [Test]
    public void SerializeUTC()
    {
      DateTimeTestClass c = new DateTimeTestClass();
      c.DateTimeField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.DateTimeOffsetField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12+00:00"",""PostField"":""Post""}", json);

      //test the other edge case too
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
      c.DateTimeOffsetField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01+00:00"",""PostField"":""Post""}", json);
    }

    [Test]
    public void NullableSerializeUTC()
    {
      NullableDateTimeTestClass c = new NullableDateTimeTestClass();
      c.DateTimeField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.DateTimeOffsetField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime();
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12+00:00"",""PostField"":""Post""}", json);

      //test the other edge case too
      c.DateTimeField = null;
      c.DateTimeOffsetField = null;
      c.PreField = "Pre";
      c.PostField = "Post";
      json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreEqual(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", json);
    }

    [Test]
    public void DeserializeUTC()
    {
      DateTimeTestClass c =
        JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
      Assert.AreEqual(new DateTimeOffset(2008, 12, 12, 12, 12, 12, 0, TimeSpan.Zero), c.DateTimeOffsetField);
      Assert.AreEqual("Pre", c.PreField);
      Assert.AreEqual("Post", c.PostField);

      DateTimeTestClass c2 =
       JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime(), c2.DateTimeField);
      Assert.AreEqual(new DateTimeOffset(2008, 1, 1, 1, 1, 1, 0, TimeSpan.Zero), c2.DateTimeOffsetField);
      Assert.AreEqual("Pre", c2.PreField);
      Assert.AreEqual("Post", c2.PostField);
    }

    [Test]
    public void NullableDeserializeUTC()
    {
      NullableDateTimeTestClass c =
        JsonConvert.DeserializeObject<NullableDateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
      Assert.AreEqual(new DateTimeOffset(2008, 12, 12, 12, 12, 12, 0, TimeSpan.Zero), c.DateTimeOffsetField);
      Assert.AreEqual("Pre", c.PreField);
      Assert.AreEqual("Post", c.PostField);

      NullableDateTimeTestClass c2 =
       JsonConvert.DeserializeObject<NullableDateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

      Assert.AreEqual(null, c2.DateTimeField);
      Assert.AreEqual(null, c2.DateTimeOffsetField);
      Assert.AreEqual("Pre", c2.PreField);
      Assert.AreEqual("Post", c2.PostField);
    }

    [Test]
    public void NullableDeserializeEmptyString()
    {
      string json = @"{""DateTimeField"":""""}";

      NullableDateTimeTestClass c = JsonConvert.DeserializeObject<NullableDateTimeTestClass>(json,
        new JsonSerializerSettings { Converters = new [] {new IsoDateTimeConverter()}});
      Assert.AreEqual(null, c.DateTimeField);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot convert null value to System.DateTime.")]
    public void DeserializeNullToNonNullable()
    {
      DateTimeTestClass c2 =
       JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
    }

    [Test]
    public void SerializeShouldChangeNonUTCDates()
    {
      DateTimeTestClass c = new DateTimeTestClass();
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);
      c.PreField = "Pre";
      c.PostField = "Post";
      string json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal }); //note that this fails without the Utc converter...
      c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
      string json2 = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
      Assert.AreNotEqual(json, json2);
    }

    [Test]
    public void BlogCodeSample()
    {
      Person p = new Person
                   {
                     Name = "Keith",
                     BirthDate = new DateTime(1980, 3, 8),
                     LastModified = new DateTime(2009, 4, 12, 20, 44, 55),
                   };

      string jsonText = JsonConvert.SerializeObject(p, new IsoDateTimeConverter());
      // {
      //   "Name": "Keith",
      //   "BirthDate": "1980-03-08T00:00:00",
      //   "LastModified": "2009-04-12T20:44:55"
      // }

      Console.WriteLine(jsonText);

    }
  }
}