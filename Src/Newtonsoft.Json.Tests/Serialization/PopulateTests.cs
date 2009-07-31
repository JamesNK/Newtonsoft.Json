using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class PopulateTests : TestFixtureBase
  {
    [Test]
    public void PopulatePerson()
    {
      Person p = new Person();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Populate(new StringReader(@"{""Name"":""James""}"), p);

      Assert.AreEqual("James", p.Name);
    }

    [Test]
    public void PopulateStore()
    {
      Store s = new Store();
      s.Color = StoreColor.Red;
      s.product = new List<Product>
        {
          new Product
            {
              ExpiryDate = new DateTime(2000, 12, 3, 0, 0, 0, DateTimeKind.Utc),
              Name = "ProductName!",
              Price = 9.9m
            }
        };
      s.Width = 99.99d;
      s.Mottos = new List<string> { "Can do!", "We deliver!" };

      string json = @"{
  ""Color"": 2,
  ""Establised"": ""\/Date(1264122061000+0000)\/"",
  ""Width"": 99.99,
  ""Employees"": 999,
  ""RoomsPerFloor"": [
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9
  ],
  ""Open"": false,
  ""Symbol"": ""@"",
  ""Mottos"": [
    ""Fail whale""
  ],
  ""Cost"": 100980.1,
  ""Escape"": ""\r\n\t\f\b?{\\r\\n\""'"",
  ""product"": [
    {
      ""Name"": ""ProductName!"",
      ""ExpiryDate"": ""\/Date(975801600000)\/"",
      ""Price"": 9.9,
      ""Sizes"": null
    }
  ]
}";

      JsonSerializer serializer = new JsonSerializer();
      serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
      serializer.Populate(new StringReader(json), s);

      Assert.AreEqual(1, s.Mottos.Count);
      Assert.AreEqual("Fail whale", s.Mottos[0]);
      Assert.AreEqual(1, s.product.Count);

      //Assert.AreEqual("James", p.Name);
    }

    [Test]
    public void PopulateListOfPeople()
    {
      List<Person> p = new List<Person>();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Populate(new StringReader(@"[{""Name"":""James""},{""Name"":""Jim""}]"), p);

      Assert.AreEqual(2, p.Count);
      Assert.AreEqual("James", p[0].Name);
      Assert.AreEqual("Jim", p[1].Name);
    }

    [Test]
    public void PopulateDictionary()
    {
      Dictionary<string, string> p = new Dictionary<string, string>();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Populate(new StringReader(@"{""Name"":""James""}"), p);

      Assert.AreEqual(1, p.Count);
      Assert.AreEqual("James", p["Name"]);
    }
  }
}