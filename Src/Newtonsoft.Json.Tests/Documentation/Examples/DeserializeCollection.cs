using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class DeserializeCollection
  {
    public void Example()
    {
      string json = @"[""Starcraft"",""Halo"",""Legend of Zelda""]";

      List<string> videogames = JsonConvert.DeserializeObject<List<string>>(json);

      Console.WriteLine(string.Join(", ", videogames));
      // Starcraft, Halo, Legend of Zelda
    }
  }
}
