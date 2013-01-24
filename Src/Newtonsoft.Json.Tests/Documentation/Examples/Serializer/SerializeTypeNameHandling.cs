using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class SerializeTypeNameHandling
  {
    public abstract class Business
    {
      public string Name { get; set; }
    }

    public class Hotel : Business
    {
      public int Stars { get; set; }
    }

    public class Stockholder
    {
      public string FullName { get; set; }
      public IList<Business> Businesses { get; set; }
    }

    public void Example()
    {
      Stockholder stockholder = new Stockholder
        {
          FullName = "Steve Stockholder",
          Businesses = new List<Business>
            {
              new Hotel
                {
                  Name = "Hudson Hotel",
                  Stars = 4
                }
            }
        };

      string jsonTypeNameAll = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All
        });

      Console.WriteLine(jsonTypeNameAll);
      // {
      //   "$type": "Newtonsoft.Json.Tests.Documentation.Examples.SerializeTypeNameHandling+Stockholder, Newtonsoft.Json.Tests",
      //   "FullName": "Steve Stockholder",
      //   "Businesses": {
      //     "$type": "System.Collections.Generic.List`1[[Newtonsoft.Json.Tests.Documentation.Examples.SerializeTypeNameHandling+Business, Newtonsoft.Json.Tests]], mscorlib",
      //     "$values": [
      //       {
      //         "$type": "Newtonsoft.Json.Tests.Documentation.Examples.SerializeTypeNameHandling+Hotel, Newtonsoft.Json.Tests",
      //         "Stars": 4,
      //         "Name": "Hudson Hotel"
      //       }
      //     ]
      //   }
      // }

      string jsonTypeNameAuto = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Auto
        });

      Console.WriteLine(jsonTypeNameAuto);
      // {
      //   "FullName": "Steve Stockholder",
      //   "Businesses": [
      //     {
      //       "$type": "Newtonsoft.Json.Tests.Documentation.Examples.SerializeTypeNameHandling+Hotel, Newtonsoft.Json.Tests",
      //       "Stars": 4,
      //       "Name": "Hudson Hotel"
      //     }
      //   ]
      // }

      // for security TypeNameHandling is required when deserializing
      Stockholder newStockholder = JsonConvert.DeserializeObject<Stockholder>(jsonTypeNameAuto, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Auto
        });

      Console.WriteLine(newStockholder.Businesses[0].GetType().Name);
      // Hotel
    }
  }
}