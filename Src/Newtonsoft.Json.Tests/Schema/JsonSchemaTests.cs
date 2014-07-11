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
        public void AllOf()
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
  ""allOf"":[{""$ref"":""first""}],
  ""additionalProperties"":{""type"":""string""}
}";

            JsonSchema second = JsonSchema.Parse(json, resolver);
            Assert.AreEqual(first, second.AllOf[0]);

            json =
                @"{
  ""id"":""third"",
  ""type"":""object"",
  ""allOf"":[{""$ref"":""second""}],
  ""additionalProperties"":false
}";

            JsonSchema third = JsonSchema.Parse(json, resolver);
            Assert.AreEqual(second, third.AllOf[0]);
            Assert.AreEqual(first, third.AllOf[0].AllOf[0]);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            third.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            StringAssert.AreEqual(@"{
  ""id"": ""third"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""allOf"": [
    {
      ""$ref"": ""second""
    }
  ]
}", writtenJson);

            StringWriter writer1 = new StringWriter();
            JsonTextWriter jsonWriter1 = new JsonTextWriter(writer1);
            jsonWriter1.Formatting = Formatting.Indented;

            third.WriteTo(jsonWriter1);

            writtenJson = writer1.ToString();
            StringAssert.AreEqual(@"{
  ""id"": ""third"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""allOf"": [
    {
      ""id"": ""second"",
      ""type"": ""object"",
      ""additionalProperties"": {
        ""type"": ""string""
      },
      ""allOf"": [
        {
          ""id"": ""first"",
          ""type"": ""object"",
          ""additionalProperties"": {}
        }
      ]
    }
  ]
}", writtenJson);
        }

        [Test]
        public void AllOf_Multiple()
        {
            string json = @"{
  ""type"":""object"",
  ""allOf"":[{""type"":""string""}],
  ""additionalProperties"":{""type"":""string""}
}";

            JsonSchema s = JsonSchema.Parse(json);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            string newJson = s.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""allOf"": [
    {
      ""type"": ""string""
    }
  ]
}", newJson);


            json = @"{
  ""type"":""object"",
  ""allOf"":[{""type"":""string""}],
  ""additionalProperties"":{""type"":""string""}
}";

            s = JsonSchema.Parse(json);

            writer = new StringWriter();
            jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            newJson = s.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""allOf"": [
    {
      ""type"": ""string""
    }
  ]
}", newJson);


            json = @"{
  ""type"":""object"",
  ""allOf"":[{""type"":""string""},{""type"":""object""}],
  ""additionalProperties"":{""type"":""string""}
}";

            s = JsonSchema.Parse(json);

            writer = new StringWriter();
            jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            newJson = s.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""allOf"": [
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
        public void WriteTo_AnyOf()
        {
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            string json =
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""anyOf"": [
    {
      ""type"": ""number""
    }
  ]
}";

            JsonSchema schema = JsonSchema.Parse(json, resolver);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            Assert.AreEqual(json, writtenJson);
        }

        [Test]
        public void WriteTo_OneOf()
        {
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            string json =
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""oneOf"": [
    {
      ""type"": ""number""
    }
  ]
}";

            JsonSchema schema = JsonSchema.Parse(json, resolver);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            Assert.AreEqual(json, writtenJson);
        }

        [Test]
        public void WriteTo_Not()
        {
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            string json =
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""not"": {
    ""type"": ""number""
  }
}";

            JsonSchema schema = JsonSchema.Parse(json, resolver);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            Assert.AreEqual(json, writtenJson);
        }

        [Test]
        public void WriteTo_Dependencies()
        {
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            string json =
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""dependencies"": {
    ""bar"": {
      ""properties"": {
        ""foo"": {
          ""type"": ""integer""
        },
        ""bar"": {
          ""type"": ""integer""
        }
      }
    }
  }
}";

            JsonSchema schema = JsonSchema.Parse(json, resolver);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            Assert.AreEqual(json, writtenJson);
        }

        [Test]
        public void WriteTo_Required()
        {
            JsonSchemaResolver resolver = new JsonSchemaResolver();

            string json =
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""required"": [
    ""firstName"",
    ""lastName""
  ]
}";

            JsonSchema schema = JsonSchema.Parse(json, resolver);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter, resolver);

            string writtenJson = writer.ToString();
            Assert.AreEqual(json, writtenJson);
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""id"": ""CircularReferenceArray"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""description"": ""CircularReference"",
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""CircularReferenceArray""
  }
}", writtenJson);
        }

        [Test]
        public void WriteTo_NotMultiple()
        {
            JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""not"":{""type"":[""string"",""object"",""array""]}
}");

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter);

            string json = writer.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""not"": {
    ""type"": [
      ""string"",
      ""object"",
      ""array""
    ]
  }
}", json);
        }

        [Test]
        public void WriteTo_NotSingle()
        {
            JsonSchema schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""not"": {""type"": ""any""}
}");

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter);

            string json = writer.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""not"": {
    ""type"": ""any""
  }
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""exclusiveMinimum"": true,
  ""exclusiveMaximum"": true
}", json);
        }

        [Test]
        public void WriteTo_MinimumProperties_MaximumProperties()
        {
            JsonSchema schema = new JsonSchema();
            schema.MinimumProperties = 10;
            schema.MaximumProperties = 20;

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter);

            string json = writer.ToString();

            Assert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""minProperties"": 10,
  ""maxProperties"": 20
}", json);
        }

        [Test]
        public void WriteTo_MultipleOf()
        {
            JsonSchema schema = new JsonSchema();
            schema.MultipleOf = 2.5;

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter);

            string json = writer.ToString();

            Assert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""multipleOf"": 2.5
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""items"": []
}", json);
        }

        [Test]
        public void WriteTo_PositionalItemsValidation_TrueWithItemsSchema()
        {
            JsonSchema schema = new JsonSchema();
            schema.PositionalItemsValidation = true;
            schema.Items = new List<JsonSchema> { new JsonSchema { Type = JsonSchemaType.String } };

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;

            schema.WriteTo(jsonWriter);

            string json = writer.ToString();

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
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

            StringAssert.AreEqual(@"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""items"": {
    ""type"": ""string""
  }
}", json);
        }

        [Test]
        public void IntegerValidatesAgainstFloatFlags()
        {
            JsonSchema schema = JsonSchema.Parse(@"{
  ""type"": ""object"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""properties"": {
  ""NumberProperty"": {
    ""type"": [
        ""number"",
        ""null""
      ]
    }
  }
}");

            JObject json = JObject.Parse(@"{
        ""NumberProperty"": 23
      }");

            Assert.IsTrue(json.IsValid(schema));
        }

        [Test]
        public void DeepConditionalSchema()
        {
            JsonSchema schema = JsonSchema.Parse(@"{
  ""type"": ""object"",
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""properties"": {
    ""Level1"": {
      ""properties"": {
        ""Level2"": {
          ""required"": [""Level3""]
        }
      }
    }
  },
  ""not"": {
    ""properties"": {
      ""Level1"": {
        ""properties"": {
          ""Level2"": {
            ""properties"": {
              ""Level3"": {
                ""type"": ""number""
              }
            }
          }
        }
      }
    }
  }
}");

            JObject json = JObject.Parse(@"{
        ""Level1"": {
            ""Level2"": {
                ""Level3"": ""abc""
            }
        }
      }");

            Assert.IsTrue(json.IsValid(schema));
        }

        [Test]
        public void WriteTo_Links()
        {
            string json = @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""title"": ""News post"",
  ""links"": [
    {
      ""rel"": ""comments"",
      ""href"": ""/{id}/comments""
    },
    {
      ""rel"": ""search"",
      ""href"": ""/{id}/comments"",
      ""schema"": {
        ""type"": ""object"",
        ""properties"": {
          ""searchTerm"": {
            ""type"": ""string""
          },
          ""itemsPerPage"": {
            ""type"": ""integer"",
            ""minimum"": 10.0,
            ""multipleOf"": 10.0,
            ""default"": 20
          }
        },
        ""required"": [
          ""searchTerm""
        ]
      }
    },
    {
      ""title"": ""Post a comment"",
      ""rel"": ""create"",
      ""href"": ""/{id}/comments"",
      ""method"": ""POST"",
      ""schema"": {
        ""type"": ""object"",
        ""properties"": {
          ""message"": {
            ""type"": ""string""
          }
        },
        ""required"": [
          ""message""
        ]
      }
    },
    {
      ""rel"": ""icon"",
      ""href"": ""{id}/icon"",
      ""mediaType"": ""image/*""
    }
  ]
}";

            JsonSchema schema = JsonSchema.Parse(json);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;
            schema.WriteTo(jsonWriter);
            string newJson = writer.ToString();

            Assert.AreEqual(json, newJson);
        }

        [Test]
        public void WriteTo_Media()
        {
            string json = @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""string"",
  ""media"": {
    ""binaryEncoding"": ""base64"",
    ""type"": ""image/png""
  }
}";

            JsonSchema schema = JsonSchema.Parse(json);

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Formatting = Formatting.Indented;
            schema.WriteTo(jsonWriter);
            string newJson = writer.ToString();

            Assert.AreEqual(json, newJson);
        }
    }
}