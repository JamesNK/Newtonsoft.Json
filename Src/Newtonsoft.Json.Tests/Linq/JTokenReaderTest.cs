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
using System.Text;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json;
using System.IO;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JTokenReaderTest : TestFixtureBase
    {
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
        [Test]
        public void ConvertBigIntegerToDouble()
        {
            var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

            JsonReader reader = jObject.CreateReader();
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(10000000000000000000d, reader.ReadAsDouble());
            Assert.IsTrue(reader.Read());
        }
#endif

        [Test]
        public void ErrorTokenIndex()
        {
            JObject json = JObject.Parse(@"{""IntList"":[1, ""two""]}");

            ExceptionAssert.Throws<Exception>(() =>
            {
                JsonSerializer serializer = new JsonSerializer();

                serializer.Deserialize<TraceTestObject>(json.CreateReader());
            }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");
        }

#if !NET20
        [Test]
        public void YahooFinance()
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

                jsonReader.Read();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
                Assert.AreEqual(false, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test1", jsonReader.Value);
                Assert.AreEqual(false, lineInfo.HasLineInfo());

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
                Assert.AreEqual(false, lineInfo.HasLineInfo());
                Assert.AreEqual(0, lineInfo.LinePosition);
                Assert.AreEqual(0, lineInfo.LineNumber);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test2", jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test3", jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
                Assert.AreEqual("Test3Value", jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test4", jsonReader.Value);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.Null, jsonReader.TokenType);
                Assert.AreEqual(null, jsonReader.Value);

                Assert.IsTrue(jsonReader.Read());
                Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

                Assert.IsFalse(jsonReader.Read());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }

            using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
            {
                Assert.IsTrue(jsonReader.Read());
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test2", jsonReader.Value);

                Assert.IsTrue(jsonReader.Read());
                Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
                Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

                Assert.IsFalse(jsonReader.Read());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Test]
        public void ReadAsDateTimeOffsetBadString()
        {
            string json = @"{""Offset"":""blablahbla""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
        }

        [Test]
        public void ReadAsDateTimeOffsetBoolean()
        {
            string json = @"{""Offset"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
        }

        [Test]
        public void ReadAsDateTimeOffsetString()
        {
            string json = @"{""Offset"":""2012-01-24T03:50Z""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDateTimeOffset();
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
        }
#endif

        [Test]
        public void ReadLineInfo()
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

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
                Assert.AreEqual(1, lineInfo.LineNumber);
                Assert.AreEqual(1, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o, jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "CPU");
                Assert.AreEqual(2, lineInfo.LineNumber);
                Assert.AreEqual(6, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("CPU"), jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "Intel");
                Assert.AreEqual(2, lineInfo.LineNumber);
                Assert.AreEqual(14, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("CPU").Value, jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
                Assert.AreEqual(jsonReader.Value, "Drives");
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(9, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("Drives"), jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(11, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o.Property("Drives").Value, jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "DVD read/writer");
                Assert.AreEqual(4, lineInfo.LineNumber);
                Assert.AreEqual(21, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"][0], jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
                Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
                Assert.AreEqual(5, lineInfo.LineNumber);
                Assert.AreEqual(29, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"][1], jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
                Assert.AreEqual(3, lineInfo.LineNumber);
                Assert.AreEqual(11, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o["Drives"], jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
                Assert.AreEqual(1, lineInfo.LineNumber);
                Assert.AreEqual(1, lineInfo.LinePosition);
                Assert.AreEqual(true, lineInfo.HasLineInfo());
                Assert.AreEqual(o, jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(null, jsonReader.CurrentToken);

                jsonReader.Read();
                Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
                Assert.AreEqual(null, jsonReader.CurrentToken);
            }
        }

        [Test]
        public void ReadBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            JObject o =
                new JObject(
                    new JProperty("Test1", data)
                    );

            using (JTokenReader jsonReader = new JTokenReader(o))
            {
                jsonReader.Read();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test1", jsonReader.Value);

                byte[] readBytes = jsonReader.ReadAsBytes();
                Assert.AreEqual(data, readBytes);

                Assert.IsTrue(jsonReader.Read());
                Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

                Assert.IsFalse(jsonReader.Read());
                Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
            }
        }

        [Test]
        public void ReadBytesFailure()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                JObject o =
                    new JObject(
                        new JProperty("Test1", 1)
                        );

                using (JTokenReader jsonReader = new JTokenReader(o))
                {
                    jsonReader.Read();
                    Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

                    jsonReader.Read();
                    Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                    Assert.AreEqual("Test1", jsonReader.Value);

                    jsonReader.ReadAsBytes();
                }
            }, "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
        }

        public class HasBytes
        {
            public byte[] Bytes { get; set; }
        }

        [Test]
        public void ReadBytesFromString()
        {
            var bytes = new HasBytes { Bytes = new byte[] { 1, 2, 3, 4 } };
            var json = JsonConvert.SerializeObject(bytes);

            TextReader textReader = new StringReader(json);
            JsonReader jsonReader = new JsonTextReader(textReader);

            var jToken = JToken.ReadFrom(jsonReader);

            jsonReader = new JTokenReader(jToken);

            var result2 = (HasBytes)JsonSerializer.Create(null)
                .Deserialize(jsonReader, typeof(HasBytes));

            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, result2.Bytes);
        }

        [Test]
        public void ReadBytesFromEmptyString()
        {
            var bytes = new HasBytes { Bytes = new byte[0] };
            var json = JsonConvert.SerializeObject(bytes);

            TextReader textReader = new StringReader(json);
            JsonReader jsonReader = new JsonTextReader(textReader);

            var jToken = JToken.ReadFrom(jsonReader);

            jsonReader = new JTokenReader(jToken);

            var result2 = (HasBytes)JsonSerializer.Create(null)
                .Deserialize(jsonReader, typeof(HasBytes));

            CollectionAssert.AreEquivalent(new byte[0], result2.Bytes);
        }

        public class ReadAsBytesTestObject
        {
            public byte[] Data;
        }

        [Test]
        public void ReadAsBytesNull()
        {
            JsonSerializer s = new JsonSerializer();

            JToken nullToken = JToken.ReadFrom(new JsonTextReader(new StringReader("{ Data: null }")));
            ReadAsBytesTestObject x = s.Deserialize<ReadAsBytesTestObject>(new JTokenReader(nullToken));
            Assert.IsNull(x.Data);
        }

        [Test]
        public void DeserializeByteArrayWithTypeNameHandling()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            string json = JsonConvert.SerializeObject(test, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            JObject o = JObject.Parse(json);

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            using (JsonReader nodeReader = o.CreateReader())
            {
                // Get exception here
                TestObject newObject = (TestObject)serializer.Deserialize(nodeReader);

                Assert.AreEqual("Test", newObject.Name);
                CollectionAssert.AreEquivalent(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }

        [Test]
        public void DeserializeStringInt()
        {
            string json = @"{
  ""PreProperty"": ""99"",
  ""PostProperty"": ""-1""
}";

            JObject o = JObject.Parse(json);

            JsonSerializer serializer = new JsonSerializer();

            using (JsonReader nodeReader = o.CreateReader())
            {
                MyClass c = serializer.Deserialize<MyClass>(nodeReader);

                Assert.AreEqual(99, c.PreProperty);
                Assert.AreEqual(-1, c.PostProperty);
            }
        }

        [Test]
        public void ReadAsDecimalInt()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(1m, reader.Value);
        }

        [Test]
        public void ReadAsInt32Int()
        {
            string json = @"{""Name"":1}";

            JObject o = JObject.Parse(json);

            JTokenReader reader = (JTokenReader)o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.AreEqual(o, reader.CurrentToken);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(o.Property("Name"), reader.CurrentToken);

            reader.ReadAsInt32();
            Assert.AreEqual(o["Name"], reader.CurrentToken);
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(typeof(int), reader.ValueType);
            Assert.AreEqual(1, reader.Value);
        }

        [Test]
        public void ReadAsInt32BadString()
        {
            string json = @"{""Name"":""hi""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
        }

        [Test]
        public void ReadAsInt32Boolean()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Test]
        public void ReadAsDecimalString()
        {
            string json = @"{""Name"":""1.1""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(typeof(decimal), reader.ValueType);
            Assert.AreEqual(1.1m, reader.Value);
        }

        [Test]
        public void ReadAsDecimalBadString()
        {
            string json = @"{""Name"":""blah""}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
        }

        [Test]
        public void ReadAsDecimalBoolean()
        {
            string json = @"{""Name"":true}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
        }

        [Test]
        public void ReadAsDecimalNull()
        {
            string json = @"{""Name"":null}";

            JObject o = JObject.Parse(json);

            JsonReader reader = o.CreateReader();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(null, reader.ValueType);
            Assert.AreEqual(null, reader.Value);
        }

        [Test]
        public void InitialPath_PropertyBase_PropertyToken()
        {
            JObject o = new JObject
            {
                { "prop1", true }
            };

            JTokenReader reader = new JTokenReader(o, "baseprop");

            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop.prop1", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop.prop1", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);
        }

        [Test]
        public void InitialPath_ArrayBase_PropertyToken()
        {
            JObject o = new JObject
            {
                { "prop1", true }
            };

            JTokenReader reader = new JTokenReader(o, "[0]");

            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0].prop1", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0].prop1", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual("[0]", reader.Path);
        }

        [Test]
        public void InitialPath_PropertyBase_ArrayToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a, "baseprop");

            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop[1]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual("baseprop", reader.Path);
        }

        [Test]
        public void InitialPath_ArrayBase_ArrayToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a, "[0]");

            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0][0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0][1]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("[0]", reader.Path);

            Assert.IsFalse(reader.Read());
            Assert.AreEqual("[0]", reader.Path);
        }

        [Test]
        public void ReadAsDouble_InvalidToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsDouble(); },
                "Error reading double. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public void ReadAsBoolean_InvalidToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsBoolean(); },
                "Error reading boolean. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public void ReadAsDateTime_InvalidToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsDateTime(); },
                "Error reading date. Unexpected token: StartArray. Path ''.");
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffset_InvalidToken()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            ExceptionAssert.Throws<JsonReaderException>(
                () => { reader.ReadAsDateTimeOffset(); },
                "Error reading date. Unexpected token: StartArray. Path ''.");
        }

        [Test]
        public void ReadAsDateTimeOffset_DateTime()
        {
            JValue v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
        }

        [Test]
        public void ReadAsDateTimeOffset_String()
        {
            JValue v = new JValue("2012-01-24T03:50Z");

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
        }

        [Test]
        public void ReadAsDateTime_DateTimeOffset()
        {
            JValue v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
        }
#endif

        [Test]
        public void ReadAsDateTime_String()
        {
            JValue v = new JValue("2012-01-24T03:50Z");

            JTokenReader reader = new JTokenReader(v);

            Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
        }

        [Test]
        public void ReadAsDouble_String_Success()
        {
            JValue s = JValue.CreateString("123.4");

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(123.4d, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_Null_Success()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsDouble());
        }

        [Test]
        public void ReadAsDouble_Integer_Success()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(1d, reader.ReadAsDouble());
        }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
        [Test]
        public void ReadAsBoolean_BigInteger_Success()
        {
            JValue s = new JValue(BigInteger.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999"));

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(true, reader.ReadAsBoolean());
        }
#endif

        [Test]
        public void ReadAsBoolean_String_Success()
        {
            JValue s = JValue.CreateString("true");

            JTokenReader reader = new JTokenReader(s);

            Assert.AreEqual(true, reader.ReadAsBoolean());
        }

        [Test]
        public void ReadAsBoolean_Null_Success()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsBoolean());
        }

        [Test]
        public void ReadAsBoolean_Integer_Success()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(true, reader.ReadAsBoolean());
        }

        [Test]
        public void ReadAsDateTime_Null_Success()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsDateTime());
        }

#if !NET20
        [Test]
        public void ReadAsDateTimeOffset_Null_Success()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsDateTimeOffset());
        }
#endif

        [Test]
        public void ReadAsString_Integer_Success()
        {
            JValue n = new JValue(1);

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual("1", reader.ReadAsString());
        }

        [Test]
        public void ReadAsString_Guid_Success()
        {
            JValue n = new JValue(new Uri("http://www.test.com"));

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual("http://www.test.com", reader.ReadAsString());
        }

        [Test]
        public void ReadAsBytes_Integer_Success()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsBytes());
        }

        [Test]
        public void ReadAsBytes_Array()
        {
            JArray a = new JArray
            {
                1, 2
            };

            JTokenReader reader = new JTokenReader(a);

            byte[] bytes = reader.ReadAsBytes();

            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(1, bytes[0]);
            Assert.AreEqual(2, bytes[1]);
        }

        [Test]
        public void ReadAsBytes_Null()
        {
            JValue n = JValue.CreateNull();

            JTokenReader reader = new JTokenReader(n);

            Assert.AreEqual(null, reader.ReadAsBytes());
        }
    }
}