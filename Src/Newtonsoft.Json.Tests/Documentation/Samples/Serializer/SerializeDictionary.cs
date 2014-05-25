using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class SerializeDictionary
    {
        public void Example()
        {
            #region Usage
            Dictionary<string, int> points = new Dictionary<string, int>
            {
                { "James", 9001 },
                { "Jo", 3474 },
                { "Jess", 11926 }
            };

            string json = JsonConvert.SerializeObject(points, Formatting.Indented);

            Console.WriteLine(json);
            // {
            //   "James": 9001,
            //   "Jo": 3474,
            //   "Jess": 11926
            // }
            #endregion
        }
    }
}