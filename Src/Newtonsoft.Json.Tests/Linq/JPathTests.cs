using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Collections;

namespace Newtonsoft.Json.Tests.Linq
{
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
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Unexpected character while parsing path indexer: [")]
    public void BadCharactersInIndexer()
    {
      new JPath("Blah[[0]].Two.Three[1].Four");
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Path ended with open indexer. Expected ]")]
    public void UnclosedIndexer()
    {
      new JPath("Blah[0");
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
    [ExpectedException(typeof(Exception), ExpectedMessage = "Empty path indexer.")]
    public void EmptyIndexer()
    {
      new JPath("[]");
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Unexpected character while parsing path: ]")]
    public void IndexerCloseInProperty()
    {
      new JPath("]");
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
    [ExpectedException(typeof(Exception), ExpectedMessage = "Unexpected character following indexer: B")]
    public void MissingDotAfterIndexer()
    {
      new JPath("[1]Blah");
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
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Index 1 not valid on JObject.")]
    public void EvaluateIndexerOnObjectWithError()
    {
      JObject o = new JObject(
        new JProperty("Blah", 1));

      o.SelectToken("[1]", true);
    }

    [Test]
    public void EvaluatePropertyOnArray()
    {
      JArray a = new JArray(1, 2, 3, 4, 5);

      JToken t = a.SelectToken("BlahBlah");
      Assert.IsNull(t);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Property 'BlahBlah' not valid on JArray.")]
    public void EvaluatePropertyOnArrayWithError()
    {
      JArray a = new JArray(1, 2, 3, 4, 5);

      a.SelectToken("BlahBlah", true);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Index 1 not valid on JConstructor.")]
    public void EvaluateIndexerOnConstructorWithError()
    {
      JConstructor c = new JConstructor("Blah");

      c.SelectToken("[1]", true);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Property 'Missing' does not exist on JObject.")]
    public void EvaluateMissingPropertyWithError()
    {
      JObject o = new JObject(
        new JProperty("Blah", 1));

      o.SelectToken("Missing", true);
    }

    [Test]
    public void EvaluateOutOfBoundsIndxer()
    {
      JArray a = new JArray(1, 2, 3, 4, 5);

      JToken t = a.SelectToken("[1000].Ha");
      Assert.IsNull(t);
    }

    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException), ExpectedMessage = "Index 1000 outside the bounds of JArray.")]
    public void EvaluateOutOfBoundsIndxerWithError()
    {
      JArray a = new JArray(1, 2, 3, 4, 5);

      a.SelectToken("[1000].Ha", true);
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
  }
}