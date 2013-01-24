using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class DeserializeFromBsonCollection
  {
    public class Event
    {
      public string Name { get; set; }
      public DateTime StartDate { get; set; }
    }

    public void Example()
    {
      byte[] data = Convert.FromBase64String("awAAAAMwADAAAAACTmFtZQAOAAAATW92aWUgcHJlbWllcgAJU3RhcnREYXRlAMDgKWE8AQAAAAMxADAAAAACTmFtZQAOAAAATW92aWUgcHJlbWllcgAJU3RhcnREYXRlAMDgKWE8AQAAAAA=");

      MemoryStream ms = new MemoryStream(data);
      using (BsonReader reader = new BsonReader(ms))
      {
        reader.ReadRootValueAsArray = true;

        JsonSerializer serializer = new JsonSerializer();

        IList<Event> events = serializer.Deserialize<IList<Event>>(reader);

        Console.WriteLine(events.Count);
        // 2
      }
    }
  }
}