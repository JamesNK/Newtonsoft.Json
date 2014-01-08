using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
    public class LoadJsonSchemaFromFile
    {
        public void Example()
        {
            #region Usage
            // read file into a string and parse JsonSchema from the string
            JsonSchema schema1 = JsonSchema.Parse(File.ReadAllText(@"c:\schema.json"));

            // read JsonSchema directly from a file
            using (StreamReader file = File.OpenText(@"c:\schema.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSchema schema2 = JsonSchema.Read(reader);
            }
            #endregion
        }
    }
}