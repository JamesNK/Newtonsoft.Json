using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class ReadJson
    {
        public void Example()
        {
            #region Usage
            JObject o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));

            // read JSON directly from a file
            using (StreamReader file = File.OpenText(@"c:\videogames.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o2 = (JObject)JToken.ReadFrom(reader);
            }
            #endregion
        }
    }
}