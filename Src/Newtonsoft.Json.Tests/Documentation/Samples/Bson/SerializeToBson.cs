using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Bson
{
    public class SerializeToBson
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
            Event e = new Event
            {
                Name = "Movie Premiere",
                StartDate = new DateTime(2013, 1, 22, 20, 30, 0)
            };

            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, e);
            }

            string data = Convert.ToBase64String(ms.ToArray());

            Console.WriteLine(data);
            // MQAAAAJOYW1lAA8AAABNb3ZpZSBQcmVtaWVyZQAJU3RhcnREYXRlAMDgKWE8AQAAAA==
            #endregion
        }
    }
}