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

#if HAVE_DATE_ONLY
using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class TimeOnlyConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeTimeOnly()
        {
            TimeOnly timeOnly = TimeOnly.Parse("12:01:01");

            string result = JsonConvert.SerializeObject(timeOnly, new TimeOnlyConverter());

            Assert.AreEqual("\"12:01:01\"", result);
        }

        [Test]
        public void SerializeNullTimeOnly()
        {
            TimeOnly? timeOnly = null;

            string result = JsonConvert.SerializeObject(timeOnly, new TimeOnlyConverter());

            Assert.AreEqual("null", result);
        }

        [Test]
        public void WriteJsonInvalidType()
        {
            TimeOnlyConverter converter = new TimeOnlyConverter();

            ExceptionAssert.Throws<JsonSerializationException>(
                () => converter.WriteJson(new JTokenWriter(), new object(), new JsonSerializer()),
                "Converter cannot write specified value to JSON. System.TimeOnly is required."
            );
        }

        [Test]
        public void DeserializeStringToDateOnly()
        {
            TimeOnly result = JsonConvert.DeserializeObject<TimeOnly>("\"12:01:01\"", new TimeOnlyConverter());

            Assert.AreEqual(new TimeOnly(12, 1, 1), result);
        }

        [Theory]
        [InlineData("23:59:59", "23:59:59")]
        [InlineData("23:59:59.9", "23:59:59.9000000")]
        [InlineData("02:48:05.4775807", "02:48:05.4775807")]
        [InlineData("02:48:05.4775808", "02:48:05.4775808")]
        [InlineData("\\u0032\\u0033\\u003A\\u0035\\u0039\\u003A\\u0035\\u0039", "23:59:59")]
        [InlineData("00:00:00.0000000", "00:00:00")] // TimeOnly.MinValue
        [InlineData("23:59:59.9999999", "23:59:59.9999999")] // TimeOnly.MaxValue
        public void DeserializeStringToTimeOnly(string input, string expected)
        {
            TimeOnly result = JsonConvert.DeserializeObject<TimeOnly>($"\"{input}\"", new TimeOnlyConverter());

            Assert.AreEqual(result, TimeOnly.Parse(expected));
        }

        [Test]
        public void DeserializeNullToNullable()
        {
            TimeOnly? result = JsonConvert.DeserializeObject<TimeOnly?>("null", new TimeOnlyConverter());

            Assert.IsNull(result);
        }
    }
}
#endif