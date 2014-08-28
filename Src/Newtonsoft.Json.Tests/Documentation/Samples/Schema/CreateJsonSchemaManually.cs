using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
    public class CreateJsonSchemaManually
    {
        public void Example()
        {
            #region Usage
            JsonSchema schema = new JsonSchema();
            schema.Type = JsonSchemaType.Object;
            schema.Properties = new Dictionary<string, JsonSchema>
            {
                { "name", new JsonSchema { Type = JsonSchemaType.String } },
                {
                    "hobbies", new JsonSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new List<JsonSchema> { new JsonSchema { Type = JsonSchemaType.String } }
                    }
                },
            };

            string schemaJson = schema.ToString();

            Console.WriteLine(schemaJson);
            // {
            //   "type": "object",
            //   "properties": {
            //     "name": {
            //       "type": "string"
            //     },
            //     "hobbies": {
            //       "type": "array",
            //       "items": {
            //         "type": "string"
            //       }
            //     }
            //   }
            // }

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