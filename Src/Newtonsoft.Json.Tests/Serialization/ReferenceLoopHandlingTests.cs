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

using System.Collections.Generic;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

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

    [Test]
    public void SerializePropertyItemReferenceLoopHandling()
    {
      PropertyItemReferenceLoopHandling c = new PropertyItemReferenceLoopHandling();
      c.Text = "Text!";
      c.SetData(new List<PropertyItemReferenceLoopHandling> { c });

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Text"": ""Text!"",
  ""Data"": [
    {
      ""Text"": ""Text!"",
      ""Data"": [
        {
          ""Text"": ""Text!"",
          ""Data"": [
            {
              ""Text"": ""Text!"",
              ""Data"": null
            }
          ]
        }
      ]
    }
  ]
}", json);
    }
  }

  public class PropertyItemReferenceLoopHandling
  {
    private IList<PropertyItemReferenceLoopHandling> _data;
    private int _accessCount;

    public string Text { get; set; }

    [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public IList<PropertyItemReferenceLoopHandling> Data
    {
      get
      {
        if (_accessCount >= 3)
          return null;

        _accessCount++;
        return new List<PropertyItemReferenceLoopHandling>(_data);
      }
    }

    public void SetData(IList<PropertyItemReferenceLoopHandling> data)
    {
      _data = data;
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
