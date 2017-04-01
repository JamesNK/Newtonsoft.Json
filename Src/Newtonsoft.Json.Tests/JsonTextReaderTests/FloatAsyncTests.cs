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
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class FloatAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task Float_ReadAsString_ExactAsync()
        {
            const string testJson = "{float: 0.0620}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            string s = await reader.ReadAsStringAsync();
            Assert.AreEqual("0.0620", s);
        }

        [Test]
        public async Task Float_NaN_ReadAsync()
        {
            const string testJson = "{float: NaN}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NaN, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task Float_NaN_ReadAsInt32Async()
        {
            const string testJson = "{float: NaN}";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await  reader.ReadAsInt32Async(), "Cannot read NaN value. Path 'float', line 1, position 11.");
        }

        [Test]
        public async Task Float_NaNAndInifinity_ReadAsDoubleAsync()
        {
            const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(await reader.ReadAsync());

            Assert.AreEqual(double.NaN, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NaN, reader.Value);

            Assert.AreEqual(double.PositiveInfinity, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.PositiveInfinity, reader.Value);

            Assert.AreEqual(double.NegativeInfinity, reader.ReadAsDouble());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(double.NegativeInfinity, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task Float_NaNAndInifinity_ReadAsStringAsync()
        {
            const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(testJson));

            Assert.IsTrue(await reader.ReadAsync());

            Assert.AreEqual(JsonConvert.NaN, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.NaN, reader.Value);

            Assert.AreEqual(JsonConvert.PositiveInfinity, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.PositiveInfinity, reader.Value);

            Assert.AreEqual(JsonConvert.NegativeInfinity, reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(JsonConvert.NegativeInfinity, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task FloatParseHandling_ReadAsStringAsync()
        {
            string json = "[9223372036854775807, 1.7976931348623157E+308, 792281625142643375935439503.35, 792281625142643375935555555555555555555555555555555555555555555555555439503.35]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.AreEqual("9223372036854775807", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("9223372036854775807", reader.Value);

            Assert.AreEqual("1.7976931348623157E+308", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("1.7976931348623157E+308", reader.Value);

            Assert.AreEqual("792281625142643375935439503.35", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("792281625142643375935439503.35", reader.Value);

            Assert.AreEqual("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.ReadAsString());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public async Task FloatParseHandlingAsync()
        {
            string json = "[1.0,1,9.9,1E-06]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(1.0m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(typeof(long), reader.ValueType);
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(9.9m, reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(Convert.ToDecimal(1E-06), reader.Value);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public async Task FloatParseHandling_NaNAsync()
        {
            string json = "[NaN]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Cannot read NaN value. Path '', line 1, position 4.");
        }

        [Test]
        public async Task FloatingPointNonFiniteNumbersAsync()
        {
            string input = @"[
  NaN,
  Infinity,
  -Infinity
]";

            StringReader sr = new StringReader(input);

            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.NaN);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.PositiveInfinity);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.Float);
                Assert.AreEqual(jsonReader.Value, double.NegativeInfinity);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
            }
        }

        [Test]
        public async Task ReadFloatingPointNumberAsync()
        {
            string json =
                @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN,0e-10,0.25e-5,0.3e10]";

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(json)))
            {
                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.1, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1.0, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1.000001, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(1E-06, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(4.94065645841247E-324, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.PositiveInfinity, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NegativeInfinity, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NaN, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.MaxValue, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.MinValue, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.PositiveInfinity, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NegativeInfinity, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(double.NaN, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0d, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(0.0000025d, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Float, jsonReader.TokenType);
                Assert.AreEqual(3000000000d, jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);
            }
        }
    }
}

#endif
