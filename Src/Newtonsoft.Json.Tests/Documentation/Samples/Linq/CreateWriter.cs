using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class CreateWriter
    {
        public void Example()
        {
            #region Usage
            JObject o = new JObject
            {
                { "name1", "value1" },
                { "name2", "value2" }
            };

            JsonWriter writer = o.CreateWriter();
            writer.WritePropertyName("name3");
            writer.WriteStartArray();
            writer.WriteValue(1);
            writer.WriteValue(2);
            writer.WriteEndArray();

            Console.WriteLine(o.ToString());
            // {
            //   "name1": "value1",
            //   "name2": "value2",
            //   "name3": [
            //     1,
            //     2
            //   ]
            // }
            #endregion
        }
    }
}