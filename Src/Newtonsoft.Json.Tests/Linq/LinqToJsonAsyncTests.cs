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
using System.Threading.Tasks;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using System.Linq;
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class LinqToJsonAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task CommentsAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(4, a.Count);
            Assert.AreEqual(JTokenType.Comment, a[0].Type);
            Assert.AreEqual(" hi", ((JValue)a[0]).Value);
        }

        [Test]
        public async Task CommentsAndReadFrom_IgnoreCommentsAsync()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(JTokenType.Integer, a[0].Type);
            Assert.AreEqual(1L, ((JValue)a[0]).Value);
        }

        [Test]
        public async Task StartingCommentAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(JTokenType.Comment, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(5, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingCommentAndReadFrom_IgnoreCommentsAsync()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            Assert.AreEqual(JTokenType.Array, a.Type);

            IJsonLineInfo lineInfo = a;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(3, lineInfo.LineNumber);
            Assert.AreEqual(1, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingUndefinedAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)await JToken.ReadFromAsync(jsonReader);

            Assert.AreEqual(JTokenType.Undefined, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(9, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingEndArrayAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"[]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            await jsonReader.ReadAsync();
            await jsonReader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await JToken.ReadFromAsync(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
        }

        [Test]
        public async Task LinqToJsonDeserializeAsync()
        {
            JObject o = new JObject(
                new JProperty("Name", "John Smith"),
                new JProperty("BirthDate", new DateTime(1983, 3, 20))
                );

            JsonSerializer serializer = new JsonSerializer();
            Person p = (Person)await serializer.DeserializeAsync(new JTokenReader(o), typeof(Person));

            Assert.AreEqual("John Smith", p.Name);
        }

        [Test]
        public async Task ToStringJsonConverterAsync()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            await serializer.SerializeAsync(writer, o);

            string json = sw.ToString();

            StringAssert.AreEqual(@"{
  ""Test1"": new Date(
    971586305000
  ),
  ""Test2"": new Date(
    971546045000
  ),
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", json);
        }

        [Test]
        public async Task DateTimeOffsetAsync()
        {
            List<DateTimeOffset> testDates = new List<DateTimeOffset>
            {
                new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
            };

            JsonSerializer jsonSerializer = new JsonSerializer();

            JTokenWriter jsonWriter;
            using (jsonWriter = new JTokenWriter())
            {
                await jsonSerializer.SerializeAsync(jsonWriter, testDates);
            }

            Assert.AreEqual(4, jsonWriter.Token.Children().Count());
        }

        [Test]
        public async Task SerializeWithNoRedundentIdPropertiesTestAsync()
        {
            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            Dictionary<string, object> dic3 = new Dictionary<string, object>();
            List<object> list1 = new List<object>();
            List<object> list2 = new List<object>();

            dic1.Add("list1", list1);
            dic1.Add("list2", list2);
            dic1.Add("dic1", dic1);
            dic1.Add("dic2", dic2);
            dic1.Add("dic3", dic3);
            dic1.Add("integer", 12345);

            list1.Add("A string!");
            list1.Add(dic1);
            list1.Add(new List<object>());

            dic3.Add("dic3", dic3);

            string json = await SerializeWithNoRedundentIdPropertiesAsync(dic1);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""list1"": [
    ""A string!"",
    {
      ""$ref"": ""1""
    },
    []
  ],
  ""list2"": [],
  ""dic1"": {
    ""$ref"": ""1""
  },
  ""dic2"": {},
  ""dic3"": {
    ""$id"": ""3"",
    ""dic3"": {
      ""$ref"": ""3""
    }
  },
  ""integer"": 12345
}", json);
        }

        private static async Task<string> SerializeWithNoRedundentIdPropertiesAsync(object o)
        {
            JTokenWriter writer = new JTokenWriter();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            await serializer.SerializeAsync(writer, o);

            JToken t = writer.Token;

            if (t is JContainer)
            {
                JContainer c = t as JContainer;

                // find all the $id properties in the JSON
                IList<JProperty> ids = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$id").ToList();

                if (ids.Count > 0)
                {
                    // find all the $ref properties in the JSON
                    IList<JProperty> refs = c.Descendants().OfType<JProperty>().Where(d => d.Name == "$ref").ToList();

                    foreach (JProperty idProperty in ids)
                    {
                        // check whether the $id property is used by a $ref
                        bool idUsed = refs.Any(r => idProperty.Value.ToString() == r.Value.ToString());

                        if (!idUsed)
                        {
                            // remove unused $id
                            idProperty.Remove();
                        }
                    }
                }
            }

            return t.ToString();
        }
    }
}

#endif