using System.Collections.Generic;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
  public class ReferenceLoopHandlingTests : TestFixtureBase
  {
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

    [Test]
    public void IgnoreObjectReferenceLoop()
    {
      ReferenceLoopHandlingObjectContainerAttribute o = new ReferenceLoopHandlingObjectContainerAttribute();
      o.Value = o;

      string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
        {
          ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        });
      Assert.AreEqual("{}", json);
    }

    [Test]
    public void IgnoreObjectReferenceLoopWithPropertyOverride()
    {
      ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride o = new ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride();
      o.Value = o;

      string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual(@"{
  ""Value"": {
    ""Value"": {
      ""Value"": {
        ""Value"": {
          ""Value"": {
            ""Value"": null
          }
        }
      }
    }
  }
}", json);
    }

    [Test]
    public void IgnoreArrayReferenceLoop()
    {
      ReferenceLoopHandlingList a = new ReferenceLoopHandlingList();
      a.Add(a);

      string json = JsonConvert.SerializeObject(a, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual("[]", json);
    }

    [Test]
    public void IgnoreDictionaryReferenceLoop()
    {
      ReferenceLoopHandlingDictionary d = new ReferenceLoopHandlingDictionary();
      d.Add("First", d);

      string json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
      {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
      });
      Assert.AreEqual("{}", json);
    }
  }

  [JsonArray(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingList : List<ReferenceLoopHandlingList>
  {
  }

  [JsonDictionary(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingDictionary : Dictionary<string, ReferenceLoopHandlingDictionary>
  {
  }

  [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingObjectContainerAttribute
  {
    public ReferenceLoopHandlingObjectContainerAttribute Value { get; set; }
  }

  [JsonObject(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
  public class ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride
  {
    private ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride _value;
    private int _getCount;

    [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public ReferenceLoopHandlingObjectContainerAttributeWithPropertyOverride Value
    {
      get
      {
        if (_getCount < 5)
        {
          _getCount++;
          return _value;
        }
        return null;
      }
      set { _value = value; }
    }
  }
}
