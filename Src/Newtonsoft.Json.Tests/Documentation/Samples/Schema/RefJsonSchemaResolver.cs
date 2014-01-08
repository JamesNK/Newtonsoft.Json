using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Schema
{
    public class RefJsonSchemaResolver
    {
        public void Example()
        {
            #region Usage
            string schemaJson;
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            schemaJson = @"{
              'id': 'person',
              'type': 'object',
              'properties': {
                'name': {'type':'string'},
                'age': {'type':'integer'}
              }
            }";

            JsonSchema personSchema = JsonSchema.Parse(schemaJson, resolver);

            schemaJson = @"{
              'id': 'employee',
              'type': 'object',
              'extends': {'$ref':'person'},
              'properties': {
                'salary': {'type':'number'},
                'jobTitle': {'type':'string'}
              }
            }";

            JsonSchema employeeSchema = JsonSchema.Parse(schemaJson, resolver);

            string json = @"{
              'name': 'James',
              'age': 29,
              'salary': 9000.01,
              'jobTitle': 'Junior Vice President'
            }";

            JObject employee = JObject.Parse(json);

            bool valid = employee.IsValid(employeeSchema);

            Console.WriteLine(valid);
            // true
            #endregion
        }
    }
}