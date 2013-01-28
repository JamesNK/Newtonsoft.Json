using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class WriteToJsonFile
  {
    public void Example()
    {
      #region Usage
      JObject videogameRatings = new JObject(
        new JProperty("Halo", 9),
        new JProperty("Starcraft", 9),
        new JProperty("Call of Duty", 7.5));

      File.WriteAllText(@"c:\videogames.json", videogameRatings.ToString());

      // write JSON directly to a file
      using (StreamWriter file = File.CreateText(@"c:\videogames.json"))
      using (JsonTextWriter writer = new JsonTextWriter(file))
      {
        videogameRatings.WriteTo(writer);
      }
      #endregion
    }
  }
}
