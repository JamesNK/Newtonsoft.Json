using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class FromObject
    {
        #region Types
        public class Computer
        {
            public string Cpu { get; set; }
            public int Memory { get; set; }
            public IList<string> Drives { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            JValue i = (JValue)JToken.FromObject(12345);

            Console.WriteLine(i.Type);
            // Integer
            Console.WriteLine(i.ToString());
            // 12345

            JValue s = (JValue)JToken.FromObject("A string");

            Console.WriteLine(s.Type);
            // String
            Console.WriteLine(s.ToString());
            // A string

            Computer computer = new Computer
            {
                Cpu = "Intel",
                Memory = 32,
                Drives = new List<string>
                {
                    "DVD",
                    "SSD"
                }
            };

            JObject o = (JObject)JToken.FromObject(computer);

            Console.WriteLine(o.ToString());
            // {
            //   "Cpu": "Intel",
            //   "Memory": 32,
            //   "Drives": [
            //     "DVD",
            //     "SSD"
            //   ]
            // }

            JArray a = (JArray)JToken.FromObject(computer.Drives);

            Console.WriteLine(a.ToString());
            // [
            //   "DVD",
            //   "SSD"
            // ]
            #endregion
        }
    }
}