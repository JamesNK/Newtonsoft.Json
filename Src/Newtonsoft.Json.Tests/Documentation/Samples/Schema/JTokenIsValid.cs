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
            JsonSchema schema = JsonSchema.Parse(@"{
              'type': 'object',
              'properties': {
                'name': {'type':'string'},
                'hobbies': {
                  'type': 'array',
                  'items': {'type':'string'}
                }
              }
            }");

            JObject person = JObject.Parse(@"{
              'name': 'James',
              'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
            }");

            IList<string> errorMessages;
            bool valid = person.IsValid(schema, out errorMessages);

            Console.WriteLine(valid);
            // true
            #endregion
        }
    }
}