using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Schema
{
  public class JsonSchemaLoad
  {
    public void Example()
    {
      string schemaJson = @"{'type': 'object'}";

      JsonSchema schema;
      using (TextReader file = File.OpenText(@"c:\schema.json"))
      using (JsonTextReader reader = new JsonTextReader(file))
      {
        schema = JsonSchema.Read(reader);
      }

      JObject o = JObject.Parse(@"{}");

      bool valid = o.IsValid(schema);

      Console.WriteLine(valid);
      // true
    }
  }
}