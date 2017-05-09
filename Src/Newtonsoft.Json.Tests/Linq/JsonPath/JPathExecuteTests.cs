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
#if !(PORTABLE || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3
using System.Numerics;
#endif
using Newtonsoft.Json.Linq.JsonPath;
using Newtonsoft.Json.Tests.Bson;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Linq.JsonPath
{
    [TestFixture]
    public class JPathExecuteTests : TestFixtureBase
    {
        [Test]
        public void RecursiveWildcard()
        {
            string json = @"{
    ""a"": [
        {
            ""id"": 1
        }
    ],
    ""b"": [
        {
            ""id"": 2
        },
        {
            ""id"": 3,
            ""c"": {
                ""id"": 4
            }
        }
    ],
    ""d"": [
        {
            ""id"": 5
        }
    ]
}";

            JObject models = JObject.Parse(json);

            var results = models.SelectTokens("$.b..*.id").ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(2, (int)results[0]);
            Assert.AreEqual(3, (int)results[1]);
            Assert.AreEqual(4, (int)results[2]);
        }

        [Test]
        public void ScanFilter()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JObject.Parse(json);

            var results = models.SelectTokens("$.elements..[?(@.id=='AAA')]").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(models["elements"][0]["children"][0]["children"][0], results[0]);
        }

        [Test]
        public void FilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JObject.Parse(json);

            var results = models.SelectTokens("$.elements[?(true)]").ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0], models["elements"][0]);
            Assert.AreEqual(results[1], models["elements"][1]);
        }

        [Test]
        public void ScanFilterTrue()
        {
            string json = @"{
  ""elements"": [
    {
      ""id"": ""A"",
      ""children"": [
        {
          ""id"": ""AA"",
          ""children"": [
            {
              ""id"": ""AAA""
            },
            {
              ""id"": ""AAB""
            }
          ]
        },
        {
          ""id"": ""AB""
        }
      ]
    },
    {
      ""id"": ""B"",
      ""children"": []
    }
  ]
}";

            JObject models = JObject.Parse(json);

            var results = models.SelectTokens("$.elements..[?(true)]").ToList();

            Assert.AreEqual(25, results.Count);
        }

        [Test]
        public void ScanQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val1"",
            ""Prop2"": ""Val2""
        }
    }
}";

            JObject models = JObject.Parse(json);

            int result = models.SelectTokens("$..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);

            result = models.SelectTokens("..['My.Child.Node']").Count();
            Assert.AreEqual(1, result);
        }

        [Test]
        public void ScanMultipleQuoted()
        {
            string json = @"{
    ""Node1"": {
        ""Child1"": {
            ""Name"": ""IsMe"",
            ""TargetNode"": {
                ""Prop1"": ""Val1"",
                ""Prop2"": ""Val2""
            }
        },
        ""My.Child.Node"": {
            ""TargetNode"": {
                ""Prop1"": ""Val3"",
                ""Prop2"": ""Val4""
            }
        }
    },
    ""Node2"": {
        ""TargetNode"": {
            ""Prop1"": ""Val5"",
            ""Prop2"": ""Val6""
        }
    }
}";

            JObject models = JObject.Parse(json);

            var results = models.SelectTokens("$..['My.Child.Node','Prop1','Prop2']").ToList();
            Assert.AreEqual("Val1", (string)results[0]);
            Assert.AreEqual("Val2", (string)results[1]);
            Assert.AreEqual(JTokenType.Object, results[2].Type);
            Assert.AreEqual("Val3", (string)results[3]);
            Assert.AreEqual("Val4", (string)results[4]);
            Assert.AreEqual("Val5", (string)results[5]);
            Assert.AreEqual("Val6", (string)results[6]);
        }

        [Test]
        public void ParseWithEmptyArrayContent()
        {
            var json = @"{
    'controls': [
        {
            'messages': {
                'addSuggestion': {
                    'en-US': 'Add'
                }
            }
        },
        {
            'header': {
                'controls': []
            },
            'controls': [
                {
                    'controls': [
                        {
                            'defaultCaption': {
                                'en-US': 'Sort by'
                            },
                            'sortOptions': [
                                {
                                    'label': {
                                        'en-US': 'Name'
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
}";
            JObject jToken = JObject.Parse(json);
            IList<JToken> tokens = jToken.SelectTokens("$..en-US").ToList();

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual("Add", (string)tokens[0]);
            Assert.AreEqual("Sort by", (string)tokens[1]);
            Assert.AreEqual("Name", (string)tokens[2]);
        }

        [Test]
        public void SelectTokenAfterEmptyContainer()
        {
            string json = @"{
    'cont': [],
    'test': 'no one will find me'
}";

            JObject o = JObject.Parse(json);

            IList<JToken> results = o.SelectTokens("$..test").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("no one will find me", (string)results[0]);
        }

        [Test]
        public void EvaluatePropertyWithRequired()
        {
            string json = "{\"bookId\":\"1000\"}";
            JObject o = JObject.Parse(json);

            string bookId = (string)o.SelectToken("bookId", true);

            Assert.AreEqual("1000", bookId);
        }

        [Test]
        public void EvaluateEmptyPropertyIndexer()
        {
            JObject o = new JObject(
                new JProperty("", 1));

            JToken t = o.SelectToken("['']");
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateEmptyString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("");
            Assert.AreEqual(o, t);

            t = o.SelectToken("['']");
            Assert.AreEqual(null, t);
        }

        [Test]
        public void EvaluateEmptyStringWithMatchingEmptyProperty()
        {
            JObject o = new JObject(
                new JProperty(" ", 1));

            JToken t = o.SelectToken("[' ']");
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateWhitespaceString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken(" ");
            Assert.AreEqual(o, t);
        }

        [Test]
        public void EvaluateDollarString()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("$");
            Assert.AreEqual(o, t);
        }

        [Test]
        public void EvaluateDollarTypeString()
        {
            JObject o = new JObject(
                new JProperty("$values", new JArray(1, 2, 3)));

            JToken t = o.SelectToken("$values[1]");
            Assert.AreEqual(2, (int)t);
        }

        [Test]
        public void EvaluateSingleProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateWildcardProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1),
                new JProperty("Blah2", 2));

            IList<JToken> t = o.SelectTokens("$.*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [Test]
        public void QuoteName()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("['Blah']");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(1, (int)t);
        }

        [Test]
        public void EvaluateMissingProperty()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("Missing[1]");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateIndexerOnObject()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JToken t = o.SelectToken("[1]");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateIndexerOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[1]", true); }, @"Index 1 not valid on JObject.");
        }

        [Test]
        public void EvaluateWildcardIndexOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[*]", true); }, @"Index * not valid on JObject.");
        }

        [Test]
        public void EvaluateSliceOnObjectWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("[:]", true); }, @"Array slice is not valid on JObject.");
        }

        [Test]
        public void EvaluatePropertyOnArray()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("BlahBlah");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateMultipleResultsError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[0, 1]"); }, @"Path returned multiple tokens.");
        }

        [Test]
        public void EvaluatePropertyOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("BlahBlah", true); }, @"Property 'BlahBlah' not valid on JArray.");
        }

        [Test]
        public void EvaluateNoResultsWithMultipleArrayIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[9,10]", true); }, @"Index 9 outside the bounds of JArray.");
        }

        [Test]
        public void EvaluateConstructorOutOfBoundsIndxerWithError()
        {
            JConstructor c = new JConstructor("Blah");

            ExceptionAssert.Throws<JsonException>(() => { c.SelectToken("[1]", true); }, @"Index 1 outside the bounds of JConstructor.");
        }

        [Test]
        public void EvaluateConstructorOutOfBoundsIndxer()
        {
            JConstructor c = new JConstructor("Blah");

            Assert.IsNull(c.SelectToken("[1]"));
        }

        [Test]
        public void EvaluateMissingPropertyWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("Missing", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [Test]
        public void EvaluatePropertyWithoutError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            JValue v = (JValue)o.SelectToken("Blah", true);
            Assert.AreEqual(1, v.Value);
        }

        [Test]
        public void EvaluateMissingPropertyIndexWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(() => { o.SelectToken("['Missing','Missing2']", true); }, "Property 'Missing' does not exist on JObject.");
        }

        [Test]
        public void EvaluateMultiPropertyIndexOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("['Missing','Missing2']", true); }, "Properties 'Missing', 'Missing2' not valid on JArray.");
        }

        [Test]
        public void EvaluateArraySliceWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[99:]", true); }, "Array slice of 99 to * returned no results.");

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[1:-19]", true); }, "Array slice of 1 to -19 returned no results.");

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[:-19]", true); }, "Array slice of * to -19 returned no results.");

            a = new JArray();

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[:]", true); }, "Array slice of * to * returned no results.");
        }

        [Test]
        public void EvaluateOutOfBoundsIndxer()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("[1000].Ha");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluateArrayOutOfBoundsIndxerWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(() => { a.SelectToken("[1000].Ha", true); }, "Index 1000 outside the bounds of JArray.");
        }

        [Test]
        public void EvaluateArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            JToken t = a.SelectToken("[1]");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(2, (int)t);
        }

        [Test]
        public void EvaluateArraySlice()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);
            IList<JToken> t = null;

            t = a.SelectTokens("[-3:]").ToList();
            Assert.AreEqual(3, t.Count);
            Assert.AreEqual(7, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(9, (int)t[2]);

            t = a.SelectTokens("[-1:-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(9, (int)t[0]);

            t = a.SelectTokens("[-2:-1]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(8, (int)t[0]);

            t = a.SelectTokens("[1:1]").ToList();
            Assert.AreEqual(0, t.Count);

            t = a.SelectTokens("[1:2]").ToList();
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(2, (int)t[0]);

            t = a.SelectTokens("[::-1]").ToList();
            Assert.AreEqual(9, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(8, (int)t[1]);
            Assert.AreEqual(7, (int)t[2]);
            Assert.AreEqual(6, (int)t[3]);
            Assert.AreEqual(5, (int)t[4]);
            Assert.AreEqual(4, (int)t[5]);
            Assert.AreEqual(3, (int)t[6]);
            Assert.AreEqual(2, (int)t[7]);
            Assert.AreEqual(1, (int)t[8]);

            t = a.SelectTokens("[::-2]").ToList();
            Assert.AreEqual(5, t.Count);
            Assert.AreEqual(9, (int)t[0]);
            Assert.AreEqual(7, (int)t[1]);
            Assert.AreEqual(5, (int)t[2]);
            Assert.AreEqual(3, (int)t[3]);
            Assert.AreEqual(1, (int)t[4]);
        }

        [Test]
        public void EvaluateWildcardArray()
        {
            JArray a = new JArray(1, 2, 3, 4);

            List<JToken> t = a.SelectTokens("[*]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.AreEqual(3, (int)t[2]);
            Assert.AreEqual(4, (int)t[3]);
        }

        [Test]
        public void EvaluateArrayMultipleIndexes()
        {
            JArray a = new JArray(1, 2, 3, 4);

            IEnumerable<JToken> t = a.SelectTokens("[1,2,0]");
            Assert.IsNotNull(t);
            Assert.AreEqual(3, t.Count());
            Assert.AreEqual(2, (int)t.ElementAt(0));
            Assert.AreEqual(3, (int)t.ElementAt(1));
            Assert.AreEqual(1, (int)t.ElementAt(2));
        }

        [Test]
        public void EvaluateScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
        }

        [Test]
        public void EvaluateWildcardScan()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JArray a = new JArray(o1, o2);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(5, t.Count);
            Assert.IsTrue(JToken.DeepEquals(a, t[0]));
            Assert.IsTrue(JToken.DeepEquals(o1, t[1]));
            Assert.AreEqual(1, (int)t[2]);
            Assert.IsTrue(JToken.DeepEquals(o2, t[3]));
            Assert.AreEqual(2, (int)t[4]);
        }

        [Test]
        public void EvaluateScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..Name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.AreEqual(1, (int)t[0]);
            Assert.AreEqual(2, (int)t[1]);
            Assert.IsTrue(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[2]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(3), t[3]));
        }

        [Test]
        public void EvaluateWildcardScanNestResults()
        {
            JObject o1 = new JObject { { "Name", 1 } };
            JObject o2 = new JObject { { "Name", 2 } };
            JObject o3 = new JObject { { "Name", new JObject { { "Name", new JArray(3) } } } };
            JArray a = new JArray(o1, o2, o3);

            IList<JToken> t = a.SelectTokens("$..*").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(9, t.Count);

            Assert.IsTrue(JToken.DeepEquals(a, t[0]));
            Assert.IsTrue(JToken.DeepEquals(o1, t[1]));
            Assert.AreEqual(1, (int)t[2]);
            Assert.IsTrue(JToken.DeepEquals(o2, t[3]));
            Assert.AreEqual(2, (int)t[4]);
            Assert.IsTrue(JToken.DeepEquals(o3, t[5]));
            Assert.IsTrue(JToken.DeepEquals(new JObject { { "Name", new JArray(3) } }, t[6]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(3), t[7]));
            Assert.AreEqual(3, (int)t[8]);
        }

        [Test]
        public void EvaluateSinglePropertyReturningArray()
        {
            JObject o = new JObject(
                new JProperty("Blah", new[] { 1, 2, 3 }));

            JToken t = o.SelectToken("Blah");
            Assert.IsNotNull(t);
            Assert.AreEqual(JTokenType.Array, t.Type);

            t = o.SelectToken("Blah[2]");
            Assert.AreEqual(JTokenType.Integer, t.Type);
            Assert.AreEqual(3, (int)t);
        }

        [Test]
        public void EvaluateLastSingleCharacterProperty()
        {
            JObject o2 = JObject.Parse("{'People':[{'N':'Jeff'}]}");
            string a2 = (string)o2.SelectToken("People[0].N");

            Assert.AreEqual("Jeff", a2);
        }

        [Test]
        public void ExistsQuery()
        {
            JArray a = new JArray(new JObject(new JProperty("hi", "ho")), new JObject(new JProperty("hi2", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", "ho")), t[0]));
        }

        [Test]
        public void EqualsQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", "ho")),
                new JObject(new JProperty("hi", "ha")));

            IList<JToken> t = a.SelectTokens("[ ?( @.['hi'] == 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", "ha")), t[0]));
        }

        [Test]
        public void NotEqualsQuery()
        {
            JArray a = new JArray(
                new JArray(new JObject(new JProperty("hi", "ho"))),
                new JArray(new JObject(new JProperty("hi", "ha"))));

            IList<JToken> t = a.SelectTokens("[ ?( @..hi <> 'ha' ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(1, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JArray(new JObject(new JProperty("hi", "ho"))), t[0]));
        }

        [Test]
        public void NoPathQuery()
        {
            JArray a = new JArray(1, 2, 3);

            IList<JToken> t = a.SelectTokens("[ ?( @ > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(2, (int)t[0]);
            Assert.AreEqual(3, (int)t[1]);
        }

        [Test]
        public void MultipleQueries()
        {
            JArray a = new JArray(1, 2, 3, 4, 5, 6, 7, 8, 9);

            // json path does item based evaluation - http://www.sitepen.com/blog/2008/03/17/jsonpath-support/
            // first query resolves array to ints
            // int has no children to query
            IList<JToken> t = a.SelectTokens("[?(@ <> 1)][?(@ <> 4)][?(@ < 7)]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(0, t.Count);
        }

        [Test]
        public void GreaterQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }

        [Test]
        public void LesserQuery_ValueFirst()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( 1 < @.hi ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }

#if !(PORTABLE || DNXCORE50 || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3
        [Test]
        public void GreaterQueryBigInteger()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", new BigInteger(1))),
                new JObject(new JProperty("hi", new BigInteger(2))),
                new JObject(new JProperty("hi", new BigInteger(3))));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi > 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[1]));
        }
#endif

        [Test]
        public void GreaterOrEqualQuery()
        {
            JArray a = new JArray(
                new JObject(new JProperty("hi", 1)),
                new JObject(new JProperty("hi", 2)),
                new JObject(new JProperty("hi", 2.0)),
                new JObject(new JProperty("hi", 3)));

            IList<JToken> t = a.SelectTokens("[ ?( @.hi >= 1 ) ]").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(4, t.Count);
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 1)), t[0]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2)), t[1]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 2.0)), t[2]));
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("hi", 3)), t[3]));
        }

        [Test]
        public void NestedQuery()
        {
            JArray a = new JArray(
                new JObject(
                    new JProperty("name", "Bad Boys"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "Independence Day"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Will Smith"))))),
                new JObject(
                    new JProperty("name", "The Rock"),
                    new JProperty("cast", new JArray(
                        new JObject(new JProperty("name", "Nick Cage")))))
                );

            IList<JToken> t = a.SelectTokens("[?(@.cast[?(@.name=='Will Smith')])].name").ToList();
            Assert.IsNotNull(t);
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual("Bad Boys", (string)t[0]);
            Assert.AreEqual("Independence Day", (string)t[1]);
        }

        [Test]
        public void PathWithConstructor()
        {
            JArray a = JArray.Parse(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]");

            JValue v = (JValue)a.SelectToken("[1].Property2[1][0]");
            Assert.AreEqual(1L, v.Value);
        }

        [Test]
        public void MultiplePaths()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(@.price > @.max_price)]").ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(a[2], results[0]);
        }

        [Test]
        public void Exists_True()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [Test]
        public void Exists_Null()
        {
            JArray a = JArray.Parse(@"[
  {
    ""price"": 199,
    ""max_price"": 200
  },
  {
    ""price"": 200,
    ""max_price"": 200
  },
  {
    ""price"": 201,
    ""max_price"": 200
  }
]");

            var results = a.SelectTokens("[?(true)]").ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(a[0], results[0]);
            Assert.AreEqual(a[1], results[1]);
            Assert.AreEqual(a[2], results[2]);
        }

        [Test]
        public void WildcardWithProperty()
        {
            JObject o = JObject.Parse(@"{
    ""station"": 92000041000001, 
    ""containers"": [
        {
            ""id"": 1,
            ""text"": ""Sort system"",
            ""containers"": [
                {
                    ""id"": ""2"",
                    ""text"": ""Yard 11""
                },
                {
                    ""id"": ""92000020100006"",
                    ""text"": ""Sort yard 12""
                },
                {
                    ""id"": ""92000020100005"",
                    ""text"": ""Yard 13""
                } 
            ]
        }, 
        {
            ""id"": ""92000020100011"",
            ""text"": ""TSP-1""
        }, 
        {
            ""id"":""92000020100007"",
            ""text"": ""Passenger 15""
        }
    ]
}");

            IList<JToken> tokens = o.SelectTokens("$..*[?(@.text)]").ToList();
            int i = 0;
            Assert.AreEqual("Sort system", (string)tokens[i++]["text"]);
            Assert.AreEqual("TSP-1", (string)tokens[i++]["text"]);
            Assert.AreEqual("Passenger 15", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 11", (string)tokens[i++]["text"]);
            Assert.AreEqual("Sort yard 12", (string)tokens[i++]["text"]);
            Assert.AreEqual("Yard 13", (string)tokens[i++]["text"]);
            Assert.AreEqual(6, tokens.Count);
        }

        [Test]
        public void QueryAgainstNonStringValues()
        {
            IList<object> values = new List<object>
            {
                "ff2dc672-6e15-4aa2-afb0-18f4f69596ad",
                new Guid("ff2dc672-6e15-4aa2-afb0-18f4f69596ad"),
                "http://localhost",
                new Uri("http://localhost"),
                "2000-12-05T05:07:59Z",
                new DateTime(2000, 12, 5, 5, 7, 59, DateTimeKind.Utc),
#if !NET20
                "2000-12-05T05:07:59-10:00",
                new DateTimeOffset(2000, 12, 5, 5, 7, 59, -TimeSpan.FromHours(10)),
#endif
                "SGVsbG8gd29ybGQ=",
                Encoding.UTF8.GetBytes("Hello world"),
                "365.23:59:59",
                new TimeSpan(365, 23, 59, 59)
            };

            JObject o = new JObject(
                new JProperty("prop",
                    new JArray(
                        values.Select(v => new JObject(new JProperty("childProp", v)))
                        )
                    )
                );

            IList<JToken> t = o.SelectTokens("$.prop[?(@.childProp =='ff2dc672-6e15-4aa2-afb0-18f4f69596ad')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='http://localhost')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59Z')]").ToList();
            Assert.AreEqual(2, t.Count);

#if !NET20
            t = o.SelectTokens("$.prop[?(@.childProp =='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(2, t.Count);
#endif

            t = o.SelectTokens("$.prop[?(@.childProp =='SGVsbG8gd29ybGQ=')]").ToList();
            Assert.AreEqual(2, t.Count);

            t = o.SelectTokens("$.prop[?(@.childProp =='365.23:59:59')]").ToList();
            Assert.AreEqual(2, t.Count);
        }

        [Test]
        public void Example()
        {
            JObject o = JObject.Parse(@"{
        ""Stores"": [
          ""Lambton Quay"",
          ""Willis Street""
        ],
        ""Manufacturers"": [
          {
            ""Name"": ""Acme Co"",
            ""Products"": [
              {
                ""Name"": ""Anvil"",
                ""Price"": 50
              }
            ]
          },
          {
            ""Name"": ""Contoso"",
            ""Products"": [
              {
                ""Name"": ""Elbow Grease"",
                ""Price"": 99.95
              },
              {
                ""Name"": ""Headlight Fluid"",
                ""Price"": 4
              }
            ]
          }
        ]
      }");

            string name = (string)o.SelectToken("Manufacturers[0].Name");
            // Acme Co

            decimal productPrice = (decimal)o.SelectToken("Manufacturers[0].Products[0].Price");
            // 50

            string productName = (string)o.SelectToken("Manufacturers[1].Products[0].Name");
            // Elbow Grease

            Assert.AreEqual("Acme Co", name);
            Assert.AreEqual(50m, productPrice);
            Assert.AreEqual("Elbow Grease", productName);

            IList<string> storeNames = o.SelectToken("Stores").Select(s => (string)s).ToList();
            // Lambton Quay
            // Willis Street

            IList<string> firstProductNames = o["Manufacturers"].Select(m => (string)m.SelectToken("Products[1].Name")).ToList();
            // null
            // Headlight Fluid

            decimal totalPrice = o["Manufacturers"].Sum(m => (decimal)m.SelectToken("Products[0].Price"));
            // 149.95

            Assert.AreEqual(2, storeNames.Count);
            Assert.AreEqual("Lambton Quay", storeNames[0]);
            Assert.AreEqual("Willis Street", storeNames[1]);
            Assert.AreEqual(2, firstProductNames.Count);
            Assert.AreEqual(null, firstProductNames[0]);
            Assert.AreEqual("Headlight Fluid", firstProductNames[1]);
            Assert.AreEqual(149.95m, totalPrice);
        }

        [Test]
        public void NotEqualsAndNonPrimativeValues()
        {
            string json = @"[
  {
    ""name"": ""string"",
    ""value"": ""aString""
  },
  {
    ""name"": ""number"",
    ""value"": 123
  },
  {
    ""name"": ""array"",
    ""value"": [
      1,
      2,
      3,
      4
    ]
  },
  {
    ""name"": ""object"",
    ""value"": {
      ""1"": 1
    }
  }
]";

            JArray a = JArray.Parse(json);

            List<JToken> result = a.SelectTokens("$.[?(@.value!=1)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!='2000-12-05T05:07:59-10:00')]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=null)]").ToList();
            Assert.AreEqual(4, result.Count);

            result = a.SelectTokens("$.[?(@.value!=123)]").ToList();
            Assert.AreEqual(3, result.Count);

            result = a.SelectTokens("$.[?(@.value)]").ToList();
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void RootInFilter()
        {
            string json = @"[
   {
      ""store"" : {
         ""book"" : [
            {
               ""category"" : ""reference"",
               ""author"" : ""Nigel Rees"",
               ""title"" : ""Sayings of the Century"",
               ""price"" : 8.95
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Evelyn Waugh"",
               ""title"" : ""Sword of Honour"",
               ""price"" : 12.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""Herman Melville"",
               ""title"" : ""Moby Dick"",
               ""isbn"" : ""0-553-21311-3"",
               ""price"" : 8.99
            },
            {
               ""category"" : ""fiction"",
               ""author"" : ""J. R. R. Tolkien"",
               ""title"" : ""The Lord of the Rings"",
               ""isbn"" : ""0-395-19395-8"",
               ""price"" : 22.99
            }
         ],
         ""bicycle"" : {
            ""color"" : ""red"",
            ""price"" : 19.95
         }
      },
      ""expensive"" : 10
   }
]";

            JArray a = JArray.Parse(json);

            List<JToken> result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 20)]").ToList();
            Assert.AreEqual(1, result.Count);

            result = a.SelectTokens("$.[?($.[0].store.bicycle.price < 10)]").ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void RootInFilterWithRootObject()
        {
            string json = @"{
                ""store"" : {
                    ""book"" : [
                        {
                            ""category"" : ""reference"",
                            ""author"" : ""Nigel Rees"",
                            ""title"" : ""Sayings of the Century"",
                            ""price"" : 8.95
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Evelyn Waugh"",
                            ""title"" : ""Sword of Honour"",
                            ""price"" : 12.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""Herman Melville"",
                            ""title"" : ""Moby Dick"",
                            ""isbn"" : ""0-553-21311-3"",
                            ""price"" : 8.99
                        },
                        {
                            ""category"" : ""fiction"",
                            ""author"" : ""J. R. R. Tolkien"",
                            ""title"" : ""The Lord of the Rings"",
                            ""isbn"" : ""0-395-19395-8"",
                            ""price"" : 22.99
                        }
                    ],
                    ""bicycle"" : [
                        {
                            ""color"" : ""red"",
                            ""price"" : 19.95
                        }
                    ]
                },
                ""expensive"" : 10
            }";

            JObject a = JObject.Parse(json);

            List<JToken> result = a.SelectTokens("$..book[?(@.price <= $['expensive'])]").ToList();
            Assert.AreEqual(2, result.Count);

            result = a.SelectTokens("$.store..[?(@.price > $.expensive)]").ToList();
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void RootInFilterWithInitializers()
        {
            JObject rootObject = new JObject
            {
                { "referenceDate", new JValue(DateTime.MinValue) },
                {
                    "dateObjectsArray",
                    new JArray()
                    {
                        new JObject { { "date", new JValue(DateTime.MinValue) } },
                        new JObject { { "date", new JValue(DateTime.MaxValue) } },
                        new JObject { { "date", new JValue(DateTime.Now) } },
                        new JObject { { "date", new JValue(DateTime.MinValue) } },
                    }
                }
            };

            List<JToken> result = rootObject.SelectTokens("$.dateObjectsArray[?(@.date == $.referenceDate)]").ToList();
            Assert.AreEqual(2, result.Count);
        }
    }
}