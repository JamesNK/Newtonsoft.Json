using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  public class JsonArrayAttributeTests : TestFixtureBase
  {
    [Test]
    public void IsReferenceTest()
    {
      JsonPropertyAttribute attribute = new JsonPropertyAttribute();
      Assert.AreEqual(null, attribute._isReference);
      Assert.AreEqual(false, attribute.IsReference);

      attribute.IsReference = false;
      Assert.AreEqual(false, attribute._isReference);
      Assert.AreEqual(false, attribute.IsReference);

      attribute.IsReference = true;
      Assert.AreEqual(true, attribute._isReference);
      Assert.AreEqual(true, attribute.IsReference);
    }

    [Test]
    public void NullValueHandlingTest()
    {
      JsonPropertyAttribute attribute = new JsonPropertyAttribute();
      Assert.AreEqual(null, attribute._nullValueHandling);
      Assert.AreEqual(NullValueHandling.Include, attribute.NullValueHandling);

      attribute.NullValueHandling = NullValueHandling.Ignore;
      Assert.AreEqual(NullValueHandling.Ignore, attribute._nullValueHandling);
      Assert.AreEqual(NullValueHandling.Ignore, attribute.NullValueHandling);
    }

    [Test]
    public void DefaultValueHandlingTest()
    {
      JsonPropertyAttribute attribute = new JsonPropertyAttribute();
      Assert.AreEqual(null, attribute._defaultValueHandling);
      Assert.AreEqual(DefaultValueHandling.Include, attribute.DefaultValueHandling);

      attribute.DefaultValueHandling = DefaultValueHandling.Ignore;
      Assert.AreEqual(DefaultValueHandling.Ignore, attribute._defaultValueHandling);
      Assert.AreEqual(DefaultValueHandling.Ignore, attribute.DefaultValueHandling);
    }

    [Test]
    public void ReferenceLoopHandlingTest()
    {
      JsonPropertyAttribute attribute = new JsonPropertyAttribute();
      Assert.AreEqual(null, attribute._defaultValueHandling);
      Assert.AreEqual(ReferenceLoopHandling.Error, attribute.ReferenceLoopHandling);

      attribute.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
      Assert.AreEqual(ReferenceLoopHandling.Ignore, attribute._referenceLoopHandling);
      Assert.AreEqual(ReferenceLoopHandling.Ignore, attribute.ReferenceLoopHandling);
    }
  }
}