using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class JsonPropertyRequired
    {
        #region Types
        public class Videogame
        {
            [JsonProperty(Required = Required.Always)]
            public string Name { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            public DateTime? ReleaseDate { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            string json = @"{
              'Name': 'Starcraft III',
              'ReleaseDate': null
            }";

            Videogame starcraft = JsonConvert.DeserializeObject<Videogame>(json);

            Console.WriteLine(starcraft.Name);
            // Starcraft III

            Console.WriteLine(starcraft.ReleaseDate);
            // null
            #endregion
        }
    }
}