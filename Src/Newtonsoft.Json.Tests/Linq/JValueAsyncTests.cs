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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JValueAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task FloatParseHandlingAsync()
        {
            JValue v = (JValue)await JToken.ReadFromAsync(
                new JsonTextReader(new StringReader("9.9"))
                {
                    FloatParseHandling = FloatParseHandling.Decimal
                });

            Assert.AreEqual(9.9m, v.Value);
            Assert.AreEqual(typeof(decimal), v.Value.GetType());
        }

        public class Rate
        {
            public decimal Compoundings { get; set; }
        }

        private readonly Rate _rate = new Rate { Compoundings = 12.166666666666666666666666667m };


        [Test]
        public async Task ParseAndConvertDateTimeOffsetAsync()
        {
            var json = @"{ d: ""\/Date(0+0100)\/"" }";

            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                jsonReader.DateParseHandling = DateParseHandling.DateTimeOffset;

                var obj = await JObject.LoadAsync(jsonReader);
                var d = (JValue)obj["d"];

                CustomAssert.IsInstanceOfType(typeof(DateTimeOffset), d.Value);
                TimeSpan offset = ((DateTimeOffset)d.Value).Offset;
                Assert.AreEqual(TimeSpan.FromHours(1), offset);

                DateTimeOffset dateTimeOffset = (DateTimeOffset)d;
                Assert.AreEqual(TimeSpan.FromHours(1), dateTimeOffset.Offset);
            }
        }

#if !PORTABLE

#endif

        [Test]
        public async Task ParseIsoTimeZonesAsync()
        {
            DateTimeOffset expectedDate = new DateTimeOffset(2013, 08, 14, 4, 38, 31, TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(30)));
            JsonTextReader reader = new JsonTextReader(new StringReader("'2013-08-14T04:38:31.000+1230'"));
            reader.DateParseHandling = DateParseHandling.DateTimeOffset;
            JValue date = (JValue)await JToken.ReadFromAsync(reader);
            Assert.AreEqual(expectedDate, date.Value);

            DateTimeOffset expectedDate2 = new DateTimeOffset(2013, 08, 14, 4, 38, 31, TimeSpan.FromHours(12));
            JsonTextReader reader2 = new JsonTextReader(new StringReader("'2013-08-14T04:38:31.000+12'"));
            reader2.DateParseHandling = DateParseHandling.DateTimeOffset;
            JValue date2 = (JValue)await JToken.ReadFromAsync(reader2);
            Assert.AreEqual(expectedDate2, date2.Value);
        }
    }
}

#endif