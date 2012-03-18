using System.Collections.Generic;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
  [TestFixture]
  public class JTokenEqualityComparerTests : TestFixtureBase
  {
    [Test]
    public void JValueDictionary()
    {
      Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
      JValue v11 = new JValue(1);
      JValue v12 = new JValue(1);

      dic[v11] = 1;
      dic[v12] += 1;
      Assert.AreEqual(2, dic[v11]);
    }

    [Test]
    public void JArrayDictionary()
    {
      Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
      JArray v11 = new JArray();
      JArray v12 = new JArray();

      dic[v11] = 1;
      dic[v12] += 1;
      Assert.AreEqual(2, dic[v11]);
    }

    [Test]
    public void JObjectDictionary()
    {
      Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
      JObject v11 = new JObject() { { "Test", new JValue(1) }, { "Test1", new JValue(1) } };
      JObject v12 = new JObject() { { "Test", new JValue(1) }, { "Test1", new JValue(1) } };

      dic[v11] = 1;
      dic[v12] += 1;
      Assert.AreEqual(2, dic[v11]);
    }

    [Test]
    public void JConstructorDictionary()
    {
      Dictionary<JToken, int> dic = new Dictionary<JToken, int>(JToken.EqualityComparer);
      JConstructor v11 = new JConstructor("ConstructorValue");
      JConstructor v12 = new JConstructor("ConstructorValue");

      dic[v11] = 1;
      dic[v12] += 1;
      Assert.AreEqual(2, dic[v11]);
    }
  }
}