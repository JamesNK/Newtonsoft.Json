using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateReader
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

            JsonReader reader = o.CreateReader();
            while (reader.Read())
            {
                Console.Write(reader.TokenType);
                if (reader.Value != null)
                    Console.Write(" - " + reader.Value);

                Console.WriteLine();
            }

            // StartObject
            // PropertyName - Cpu
            // String - Intel
            // PropertyName - Memory
            // Integer - 32
            // PropertyName - Drives
            // StartArray
            // String - DVD
            // String - SSD
            // EndArray
            // EndObject
            #endregion
        }
    }
}