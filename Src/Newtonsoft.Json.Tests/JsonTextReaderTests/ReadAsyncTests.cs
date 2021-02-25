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
using System.Globalization;
using Newtonsoft.Json.Linq;
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
using System.Numerics;
#endif
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Tests.TestObjects.JsonTextReaderTests;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.JsonTextReaderTests
{
    [TestFixture]
#if !DNXCORE50
    [Category("JsonTextReaderTests")]
#endif
    public class ReadAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task Read_EmptyStream_ReturnsFalse()
        {
            MemoryStream ms = new MemoryStream();
            StreamReader sr = new StreamReader(ms);

            JsonTextReader reader = new JsonTextReader(sr);
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsInt32Async_IntegerTooLarge_ThrowsJsonReaderException()
        {
            JValue token = new JValue(long.MaxValue);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                () => token.CreateReader().ReadAsInt32Async(),
                "Could not convert to integer: 9223372036854775807. Path ''."
            );
        }

        [Test]
        public async Task ReadAsDecimalAsync_IntegerTooLarge_ThrowsJsonReaderException()
        {
            JValue token = new JValue(double.MaxValue);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                () => token.CreateReader().ReadAsDecimalAsync(),
                "Could not convert to decimal: 1.79769313486232E+308. Path ''.",
                "Could not convert to decimal: 1.7976931348623157E+308. Path ''."
            );
        }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task ReadAsInt32Async_BigIntegerValue_Success()
        {
            JValue token = new JValue(BigInteger.Parse("1"));

            int? i = await token.CreateReader().ReadAsInt32Async();
            Assert.AreEqual(1, i);
        }
#endif

        [Test]
        public async Task ReadMissingInt64()
        {
            string json = "{ A: \"\", B: 1, C: , D: 1.23, E: 3.45, F: null }";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("C", reader.Value);

            await reader.ReadAsync();
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);
            Assert.AreEqual(null, reader.Value);
        }

        [Test]
        public async Task ReadAsInt32AsyncWithUndefined()
        {
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader("undefined"));
                    await reader.ReadAsInt32Async();
                },
                "Unexpected character encountered while parsing value: u. Path '', line 1, position 1.");
        }

#if !(PORTABLE || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task ReadAsBooleanAsync()
        {
            string json = @"[
  1,
  0,
  1.1,
  0.0,
  0.000000000001,
  9999999999,
  -9999999999,
  9999999999999999999999999999999999999999999999999999999999999999999999,
  -9999999999999999999999999999999999999999999999999999999999999999999999,
  'true',
  'TRUE',
  'false',
  'FALSE',
  // comment!
  /* comment! */
  '',
  null
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[10];
#endif

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[0]", reader.Path);

            Assert.AreEqual(false, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[1]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[2]", reader.Path);

            Assert.AreEqual(false, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[3]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[4]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[5]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[6]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[7]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[8]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[9]", reader.Path);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[10]", reader.Path);

            Assert.AreEqual(false, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[11]", reader.Path);

            Assert.AreEqual(false, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[12]", reader.Path);

            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[13]", reader.Path);

            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
            Assert.AreEqual("[14]", reader.Path);

            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("", reader.Path);

            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
            Assert.AreEqual("", reader.Path);
        }
#endif

        [Test]
        public async Task ReadAsBoolean_NullCharAsync()
        {
            string json = '\0' + @"true" + '\0' + '\0';

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
        }

        [Test]
        public async Task ReadAsBytesAsync()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world");

            string json = @"""" + Convert.ToBase64String(data) + @"""";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            byte[] result = await reader.ReadAsBytesAsync();

            CollectionAssert.AreEquivalent(data, result);
        }

        [Test]
        public async Task ReadAsBooleanNoContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(await reader.ReadAsBooleanAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadAsBytesIntegerArrayWithCommentsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

            byte[] data = await reader.ReadAsBytesAsync();
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(2, data[1]);
        }

        [Test]
        public async Task ReadUnicodeAsync()
        {
            string json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[5];
#endif

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Message", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"Hi,I" + '\u0092' + "ve send you smth", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadHexidecimalWithAllLettersAsync()
        {
            string json = @"{""text"":0xabcdef12345}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(11806310474565, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

#if DEBUG
        [Test]
        public async Task ReadLargeObjectsAsync()
        {
            const int nrItems = 2;
            const int length = 1200;
            const int largeBufferLength = 2048;

            byte apostrophe = Encoding.ASCII.GetBytes(@"""").First();
            byte openingBracket = Encoding.ASCII.GetBytes(@"[").First();
            byte comma = Encoding.ASCII.GetBytes(@",").First();
            byte closingBracket = Encoding.ASCII.GetBytes(@"]").First();

            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(openingBracket);
                for (int i = 0; i < nrItems; i++)
                {
                    ms.WriteByte(apostrophe);

                    for (int j = 0; j <= length; j++)
                    {
                        byte current = Convert.ToByte((j % 10) + 48);
                        ms.WriteByte(current);
                    }

                    ms.WriteByte(apostrophe);
                    if (i < nrItems - 1)
                    {
                        ms.WriteByte(comma);
                    }
                }

                ms.WriteByte(closingBracket);
                ms.Seek(0, SeekOrigin.Begin);

                JsonTextReader reader = new JsonTextReader(new StreamReader(ms));
                reader.LargeBufferLength = largeBufferLength;

                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.String, reader.TokenType);
                Assert.AreEqual(largeBufferLength, reader.CharBuffer.Length);

                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.String, reader.TokenType);
                // buffer has been shifted before reading the second string
                Assert.AreEqual(largeBufferLength, reader.CharBuffer.Length);

                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

                Assert.IsFalse(await reader.ReadAsync());
            }
        }
#endif

        [Test]
        public async Task ReadSingleBytesAsync()
        {
            StringReader s = new StringReader(@"""SGVsbG8gd29ybGQu""");
            JsonTextReader reader = new JsonTextReader(s);

            byte[] data = await reader.ReadAsBytesAsync();
            Assert.IsNotNull(data);

            string text = Encoding.UTF8.GetString(data, 0, data.Length);
            Assert.AreEqual("Hello world.", text);
        }

        [Test]
        public async Task ReadOctalNumberAsync()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(250L, jsonReader.Value);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(await jsonReader.ReadAsync());
        }

        [Test]
        public async Task ReadOctalNumberAsInt64Async()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

            await jsonReader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual(250L, (long)jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual(250L, (long)jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(long), jsonReader.ValueType);
            Assert.AreEqual(250L, (long)jsonReader.Value);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(await jsonReader.ReadAsync());
        }

        [Test]
        public async Task ReadOctalNumberAsInt32Async()
        {
            StringReader s = new StringReader(@"[0372, 0xFA, 0XFA]");
            JsonTextReader jsonReader = new JsonTextReader(s);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

            await jsonReader.ReadAsInt32Async();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            await jsonReader.ReadAsInt32Async();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            await jsonReader.ReadAsInt32Async();
            Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
            Assert.AreEqual(typeof(int), jsonReader.ValueType);
            Assert.AreEqual(250, jsonReader.Value);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

            Assert.IsFalse(await jsonReader.ReadAsync());
        }

        [Test]
        public async Task ReadAsDecimalNoContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(await reader.ReadAsDecimalAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadAsBytesNoContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(await reader.ReadAsBytesAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetNoContentAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@""));

            Assert.IsNull(await reader.ReadAsDateTimeOffsetAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetAsync()
        {
            string json = "{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetNegativeAsync()
        {
            string json = @"{""Offset"":""\/Date(946706400000-0600)\/""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetBadStringAsync()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDateTimeOffsetAsync();
            }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }

        [Test]
        public async Task ReadAsDateTimeOffsetHoursOnlyAsync()
        {
            string json = "{\"Offset\":\"\\/Date(946663200000+06)\\/\"}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetWithMinutesAsync()
        {
            string json = @"{""Offset"":""\/Date(946708260000-0631)\/""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetIsoDateAsync()
        {
            string json = @"{""Offset"":""2011-08-01T21:25Z""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetUnitedStatesDateAsync()
        {
            string json = @"{""Offset"":""1/30/2011""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("en-US");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

            DateTimeOffset dt = (DateTimeOffset)reader.Value;
            Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDateTimeOffsetNewZealandDateAsync()
        {
            string json = @"{""Offset"":""30/1/2011""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("en-NZ");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

            DateTimeOffset dt = (DateTimeOffset)reader.Value;
            Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDecimalIntAsync()
        {
            string json = @"{""Name"":1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(1m, reader.Value);
        }

        [Test]
        public async Task ReadAsIntDecimalAsync()
        {
            string json = @"{""Name"": 1.1}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsInt32Async();
            }, "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
        }

        [Test]
        public async Task ReadAsDecimalAsync()
        {
            string json = @"{""decimal"":-7.92281625142643E+28}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            decimal? d = await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(-79228162514264300000000000000m, d);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadAsDecimalFrenchAsync()
        {
            string json = @"{""decimal"":""9,99""}";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Culture = new CultureInfo("fr-FR");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            decimal? d = await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(9.99m, d);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public async Task ReadBufferOnControlCharAsync()
        {
            string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[5];
#endif

            for (int i = 0; i < 13; i++)
            {
                await reader.ReadAsync();
            }

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(new DateTime(631136448000000000), reader.Value);
        }

        [Test]
        public async Task ReadBufferOnEndCommentAsync()
        {
            string json = @"/*comment*/ { /*comment*/
        ""Name"": /*comment*/ ""Apple"" /*comment*/, /*comment*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment*/ [ /*comment*/
          ""Small"", /*comment*/
          ""Medium"" /*comment*/,
          /*comment*/ ""Large""
        /*comment*/ ] /*comment*/
      } /*comment*/";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[5];
#endif

            for (int i = 0; i < 26; i++)
            {
                Assert.IsTrue(await reader.ReadAsync());
            }

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsDouble_NullAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("null"));
            Assert.AreEqual(null, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_SuccessAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("'12.34'"));
            Assert.AreEqual(12.34d, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_HexAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("0XCAFEBABE"));
            Assert.AreEqual(3405691582d, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_AllowThousandsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("'1,112.34'"));
            Assert.AreEqual(1112.34d, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_FailureAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['Trump',1]"));

            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsDoubleAsync();
            }, "Could not convert string to double: Trump. Path '[0]', line 1, position 8.");

            Assert.AreEqual(1d, await reader.ReadAsDoubleAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsString_BooleanAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false}"));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            string s = await reader.ReadAsStringAsync();
            Assert.AreEqual("false", s);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task Read_Boolean_FailureAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsync();
            }, "Error parsing boolean value. Path 'Test1', line 1, position 14.");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadAsString_Boolean_FailureAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                await reader.ReadAsStringAsync();
            }, "Unexpected character encountered while parsing value: 1. Path 'Test1', line 1, position 14.");

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadValue_EmptyString_PositionAsync()
        {
            string json = @"['','','','','','','']";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            await reader.ReadAsync();
            await reader.ReadAsInt32Async();
            Assert.AreEqual("[0]", reader.Path);
            await reader.ReadAsDecimalAsync();
            Assert.AreEqual("[1]", reader.Path);
            await reader.ReadAsDateTimeAsync();
            Assert.AreEqual("[2]", reader.Path);
            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual("[3]", reader.Path);
            await reader.ReadAsStringAsync();
            Assert.AreEqual("[4]", reader.Path);
            await reader.ReadAsBytesAsync();
            Assert.AreEqual("[5]", reader.Path);
            await reader.ReadAsDoubleAsync();
            Assert.AreEqual("[6]", reader.Path);

            Assert.IsNull(await reader.ReadAsStringAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNull(await reader.ReadAsStringAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);

            Assert.IsNull(await reader.ReadAsBytesAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadValueCommentsAsync()
        {
            string json = @"/*comment*/[/*comment*/1/*comment*/,2,/*comment*//*comment*/""three""/*comment*/,""four""/*comment*/,null,/*comment*/null,3.99,1.1/*comment*/,''/*comment*/]/*comment*/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.AreEqual(1, await reader.ReadAsInt32Async());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.AreEqual(2, await reader.ReadAsInt32Async());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.AreEqual("three", await reader.ReadAsStringAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.AreEqual("four", await reader.ReadAsStringAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.AreEqual(null, await reader.ReadAsStringAsync());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.AreEqual(null, await reader.ReadAsInt32Async());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.AreEqual(3.99m, await reader.ReadAsDecimalAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.AreEqual(1.1m, await reader.ReadAsDecimalAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            CollectionAssert.AreEquivalent(new byte[0], await reader.ReadAsBytesAsync());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.AreEqual(null, await reader.ReadAsInt32Async());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadContentDelimitedByCommentsAsync()
        {
            string json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ""ExpiryDate"":/*comment*/new
" + StringUtils.LineFeed + @"Date
(/*comment*/null/*comment*/),
        ""Price"": 3.99,
        ""Sizes"":/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Name", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("ExpiryDate", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
            Assert.AreEqual(5, reader.LineNumber);
            Assert.AreEqual("Date", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Test]
        public async Task ReadNullIntLineNumberAndPositionAsync()
        {
            string json = @"[
  1,
  2,
  3,
  null
]";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            await reader.ReadAsync();
            Assert.AreEqual(1, reader.LineNumber);

            await reader.ReadAsInt32Async();
            Assert.AreEqual(2, reader.LineNumber);
            Assert.AreEqual("[0]", reader.Path);

            await reader.ReadAsInt32Async();
            Assert.AreEqual(3, reader.LineNumber);
            Assert.AreEqual("[1]", reader.Path);

            await reader.ReadAsInt32Async();
            Assert.AreEqual(4, reader.LineNumber);
            Assert.AreEqual("[2]", reader.Path);

            await reader.ReadAsInt32Async();
            Assert.AreEqual(5, reader.LineNumber);
            Assert.AreEqual("[3]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(6, reader.LineNumber);
            Assert.AreEqual(string.Empty, reader.Path);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadingFromSlowStreamAsync()
        {
            string json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(false, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(true, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(false, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("test!", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(1.11d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(0.0000025d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(3000000000d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(602214180000000000000000d, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(reader.Value, "Purple\r \n monkey's:\tdishwasher");

            Assert.IsTrue(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadCommentInsideArrayAsync()
        {
            string json = @"{
    ""projects"": [
        ""src"",
        //""
        ""test""
    ]
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual("src", jsonTextReader.Value);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, jsonTextReader.TokenType);
            Assert.AreEqual(@"""", jsonTextReader.Value);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual("test", jsonTextReader.Value);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
        }

        [Test]
        public async Task ReadAsBytes_Base64AndGuidAsync()
        {
            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'"));
            byte[] data = await jsonTextReader.ReadAsBytesAsync();
            byte[] expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

            CollectionAssert.AreEqual(expected, data);

            jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'"));
            data = await jsonTextReader.ReadAsBytesAsync();
            expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

            CollectionAssert.AreEqual(expected, data);
        }

        [Test]
        public async Task ReadSingleQuoteInsideDoubleQuoteStringAsync()
        {
            string json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
            await jsonTextReader.ReadAsync();
            await jsonTextReader.ReadAsync();
            await jsonTextReader.ReadAsync();

            Assert.AreEqual(@"Forest's Bakery And Cafe", jsonTextReader.Value);
        }

        [Test]
        public async Task ReadMultilineStringAsync()
        {
            string json = @"""first line
second line
third line""";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);

            Assert.AreEqual(@"first line
second line
third line", jsonTextReader.Value);
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task ReadBigIntegerAsync()
        {
            string json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333,
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);
            Assert.AreEqual(typeof(BigInteger), jsonTextReader.ValueType);
            Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(await jsonTextReader.ReadAsync());

            JObject o = JObject.Parse(json);
            var i = (BigInteger)((JValue)o["ChildId"]).Value;
            Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), i);
        }
#endif

        [Test]
        public async Task ReadBadMSDateAsStringAsync()
        {
            string json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
            Assert.AreEqual(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

            Assert.IsTrue(await jsonTextReader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

            Assert.IsFalse(await jsonTextReader.ReadAsync());
        }

        [Test]
        public async Task ReadConstructorAsync()
        {
            string json = @"{""DefaultConverter"":new Date(0, ""hi""),""MemberConverter"":""1970-01-01T00:00:00Z""}";

            JsonReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
            Assert.AreEqual("Date", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(0L, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("hi", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("MemberConverter", reader.Value);
        }

        [Test]
        public async Task ReadingIndentedAsync()
        {
            string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            StringReader sr = new StringReader(input);

            using (JsonTextReader jsonReader = new JsonTextReader(sr))
            {
#if DEBUG
                jsonReader.CharBuffer = new char[5];
#endif

                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(0, jsonReader.LineNumber);
                Assert.AreEqual(0, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
                Assert.AreEqual(1, jsonReader.LineNumber);
                Assert.AreEqual(1, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "CPU");
                Assert.AreEqual(2, jsonReader.LineNumber);
                Assert.AreEqual(6, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
                Assert.AreEqual("Intel", jsonReader.Value);
                Assert.AreEqual(2, jsonReader.LineNumber);
                Assert.AreEqual(14, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "Drives");
                Assert.AreEqual(3, jsonReader.LineNumber);
                Assert.AreEqual(9, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
                Assert.AreEqual(3, jsonReader.LineNumber);
                Assert.AreEqual(11, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "DVD read/writer");
                Assert.AreEqual(jsonReader.QuoteChar, '\'');
                Assert.AreEqual(4, jsonReader.LineNumber);
                Assert.AreEqual(21, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
                Assert.AreEqual(jsonReader.QuoteChar, '"');
                Assert.AreEqual(5, jsonReader.LineNumber);
                Assert.AreEqual(29, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
                Assert.AreEqual(6, jsonReader.LineNumber);
                Assert.AreEqual(3, jsonReader.LinePosition);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
                Assert.AreEqual(7, jsonReader.LineNumber);
                Assert.AreEqual(1, jsonReader.LinePosition);

                Assert.IsFalse(await jsonReader.ReadAsync());
            }
        }

        [Test]
        public async Task ReadLongStringAsync()
        {
            string s = new string('a', 10000);
            JsonReader reader = new JsonTextReader(new StringReader("'" + s + "'"));
            await reader.ReadAsync();

            Assert.AreEqual(s, reader.Value);
        }

        [Test]
        public async Task ReadLongJsonArrayAsync()
        {
            int valueCount = 10000;
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartArray();
            for (int i = 0; i < valueCount; i++)
            {
                writer.WriteValue(i);
            }

            writer.WriteEndArray();

            string json = sw.ToString();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            Assert.IsTrue(await reader.ReadAsync());
            for (int i = 0; i < valueCount; i++)
            {
                Assert.IsTrue(await reader.ReadAsync());
                Assert.AreEqual((long)i, reader.Value);
            }

            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task NullCharReadingAsync()
        {
            string json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("\0sd\0f\0", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);
            Assert.AreEqual("\0sd\0f\0", reader.Value);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadNullTerminatorStringsAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
            Assert.IsTrue(await reader.ReadAsync());

            Assert.AreEqual("h\0i", reader.Value);
        }

        [Test]
        public async Task ReadBytesNoStartWithUnexpectedEndAsync()
        {
            JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
            Assert.IsTrue(await reader.ReadAsync());

            Assert.IsNull(await reader.ReadAsBytesAsync());
            Assert.AreEqual(JsonToken.None, reader.TokenType);
        }

        [Test]
        public async Task ReadNewLinesAsync()
        {
            string newLinesText = StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed + StringUtils.LineFeed + StringUtils.CarriageReturnLineFeed + " " + StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed;

            string json = newLinesText + "{" + newLinesText + "'" + newLinesText + "name1" + newLinesText + "'" + newLinesText + ":" + newLinesText + "[" + newLinesText + "new" + newLinesText + "Date" + newLinesText + "(" + newLinesText + "1" + newLinesText + "," + newLinesText + "null" + newLinesText + "/*" + newLinesText + "blah comment" + newLinesText + "*/" + newLinesText + ")" + newLinesText + "," + newLinesText + "1.1111" + newLinesText + "]" + newLinesText + "," + newLinesText + "name2" + newLinesText + ":" + newLinesText + "{" + newLinesText + "}" + newLinesText + "}" + newLinesText;

            int count = 0;
            StringReader sr = new StringReader(newLinesText);
            while (sr.ReadLine() != null)
            {
                count++;
            }

            JsonTextReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(7, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(31, reader.LineNumber);
            Assert.AreEqual(newLinesText + "name1" + newLinesText, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(37, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(55, reader.LineNumber);
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
            Assert.AreEqual("Date", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(61, reader.LineNumber);
            Assert.AreEqual(1L, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(73, reader.LineNumber);
            Assert.AreEqual(null, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(91, reader.LineNumber);
            Assert.AreEqual(newLinesText + "blah comment" + newLinesText, reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(97, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(109, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(115, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(133, reader.LineNumber);
            Assert.AreEqual("name2", reader.Value);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(139, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(145, reader.LineNumber);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(151, reader.LineNumber);
        }

        [Test]
        public async Task ReadBytesFollowingNumberInArrayAsync()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"[1,'" + Convert.ToBase64String(helloWorldData) + @"']"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            byte[] data = await reader.ReadAsBytesAsync();
            CollectionAssert.AreEquivalent(helloWorldData, data);
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadBytesFollowingNumberInObjectAsync()
        {
            string helloWorld = "Hello world!";
            byte[] helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

            JsonReader reader = new JsonTextReader(new StringReader(@"{num:1,data:'" + Convert.ToBase64String(helloWorldData) + @"'}"));
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.IsTrue(await reader.ReadAsync());
            byte[] data = await reader.ReadAsBytesAsync();
            CollectionAssert.AreEquivalent(helloWorldData, data);
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(await reader.ReadAsync());
        }

        [Test]
        public async Task ReadingEscapedStringsAsync()
        {
            string input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

            StringReader sr = new StringReader(input);

            using (JsonReader jsonReader = new JsonTextReader(sr))
            {
                Assert.AreEqual(0, jsonReader.Depth);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
                Assert.AreEqual(0, jsonReader.Depth);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual(1, jsonReader.Depth);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
                Assert.AreEqual('\'', jsonReader.QuoteChar);
                Assert.AreEqual(1, jsonReader.Depth);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);
                Assert.AreEqual(0, jsonReader.Depth);
            }
        }

        [Test]
        public async Task ReadRandomJsonAsync()
        {
            string json = @"[
  true,
  {
    ""integer"": 99,
    ""string"": ""how now brown cow?"",
    ""array"": [
      0,
      1,
      2,
      3,
      4,
      {
        ""decimal"": 990.00990099
      },
      5
    ]
  },
  ""This is a string."",
  null,
  null
]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (await reader.ReadAsync())
            {
            }
        }

        [Test]
        public void AsyncMethodsAlreadyCancelled()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();

            JsonTextReader reader = new JsonTextReader(new StreamReader(Stream.Null));
            Assert.IsTrue(reader.ReadAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBooleanAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBytesAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDecimalAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsInt32Async(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsStringAsync(token).IsCanceled);
        }

        private class NoOverridesDerivedJsonTextAsync : JsonTextReader
        {
            public NoOverridesDerivedJsonTextAsync()
                : base(new StreamReader(Stream.Null))
            {
            }
        }

        private class MinimalOverridesDerivedJsonReader : JsonReader
        {
            public override bool Read() => true;
        }

        [Test]
        public void AsyncMethodsAlreadyCancelledOnTextReaderSubclass()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();

            JsonTextReader reader = new NoOverridesDerivedJsonTextAsync();
            Assert.IsTrue(reader.ReadAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBooleanAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBytesAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDecimalAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsInt32Async(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsStringAsync(token).IsCanceled);
        }

        [Test]
        public void AsyncMethodsAlreadyCancelledOnReaderSubclass()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            source.Cancel();

            JsonReader reader = new MinimalOverridesDerivedJsonReader();
            Assert.IsTrue(reader.ReadAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBooleanAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsBytesAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsDecimalAsync(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsInt32Async(token).IsCanceled);
            Assert.IsTrue(reader.ReadAsStringAsync(token).IsCanceled);
        }

        [Test]
        public async Task ThrowOnDuplicateKeysDeserializingAsync()
        {
            string json = @"
                {
                    ""a"": 1,
                    ""b"": [
                        {
                            ""c"": {
                                ""d"": 1,
                                ""d"": ""2""
                            }
                        }
                    ]
                }
            ";

            JsonLoadSettings settings = new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error };

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await JToken.ReadFromAsync(reader, settings));
        }
    }
}
#endif