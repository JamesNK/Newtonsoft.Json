using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class NullValueHandlingIgnore
  {
    public class Person
    {
      public string Name { get; set; }
      public int Age { get; set; }
      public Person Partner { get; set; }
      public decimal? Salary { get; set; }
    }

    public void Example()
    {
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
          NullValueHandling = Json.NullValueHandling.Ignore
        });

      Console.WriteLine(jsonIgnoreNullValues);
      // {
      //   "Name": "Nigal Newborn",
      //   "Age": 1
      // }
    }
  }
}
