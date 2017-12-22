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
using System.Threading.Tasks;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
using System.Numerics;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JTokenReaderAsyncTests : TestFixtureBase
    {
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task ConvertBigIntegerToDoubleAsync()
        {
            var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

            JsonReader reader = jObject.CreateReader();
            Assert.IsTrue(await reader.ReadAsync());
            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(10000000000000000000d, await reader.ReadAsDoubleAsync());
            Assert.IsTrue(await reader.ReadAsync());
        }
#endif

        [Test]
        public async Task YahooFinanceAsync()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                IJsonLineInfo lineInfo = jsonReader;

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
                Assert.AreEqual(false, lineInfo.HasLineInfo());

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test1", jsonReader.Value);
                Assert.AreEqual(false, lineInfo.HasLineInfo());

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
                Assert.AreEqual(false, lineInfo.HasLineInfo());
                Assert.AreEqual(0, lineInfo.LinePosition);
                Assert.AreEqual(0, lineInfo.LineNumber);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test2", jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test3", jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
                Assert.AreEqual("Test3Value", jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test4", jsonReader.Value);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.Null, jsonReader.TokenType);
                Assert.AreEqual(null, jsonReader.Value);

                Assert.IsTrue(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

                Assert.IsFalse(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }

            using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
            {
                Assert.IsTrue(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test2", jsonReader.Value);

                Assert.IsTrue(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                Assert.IsFalse(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Test]
        public async Task ReadAsDateTimeOffsetBadStringAsync()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }

        [Test]
        public async Task ReadAsDateTimeOffsetBooleanAsync()
        {
            string json = @"{""Offset"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
        }

        [Test]
        public async Task ReadAsDateTimeOffsetStringAsync()
        {
            string json = @"{""Offset"":""2012-01-24T03:50Z""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDateTimeOffsetAsync();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
        }

        [Test]
        public async Task ReadLineInfoAsync()
        {
            string input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            JObject o = JObject.Parse(input);

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                IJsonLineInfo lineInfo = jsonReader;

                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(0, lineInfo.LineNumber);
                Assert.AreEqual(0, lineInfo.LinePosition);
                Assert.AreEqual(false, lineInfo.HasLineInfo());
                Assert.AreEqual(null, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
                Assert.AreEqual(1, lineInfo.LineNumber);
                Assert.AreEqual(1, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "CPU");
                Assert.AreEqual(2, lineInfo.LineNumber);
                Assert.AreEqual(6, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("CPU"), jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "Intel");
                Assert.AreEqual(2, lineInfo.LineNumber);
                Assert.AreEqual(14, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("CPU").Value, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "Drives");
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(9, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("Drives"), jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(11, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("Drives").Value, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "DVD read/writer");
                Assert.AreEqual(4, lineInfo.LineNumber);
                Assert.AreEqual(21, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"][0], jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
                Assert.AreEqual(5, lineInfo.LineNumber);
                Assert.AreEqual(29, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"][1], jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(11, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"], jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
                Assert.AreEqual(1, lineInfo.LineNumber);
                Assert.AreEqual(1, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(null, jsonReader.CurrentToken);

                await jsonReader.ReadAsync();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(null, jsonReader.CurrentToken);
            }
        }

        [Test]
        public async Task ReadBytesAsync()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            JObject o =
                new JObject(
                    new JProperty("Test1", data)
                    );

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

                await jsonReader.ReadAsync();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test1", jsonReader.Value);

                byte[] readBytes = await jsonReader.ReadAsBytesAsync();
                Assert.AreEqual(data, readBytes);

                Assert.IsTrue(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

                Assert.IsFalse(await jsonReader.ReadAsync());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Test]
        public async Task ReadBytesFailureAsync()
        {
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                JObject o =
                    new JObject(
                        new JProperty("Test1", 1)
                        );

                using (JTokenReader jsonReader = new JTokenReader(o))
                {
                    await jsonReader.ReadAsync();
                    Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

                    await jsonReader.ReadAsync();
                    Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                    Assert.AreEqual("Test1", jsonReader.Value);

                    await jsonReader.ReadAsBytesAsync();
                }
            }, "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
        }

        public class HasBytes
        {
            public byte[] Bytes { get; set; }
        }

        [Test]
        public async Task ReadAsDecimalIntAsync()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

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
        public async Task ReadAsInt32IntAsync()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JTokenReader reader = (JTokenReader)o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.AreEqual(o, reader.CurrentToken);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(o.Property("Name"), reader.CurrentToken);

            await reader.ReadAsInt32Async();
            Assert.AreEqual(o["Name"], reader.CurrentToken);
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(typeof(int), reader.ValueType);
            Assert.AreEqual(1, reader.Value);
        }

        [Test]
        public async Task ReadAsInt32BadStringAsync()
        {
            string json = @"{""Name"":""hi""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
        }

        [Test]
        public async Task ReadAsInt32BooleanAsync()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Test]
        public async Task ReadAsDecimalStringAsync()
        {
            string json = @"{""Name"":""1.1""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(1.1m, reader.Value);
        }

        [Test]
        public async Task ReadAsDecimalBadStringAsync()
        {
            string json = @"{""Name"":""blah""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
        }

        [Test]
        public async Task ReadAsDecimalBooleanAsync()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Test]
        public async Task ReadAsDecimalNullAsync()
        {
            string json = @"{""Name"":null}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            await reader.ReadAsDecimalAsync();
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(null, reader.ValueType);
            Assert.AreEqual(null, reader.Value);
        }

        [Test]
        public async Task InitialPath_PropertyBase_PropertyTokenAsync()
        {
            JObject o = new JObject
            {
                { "prop1", true }
            };

            JTokenReader reader = new JTokenReader(o, "baseprop");

            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop.prop1", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop.prop1", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);
        }

        [Test]
        public async Task InitialPath_ArrayBase_PropertyTokenAsync()
        {
            JObject o = new JObject
            {
                { "prop1", true }
            };

            JTokenReader reader = new JTokenReader(o, "[0]");

            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0].prop1", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0].prop1", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);
        }

        [Test]
        public async Task InitialPath_PropertyBase_ArrayTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a, "baseprop");

            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop[0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop[1]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual("baseprop", reader.Path);
        }

        [Test]
        public async Task InitialPath_ArrayBase_ArrayTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a, "[0]");

            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0][0]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0][1]", reader.Path);

            Assert.IsTrue(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsFalse(await reader.ReadAsync());
            Assert.AreEqual("[0]", reader.Path);
        }

        [Test]
        public async Task ReadAsDouble_InvalidTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                async () => { await reader.ReadAsDoubleAsync(); },
                "Error reading double. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public async Task ReadAsBoolean_InvalidTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                async () => { await reader.ReadAsBooleanAsync(); },
                "Error reading boolean. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public async Task ReadAsDateTime_InvalidTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                async () => { await reader.ReadAsDateTimeAsync(); },
                "Error reading date. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public async Task ReadAsDateTimeOffset_InvalidTokenAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(
                async () => { await reader.ReadAsDateTimeOffsetAsync(); },
                "Error reading date. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public async Task ReadAsDateTimeOffset_DateTimeAsync()
        {
            JValue v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
        }

        [Test]
        public async Task ReadAsDateTimeOffset_StringAsync()
        {
            JValue v = new JValue("2012-01-24T03:50Z");

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
        }

        [Test]
        public async Task ReadAsDateTime_DateTimeOffsetAsync()
        {
            JValue v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
        }

        [Test]
        public async Task ReadAsDateTime_StringAsync()
        {
            JValue v = new JValue("2012-01-24T03:50Z");

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
        }

        [Test]
        public async Task ReadAsDouble_String_SuccessAsync()
        {
            JValue s = JValue.CreateString("123.4");

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(123.4d, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_Null_SuccessAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsDoubleAsync());
        }

        [Test]
        public async Task ReadAsDouble_Integer_SuccessAsync()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(1d, await reader.ReadAsDoubleAsync());
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public async Task ReadAsBoolean_BigInteger_SuccessAsync()
        {
            JValue s = new JValue(BigInteger.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999"));

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
        }
#endif

        [Test]
        public async Task ReadAsBoolean_String_SuccessAsync()
        {
            JValue s = JValue.CreateString("true");

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
        }

        [Test]
        public async Task ReadAsBoolean_Null_SuccessAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsBooleanAsync());
        }

        [Test]
        public async Task ReadAsBoolean_Integer_SuccessAsync()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(true, await reader.ReadAsBooleanAsync());
        }

        [Test]
        public async Task ReadAsDateTime_Null_SuccessAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsDateTimeAsync());
        }

        [Test]
        public async Task ReadAsDateTimeOffset_Null_SuccessAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsDateTimeOffsetAsync());
        }

        [Test]
        public async Task ReadAsString_Integer_SuccessAsync()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual("1", await reader.ReadAsStringAsync());
        }

        [Test]
        public async Task ReadAsString_Guid_SuccessAsync()
        {
            JValue n = new JValue(new Uri("http://www.test.com"));

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual("http://www.test.com", await reader.ReadAsStringAsync());
        }

        [Test]
        public async Task ReadAsBytes_Integer_SuccessAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsBytesAsync());
        }

        [Test]
        public async Task ReadAsBytes_ArrayAsync()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            byte[] bytes = await reader.ReadAsBytesAsync();

            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(1, bytes[0]);
            Assert.AreEqual(2, bytes[1]);
        }

        [Test]
        public async Task ReadAsBytes_NullAsync()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, await reader.ReadAsBytesAsync());
        }
    }
}

#endif