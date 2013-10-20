using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    public class WriteJTokenToBson
    {
        public void Example()
        {
            #region Usage
            JObject o = new JObject
            {
                { "name1", "value1" },
                { "name2", "value2" }
            };

            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                o.WriteTo(writer);
            }

            string data = Convert.ToBase64String(ms.ToArray());

            Console.WriteLine(data);
            // KQAAAAJuYW1lMQAHAAAAdmFsdWUxAAJuYW1lMgAHAAAAdmFsdWUyAAA=
            #endregion
        }
    }
}