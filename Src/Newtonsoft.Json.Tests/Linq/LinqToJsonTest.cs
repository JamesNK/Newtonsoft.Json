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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.IO;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class LinqToJsonTest : TestFixtureBase
    {
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
        public class DemoClass
        {
            public decimal maxValue;
        }

        [Test]
        public void ToObjectDecimal()
        {
            var jArray = JArray.Parse("[{ maxValue:10000000000000000000 }]");
            var list = jArray.ToObject<List<DemoClass>>();

            Assert.AreEqual(10000000000000000000m, list[0].maxValue);
        }
#endif

        [Test]
        public void ToObjectFromGuidToString()
        {
            JValue token = new JValue(new Guid("91274484-3b20-48b4-9d18-7d936b2cb88f"));
            string value = token.ToObject<string>();
            Assert.AreEqual("91274484-3b20-48b4-9d18-7d936b2cb88f", value);
        }

        [Test]
        public void ToObjectFromIntegerToString()
        {
            JValue token = new JValue(1234);
            string value = token.ToObject<string>();
            Assert.AreEqual("1234", value);
        }

        [Test]
        public void ToObjectFromStringToInteger()
        {
            JValue token = new JValue("1234");
            int value = token.ToObject<int>();
            Assert.AreEqual(1234, value);
        }

        [Test]
        public void FromObjectGuid()
        {
            var token1 = new JValue(Guid.NewGuid());
            var token2 = JToken.FromObject(token1);
            Assert.IsTrue(JToken.DeepEquals(token1, token2));
            Assert.AreEqual(token1.Type, token2.Type);
        }

        [Test]
        public void FromObjectTimeSpan()
        {
            var token1 = new JValue(TimeSpan.FromDays(1));
            var token2 = JToken.FromObject(token1);
            Assert.IsTrue(JToken.DeepEquals(token1, token2));
            Assert.AreEqual(token1.Type, token2.Type);
        }

        [Test]
        public void FromObjectUri()
        {
            var token1 = new JValue(new Uri("http://www.newtonsoft.com"));
            var token2 = JToken.FromObject(token1);
            Assert.IsTrue(JToken.DeepEquals(token1, token2));
            Assert.AreEqual(token1.Type, token2.Type);
        }

        [Test]
        public void ToObject_Guid()
        {
            JObject anon = new JObject
            {
                ["id"] = Guid.NewGuid()
            };
            Assert.AreEqual(JTokenType.Guid, anon["id"].Type);

            Dictionary<string, JToken> dict = anon.ToObject<Dictionary<string, JToken>>();
            Assert.AreEqual(JTokenType.Guid, dict["id"].Type);
        }

        public class TestClass_ULong
        {
            public ulong Value { get; set; }
        }

        [Test]
        public void FromObject_ULongMaxValue()
        {
            TestClass_ULong instance = new TestClass_ULong { Value = ulong.MaxValue };
            JObject output = JObject.FromObject(instance);

            StringAssert.AreEqual(@"{
  ""Value"": 18446744073709551615
}", output.ToString());
        }

        public class TestClass_Byte
        {
            public byte Value { get; set; }
        }

        [Test]
        public void FromObject_ByteMaxValue()
        {
            TestClass_Byte instance = new TestClass_Byte { Value = byte.MaxValue };
            JObject output = JObject.FromObject(instance);

            StringAssert.AreEqual(@"{
  ""Value"": 255
}", output.ToString());
        }

        [Test]
        public void ToObject_Base64AndGuid()
        {
            JObject o = JObject.Parse("{'responseArray':'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'}");
            byte[] data = o["responseArray"].ToObject<byte[]>();
            byte[] expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

            CollectionAssert.AreEqual(expected, data);

            o = JObject.Parse("{'responseArray':'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'}");
            data = o["responseArray"].ToObject<byte[]>();
            expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

            CollectionAssert.AreEqual(expected, data);
        }

        [Test]
        public void IncompleteContainers()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JArray.Parse("[1,"),
                "Unexpected end of content while loading JArray. Path '[0]', line 1, position 3.");

            ExceptionAssert.Throws<JsonReaderException>(
                () => JArray.Parse("[1"),
                "Unexpected end of content while loading JArray. Path '[0]', line 1, position 2.");

            ExceptionAssert.Throws<JsonReaderException>(
                () => JObject.Parse("{'key':1,"),
                "Unexpected end of content while loading JObject. Path 'key', line 1, position 9.");

            ExceptionAssert.Throws<JsonReaderException>(
                () => JObject.Parse("{'key':1"),
                "Unexpected end of content while loading JObject. Path 'key', line 1, position 8.");
        }

        [Test]
        public void EmptyJEnumerableCount()
        {
            JEnumerable<JToken> tokens = new JEnumerable<JToken>();

            Assert.AreEqual(0, tokens.Count());
        }

        [Test]
        public void EmptyJEnumerableAsEnumerable()
        {
            IEnumerable tokens = new JEnumerable<JToken>();

            Assert.AreEqual(0, tokens.Cast<JToken>().Count());
        }

        [Test]
        public void EmptyJEnumerableEquals()
        {
            JEnumerable<JToken> tokens1 = new JEnumerable<JToken>();
            JEnumerable<JToken> tokens2 = new JEnumerable<JToken>();

            Assert.IsTrue(tokens1.Equals(tokens2));

            object o1 = new JEnumerable<JToken>();
            object o2 = new JEnumerable<JToken>();

            Assert.IsTrue(o1.Equals(o2));
        }

        [Test]
        public void EmptyJEnumerableGetHashCode()
        {
            JEnumerable<JToken> tokens = new JEnumerable<JToken>();

            Assert.AreEqual(0, tokens.GetHashCode());
        }

        [Test]
        public void CommentsAndReadFrom()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)JToken.ReadFrom(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(4, a.Count);
            Assert.AreEqual(JTokenType.Comment, a[0].Type);
            Assert.AreEqual(" hi", ((JValue)a[0]).Value);
        }

        [Test]
        public void CommentsAndReadFrom_IgnoreComments()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)JToken.ReadFrom(jsonReader);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(JTokenType.Integer, a[0].Type);
            Assert.AreEqual(1L, ((JValue)a[0]).Value);
        }

        [Test]
        public void StartingCommentAndReadFrom()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)JToken.ReadFrom(jsonReader, new JsonLoadSettings
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
        public void StartingCommentAndReadFrom_IgnoreComments()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)JToken.ReadFrom(jsonReader, new JsonLoadSettings
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
        public void StartingUndefinedAndReadFrom()
        {
            StringReader textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)JToken.ReadFrom(jsonReader);

            Assert.AreEqual(JTokenType.Undefined, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(9, lineInfo.LinePosition);
        }

        [Test]
        public void StartingEndArrayAndReadFrom()
        {
            StringReader textReader = new StringReader(@"[]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            jsonReader.Read();
            jsonReader.Read();

            ExceptionAssert.Throws<JsonReaderException>(() => JToken.ReadFrom(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
        }

        [Test]
        public void JPropertyPath()
        {
            JObject o = new JObject
            {
                {
                    "person",
                    new JObject
                    {
                        { "$id", 1 }
                    }
                }
            };

            JContainer idProperty = o["person"]["$id"].Parent;
            Assert.AreEqual("person.$id", idProperty.Path);
        }

        [Test]
        public void EscapedPath()
        {
            string json = @"{
  ""frameworks"": {
    ""dnxcore50"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": ""NuGet""
        }
      }
    }
  }
}";

            JObject o = JObject.Parse(json);

            JToken v1 = o["frameworks"]["dnxcore50"]["dependencies"]["System.Xml.ReaderWriter"]["source"];

            Assert.AreEqual("frameworks.dnxcore50.dependencies['System.Xml.ReaderWriter'].source", v1.Path);

            JToken v2 = o.SelectToken(v1.Path);

            Assert.AreEqual(v1, v2);
        }

        [Test]
        public void EscapedPathTests()
        {
            EscapedPathAssert("this has spaces", "['this has spaces']");
            EscapedPathAssert("(RoundBraces)", "['(RoundBraces)']");
            EscapedPathAssert("[SquareBraces]", "['[SquareBraces]']");
            EscapedPathAssert("this.has.dots", "['this.has.dots']");
        }

        private void EscapedPathAssert(string propertyName, string expectedPath)
        {
            int v1 = int.MaxValue;
            JValue value = new JValue(v1);

            JObject o = new JObject(new JProperty(propertyName, value));

            Assert.AreEqual(expectedPath, value.Path);

            JValue selectedValue = (JValue)o.SelectToken(value.Path);

            Assert.AreEqual(value, selectedValue);
        }

        [Test]
        public void ForEach()
        {
            JArray items = new JArray(new JObject(new JProperty("name", "value!")));

            foreach (JObject friend in items)
            {
                StringAssert.AreEqual(@"{
  ""name"": ""value!""
}", friend.ToString());
            }
        }

        [Test]
        public void DoubleValue()
        {
            JArray j = JArray.Parse("[-1E+4,100.0e-2]");

            double value = (double)j[0];
            Assert.AreEqual(-10000d, value);

            value = (double)j[1];
            Assert.AreEqual(1d, value);
        }

        [Test]
        public void Manual()
        {
            JArray array = new JArray();
            JValue text = new JValue("Manual text");
            JValue date = new JValue(new DateTime(2000, 5, 23));

            array.Add(text);
            array.Add(date);

            string json = array.ToString();
            // [
            //   "Manual text",
            //   "\/Date(958996800000+1200)\/"
            // ]
        }

        [Test]
        public void LinqToJsonDeserialize()
        {
            JObject o = new JObject(
                new JProperty("Name", "John Smith"),
                new JProperty("BirthDate", new DateTime(1983, 3, 20))
                );

            JsonSerializer serializer = new JsonSerializer();
            Person p = (Person)serializer.Deserialize(new JTokenReader(o), typeof(Person));

            Assert.AreEqual("John Smith", p.Name);
        }

        [Test]
        public void ObjectParse()
        {
            string json = @"{
        CPU: 'Intel',
        Drives: [
          'DVD read/writer',
          ""500 gigabyte hard drive""
        ]
      }";

            JObject o = JObject.Parse(json);
            IList<JProperty> properties = o.Properties().ToList();

            Assert.AreEqual("CPU", properties[0].Name);
            Assert.AreEqual("Intel", (string)properties[0].Value);
            Assert.AreEqual("Drives", properties[1].Name);

            JArray list = (JArray)properties[1].Value;
            Assert.AreEqual(2, list.Children().Count());
            Assert.AreEqual("DVD read/writer", (string)list.Children().ElementAt(0));
            Assert.AreEqual("500 gigabyte hard drive", (string)list.Children().ElementAt(1));

            List<object> parameterValues =
                (from p in o.Properties()
                    where p.Value is JValue
                    select ((JValue)p.Value).Value).ToList();

            Assert.AreEqual(1, parameterValues.Count);
            Assert.AreEqual("Intel", parameterValues[0]);
        }

        [Test]
        public void CreateLongArray()
        {
            string json = @"[0,1,2,3,4,5,6,7,8,9]";

            JArray a = JArray.Parse(json);
            List<int> list = a.Values<int>().ToList();

            List<int> expected = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            CollectionAssert.AreEqual(expected, list);
        }

        [Test]
        public void GoogleSearchAPI()
        {
            #region GoogleJson
            string json = @"{
    results:
        [
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://www.google.com/"",
                url : ""http://www.google.com/"",
                visibleUrl : ""www.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:zhool8dxBV4J:www.google.com"",
                title : ""Google"",
                titleNoFormatting : ""Google"",
                content : ""Enables users to search the Web, Usenet, and 
images. Features include PageRank,   caching and translation of 
results, and an option to find similar pages.""
            },
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://news.google.com/"",
                url : ""http://news.google.com/"",
                visibleUrl : ""news.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:Va_XShOz_twJ:news.google.com"",
                title : ""Google News"",
                titleNoFormatting : ""Google News"",
                content : ""Aggregated headlines and a search engine of many of the world's news sources.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://groups.google.com/"",
                url : ""http://groups.google.com/"",
                visibleUrl : ""groups.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:x2uPD3hfkn0J:groups.google.com"",
                title : ""Google Groups"",
                titleNoFormatting : ""Google Groups"",
                content : ""Enables users to search and browse the Usenet 
archives which consist of over 700   million messages, and post new 
comments.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://maps.google.com/"",
                url : ""http://maps.google.com/"",
                visibleUrl : ""maps.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:dkf5u2twBXIJ:maps.google.com"",
                title : ""Google Maps"",
                titleNoFormatting : ""Google Maps"",
                content : ""Provides directions, interactive maps, and 
satellite/aerial imagery of the United   States. Can also search by 
keyword such as type of business.""
            }
        ],
        
    adResults:
        [
            {
                GsearchResultClass:""GwebSearch.ad"",
                title : ""Gartner Symposium/ITxpo"",
                content1 : ""Meet brilliant Gartner IT analysts"",
                content2 : ""20-23 May 2007- Barcelona, Spain"",
                url : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                impressionUrl : 
""http://www.google.com/uds/css/ad-indicator-on.gif?ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB"", 

                unescapedUrl : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                visibleUrl : ""www.gartner.com""
            }
        ]
}
";
            #endregion

            JObject o = JObject.Parse(json);

            List<JObject> resultObjects = o["results"].Children<JObject>().ToList();

            Assert.AreEqual(32, resultObjects.Properties().Count());

            Assert.AreEqual(32, resultObjects.Cast<JToken>().Values().Count());

            Assert.AreEqual(4, resultObjects.Cast<JToken>().Values("GsearchResultClass").Count());

            Assert.AreEqual(5, o.PropertyValues().Cast<JArray>().Children().Count());

            List<string> resultUrls = o["results"].Children().Values<string>("url").ToList();

            List<string> expectedUrls = new List<string>() { "http://www.google.com/", "http://news.google.com/", "http://groups.google.com/", "http://maps.google.com/" };

            CollectionAssert.AreEqual(expectedUrls, resultUrls);

            List<JToken> descendants = o.Descendants().ToList();
            Assert.AreEqual(89, descendants.Count);
        }

        [Test]
        public void JTokenToString()
        {
            string json = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

            JObject o = JObject.Parse(json);

            StringAssert.AreEqual(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}", o.ToString());

            JArray list = o.Value<JArray>("Drives");

            StringAssert.AreEqual(@"[
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", list.ToString());

            JProperty cpuProperty = o.Property("CPU");
            Assert.AreEqual(@"""CPU"": ""Intel""", cpuProperty.ToString());

            JProperty drivesProperty = o.Property("Drives");
            StringAssert.AreEqual(@"""Drives"": [
  ""DVD read/writer"",
  ""500 gigabyte hard drive""
]", drivesProperty.ToString());
        }

        [Test]
        public void JTokenToStringTypes()
        {
            string json = @"{""Color"":2,""Establised"":new Date(1264118400000),""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""ExpiryDate"":new Date(949532490000),""Price"":0},{""Name"":""Alien"",""ExpiryDate"":new Date(-62135596800000),""Price"":0}]}";

            JObject o = JObject.Parse(json);

            StringAssert.AreEqual(@"""Establised"": new Date(
  1264118400000
)", o.Property("Establised").ToString());
            StringAssert.AreEqual(@"new Date(
  1264118400000
)", o.Property("Establised").Value.ToString());
            Assert.AreEqual(@"""Width"": 1.1", o.Property("Width").ToString());
            Assert.AreEqual(@"1.1", ((JValue)o.Property("Width").Value).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(@"""Open"": false", o.Property("Open").ToString());
            Assert.AreEqual(@"False", o.Property("Open").Value.ToString());

            json = @"[null,undefined]";

            JArray a = JArray.Parse(json);
            StringAssert.AreEqual(@"[
  null,
  undefined
]", a.ToString());
            Assert.AreEqual(@"", a.Children().ElementAt(0).ToString());
            Assert.AreEqual(@"", a.Children().ElementAt(1).ToString());
        }

        [Test]
        public void CreateJTokenTree()
        {
            JObject o =
                new JObject(
                    new JProperty("Test1", "Test1Value"),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            Assert.AreEqual(4, o.Properties().Count());

            StringAssert.AreEqual(@"{
  ""Test1"": ""Test1Value"",
  ""Test2"": ""Test2Value"",
  ""Test3"": ""Test3Value"",
  ""Test4"": null
}", o.ToString());

            JArray a =
                new JArray(
                    o,
                    new DateTime(2000, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                    55,
                    new JArray(
                        "1",
                        2,
                        3.0,
                        new DateTime(4, 5, 6, 7, 8, 9, DateTimeKind.Utc)
                        ),
                    new JConstructor(
                        "ConstructorName",
                        "param1",
                        2,
                        3.0
                        )
                    );

            Assert.AreEqual(5, a.Count());
            StringAssert.AreEqual(@"[
  {
    ""Test1"": ""Test1Value"",
    ""Test2"": ""Test2Value"",
    ""Test3"": ""Test3Value"",
    ""Test4"": null
  },
  ""2000-10-10T00:00:00Z"",
  55,
  [
    ""1"",
    2,
    3.0,
    ""0004-05-06T07:08:09Z""
  ],
  new ConstructorName(
    ""param1"",
    2,
    3.0
  )
]", a.ToString());
        }

        private class Post
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public IList<string> Categories { get; set; }
        }

        private List<Post> GetPosts()
        {
            return new List<Post>()
            {
                new Post()
                {
                    Title = "LINQ to JSON beta",
                    Description = "Annoucing LINQ to JSON",
                    Link = "http://james.newtonking.com/projects/json-net.aspx",
                    Categories = new List<string>() { "Json.NET", "LINQ" }
                },
                new Post()
                {
                    Title = "Json.NET 1.3 + New license + Now on CodePlex",
                    Description = "Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
                    Link = "http://james.newtonking.com/projects/json-net.aspx",
                    Categories = new List<string>() { "Json.NET", "CodePlex" }
                }
            };
        }

        [Test]
        public void FromObjectExample()
        {
            Post p = new Post
            {
                Title = "How to use FromObject",
                Categories = new[] { "LINQ to JSON" }
            };

            // serialize Post to JSON then parse JSON – SLOW!
            //JObject o = JObject.Parse(JsonConvert.SerializeObject(p));

            // create JObject directly from the Post
            JObject o = JObject.FromObject(p);

            o["Title"] = o["Title"] + " - Super effective!";

            string json = o.ToString();
            // {
            //   "Title": "How to use FromObject - It's super effective!",
            //   "Categories": [
            //     "LINQ to JSON"
            //   ]
            // }

            StringAssert.AreEqual(@"{
  ""Title"": ""How to use FromObject - Super effective!"",
  ""Description"": null,
  ""Link"": null,
  ""Categories"": [
    ""LINQ to JSON""
  ]
}", json);
        }

        [Test]
        public void QueryingExample()
        {
            JArray posts = JArray.Parse(@"[
              {
                'Title': 'JSON Serializer Basics',
                'Date': '2013-12-21T00:00:00',
                'Categories': []
              },
              {
                'Title': 'Querying LINQ to JSON',
                'Date': '2014-06-03T00:00:00',
                'Categories': [
                  'LINQ to JSON'
                ]
              }
            ]");

            JToken serializerBasics = posts
                .Single(p => (string)p["Title"] == "JSON Serializer Basics");
            // JSON Serializer Basics

            IList<JToken> since2012 = posts
                .Where(p => (DateTime)p["Date"] > new DateTime(2012, 1, 1)).ToList();
            // JSON Serializer Basics
            // Querying LINQ to JSON

            IList<JToken> linqToJson = posts
                .Where(p => p["Categories"].Any(c => (string)c == "LINQ to JSON")).ToList();
            // Querying LINQ to JSON

            Assert.IsNotNull(serializerBasics);
            Assert.AreEqual(2, since2012.Count);
            Assert.AreEqual(1, linqToJson.Count);
        }

        [Test]
        public void CreateJTokenTreeNested()
        {
            List<Post> posts = GetPosts();

            JObject rss =
                new JObject(
                    new JProperty("channel",
                        new JObject(
                            new JProperty("title", "James Newton-King"),
                            new JProperty("link", "http://james.newtonking.com"),
                            new JProperty("description", "James Newton-King's blog."),
                            new JProperty("item",
                                new JArray(
                                    from p in posts
                                    orderby p.Title
                                    select new JObject(
                                        new JProperty("title", p.Title),
                                        new JProperty("description", p.Description),
                                        new JProperty("link", p.Link),
                                        new JProperty("category",
                                            new JArray(
                                                from c in p.Categories
                                                select new JValue(c)))))))));

            StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Annoucing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", rss.ToString());

            var postTitles =
                from p in rss["channel"]["item"]
                select p.Value<string>("title");

            Assert.AreEqual("Json.NET 1.3 + New license + Now on CodePlex", postTitles.ElementAt(0));
            Assert.AreEqual("LINQ to JSON beta", postTitles.ElementAt(1));

            var categories =
                from c in rss["channel"]["item"].Children()["category"].Values<string>()
                group c by c
                into g
                orderby g.Count() descending
                select new { Category = g.Key, Count = g.Count() };

            Assert.AreEqual("Json.NET", categories.ElementAt(0).Category);
            Assert.AreEqual(2, categories.ElementAt(0).Count);
            Assert.AreEqual("CodePlex", categories.ElementAt(1).Category);
            Assert.AreEqual(1, categories.ElementAt(1).Count);
            Assert.AreEqual("LINQ", categories.ElementAt(2).Category);
            Assert.AreEqual(1, categories.ElementAt(2).Count);
        }

        [Test]
        public void BasicQuerying()
        {
            string json = @"{
                        ""channel"": {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      }";

            JObject o = JObject.Parse(json);

            Assert.AreEqual(null, o["purple"]);
            Assert.AreEqual(null, o.Value<string>("purple"));

            CustomAssert.IsInstanceOfType(typeof(JArray), o["channel"]["item"]);

            Assert.AreEqual(2, o["channel"]["item"].Children()["title"].Count());
            Assert.AreEqual(0, o["channel"]["item"].Children()["monkey"].Count());

            Assert.AreEqual("Json.NET 1.3 + New license + Now on CodePlex", (string)o["channel"]["item"][0]["title"]);

            CollectionAssert.AreEqual(new string[] { "Json.NET 1.3 + New license + Now on CodePlex", "LINQ to JSON beta" }, o["channel"]["item"].Children().Values<string>("title").ToArray());
        }

        [Test]
        public void JObjectIntIndex()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                JObject o = new JObject();
                Assert.AreEqual(null, o[0]);
            }, "Accessed JObject values with invalid key value: 0. Object property name expected.");
        }

        [Test]
        public void JArrayStringIndex()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                JArray a = new JArray();
                Assert.AreEqual(null, a["purple"]);
            }, @"Accessed JArray values with invalid key value: ""purple"". Int32 array index expected.");
        }

        [Test]
        public void JConstructorStringIndex()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                JConstructor c = new JConstructor("ConstructorValue");
                Assert.AreEqual(null, c["purple"]);
            }, @"Accessed JConstructor values with invalid key value: ""purple"". Argument position index expected.");
        }

#if !NET20
        [Test]
        public void ToStringJsonConverter()
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
            serializer.Serialize(writer, o);

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
        public void DateTimeOffset()
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
                jsonSerializer.Serialize(jsonWriter, testDates);
            }

            Assert.AreEqual(4, jsonWriter.Token.Children().Count());
        }
#endif

        [Test]
        public void FromObject()
        {
            List<Post> posts = GetPosts();

            JObject o = JObject.FromObject(new
            {
                channel = new
                {
                    title = "James Newton-King",
                    link = "http://james.newtonking.com",
                    description = "James Newton-King's blog.",
                    item =
                        from p in posts
                        orderby p.Title
                        select new
                        {
                            title = p.Title,
                            description = p.Description,
                            link = p.Link,
                            category = p.Categories
                        }
                }
            });

            StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Annoucing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", o.ToString());

            CustomAssert.IsInstanceOfType(typeof(JObject), o);
            CustomAssert.IsInstanceOfType(typeof(JObject), o["channel"]);
            Assert.AreEqual("James Newton-King", (string)o["channel"]["title"]);
            Assert.AreEqual(2, o["channel"]["item"].Children().Count());

            JArray a = JArray.FromObject(new List<int>() { 0, 1, 2, 3, 4 });
            CustomAssert.IsInstanceOfType(typeof(JArray), a);
            Assert.AreEqual(5, a.Count());
        }

        [Test]
        public void FromAnonDictionary()
        {
            List<Post> posts = GetPosts();

            JObject o = JObject.FromObject(new
            {
                channel = new Dictionary<string, object>
                {
                    { "title", "James Newton-King" },
                    { "link", "http://james.newtonking.com" },
                    { "description", "James Newton-King's blog." },
                    {
                        "item",
                        (from p in posts
                            orderby p.Title
                            select new
                            {
                                title = p.Title,
                                description = p.Description,
                                link = p.Link,
                                category = p.Categories
                            })
                    }
                }
            });

            StringAssert.AreEqual(@"{
  ""channel"": {
    ""title"": ""James Newton-King"",
    ""link"": ""http://james.newtonking.com"",
    ""description"": ""James Newton-King's blog."",
    ""item"": [
      {
        ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
        ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""CodePlex""
        ]
      },
      {
        ""title"": ""LINQ to JSON beta"",
        ""description"": ""Annoucing LINQ to JSON"",
        ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
        ""category"": [
          ""Json.NET"",
          ""LINQ""
        ]
      }
    ]
  }
}", o.ToString());

            CustomAssert.IsInstanceOfType(typeof(JObject), o);
            CustomAssert.IsInstanceOfType(typeof(JObject), o["channel"]);
            Assert.AreEqual("James Newton-King", (string)o["channel"]["title"]);
            Assert.AreEqual(2, o["channel"]["item"].Children().Count());

            JArray a = JArray.FromObject(new List<int>() { 0, 1, 2, 3, 4 });
            CustomAssert.IsInstanceOfType(typeof(JArray), a);
            Assert.AreEqual(5, a.Count());
        }

        [Test]
        public void AsJEnumerable()
        {
            JObject o = null;
            IJEnumerable<JToken> enumerable = null;

            enumerable = o.AsJEnumerable();
            Assert.IsNull(enumerable);

            o =
                new JObject(
                    new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", null)
                    );

            enumerable = o.AsJEnumerable();
            Assert.IsNotNull(enumerable);
            Assert.AreEqual(o, enumerable);

            DateTime d = enumerable["Test1"].Value<DateTime>();

            Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), d);
        }

#if !(NET20 || NET35 || PORTABLE40)
        [Test]
        public void CovariantIJEnumerable()
        {
            IEnumerable<JObject> o = new[]
            {
                JObject.FromObject(new { First = 1, Second = 2 }),
                JObject.FromObject(new { First = 1, Second = 2 })
            };

            IJEnumerable<JToken> values = o.Properties();
            Assert.AreEqual(4, values.Count());
        }
#endif

#if !NET20
        [Test]
        public void LinqCast()
        {
            JToken olist = JArray.Parse("[12,55]");

            List<int> list1 = olist.AsEnumerable().Values<int>().ToList();

            Assert.AreEqual(12, list1[0]);
            Assert.AreEqual(55, list1[1]);
        }
#endif

        [Test]
        public void ChildrenExtension()
        {
            string json = @"[
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        },
                        {
                          ""title"": ""James Newton-King"",
                          ""link"": ""http://james.newtonking.com"",
                          ""description"": ""James Newton-King's blog."",
                          ""item"": [
                            {
                              ""title"": ""Json.NET 1.3 + New license + Now on CodePlex"",
                              ""description"": ""Annoucing the release of Json.NET 1.3, the MIT license and being available on CodePlex"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""CodePlex""
                              ]
                            },
                            {
                              ""title"": ""LINQ to JSON beta"",
                              ""description"": ""Annoucing LINQ to JSON"",
                              ""link"": ""http://james.newtonking.com/projects/json-net.aspx"",
                              ""category"": [
                                ""Json.NET"",
                                ""LINQ""
                              ]
                            }
                          ]
                        }
                      ]";

            JArray o = JArray.Parse(json);

            Assert.AreEqual(4, o.Children()["item"].Children()["title"].Count());
            CollectionAssert.AreEqual(new string[]
            {
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta",
                "Json.NET 1.3 + New license + Now on CodePlex",
                "LINQ to JSON beta"
            },
                o.Children()["item"].Children()["title"].Values<string>().ToArray());
        }

        [Test]
        public void UriGuidTimeSpanTestClassEmptyTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass();
            JObject o = JObject.FromObject(c1);

            StringAssert.AreEqual(@"{
  ""Guid"": ""00000000-0000-0000-0000-000000000000"",
  ""NullableGuid"": null,
  ""TimeSpan"": ""00:00:00"",
  ""NullableTimeSpan"": null,
  ""Uri"": null
}", o.ToString());

            UriGuidTimeSpanTestClass c2 = o.ToObject<UriGuidTimeSpanTestClass>();
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public void UriGuidTimeSpanTestClassValuesTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass
            {
                Guid = new Guid("1924129C-F7E0-40F3-9607-9939C531395A"),
                NullableGuid = new Guid("9E9F3ADF-E017-4F72-91E0-617EBE85967D"),
                TimeSpan = TimeSpan.FromDays(1),
                NullableTimeSpan = TimeSpan.FromHours(1),
                Uri = new Uri("http://testuri.com")
            };
            JObject o = JObject.FromObject(c1);

            StringAssert.AreEqual(@"{
  ""Guid"": ""1924129c-f7e0-40f3-9607-9939c531395a"",
  ""NullableGuid"": ""9e9f3adf-e017-4f72-91e0-617ebe85967d"",
  ""TimeSpan"": ""1.00:00:00"",
  ""NullableTimeSpan"": ""01:00:00"",
  ""Uri"": ""http://testuri.com""
}", o.ToString());

            UriGuidTimeSpanTestClass c2 = o.ToObject<UriGuidTimeSpanTestClass>();
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);

            string j = JsonConvert.SerializeObject(c1, Formatting.Indented);

            StringAssert.AreEqual(j, o.ToString());
        }

        [Test]
        public void ParseWithPrecendingComments()
        {
            string json = @"/* blah */ {'hi':'hi!'}";
            JObject o = JObject.Parse(json);
            Assert.AreEqual("hi!", (string)o["hi"]);

            json = @"/* blah */ ['hi!']";
            JArray a = JArray.Parse(json);
            Assert.AreEqual("hi!", (string)a[0]);
        }

#if !(NET35 || NET20)
        [Test]
        public void ExceptionFromOverloadWithJValue()
        {
            dynamic name = new JValue("Matthew Doig");

            IDictionary<string, string> users = new Dictionary<string, string>();

            // unfortunatly there doesn't appear to be a way around this
            ExceptionAssert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() =>
            {
                users.Add("name2", name);

                Assert.AreEqual(users["name2"], "Matthew Doig");
            }, "The best overloaded method match for 'System.Collections.Generic.IDictionary<string,string>.Add(string, string)' has some invalid arguments");
        }
#endif

#if !(NET20)
        [JsonConverter(typeof(StringEnumConverter))]
        public enum FooBar
        {
            [EnumMember(Value = "SOME_VALUE")]
            SomeValue,

            [EnumMember(Value = "SOME_OTHER_VALUE")]
            SomeOtherValue
        }

        public class MyObject
        {
            public FooBar FooBar { get; set; }
        }

        [Test]
        public void ToObject_Enum_Converter()
        {
            JObject o = JObject.Parse("{'FooBar':'SOME_OTHER_VALUE'}");

            FooBar e = o["FooBar"].ToObject<FooBar>();
            Assert.AreEqual(FooBar.SomeOtherValue, e);
        }

        public enum FooBarNoEnum
        {
            [EnumMember(Value = "SOME_VALUE")]
            SomeValue,

            [EnumMember(Value = "SOME_OTHER_VALUE")]
            SomeOtherValue
        }

        public class MyObjectNoEnum
        {
            public FooBarNoEnum FooBarNoEnum { get; set; }
        }

        [Test]
        public void ToObject_Enum_NoConverter()
        {
            JObject o = JObject.Parse("{'FooBarNoEnum':'SOME_OTHER_VALUE'}");

            FooBarNoEnum e = o["FooBarNoEnum"].ToObject<FooBarNoEnum>();
            Assert.AreEqual(FooBarNoEnum.SomeOtherValue, e);
        }
#endif

        [Test]
        public void SerializeWithNoRedundentIdPropertiesTest()
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

            var json = SerializeWithNoRedundentIdProperties(dic1);

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

        private static string SerializeWithNoRedundentIdProperties(object o)
        {
            JTokenWriter writer = new JTokenWriter();
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            serializer.Serialize(writer, o);

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

            string json = t.ToString();
            return json;
        }

        [Test]
        public void HashCodeTests()
        {
            JObject o1 = new JObject
            {
                ["prop"] = 1
            };
            JObject o2 = new JObject
            {
                ["prop"] = 1
            };

            Assert.IsFalse(ReferenceEquals(o1, o2));
            Assert.IsFalse(Equals(o1, o2));
            Assert.IsFalse(o1.GetHashCode() == o2.GetHashCode());
            Assert.IsTrue(o1.GetDeepHashCode() == o2.GetDeepHashCode());
            Assert.IsTrue(JToken.DeepEquals(o1, o2));

            JArray a1 = new JArray
            {
                1
            };
            JArray a2 = new JArray
            {
                1
            };

            Assert.IsFalse(ReferenceEquals(a1, a2));
            Assert.IsFalse(Equals(a1, a2));
            Assert.IsFalse(a1.GetHashCode() == a2.GetHashCode());
            Assert.IsTrue(a1.GetDeepHashCode() == a2.GetDeepHashCode());
            Assert.IsTrue(JToken.DeepEquals(a1, a2));
        }
    }
}