using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
  public class JTokenIsValid
  {
    public void Example()
    {
      #region Usage
      string schemaJson = @"{
        'description': 'A person',
        'type': 'object',
        'properties': {
          'name': {'type':'string'},
          'hobbies': {
            'type': 'array',
            'items': {'type':'string'}
          }
        }
      }";

      JsonSchema schema = JsonSchema.Parse(schemaJson);

      JObject person = JObject.Parse(@"{
        'name': 'James',
        'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
      }");

      bool valid = person.IsValid(schema);

      Console.WriteLine(valid);
      // true
      #endregion
    }
  }
}