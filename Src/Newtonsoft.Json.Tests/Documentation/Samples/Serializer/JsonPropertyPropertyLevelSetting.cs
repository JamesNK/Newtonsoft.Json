using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class JsonPropertyPropertyLevelSetting
    {
        #region Types
        public class Vessel
        {
            public string Name { get; set; }
            public string Class { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? LaunchDate { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Vessel vessel = new Vessel
            {
                Name = "Red October",
                Class = "Typhoon"
            };

            string json = JsonConvert.SerializeObject(vessel, Formatting.Indented);

            Console.WriteLine(json);
            // {
            //   "Name": "Red October",
            //   "Class": "Typhoon"
            // }
            #endregion
        }
    }
}