using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class SerializeCollection
    {
        public void Example()
        {
            #region Usage
            List<string> videogames = new List<string>
            {
                "Starcraft",
                "Halo",
                "Legend of Zelda"
            };

            string json = JsonConvert.SerializeObject(videogames);

            Console.WriteLine(json);
            // ["Starcraft","Halo","Legend of Zelda"]
            #endregion
        }
    }
}