#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#pragma warning disable 618
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
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

#pragma warning restore 618