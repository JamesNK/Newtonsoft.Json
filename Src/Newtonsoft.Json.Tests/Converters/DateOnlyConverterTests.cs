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
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class DateOnlyConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeDateOnly()
        {
            DateOnly dateOnly = DateOnly.Parse("2022-05-14");

            string result = JsonConvert.SerializeObject(dateOnly, new DateOnlyConverter());

            Assert.AreEqual("\"2022-05-14\"", result);
        }

        [Test]
        public void SerializeNullDateOnly()
        {
            DateOnly? dateOnly = null;

            string result = JsonConvert.SerializeObject(dateOnly, new DateOnlyConverter());

            Assert.AreEqual("null", result);
        }

        [Test]
        public void WriteJsonInvalidType()
        {
            DateOnlyConverter converter = new DateOnlyConverter();

            ExceptionAssert.Throws<JsonSerializationException>(
                () => converter.WriteJson(new JTokenWriter(), new object(), new JsonSerializer()),
                "Converter cannot write specified value to JSON. System.DateOnly is required."
            );
        }

        [Theory]
        [InlineData("1970-01-01", "1970-01-01")]
        [InlineData("2002-02-13", "2002-02-13")]
        [InlineData("2022-05-10", "2022-05-10")]
        [InlineData("\\u0032\\u0030\\u0032\\u0032\\u002D\\u0030\\u0035\\u002D\\u0031\\u0030", "2022-05-10")]
        [InlineData("0001-01-01", "0001-01-01")] // DateOnly.MinValue
        [InlineData("9999-12-31", "9999-12-31")] // DateOnly.MaxValue
        public void DeserializeStringToDateOnly(string input, string expected)
        {
            DateOnly result = JsonConvert.DeserializeObject<DateOnly>($"\"{input}\"", new DateOnlyConverter());

            Assert.AreEqual(result, DateOnly.Parse(expected, CultureInfo.InvariantCulture));
        }

        [Test]
        public void DeserializeNullToNullable()
        {
            DateOnly? result = JsonConvert.DeserializeObject<DateOnly?>("null", new DateOnlyConverter());

            Assert.IsNull(result);
        }
    }
}
#endif