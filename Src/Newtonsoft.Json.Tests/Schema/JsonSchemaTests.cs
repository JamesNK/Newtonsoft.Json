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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests.Schema
{
  [TestFixture]
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
      Assert.AreEqual(first, second.Extends[0]);

      json =
        @"{
  ""id"":""third"",
  ""type"":""object"",
  ""extends"":{""$ref"":""second""},
  ""additionalProperties"":false
}";

      JsonSchema third = JsonSchema.Parse(json, resolver);
      Assert.AreEqual(second, third.Extends[0]);
      Assert.AreEqual(first, third.Extends[0].Extends[0]);

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
    public void Extends_Multiple()
    {
      string json = @"{
  ""type"":""object"",
  ""extends"":{""type"":""string""},
  ""additionalProperties"":{""type"":""string""}
}";

      JsonSchema s = JsonSchema.Parse(json);

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      string newJson = s.ToString();

      Assert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": {
    ""type"": ""string""
  }
}", newJson);
      

      json = @"{
  ""type"":""object"",
  ""extends"":[{""type"":""string""}],
  ""additionalProperties"":{""type"":""string""}
}";

      s = JsonSchema.Parse(json);

      writer = new StringWriter();
      jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      newJson = s.ToString();

      Assert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": {
    ""type"": ""string""
  }
}", newJson);


      json = @"{
  ""type"":""object"",
  ""extends"":[{""type"":""string""},{""type"":""object""}],
  ""additionalProperties"":{""type"":""string""}
}";

      s = JsonSchema.Parse(json);

      writer = new StringWriter();
      jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      newJson = s.ToString();

      Assert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": [
    {
      ""type"": ""string""
    },
    {
      ""type"": ""object""
    }
  ]
}", newJson);
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
    public void WriteTo_ExclusiveMinimum_ExclusiveMaximum()
    {
      JsonSchema schema = new JsonSchema();
      schema.ExclusiveMinimum = true;
      schema.ExclusiveMaximum = true;

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""exclusiveMinimum"": true,
  ""exclusiveMaximum"": true
}", json);
    }

    [Test]
    public void WriteTo_PatternProperties()
    {
      JsonSchema schema = new JsonSchema();
      schema.PatternProperties = new Dictionary<string, JsonSchema>
        {
          { "[abc]", new JsonSchema() }
        };

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""patternProperties"": {
    ""[abc]"": {}
  }
}", json);
    }

    [Test]
    public void ToString_AdditionalItems()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
    ""additionalItems"": {""type"": ""integer""}
}");

      string json = schema.ToString();

      Assert.AreEqual(@"{
  ""additionalItems"": {
    ""type"": ""integer""
  }
}", json);
    }

    [Test]
    public void WriteTo_PositionalItemsValidation_True()
    {
      JsonSchema schema = new JsonSchema();
      schema.PositionalItemsValidation = true;

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""items"": []
}", json);
    }

    [Test]
    public void WriteTo_PositionalItemsValidation_TrueWithItemsSchema()
    {
      JsonSchema schema = new JsonSchema();
      schema.PositionalItemsValidation = true;
      schema.Items = new List<JsonSchema> { new JsonSchema { Type = JsonSchemaType.String }};

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""items"": [
    {
      ""type"": ""string""
    }
  ]
}", json);
    }

    [Test]
    public void WriteTo_PositionalItemsValidation_FalseWithItemsSchema()
    {
      JsonSchema schema = new JsonSchema();
      schema.Items = new List<JsonSchema> { new JsonSchema { Type = JsonSchemaType.String } };

      StringWriter writer = new StringWriter();
      JsonTextWriter jsonWriter = new JsonTextWriter(writer);
      jsonWriter.Formatting = Formatting.Indented;

      schema.WriteTo(jsonWriter);

      string json = writer.ToString();

      Assert.AreEqual(@"{
  ""items"": {
    ""type"": ""string""
  }
}", json);
    }
  }
}
