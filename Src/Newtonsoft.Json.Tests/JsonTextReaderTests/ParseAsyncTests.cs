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
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class ParseAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task ParseAdditionalContent_WhitespaceAsync()
        {
            string json = @"[
""Small"",
""Medium"",
""Large""
]   

";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (await reader.ReadAsync())
            {
            }
        }

        [Test]
        public async Task ParsingQuotedPropertyWithControlCharactersAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"{'hi\r\nbye':1}"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("hi\r\nbye", reader.Value);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(1L, reader.Value);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ParseIntegersAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("1"));
            Assert.AreEqual(1, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader("-1"));
            Assert.AreEqual(-1, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader("0"));
            Assert.AreEqual(0, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader("-0"));
            Assert.AreEqual(0, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
            Assert.AreEqual(int.MaxValue, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
            Assert.AreEqual(int.MinValue, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

            reader = new JsonTextReader(new StringReader("1.1"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, await reader.ReadAsInt32Async());

            reader = new JsonTextReader(new StringReader("-"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ParseDecimalsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("1.1"));
            Assert.AreEqual(1.1m, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.AreEqual(-1.1m, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.AreEqual(0.0m, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.AreEqual(0, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            reader.FloatParseHandling = FloatParseHandling.Decimal;
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.AreEqual(0.000001m, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, await reader.ReadAsDecimalAsync());

            reader = new JsonTextReader(new StringReader("-"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
        }

        [Test]
        public async Task ParseDoublesAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("1.1"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.000001d, reader.Value);

            reader = new JsonTextReader(new StringReader(""));
            Assert.IsFalse(await reader.ReadAsync());

            reader = new JsonTextReader(new StringReader("-"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-' is not a valid number. Path '', line 1, position 1.");

            reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.MaxValue, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.MinValue, reader.Value);

            reader = new JsonTextReader(new StringReader("1E+309"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");

            reader = new JsonTextReader(new StringReader("-1E+5000"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");

            reader = new JsonTextReader(new StringReader("5.1231231E"));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

            reader = new JsonTextReader(new StringReader("1E-23"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(1e-23, reader.Value);
        }

        [Test]
        public async Task ParseArrayWithMissingValuesAsync()
        {
            string json = "[,,, \n\r\n \0   \r  , ,    ]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public async Task ParseBooleanWithNoExtraContentAsync()
        {
            string json = "[true ";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ParseContentDelimitedByNonStandardWhitespaceAsync()
        {
            string json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ParseObjectWithNoEndAsync()
        {
            string json = "{hi:1, ";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ParseEmptyArrayAsync()
        {
            string json = "[]";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public async Task ParseEmptyObjectAsync()
        {
            string json = "{}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ParseEmptyConstructorAsync()
        {
            string json = "new Date()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Test]
        public async Task ParseHexNumberAsync()
        {
            string json = @"0x20";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(32m, reader.Value);
        }

        [Test]
        public async Task ParseNumbersAsync()
        {
            string json = @"[0,1,2 , 3]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Test]
        public async Task ParseLineFeedDelimitedConstructorAsync()
        {
            string json = "new Date\n()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("Date", reader.Value);
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Test]
        public async Task ParseNullStringConstructorAsync()
        {
            string json = "new Date\0()";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[7];
#endif

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("Date", reader.Value);
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Test]
        public async Task ParseOctalNumberAsync()
        {
            string json = @"010";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(8m, reader.Value);
        }

        [Test]
        public async Task DateParseHandlingAsync()
        {
            string json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.DateTime;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.DateTimeOffset;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.None;

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(@"1970-01-01T00:00:00Z", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(@"/Date(0)/", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.DateTime;

            Assert.IsTrue(await reader.ReadAsync());
            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = DateParseHandling.DateTimeOffset;

            Assert.IsTrue(await reader.ReadAsync());
            await reader.ReadAsDateTimeAsync();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            await reader.ReadAsDateTimeAsync();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(await reader.ReadAsync());
        }
    }
}

#endif