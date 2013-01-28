using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
  public class JsonValidatingReaderAndSerializer
  {
    #region Types
    public class Person
    {
      public string Name { get; set; }
      public IList<string> Hobbies { get; set; }
    }
    #endregion

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

      string json = @"{
        'name': 'James',
        'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
      }";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      JsonValidatingReader validatingReader = new JsonValidatingReader(reader);
      validatingReader.Schema = JsonSchema.Parse(schemaJson);

      IList<string> messages = new List<string>();
      validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

      JsonSerializer serializer = new JsonSerializer();
      Person p = serializer.Deserialize<Person>(validatingReader);

      Console.WriteLine(p.Name);
      // James

      bool isValid = (messages.Count == 0);

      Console.WriteLine(isValid);
      // true
      #endregion
    }
  }
}