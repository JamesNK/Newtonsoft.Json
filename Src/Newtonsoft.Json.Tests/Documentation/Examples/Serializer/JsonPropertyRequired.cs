using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class JsonPropertyRequired
  {
    public class Videogame
    {
      [JsonProperty(Required = Required.Always)]
      public string Name { get; set; }

      [JsonProperty(Required = Required.AllowNull)]
      public DateTime? ReleaseDate { get; set; }
    }

    public void Example()
    {
      string json = @"{
        ""Name"": ""Starcraft III"",
        ""ReleaseDate"": null
      }";

      Videogame starcraft = JsonConvert.DeserializeObject<Videogame>(json);

      Console.WriteLine(starcraft.Name);
      // Starcraft III

      Console.WriteLine(starcraft.ReleaseDate);
      // null
    }
  }
}
