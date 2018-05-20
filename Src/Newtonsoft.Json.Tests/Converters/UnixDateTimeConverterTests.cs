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
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class UnixDateTimeConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeDateTime()
        {
            DateTime unixEpoch = UnixDateTimeConverter.UnixEpoch;

            string result = JsonConvert.SerializeObject(unixEpoch, new UnixDateTimeConverter());

            Assert.AreEqual("0", result);
        }

        [Test]
        public void SerializeDateTimeNow()
        {
            DateTime now = DateTime.Now;
            long nowSeconds = (long)(now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

            string result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

            Assert.AreEqual(nowSeconds + "", result);
        }

        [Test]
        public void SerializeInvalidDate()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.SerializeObject(new DateTime(1964, 2, 7), new UnixDateTimeConverter()),
                "Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970."
            );
        }

        [Test]
        public void WriteJsonInvalidType()
        {
            UnixDateTimeConverter converter = new UnixDateTimeConverter();

            ExceptionAssert.Throws<JsonSerializationException>(
                () => converter.WriteJson(new JTokenWriter(), new object(), new JsonSerializer()),
                "Expected date object value."
            );
        }

#if !NET20
        [Test]
        public void SerializeDateTimeOffset()
        {
            DateTimeOffset now = new DateTimeOffset(2018, 1, 1, 16, 1, 16, TimeSpan.FromHours(-5));

            string result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

            Assert.AreEqual("1514840476", result);
        }

        [Test]
        public void SerializeNullableDateTimeClass()
        {
            NullableDateTimeTestClass t = new NullableDateTimeTestClass
            {
                DateTimeField = null,
                DateTimeOffsetField = null
            };

            UnixDateTimeConverter converter = new UnixDateTimeConverter();

            string result = JsonConvert.SerializeObject(t, converter);

            Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

            t = new NullableDateTimeTestClass
            {
                DateTimeField = new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc),
                DateTimeOffsetField = new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero)
            };

            result = JsonConvert.SerializeObject(t, converter);
            Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":1514840476,""DateTimeOffsetField"":2750778,""PostField"":null}", result);
        }

        [Test]
        public void DeserializeNullToNonNullable()
        {
            ExceptionAssert.Throws<Exception>(
                () => JsonConvert.DeserializeObject<DateTimeTestClass>(
                    @"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}",
                    new UnixDateTimeConverter()
                ),
                "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38."
            );
        }

        [Test]
        public void DeserializeDateTimeOffset()
        {
            UnixDateTimeConverter converter = new UnixDateTimeConverter();
            DateTimeOffset d = new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero);

            string json = JsonConvert.SerializeObject(d, converter);

            DateTimeOffset result = JsonConvert.DeserializeObject<DateTimeOffset>(json, converter);

            Assert.AreEqual(new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero), result);
        }

        [Test]
        public void DeserializeStringToDateTimeOffset()
        {
            DateTimeOffset result = JsonConvert.DeserializeObject<DateTimeOffset>(@"""1514840476""", new UnixDateTimeConverter());

            Assert.AreEqual(new DateTimeOffset(2018, 1, 1, 21, 1, 16, TimeSpan.Zero), result);
        }

        [Test]
        public void DeserializeInvalidStringToDateTimeOffset()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DateTimeOffset>(@"""PIE""", new UnixDateTimeConverter()),
                "Cannot convert invalid value to System.DateTimeOffset. Path '', line 1, position 5."
            );
        }
#endif

        [Test]
        public void DeserializeIntegerToDateTime()
        {
            DateTime result = JsonConvert.DeserializeObject<DateTime>("1514840476", new UnixDateTimeConverter());

            Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), result);
        }

        [Test]
        public void DeserializeNullToNullable()
        {
            DateTime? result = JsonConvert.DeserializeObject<DateTime?>("null", new UnixDateTimeConverter());

            Assert.IsNull(result);
        }

        [Test]
        public void DeserializeInvalidValue()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DateTime>("-1", new UnixDateTimeConverter()),
                "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to System.DateTime. Path '', line 1, position 2."
            );
        }

        [Test]
        public void DeserializeInvalidValueType()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DateTime>("false", new UnixDateTimeConverter()),
                "Unexpected token parsing date. Expected Integer or String, got Boolean. Path '', line 1, position 5."
            );
        }

        [Test]
        public void ConverterList()
        {
            UnixConverterList<object> l1 = new UnixConverterList<object>
            {
                new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc),
                new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc),
            };

            string json = JsonConvert.SerializeObject(l1, Formatting.Indented);
            StringAssert.AreEqual(@"[
  1514840476,
  3
]", json);

            UnixConverterList<object> l2 = JsonConvert.DeserializeObject<UnixConverterList<object>>(json);
            Assert.IsNotNull(l2);

            Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2[0]);
            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2[1]);
        }

        [Test]
        public void ConverterDictionary()
        {
            UnixConverterDictionary<object> l1 = new UnixConverterDictionary<object>
            {
                {"First", new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc)},
                {"Second", new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc)},
            };

            string json = JsonConvert.SerializeObject(l1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""First"": 3,
  ""Second"": 1514840476
}", json);

            UnixConverterDictionary<object> l2 = JsonConvert.DeserializeObject<UnixConverterDictionary<object>>(json);
            Assert.IsNotNull(l2);

            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2["First"]);
            Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2["Second"]);
        }

        [Test]
        public void ConverterObject()
        {
            UnixConverterObject obj1 = new UnixConverterObject
            {
                Object1 = new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc),
                Object2 = null,
                ObjectNotHandled = new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc)
            };

            string json = JsonConvert.SerializeObject(obj1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Object1"": 3,
  ""Object2"": null,
  ""ObjectNotHandled"": 1514840476
}", json);

            UnixConverterObject obj2 = JsonConvert.DeserializeObject<UnixConverterObject>(json);
            Assert.IsNotNull(obj2);

            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), obj2.Object1);
            Assert.IsNull(obj2.Object2);
            Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), obj2.ObjectNotHandled);
        }
    }

    [JsonArray(ItemConverterType = typeof(UnixDateTimeConverter))]
    public class UnixConverterList<T> : List<T> { }

    [JsonDictionary(ItemConverterType = typeof(UnixDateTimeConverter))]
    public class UnixConverterDictionary<T> : Dictionary<string, T> { }

    [JsonObject(ItemConverterType = typeof(UnixDateTimeConverter))]
    public class UnixConverterObject
    {
        public object Object1 { get; set; }

        public object Object2 { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public object ObjectNotHandled { get; set; }
    }
}