using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Bson
{
    public class DeserializeFromBson
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
            #region Usage
            byte[] data = Convert.FromBase64String("MQAAAAJOYW1lAA8AAABNb3ZpZSBQcmVtaWVyZQAJU3RhcnREYXRlAMDgKWE8AQAAAA==");

            MemoryStream ms = new MemoryStream(data);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();

                Event e = serializer.Deserialize<Event>(reader);

                Console.WriteLine(e.Name);
                // Movie Premiere
            }
            #endregion
        }
    }
}