using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class JsonPropertyItemLevelSetting
  {
    #region Types
    public class Business
    {
      public string Name { get; set; }

      [JsonProperty(ItemIsReference = true)]
      public IList<Employee> Employees { get; set; } 
    }

    public class Employee
    {
      public string Name { get; set; }

      [JsonProperty(IsReference = true)]
      public Employee Manager { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      Employee manager = new Employee
      {
        Name = "George-Michael"
      };
      Employee worker = new Employee
      {
        Name = "Maeby",
        Manager = manager
      };

      Business business = new Business
        {
          Name = "Acme Ltd.",
          Employees = new List<Employee>
            {
              manager,
              worker
            }
        };

      string json = JsonConvert.SerializeObject(business, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "Name": "Acme Ltd.",
      //   "Employees": [
      //     {
      //       "$id": "1",
      //       "Name": "George-Michael",
      //       "Manager": null
      //     },
      //     {
      //       "$id": "2",
      //       "Name": "Maeby",
      //       "Manager": {
      //         "$ref": "1"
      //       }
      //     }
      //   ]
      // }
      #endregion
    }
  }
}
