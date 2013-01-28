using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class NullValueHandlingIgnore
  {
    #region Types
    public class Person
    {
      public string Name { get; set; }
      public int Age { get; set; }
      public Person Partner { get; set; }
      public decimal? Salary { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      Person person = new Person
        {
          Name = "Nigal Newborn",
          Age = 1
        };

      string jsonIncludeNullValues = JsonConvert.SerializeObject(person, Formatting.Indented);

      Console.WriteLine(jsonIncludeNullValues);
      // {
      //   "Name": "Nigal Newborn",
      //   "Age": 1,
      //   "Partner": null,
      //   "Salary": null
      // }

      string jsonIgnoreNullValues = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
        {
          NullValueHandling = NullValueHandling.Ignore
        });

      Console.WriteLine(jsonIgnoreNullValues);
      // {
      //   "Name": "Nigal Newborn",
      //   "Age": 1
      // }
      #endregion
    }
  }
}