using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class SerializeToBson
  {
    public class Event
    {
      public string Name { get; set; }
      public DateTime StartDate { get; set; }
    }

    public void Example()
    {
      Event e = new Event
        {
          Name = "Movie premier",
          StartDate = new DateTime(2013, 1, 22, 20, 30, 0)
        };

      IList<Event> es = new List<Event>
        {
          e, e
        };

      MemoryStream ms = new MemoryStream();
      using (BsonWriter writer = new BsonWriter(ms))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(writer, es);
      }

      string data = Convert.ToBase64String(ms.ToArray());
      
      Console.WriteLine(data);
      // MAAAAAJOYW1lAA4AAABNb3ZpZSBwcmVtaWVyAAlTdGFydERhdGUAwOApYTwBAAAA
    }
  }
}