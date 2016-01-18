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

#pragma warning restore 618