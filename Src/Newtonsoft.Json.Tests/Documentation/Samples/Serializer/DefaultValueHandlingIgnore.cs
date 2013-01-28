using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class DefaultValueHandlingIgnore
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
      Person person = new Person();

      string jsonIncludeDefaultValues = JsonConvert.SerializeObject(person, Formatting.Indented);

      Console.WriteLine(jsonIncludeDefaultValues);
      // {
      //   "Name": null,
      //   "Age": 0,
      //   "Partner": null,
      //   "Salary": null
      // }

      string jsonIgnoreDefaultValues = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
        {
          DefaultValueHandling = DefaultValueHandling.Ignore
        });

      Console.WriteLine(jsonIgnoreDefaultValues);
      // {}
      #endregion
    }
  }
}
