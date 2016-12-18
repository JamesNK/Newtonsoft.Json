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
using System.Collections.Generic;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
#endif
using System.IO;
using System.Xml;
using System.Diagnostics;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class JsonSerializerAsyncTest : TestFixtureBase
    {
        public class ErroringClass
        {
            public DateTime Tags { get; set; }
        }

        [Test]
        public async Task DontCloseInputOnDeserializeErrorAsync()
        {
            using (var s = System.IO.File.OpenRead("large.json"))
            {
                try
                {
                    using (JsonTextReader reader = new JsonTextReader(new StreamReader(s)))
                    {
                        reader.SupportMultipleContent = true;
                        reader.CloseInput = false;

                        // read into array
                        await reader.ReadAsync();

                        var ser = new JsonSerializer();
                        ser.CheckAdditionalContent = false;

                        await ser.DeserializeAsync<IList<ErroringClass>>(reader);
                    }

                    Assert.Fail();
                }
                catch (Exception)
                {
                    Assert.IsTrue(s.Position > 0);

                    s.Seek(0, SeekOrigin.Begin);

                    Assert.AreEqual(0, s.Position);
                }
            }
        }

        [Test]
        public async Task PopulateResetSettingsAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""2000-01-01T01:01:01+00:00""]"));
            Assert.AreEqual(DateParseHandling.DateTime, reader.DateParseHandling);

            JsonSerializer serializer = new JsonSerializer
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            };

            IList<object> l = new List<object>();
            await serializer.PopulateAsync(reader, l);

            Assert.AreEqual(typeof(DateTimeOffset), l[0].GetType());
            Assert.AreEqual(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero), l[0]);

            Assert.AreEqual(DateParseHandling.DateTime, reader.DateParseHandling);
        }

        [Test]
        public async Task ConversionOperatorAsync()
        {
            // Creating a simple dictionary that has a non-string key
            var dictStore = new Dictionary<DictionaryKeyCast, int>();
            for (var i = 0; i < 800; i++)
            {
                dictStore.Add(new DictionaryKeyCast(i.ToString(CultureInfo.InvariantCulture), i), i);
            }
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var jsonSerializer = JsonSerializer.Create(settings);
            var ms = new MemoryStream();

            var streamWriter = new StreamWriter(ms);
            await jsonSerializer.SerializeAsync(streamWriter, dictStore);
            await streamWriter.FlushAsync();

            ms.Seek(0, SeekOrigin.Begin);

            var stopWatch = Stopwatch.StartNew();
            var deserialize = await jsonSerializer.DeserializeAsync(new StreamReader(ms), typeof(Dictionary<DictionaryKeyCast, int>));
            stopWatch.Stop();
        }

        internal class DictionaryKeyCast
        {
            private string _name;
            private int _number;

            public DictionaryKeyCast(string name, int number)
            {
                _name = name;
                _number = number;
            }

            public override string ToString()
            {
                return _name + " " + _number;
            }

            public static implicit operator DictionaryKeyCast(string dictionaryKey)
            {
                var strings = dictionaryKey.Split(' ');
                return new DictionaryKeyCast(strings[0], Convert.ToInt32(strings[1]));
            }
        }

        [Test]
        public async Task ObjectCreationHandlingReplaceAsync()
        {
            string json = "{bar:[1,2,3], foo:'hello'}";

            JsonSerializer s = new JsonSerializer();
            s.ObjectCreationHandling = ObjectCreationHandling.Replace;

            ClassWithArray wibble = (ClassWithArray)await s.DeserializeAsync(new StringReader(json), typeof(ClassWithArray));

            Assert.AreEqual("hello", wibble.Foo);

            Assert.AreEqual(1, wibble.Bar.Count);
        }

        private class MyClass
        {
            public byte[] Prop1 { get; set; }

            public MyClass()
            {
                Prop1 = new byte[0];
            }
        }

        [Test]
        public async Task DeserializeByteArrayAsync()
        {
            JsonSerializer serializer1 = new JsonSerializer();
            serializer1.Converters.Add(new IsoDateTimeConverter());
            serializer1.NullValueHandling = NullValueHandling.Ignore;

            string json = @"[{""Prop1"":""""},{""Prop1"":""""}]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            MyClass[] z = (MyClass[])await serializer1.DeserializeAsync(reader, typeof(MyClass[]));
            Assert.AreEqual(2, z.Length);
            Assert.AreEqual(0, z[0].Prop1.Length);
            Assert.AreEqual(0, z[1].Prop1.Length);
        }

        public class TimeZoneOffsetObject
        {
            public DateTimeOffset Offset { get; set; }
        }

        [Test]
        public async Task ReadWriteTimeZoneOffsetIsoAsync()
        {
            var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
            {
                Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
            });

            Assert.AreEqual("{\"Offset\":\"2000-01-01T00:00:00+06:00\"}", serializeObject);

            JsonTextReader reader = new JsonTextReader(new StringReader(serializeObject));
            reader.DateParseHandling = DateParseHandling.None;

            JsonSerializer serializer = new JsonSerializer();

            var deserializeObject = await serializer.DeserializeAsync<TimeZoneOffsetObject>(reader);

            Assert.AreEqual(TimeSpan.FromHours(6), deserializeObject.Offset.Offset);
            Assert.AreEqual(new DateTime(2000, 1, 1), deserializeObject.Offset.Date);
        }

        [Test]
        public async Task ReadWriteTimeZoneOffsetMsAjaxAsync()
        {
            var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
            {
                Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
            }, Formatting.None, new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat });

            Assert.AreEqual("{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}", serializeObject);

            JsonTextReader reader = new JsonTextReader(new StringReader(serializeObject));

            JsonSerializer serializer = new JsonSerializer();
            serializer.DateParseHandling = DateParseHandling.None;

            var deserializeObject = await serializer.DeserializeAsync<TimeZoneOffsetObject>(reader);

            Assert.AreEqual(TimeSpan.FromHours(6), deserializeObject.Offset.Offset);
            Assert.AreEqual(new DateTime(2000, 1, 1), deserializeObject.Offset.Date);
        }

        [Test]
        public async Task DeserializeDecimalPropertyExactAsync()
        {
            string json = "{Amount:123456789876543.21}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            JsonSerializer serializer = new JsonSerializer();

            Invoice i = await serializer.DeserializeAsync<Invoice>(reader);
            Assert.AreEqual(123456789876543.21m, i.Amount);
        }

        [Test]
        public async Task DeserializeDecimalDictionaryExactAsync()
        {
            string json = "{'Value':123456789876543.21}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            JsonSerializer serializer = new JsonSerializer();

            IDictionary<string, decimal> d = await serializer.DeserializeAsync<IDictionary<string, decimal>>(reader);
            Assert.AreEqual(123456789876543.21m, d["Value"]);
        }

        [Test]
        public async Task DeserializeByteArrayWithTypeNameHandlingAsync()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            byte[] objectBytes;
            using (MemoryStream stream = new MemoryStream())
            using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
            {
                await serializer.SerializeAsync(jsonWriter, test);
                await jsonWriter.FlushAsync();

                objectBytes = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(objectBytes))
            using (JsonReader jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                // Get exception here
                TestObject newObject = (TestObject)await serializer.DeserializeAsync(jsonReader);

                Assert.AreEqual("Test", newObject.Name);
                CollectionAssert.AreEquivalent(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }

        [Test]
        public async Task DeserializeObjectDictionaryAsync()
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());
            var dict = await serializer.DeserializeAsync<Dictionary<string, string>>(new JsonTextReader(new StringReader("{'k1':'','k2':'v2'}")));

            Assert.AreEqual("", dict["k1"]);
            Assert.AreEqual("v2", dict["k2"]);
        }

        public class MultipleItemsClass
        {
            public string Name { get; set; }
        }

        [Test]
        public async Task MultipleItemsAsync()
        {
            IList<MultipleItemsClass> values = new List<MultipleItemsClass>();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"{ ""name"": ""bar"" }{ ""name"": ""baz"" }"));
            reader.SupportMultipleContent = true;

            while (await reader.ReadAsync())
            {
                JsonSerializer serializer = new JsonSerializer();
                MultipleItemsClass foo = await serializer.DeserializeAsync<MultipleItemsClass>(reader);

                values.Add(foo);
            }

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("bar", values[0].Name);
            Assert.AreEqual("baz", values[1].Name);
        }

        [Test]
        public async Task TokenFromBsonAsync()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            await writer.WriteStartArrayAsync();
            await writer.WriteValueAsync("2000-01-02T03:04:05+06:00");
            await writer.WriteEndArrayAsync();

            byte[] data = ms.ToArray();
            BsonReader reader = new BsonReader(new MemoryStream(data))
            {
                ReadRootValueAsArray = true
            };

            JArray a = (JArray)await JToken.ReadFromAsync(reader);
            JValue v = (JValue)a[0];

            Assert.AreEqual(typeof(string), v.Value.GetType());
            StringAssert.AreEqual(@"[
  ""2000-01-02T03:04:05+06:00""
]", a.ToString());
        }

        [Test]
        public async Task DeserializeEmptyJsonStringAsync()
        {
            string s = (string)await new JsonSerializer().DeserializeAsync(new JsonTextReader(new StringReader("''")));
            Assert.AreEqual("", s);
        }

        [Test]
        public async Task CheckAdditionalContentAsync()
        {
            string json = "{one:1}{}";

            JsonSerializerSettings settings = new JsonSerializerSettings();
            JsonSerializer s = JsonSerializer.Create(settings);
            IDictionary<string, int> o = await s.DeserializeAsync<Dictionary<string, int>>(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(o);
            Assert.AreEqual(1, o["one"]);

            settings.CheckAdditionalContent = true;
            s = JsonSerializer.Create(settings);
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await s.DeserializeAsync<Dictionary<string, int>>(new JsonTextReader(new StringReader(json))); }, "Additional text encountered after finished reading JSON content: {. Path '', line 1, position 7.");
        }

        [Test]
        public async Task AdditionalContentAfterFinishAsync()
        {
            await ExceptionAssert.ThrowsAsync<JsonException>(async () =>
            {
                string json = "[{},1]";

                JsonSerializer serializer = new JsonSerializer();
                serializer.CheckAdditionalContent = true;

                var reader = new JsonTextReader(new StringReader(json));
                await reader.ReadAsync();
                await reader.ReadAsync();

                await serializer.DeserializeAsync(reader, typeof(MyType));
            }, "Additional text found in JSON string after finishing deserializing object.");
        }

        public class MyType
        {
        }

        [Test]
        public async Task JsonSerializerDateFormatStringAsync()
        {
            CultureInfo culture = new CultureInfo("en-NZ");
            culture.DateTimeFormat.AMDesignator = "a.m.";
            culture.DateTimeFormat.PMDesignator = "p.m.";

            IList<object> dates = new List<object>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1))
            };

            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                DateFormatString = "yyyy tt",
                Culture = culture,
                Formatting = Formatting.Indented
            });
            await serializer.SerializeAsync(jsonWriter, dates);

            Assert.IsNull(jsonWriter.DateFormatString);
            Assert.AreEqual(CultureInfo.InvariantCulture, jsonWriter.Culture);
            Assert.AreEqual(Formatting.None, jsonWriter.Formatting);

            string json = sw.ToString();

            StringAssert.AreEqual(@"[
  ""2000 p.m."",
  ""2000 p.m.""
]", json);
        }

        [Test]
        public async Task JsonSerializerStringEscapeHandlingAsync()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
                Formatting = Formatting.Indented
            });
            await serializer.SerializeAsync(jsonWriter, new { html = "<html></html>" });

            Assert.AreEqual(StringEscapeHandling.Default, jsonWriter.StringEscapeHandling);

            string json = sw.ToString();

            StringAssert.AreEqual(@"{
  ""html"": ""\u003chtml\u003e\u003c/html\u003e""
}", json);
        }

        [Test]
        public async Task DeserializeDecimalAsync()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("1234567890.123456"));
            var settings = new JsonSerializerSettings();
            var serialiser = JsonSerializer.Create(settings);
            decimal? d = await serialiser.DeserializeAsync<decimal?>(reader);

            Assert.AreEqual(1234567890.123456m, d);
        }

        [Test]
        public async Task DateFormatStringWithDateTimeAsync()
        {
            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Dec-Friday-22""", json);

            DateTime dt1 = JsonConvert.DeserializeObject<DateTime>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString
            };
            JValue v = (JValue)await JToken.ReadFromAsync(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTime), v.Value.GetType());
            Assert.AreEqual(dt, (DateTime)v.Value);

            reader = new JsonTextReader(new StringReader(@"""abc"""))
            {
                DateFormatString = dateFormatString
            };
            v = (JValue)await JToken.ReadFromAsync(reader);

            Assert.AreEqual(JTokenType.String, v.Type);
            Assert.AreEqual(typeof(string), v.Value.GetType());
            Assert.AreEqual("abc", v.Value);
        }

        [Test]
        public async Task DateFormatStringWithDateTimeAndCultureAsync()
        {
            CultureInfo culture = new CultureInfo("tr-TR");

            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Ara-Cuma-22""", json);

            DateTime dt1 = JsonConvert.DeserializeObject<DateTime>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };
            JValue v = (JValue)await JToken.ReadFromAsync(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTime), v.Value.GetType());
            Assert.AreEqual(dt, (DateTime)v.Value);

            reader = new JsonTextReader(new StringReader(@"""2000-pie-Dec-Friday-22"""))
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };
            v = (JValue)await JToken.ReadFromAsync(reader);

            Assert.AreEqual(JTokenType.String, v.Type);
            Assert.AreEqual(typeof(string), v.Value.GetType());
            Assert.AreEqual("2000-pie-Dec-Friday-22", v.Value);
        }

        [Test]
        public async Task DateFormatStringWithDateTimeOffsetAsync()
        {
            DateTimeOffset dt = new DateTimeOffset(new DateTime(2000, 12, 22));
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Dec-Friday-22""", json);

            DateTimeOffset dt1 = JsonConvert.DeserializeObject<DateTimeOffset>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString,
                DateParseHandling = DateParseHandling.DateTimeOffset
            };
            JValue v = (JValue)await JToken.ReadFromAsync(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTimeOffset), v.Value.GetType());
            Assert.AreEqual(dt, (DateTimeOffset)v.Value);
        }
    }
}

#endif