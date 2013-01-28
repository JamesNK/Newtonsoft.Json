using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
  public class JTokenIsValidWithMessages
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
        'name': null,
        'hobbies': ['Invalid content', 0.123456789]
      }");

      IList<string> messages;
      bool valid = person.IsValid(schema, out messages);

      Console.WriteLine(valid);
      // false

      foreach (string message in messages)
      {
        Console.WriteLine(message);
      }
      // Invalid type. Expected String but got Null. Line 2, position 21.
      // Invalid type. Expected String but got Float. Line 3, position 51.
      #endregion
    }
  }
}