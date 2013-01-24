using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class SerializeConditionalProperty
  {
    public class Employee
    {
      public string Name { get; set; }
      public Employee Manager { get; set; }

      public bool ShouldSerializeManager()
      {
        // don't serialize the Manager property if an employee is their own manager
        return (Manager != this);
      }
    }

    public void Example()
    {
      Employee joe = new Employee();
      joe.Name = "Joe Employee";
      Employee mike = new Employee();
      mike.Name = "Mike Manager";

      joe.Manager = mike;

      // mike is his own manager
      // ShouldSerialize will skip this property
      mike.Manager = mike;

      string json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented);
      // [
      //   {
      //     "Name": "Joe Employee",
      //     "Manager": {
      //       "Name": "Mike Manager"
      //     }
      //   },
      //   {
      //     "Name": "Mike Manager"
      //   }
      // ]
    }
  }
}