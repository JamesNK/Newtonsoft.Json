using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Bson
{
  public class DeserializeFromBsonCollection
  {
    #region Types
    public class Event
    {
      public string Name { get; set; }
      public DateTime StartDate { get; set; }
    }
    #endregion

    public void Example()
    {
      //IList<Event> e = new List<Event>
      //  {
      //    new Event {StartDate = new DateTime(2013, 3, 31), Name = "Easter"}
      //  };
      //MemoryStream ms1 = new MemoryStream();
      //using (BsonWriter writer = new BsonWriter(ms1))
      //{
      //  JsonSerializer serializer = new JsonSerializer();

      //  serializer.Serialize(writer, e);
      //}

      //string ss = Convert.ToBase64String(ms1.ToArray());
      //Console.WriteLine(ss);

      #region Usage
      string s = "MQAAAAMwACkAAAACTmFtZQAHAAAARWFzdGVyAAlTdGFydERhdGUAgDf0uj0BAAAAAA==";
      byte[] data = Convert.FromBase64String(s);

      MemoryStream ms = new MemoryStream(data);
      using (BsonReader reader = new BsonReader(ms))
      {
        reader.ReadRootValueAsArray = true;

        JsonSerializer serializer = new JsonSerializer();

        IList<Event> events = serializer.Deserialize<IList<Event>>(reader);

        Console.WriteLine(events.Count);
        // 1

        Console.WriteLine(events[0].Name);
        // Easter
      }
      #endregion
    }
  }
}