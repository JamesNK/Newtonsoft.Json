using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
  public class SaveJsonSchemaToFile
  {
    public void Example()
    {
      #region Usage
      JsonSchema schema = new JsonSchema
        {
          Type = JsonSchemaType.Object
        };

      // serialize JsonSchema to a string and then write string to a file
      File.WriteAllText(@"c:\schema.json", schema.ToString());

      // serialize JsonSchema directly to a file
      using (StreamWriter file = File.CreateText(@"c:\schema.json"))
      using (JsonTextWriter writer = new JsonTextWriter(file))
      {
        schema.WriteTo(writer);
      }
      #endregion
    }
  }
}