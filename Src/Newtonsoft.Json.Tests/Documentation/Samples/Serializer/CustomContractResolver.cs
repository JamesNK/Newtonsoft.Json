using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class CustomContractResolver
  {
    #region Types
    public class DynamicContractResolver : DefaultContractResolver
    {
      private readonly char _startingWithChar;
      public DynamicContractResolver(char startingWithChar)
      {
        _startingWithChar = startingWithChar;
      }

      protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
      {
        IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

        // only serializer properties that start with the specified character
        properties =
          properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

        return properties;
      }
    }

    public class Person
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }

      public string FullName
      {
        get { return FirstName + " " + LastName; }
      }
    }
    #endregion

    public void Example()
    {
      #region Usage
      Person person = new Person
        {
          FirstName = "Dennis",
          LastName = "Deepwater-Diver"
        };

      string startingWithF = JsonConvert.SerializeObject(person, Formatting.Indented,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('F') });

      Console.WriteLine(startingWithF);
      // {
      //   "FirstName": "Dennis",
      //   "FullName": "Dennis Deepwater-Diver"
      // }

      string startingWithL = JsonConvert.SerializeObject(person, Formatting.Indented,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('L') });

      Console.WriteLine(startingWithL);
      // {
      //   "LastName": "Deepwater-Diver"
      // }
      #endregion
    }
  }
}