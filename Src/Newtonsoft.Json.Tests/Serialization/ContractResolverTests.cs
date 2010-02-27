using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class DynamicContractResolver : DefaultContractResolver
  {
    private readonly char _startingWithChar;
    public DynamicContractResolver(char startingWithChar)
      : base(false)
    {
      _startingWithChar = startingWithChar;
    }

    protected override IList<JsonProperty> CreateProperties(JsonObjectContract contract)
    {
      IList<JsonProperty> properties = base.CreateProperties(contract);

      // only serializer properties that start with the specified character
      properties = 
        properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

      return properties;
    }
  }

  public class Book
  {
    public string BookName { get; set; }
    public decimal BookPrice { get; set; }
    public string AuthorName { get; set; }
    public int AuthorAge { get; set; }
    public string AuthorCountry { get; set; }
  }

  public class ContractResolverTests : TestFixtureBase
  {
    [Test]
    public void SingleTypeWithMultipleContractResolvers()
    {
      Book book = new Book
                    {
                      BookName = "The Gathering Storm",
                      BookPrice = 16.19m,
                      AuthorName = "Brandon Sanderson",
                      AuthorAge = 34,
                      AuthorCountry = "United States of America"
                    };

      string startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

      // {
      //   "AuthorName": "Brandon Sanderson",
      //   "AuthorAge": 34,
      //   "AuthorCountry": "United States of America"
      // }

      string startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

      // {
      //   "BookName": "The Gathering Storm",
      //   "BookPrice": 16.19
      // }

      Assert.AreEqual(@"{
  ""AuthorName"": ""Brandon Sanderson"",
  ""AuthorAge"": 34,
  ""AuthorCountry"": ""United States of America""
}", startingWithA);

      Assert.AreEqual(@"{
  ""BookName"": ""The Gathering Storm"",
  ""BookPrice"": 16.19
}", startingWithB);
    }
  }
}
