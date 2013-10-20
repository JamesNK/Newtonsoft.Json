using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateJsonCollectionInitializer
    {
        public void Example()
        {
            #region Usage
            JObject o = new JObject
            {
                { "Cpu", "Intel" },
                { "Memory", 32 },
                {
                    "Drives", new JArray
                    {
                        "DVD",
                        "SSD"
                    }
                }
            };

            Console.WriteLine(o.ToString());
            // {
            //   "Cpu": "Intel",
            //   "Memory": 32,
            //   "Drives": [
            //     "DVD",
            //     "SSD"
            //   ]
            // }
            #endregion
        }
    }
}