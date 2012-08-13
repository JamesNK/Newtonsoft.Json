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

#if !(NET35 || NET20 || PORTABLE)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using File = System.IO.File;

namespace Newtonsoft.Json.Tests.Documentation
{
  public class JsonSchemaTests
  {
    public void IsValidBasic()
    {
      #region IsValidBasic
      string schemaJson = @"{
        'description': 'A person',
        'type': 'object',
        'properties':
        {
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
      // true
      #endregion
    }

    public void IsValidMessages()
    {
      string schemaJson = @"{
        'description': 'A person',
        'type': 'object',
        'properties':
        {
          'name': {'type':'string'},
          'hobbies': {
            'type': 'array',
            'items': {'type':'string'}
          }
        }
      }";

      #region IsValidMessages
      JsonSchema schema = JsonSchema.Parse(schemaJson);

      JObject person = JObject.Parse(@"{
        'name': null,
        'hobbies': ['Invalid content', 0.123456789]
      }");

      IList<string> messages;
      bool valid = person.IsValid(schema, out messages);
      // false
      // Invalid type. Expected String but got Null. Line 2, position 21.
      // Invalid type. Expected String but got Float. Line 3, position 51.
      #endregion
    }

    public void JsonValidatingReader()
    {
      string schemaJson = "{}";

      #region JsonValidatingReader
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
      #endregion
    }

    public void LoadJsonSchema()
    {
      #region LoadJsonSchema
      // load from a string
      JsonSchema schema1 = JsonSchema.Parse(@"{'type':'object'}");

      // load from a file
      using (TextReader reader = File.OpenText(@"c:\schema\Person.json"))
      {
        JsonSchema schema2 = JsonSchema.Read(new JsonTextReader(reader));

        // do stuff
      }
      #endregion
    }

    public void ManuallyCreateJsonSchema()
    {
      #region ManuallyCreateJsonSchema
      JsonSchema schema = new JsonSchema();
      schema.Type = JsonSchemaType.Object;
      schema.Properties = new Dictionary<string, JsonSchema>
        {
          {"name", new JsonSchema {Type = JsonSchemaType.String}},
          {
            "hobbies", new JsonSchema
              {
                Type = JsonSchemaType.Array,
                Items = new List<JsonSchema> { new JsonSchema {Type = JsonSchemaType.String} }
              }
          },
        };

      JObject person = JObject.Parse(@"{
        'name': 'James',
        'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
      }");

      bool valid = person.IsValid(schema);
      // true
      #endregion

      Assert.IsTrue(valid);
    }
  }
}
#endif