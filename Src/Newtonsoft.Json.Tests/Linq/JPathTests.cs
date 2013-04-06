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
      Assert.AreEqual("Blah", path.Parts[0]);
    }

    [Test]
    public void TwoProperties()
    {
      JPath path = new JPath("Blah.Two");
      Assert.AreEqual(2, path.Parts.Count);
      Assert.AreEqual("Blah", path.Parts[0]);
      Assert.AreEqual("Two", path.Parts[1]);
    }

    [Test]
    public void SinglePropertyAndIndexer()
    {
      JPath path = new JPath("Blah[0]");
      Assert.AreEqual(2, path.Parts.Count);
      Assert.AreEqual("Blah", path.Parts[0]);
      Assert.AreEqual(0, path.Parts[1]);
    }

    [Test]
    public void MultiplePropertiesAndIndexers()
    {
      JPath path = new JPath("Blah[0].Two.Three[1].Four");
      Assert.AreEqual(6, path.Parts.Count);
      Assert.AreEqual("Blah", path.Parts[0]);
      Assert.AreEqual(0, path.Parts[1]);
      Assert.AreEqual("Two", path.Parts[2]);
      Assert.AreEqual("Three", path.Parts[3]);
      Assert.AreEqual(1, path.Parts[4]);
      Assert.AreEqual("Four", path.Parts[5]);
    }

    [Test]
    public void BadCharactersInIndexer()
    {
      ExceptionAssert.Throws<JsonException>(
        @"Unexpected character while parsing path indexer: [",
        () =>
        {
          new JPath("Blah[[0]].Two.Three[1].Four");
        });
    }

    [Test]
    public void UnclosedIndexer()
    {
      ExceptionAssert.Throws<JsonException>(
        @"Path ended with open indexer. Expected ]",
        () =>
        {
          new JPath("Blah[0");
        });
    }

    [Test]
    public void AdditionalDots()
    {
      JPath path = new JPath(".Blah..[0]..Two.Three....[1].Four.");
      Assert.AreEqual(6, path.Parts.Count);
      Assert.AreEqual("Blah", path.Parts[0]);
      Assert.AreEqual(0, path.Parts[1]);
      Assert.AreEqual("Two", path.Parts[2]);
      Assert.AreEqual("Three", path.Parts[3]);
      Assert.AreEqual(1, path.Parts[4]);
      Assert.AreEqual("Four", path.Parts[5]);
    }

    [Test]
    public void IndexerOnly()
    {
      JPath path = new JPath("[111119990]");
      Assert.AreEqual(1, path.Parts.Count);
      Assert.AreEqual(111119990, path.Parts[0]);
    }

    [Test]
    public void EmptyIndexer()
    {
      ExceptionAssert.Throws<JsonException>(
        "Empty path indexer.",
        () =>
        {
          new JPath("[]");
        });
    }

    [Test]
    public void IndexerCloseInProperty()
    {
      ExceptionAssert.Throws<JsonException>(
        "Unexpected character while parsing path: ]",
        () =>
        {
          new JPath("]");
        });
    }

    [Test]
    public void AdjacentIndexers()
    {
      JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
      Assert.AreEqual(4, path.Parts.Count);
      Assert.AreEqual(1, path.Parts[0]);
      Assert.AreEqual(0, path.Parts[1]);
      Assert.AreEqual(0, path.Parts[2]);
      Assert.AreEqual(int.MaxValue, path.Parts[3]);
    }

    [Test]
    public void MissingDotAfterIndexer()
    {
      ExceptionAssert.Throws<JsonException>(
        "Unexpected character following indexer: B",
        () =>
        {
          new JPath("[1]Blah");
        });
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
        () =>
        {
          o.SelectToken("[1]", true);
        });
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
        () =>
        {
          a.SelectToken("BlahBlah", true);
        });
    }

    [Test]
    public void EvaluateConstructorOutOfBoundsIndxerWithError()
    {
      JConstructor c = new JConstructor("Blah");

      ExceptionAssert.Throws<JsonException>(
        @"Index 1 outside the bounds of JConstructor.",
        () =>
        {
          c.SelectToken("[1]", true);
        });
    }

    [Test]
    public void EvaluateMissingPropertyWithError()
    {
      JObject o = new JObject(
        new JProperty("Blah", 1));

      ExceptionAssert.Throws<JsonException>(
        "Property 'Missing' does not exist on JObject.",
        () =>
        {
          o.SelectToken("Missing", true);
        });
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
        () =>
        {
          a.SelectToken("[1000].Ha", true);
        });
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
    public void EvaluateSinglePropertyReturningArray()
    {
      JObject o = new JObject(
        new JProperty("Blah", new [] { 1, 2, 3 }));

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