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
using Newtonsoft.Json.Tests.TestObjects;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
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

        public static string GetUtcOffsetText(DateTime d)
        {
            TimeSpan utcOffset = d.GetUtcOffset();

            return utcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture) + ":" + utcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture);
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
            Assert.AreEqual(@"""2000-12-15T22:11:03.055" + GetUtcOffsetText(d) + @"""", result);
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
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "dd/MM/yyyy",
                Culture = CultureInfo.InvariantCulture
            };

            string json = @"""09/12/2006""";

            DateTime d = JsonConvert.DeserializeObject<DateTime>(json, converter);

            Assert.AreEqual(9, d.Day);
            Assert.AreEqual(12, d.Month);
            Assert.AreEqual(2006, d.Year);
        }

#if !(NETFX_CORE || ASPNETCORE50)
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

#if !NET20
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
        public void NullableDeserializeEmptyString()
        {
            string json = @"{""DateTimeField"":""""}";

            NullableDateTimeTestClass c = JsonConvert.DeserializeObject<NullableDateTimeTestClass>(json,
                new JsonSerializerSettings { Converters = new[] { new IsoDateTimeConverter() } });
            Assert.AreEqual(null, c.DateTimeField);
        }

        [Test]
        public void DeserializeNullToNonNullable()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                DateTimeTestClass c2 =
                    JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });
            }, "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38.");
        }

        [Test]
        public void SerializeShouldChangeNonUTCDates()
        {
            DateTime localDateTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);

            DateTimeTestClass c = new DateTimeTestClass();
            c.DateTimeField = localDateTime;
            c.PreField = "Pre";
            c.PostField = "Post";
            string json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal }); //note that this fails without the Utc converter...
            c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
            string json2 = JsonConvert.SerializeObject(c, new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal });

            TimeSpan offset = localDateTime.GetUtcOffset();

            // if the current timezone is utc then local already equals utc
            if (offset == TimeSpan.Zero)
                Assert.AreEqual(json, json2);
            else
                Assert.AreNotEqual(json, json2);
        }
#endif

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

#if !NET20
        [Test]
        public void DeserializeDateTimeOffset()
        {
            var settings = new JsonSerializerSettings();
            settings.DateParseHandling = DateParseHandling.DateTimeOffset;
            settings.Converters.Add(new IsoDateTimeConverter());

            // Intentionally use an offset that is unlikely in the real world,
            // so the test will be accurate regardless of the local time zone setting.
            var offset = new TimeSpan(2, 15, 0);
            var dto = new DateTimeOffset(2014, 1, 1, 0, 0, 0, 0, offset);

            var test = JsonConvert.DeserializeObject<DateTimeOffset>("\"2014-01-01T00:00:00+02:15\"", settings);

            Assert.AreEqual(dto, test);
            Assert.AreEqual(dto.ToString("o"), test.ToString("o"));
        }
#endif
    }
}