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
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JPathTests : TestFixtureBase
    {
        [Test]
        public void SingleProperty()
        {
            JPath path = new JPath("Blah");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
        }

        [Test]
        public void SinglePropertyWithRoot()
        {
            JPath path = new JPath("$.Blah");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
        }

        [Test]
        public void WildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.*");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(null, ((FieldFilter)path.Parts[0]).Name);
        }

        [Test]
        public void WildcardArrayWithRoot()
        {
            JPath path = new JPath("$.[*]");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Parts[0]).Index);
        }

        [Test]
        public void WildcardArray()
        {
            JPath path = new JPath("[*]");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Parts[0]).Index);
        }

        [Test]
        public void WildcardArrayWithProperty()
        {
            JPath path = new JPath("[*].derp");
            Assert.AreEqual(2, path.Parts.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Parts[0]).Index);
            Assert.AreEqual("derp", ((FieldFilter)path.Parts[1]).Name);
        }

        [Test]
        public void QuotedWildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.['*']");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual("*", ((FieldFilter)path.Parts[0]).Name);
        }

        [Test]
        public void SingleScanWithRoot()
        {
            JPath path = new JPath("$..Blah");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual("Blah", ((ScanFilter)path.Parts[0]).Name);
        }

        [Test]
        public void WildcardScanWithRoot()
        {
            JPath path = new JPath("$..*");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Parts[0]).Name);
        }

        [Test]
        public void TwoProperties()
        {
            JPath path = new JPath("Blah.Two");
            Assert.AreEqual(2, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
            Assert.AreEqual("Two", ((FieldFilter)path.Parts[1]).Name);
        }

        [Test]
        public void OnePropertyOneScan()
        {
            JPath path = new JPath("Blah..Two");
            Assert.AreEqual(2, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
            Assert.AreEqual("Two", ((ScanFilter)path.Parts[1]).Name);
        }

        [Test]
        public void SinglePropertyAndIndexer()
        {
            JPath path = new JPath("Blah[0]");
            Assert.AreEqual(2, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Parts[1]).Index);
        }

        [Test]
        public void SinglePropertyAndFilter()
        {
            JPath path = new JPath("Blah[?(@.name=='hi')]");
            Assert.AreEqual(2, path.Parts.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Parts[0]).Name);
            List<object> expressions = ((QueryFilter)path.Parts[1]).Expression;
            Assert.AreEqual(1, expressions.Count);
        }

        [Test]
        public void MultiplePropertiesAndIndexers()
        {
            JPath path = new JPath("Blah[0]..Two.Three[1].Four");
            Assert.AreEqual(6, path.Parts.Count);
            Assert.AreEqual(FilterType.Field, path.Parts[0].Type);
            Assert.AreEqual("Blah", ((FieldFilter) path.Parts[0]).Name);
            Assert.AreEqual(FilterType.ArrayIndex, path.Parts[1].Type);
            Assert.AreEqual(0, ((ArrayIndexFilter) path.Parts[1]).Index);
            Assert.AreEqual(FilterType.Scan, path.Parts[2].Type);
            Assert.AreEqual("Two", ((ScanFilter)path.Parts[2]).Name);
            Assert.AreEqual("Three", ((FieldFilter)path.Parts[3]).Name);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Parts[4]).Index);
            Assert.AreEqual("Four", ((FieldFilter)path.Parts[5]).Name);
        }

        [Test]
        public void BadCharactersInIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unexpected character while parsing path indexer: [",
                () => { new JPath("Blah[[0]].Two.Three[1].Four"); });
        }

        [Test]
        public void UnclosedIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Path ended with open indexer.",
                () => { new JPath("Blah[0"); });
        }

        //[Test]
        //public void AdditionalDots()
        //{
        //    JPath path = new JPath(".Blah..[0]..Two.Three....[1].Four.");
        //    Assert.AreEqual(6, path.Parts.Count);
        //    Assert.AreEqual("Blah", path.Parts[0]);
        //    Assert.AreEqual(0, path.Parts[1]);
        //    Assert.AreEqual("Two", path.Parts[2]);
        //    Assert.AreEqual("Three", path.Parts[3]);
        //    Assert.AreEqual(1, path.Parts[4]);
        //    Assert.AreEqual("Four", path.Parts[5]);
        //}

        [Test]
        public void IndexerOnly()
        {
            JPath path = new JPath("[111119990]");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(111119990, ((ArrayIndexFilter)path.Parts[0]).Index);
        }

        [Test]
        public void MultipleIndexes()
        {
            JPath path = new JPath("[111119990,3]");
            Assert.AreEqual(1, path.Parts.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Parts[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Parts[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Parts[0]).Indexes[1]);
        }

        [Test]
        public void EmptyIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                "Array index expected.",
                () => { new JPath("[]"); });
        }

        [Test]
        public void IndexerCloseInProperty()
        {
            ExceptionAssert.Throws<JsonException>(
                "Unexpected character while parsing path: ]",
                () => { new JPath("]"); });
        }

        [Test]
        public void AdjacentIndexers()
        {
            JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
            Assert.AreEqual(4, path.Parts.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Parts[0]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Parts[1]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Parts[2]).Index);
            Assert.AreEqual(int.MaxValue, ((ArrayIndexFilter)path.Parts[3]).Index);
        }

        [Test]
        public void MissingDotAfterIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                "Unexpected character following indexer: B",
                () => { new JPath("[1]Blah"); });
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

            ExceptionAssert.Throws<JsonException>(
                @"Index 1 not valid on JObject.",
                () => { o.SelectToken("[1]", true); });
        }

        [Test]
        public void EvaluatePropertyOnArray()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            JToken t = a.SelectToken("BlahBlah");
            Assert.IsNull(t);
        }

        [Test]
        public void EvaluatePropertyOnArrayWithError()
        {
            JArray a = new JArray(1, 2, 3, 4, 5);

            ExceptionAssert.Throws<JsonException>(
                @"Property 'BlahBlah' not valid on JArray.",
                () => { a.SelectToken("BlahBlah", true); });
        }

        [Test]
        public void EvaluateConstructorOutOfBoundsIndxerWithError()
        {
            JConstructor c = new JConstructor("Blah");

            ExceptionAssert.Throws<JsonException>(
                @"Index 1 outside the bounds of JConstructor.",
                () => { c.SelectToken("[1]", true); });
        }

        [Test]
        public void EvaluateMissingPropertyWithError()
        {
            JObject o = new JObject(
                new JProperty("Blah", 1));

            ExceptionAssert.Throws<JsonException>(
                "Property 'Missing' does not exist on JObject.",
                () => { o.SelectToken("Missing", true); });
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

            ExceptionAssert.Throws<JsonException>(
                "Index 1000 outside the bounds of JArray.",
                () => { a.SelectToken("[1000].Ha", true); });
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
            JObject o1 = new JObject{ {"Name", 1} };
            JObject o2 = new JObject{ {"Name", 2} };
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
    }
}