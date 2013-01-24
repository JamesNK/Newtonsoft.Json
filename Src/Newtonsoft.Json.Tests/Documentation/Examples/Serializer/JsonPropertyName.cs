using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class JsonPropertyName
  {
    public class Videogame
    {
      [JsonProperty("name")]
      public string Name { get; set; }

      [JsonProperty("release_date")]
      public DateTime ReleaseDate { get; set; }
    }

    public void Example()
    {
      Videogame starcraft = new Videogame
        {
          Name = "Starcraft",
          ReleaseDate = new DateTime(1998, 1, 1)
        };

      string json = JsonConvert.SerializeObject(starcraft, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "name": "Starcraft",
      //   "release_date": "1998-01-01T00:00:00"
      // }
    }
  }
}
