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


#if NETFX_CORE
using System;
using Newtonsoft.Json.Converters;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Windows.Data.Json;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class JsonValueConverterTests : TestFixtureBase
    {
        public class Computer
        {
            public string Cpu { get; set; }
            public List<string> Drives { get; set; }
        }

        [Test]
        public void WriteJson()
        {
            JsonObject o = JsonObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            JsonValueConverter converter = new JsonValueConverter();
            converter.WriteJson(writer, o, null);

            string json = sw.ToString();

            Assert.AreEqual(@"{""Drives"":[""DVD read/writer"",""500 gigabyte hard drive""],""CPU"":""Intel""}", json);
        }

        [Test]
        public void ReadJson()
        {
            string json = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();
            JsonObject o = (JsonObject) converter.ReadJson(writer, typeof (JsonObject), null, null);

            Assert.AreEqual(2, o.Count);
            Assert.AreEqual("Intel", o.GetNamedString("CPU"));
            Assert.AreEqual("DVD read/writer", o.GetNamedArray("Drives")[0].GetString());
            Assert.AreEqual("500 gigabyte hard drive", o.GetNamedArray("Drives")[1].GetString());
        }

        [Test]
        public void ReadJsonComments()
        {
            string json = @"{/*comment!*/
  ""CPU"": ""Intel"",/*comment!*/
  ""Drives"": [/*comment!*/
    ""DVD read/writer"",
    /*comment!*/""500 gigabyte hard drive""
  ]/*comment!*/
}";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();
            JsonObject o = (JsonObject) converter.ReadJson(writer, typeof (JsonObject), null, null);

            Assert.AreEqual(2, o.Count);
            Assert.AreEqual("Intel", o.GetNamedString("CPU"));
            Assert.AreEqual("DVD read/writer", o.GetNamedArray("Drives")[0].GetString());
            Assert.AreEqual("500 gigabyte hard drive", o.GetNamedArray("Drives")[1].GetString());
        }

        [Test]
        public void ReadJsonNullValue()
        {
            string json = "null";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();
            JsonValue v = (JsonValue) converter.ReadJson(writer, typeof (JsonValue), null, null);

            Assert.AreEqual(JsonValueType.Null, v.ValueType);
        }

        [Test]
        public void ReadJsonUnsupportedValue()
        {
            string json = "undefined";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();

            ExceptionAssert.Throws<JsonException>(() =>
            {
                converter.ReadJson(writer, typeof (JsonValue), null, null);
            }, "Unexpected or unsupported token: Undefined. Path '', line 1, position 9.");
        }

        [Test]
        public void ReadJsonUnexpectedEndInArray()
        {
            string json = "[";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();

            ExceptionAssert.Throws<JsonException>(() =>
            {
                converter.ReadJson(writer, typeof (JsonValue), null, null);
            }, "Unexpected end when reading JSON. Path '', line 1, position 1.");
        }

        [Test]
        public void ReadJsonUnexpectedEndAfterComment()
        {
            string json = "[/*comment!*/";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();

            ExceptionAssert.Throws<JsonException>(() =>
            {
                converter.ReadJson(writer, typeof (JsonValue), null, null);
            }, "Unexpected end when reading JSON. Path '', line 1, position 13.");
        }

        [Test]
        public void ReadJsonUnexpectedEndInObject()
        {
            string json = "{'hi':";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();

            ExceptionAssert.Throws<JsonException>(() =>
            {
                converter.ReadJson(writer, typeof (JsonValue), null, null);
            }, "Unexpected end when reading JSON. Path 'hi', line 1, position 6.");
        }

        [Test]
        public void ReadJsonBadJsonType()
        {
            string json = "null";

            JsonTextReader writer = new JsonTextReader(new StringReader(json));

            JsonValueConverter converter = new JsonValueConverter();

            ExceptionAssert.Throws<JsonException>(() =>
            {
                converter.ReadJson(writer, typeof (JsonObject), null, null);
            },
                "Could not convert 'Windows.Data.Json.JsonValue' to 'Windows.Data.Json.JsonObject'. Path '', line 1, position 4.");
        }

        [Test]
        public void JsonConvertDeserialize()
        {
            string json = @"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]";

            JsonArray a = JsonConvert.DeserializeObject<JsonArray>(json);

            Assert.AreEqual(2, a.Count);
            Assert.AreEqual("DVD read/writer", a[0].GetString());
            Assert.AreEqual("500 gigabyte hard drive", a[1].GetString());
        }

        [Test]
        public void JsonConvertSerialize()
        {
            JsonArray a = JsonArray.Parse(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]");

            string json = JsonConvert.SerializeObject(a, Formatting.Indented);

            Assert.AreEqual(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", json);
        }

        [Test]
        public void SerializeDouble()
        {
            JsonObject o = new JsonObject();
            o["zero"] = JsonValue.CreateNumberValue(0);
            o["int"] = JsonValue.CreateNumberValue(1);
            o["smallfraction"] = JsonValue.CreateNumberValue(3.0000000000000009);
            o["double"] = JsonValue.CreateNumberValue(1.1);
            o["probablyint"] = JsonValue.CreateNumberValue(1.0);
            o["Epsilon"] = JsonValue.CreateNumberValue(double.Epsilon);
            o["MinValue"] = JsonValue.CreateNumberValue(double.MinValue);
            o["MaxValue"] = JsonValue.CreateNumberValue(double.MaxValue);
            o["NaN"] = JsonValue.CreateNumberValue(double.NaN);
            o["NegativeInfinity"] = JsonValue.CreateNumberValue(double.NegativeInfinity);
            o["PositiveInfinity"] = JsonValue.CreateNumberValue(double.PositiveInfinity);

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);

            Assert.AreEqual(@"{
  ""PositiveInfinity"": ""Infinity"",
  ""NegativeInfinity"": ""-Infinity"",
  ""MinValue"": -1.7976931348623157E+308,
  ""double"": 1.1,
  ""int"": 1,
  ""zero"": 0,
  ""Epsilon"": 4.94065645841247E-324,
  ""MaxValue"": 1.7976931348623157E+308,
  ""NaN"": ""NaN"",
  ""smallfraction"": 3.0000000000000009,
  ""probablyint"": 1
}", json);
        }

        [Test]
        public void DeserializePerformance()
        {
            string json = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}";

            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                JsonObject o = JsonObject.Parse(json);
            }
            timer.Stop();

            string winrt = timer.Elapsed.TotalSeconds.ToString();

            timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                JObject o = JObject.Parse(json);
            }
            timer.Stop();

            string linq = timer.Elapsed.TotalSeconds.ToString();

            // warm up
            JsonConvert.DeserializeObject<Computer>(json);

            timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                Computer o = JsonConvert.DeserializeObject<Computer>(json);
            }
            timer.Stop();

            string jsonnet = timer.Elapsed.TotalSeconds.ToString();

            Debug.WriteLine("winrt: {0}, jsonnet: {1}, jsonnet linq: {2}", winrt, jsonnet, linq);
        }

        [Test]
        public void SerializePerformance()
        {
            string json = @"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}";

            JsonObject o = JsonObject.Parse(json);
            JObject o1 = JObject.Parse(json);
            Computer o2 = JsonConvert.DeserializeObject<Computer>(json);

            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                o.Stringify();
            }
            timer.Stop();

            string winrt = timer.Elapsed.TotalSeconds.ToString();

            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                o1.ToString(Formatting.None);
            }
            timer.Stop();

            string linq = timer.Elapsed.TotalSeconds.ToString();

            timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                JsonConvert.SerializeObject(o);
            }
            timer.Stop();

            string jsonnet = timer.Elapsed.TotalSeconds.ToString();

            Debug.WriteLine("winrt: {0}, jsonnet: {1}, jsonnet linq: {2}", winrt, jsonnet, linq);
        }

        [Test]
        public void ParseJson()
        {
            string json = @"{
        ""channel"": {
          ""title"": ""James Newton-King"",
          ""link"": ""http://james.newtonking.com"",
          ""description"": ""James Newton-King's blog."",
          ""item"": [
            {
              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and the source being available on CodePlex"",
              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
              ""category"": [
                ""Json.NET"",
                ""CodePlex""
              ]
            }
          ]
        }
      }";

            // Windows.Data.Json
            // -----------------
            JsonObject jsonObject = JsonObject.Parse(json);
            string itemTitle1 = jsonObject["channel"].GetObject()["item"].GetArray()[0].GetObject()["title"].GetString();

            // LINQ to JSON
            // ------------
            JObject jObject = JObject.Parse(json);
            string itemTitle2 = (string) jObject["channel"]["item"][0]["title"];
        }

        [Test]
        public void CreateJson()
        {
            // Windows.Data.Json
            // -----------------
            JsonObject jsonObject = new JsonObject
            {
                {"CPU", JsonValue.CreateStringValue("Intel")},
                {
                    "Drives", new JsonArray
                    {
                        JsonValue.CreateStringValue("DVD read/writer"),
                        JsonValue.CreateStringValue("500 gigabyte hard drive")
                    }
                }
            };
            string json1 = jsonObject.Stringify();

            // LINQ to JSON
            // ------------
            JObject jObject = new JObject
            {
                {"CPU", "Intel"},
                {
                    "Drives", new JArray
                    {
                        "DVD read/writer",
                        "500 gigabyte hard drive"
                    }
                }
            };
            string json2 = jObject.ToString();
        }

        [Test]
        public void Converting()
        {
            JsonObject jsonObject = new JsonObject
            {
                {"CPU", JsonValue.CreateStringValue("Intel")},
                {
                    "Drives", new JsonArray
                    {
                        JsonValue.CreateStringValue("DVD read/writer"),
                        JsonValue.CreateStringValue("500 gigabyte hard drive")
                    }
                }
            };

            // convert Windows.Data.Json to LINQ to JSON
            JObject o = JObject.FromObject(jsonObject);

            // convert LINQ to JSON to Windows.Data.Json
            JArray a = (JArray) o["Drives"];
            JsonArray jsonArray = a.ToObject<JsonArray>();
        }
    }
}

#endif