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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Schema
{
  public class JsonSchemaTests : TestFixtureBase
  {
    [Test]
    public void Extends()
    {
      string json;
      JsonSchemaResolver resolver = new JsonSchemaResolver();

      json = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""additionalProperties"":{}
}";

      JsonSchema first = JsonSchema.Parse(json, resolver);

      json =
        @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""additionalProperties"":{""type"":""string""}
}";

      JsonSchema second = JsonSchema.Parse(json, resolver);
      Assert.AreEqual(first, second.Extends);

      json =
        @"{
  ""id"":""third"",
  ""type"":""object"",
  ""extends"":{""$ref"":""second""},
  ""additionalProperties"":false
}";

      JsonSchema third = JsonSchema.Parse(json, resolver);
      Assert.AreEqual(second, third.Extends);
      Assert.AreEqual(first, third.Extends.Extends);

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      third.WriteTo(jsonWriter, resolver);

      string writtenJson = writer.ToString();
      Assert.AreEqual(@"{
  ""id"": ""third"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""extends"": {
    ""$ref"": ""second""
  }
}", writtenJson);

      StringWriter writer1 = new StringWriter();
      JsonTextWriter jsonWriter1 = new JsonTextWriter(writer1);
      jsonWriter1.Formatting = Formatting.Indented;

      third.WriteTo(jsonWriter1);

      writtenJson = writer1.ToString();
      Assert.AreEqual(@"{
  ""id"": ""third"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""extends"": {
    ""id"": ""second"",
    ""type"": ""object"",
    ""additionalProperties"": {
      ""type"": ""string""
    },
    ""extends"": {
      ""id"": ""first"",
      ""type"": ""object"",
      ""additionalProperties"": {}
    }
  }
}", writtenJson);
    }
    [Test]
    public void WriteTo_AdditionalProperties()
    {
      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""AdditionalProperties"",
  ""type"":[""string"", ""integer""],
  ""additionalProperties"":{""type"":[""object"", ""boolean""]}
}");

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""description"": ""AdditionalProperties"",
  ""type"": [
    ""string"",
    ""integer""
  ],
  ""additionalProperties"": {
    ""type"": [
      ""boolean"",
      ""object""
    ]
  }
}", json);
    }

    [Test]
    public void WriteTo_Properties()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":
    {
      ""type"":""array"",
      ""items"": {""type"":""string""}
    }
  }
}");

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"": {
    ""name"": {
      ""type"": ""string""
    },
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""string""
      }
    }
  }
}", json);

    }

    [Test]
    public void WriteTo_Enum()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""enum"":[""string"",""object"",""array"",""boolean"",""number"",""integer"",""null"",""any""]
}");

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""enum"": [
    ""string"",
    ""object"",
    ""array"",
    ""boolean"",
    ""number"",
    ""integer"",
    ""null"",
    ""any""
  ]
}", json);
    }

    [Test]
    public void WriteTo_CircularReference()
    {
      string json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""CircularReferenceArray""}
}";

      JsonSchema schema = JsonSchema.Parse(json);

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string writtenJson = writer.ToString();

      Assert.AreEqual(@"{
  ""id"": ""CircularReferenceArray"",
  ""description"": ""CircularReference"",
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""CircularReferenceArray""
  }
}", writtenJson);
    }

    [Test]
    public void WriteTo_DisallowMultiple()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""disallow"":[""string"",""object"",""array""]
}");

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""disallow"": [
    ""string"",
    ""object"",
    ""array""
  ]
}", json);
    }

    [Test]
    public void WriteTo_DisallowSingle()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""disallow"":""any""
}");

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""disallow"": ""any""
}", json);
    }

    [Test]
    public void WriteTo_MultipleItems()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""items"":[{},{}]
}");

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""items"": [
    {},
    {}
  ]
}", json);
    }

    [Test]
    public void ReadOptions()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""type"": ""object"",
  ""properties"": {
    ""x"": {
      ""type"": ""integer"",
      ""enum"": [
        0,
        1,
        -1
      ],
      ""options"": [
        {
          ""value"": 0,
          ""label"": ""No""
        },
        {
          ""value"": 1,
          ""label"": ""Asc""
        },
        {
          ""value"": -1,
          ""label"": ""Desc""
        }
      ]
    }
  }
}");

      Assert.AreEqual(schema.Properties["x"].Options.Count, 3);
      Assert.AreEqual(schema.Properties["x"].Options[0], "No");
      Assert.AreEqual(schema.Properties["x"].Options[1], "Asc");
      Assert.AreEqual(schema.Properties["x"].Options[-1], "Desc");
    }
  }
}
