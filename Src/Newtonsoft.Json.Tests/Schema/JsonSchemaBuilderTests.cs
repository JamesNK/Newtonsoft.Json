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
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Schema;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Schema
{
    [TestFixture]
    public class JsonSchemaBuilderTests : TestFixtureBase
    {
        [Test]
        public void Simple()
        {
            string json = @"
{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"":
  {
    ""name"": {""type"":""string""},
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {""type"":""string""}
    }
  }
}
";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("A person", schema.Description);
            Assert.AreEqual(JsonSchemaType.Object, schema.Type);

            Assert.AreEqual(2, schema.Properties.Count);

            Assert.AreEqual(JsonSchemaType.String, schema.Properties["name"].Type);
            Assert.AreEqual(JsonSchemaType.Array, schema.Properties["hobbies"].Type);
            Assert.AreEqual(JsonSchemaType.String, schema.Properties["hobbies"].Items[0].Type);
        }

        [Test]
        public void MultipleTypes()
        {
            string json = @"{
  ""description"":""Age"",
  ""type"":[""string"", ""integer""]
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Age", schema.Description);
            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Integer, schema.Type);
        }

        [Test]
        public void MultipleItems()
        {
            string json = @"{
  ""description"":""MultipleItems"",
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""array""}]
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("MultipleItems", schema.Description);
            Assert.AreEqual(JsonSchemaType.String, schema.Items[0].Type);
            Assert.AreEqual(JsonSchemaType.Array, schema.Items[1].Type);
        }

        [Test]
        public void AdditionalProperties()
        {
            string json = @"{
  ""description"":""AdditionalProperties"",
  ""type"":[""string"", ""integer""],
  ""additionalProperties"":{""type"":[""object"", ""boolean""]}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("AdditionalProperties", schema.Description);
            Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Boolean, schema.AdditionalProperties.Type);
        }

        [Test]
        public void Required()
        {
            string json = @"{
  ""description"":""Required"",
  ""properties"":
  {
    ""name"": {""type"":""string""}
  },
  ""required"":[""name""]
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Required", schema.Description);
            Assert.AreEqual("name", schema.Required[0]);
        }

        [Test]
        public void ExclusiveMinimum_ExclusiveMaximum()
        {
            string json = @"{
  ""exclusiveMinimum"":true,
  ""exclusiveMaximum"":true
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(true, schema.ExclusiveMinimum);
            Assert.AreEqual(true, schema.ExclusiveMaximum);
        }

        [Test]
        public void ReadOnly()
        {
            string json = @"{
  ""description"":""ReadOnly"",
  ""readonly"":true
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("ReadOnly", schema.Description);
            Assert.AreEqual(true, schema.ReadOnly);
        }

        [Test]
        public void Hidden()
        {
            string json = @"{
  ""description"":""Hidden"",
  ""hidden"":true
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Hidden", schema.Description);
            Assert.AreEqual(true, schema.Hidden);
        }

        [Test]
        public void Id()
        {
            string json = @"{
  ""description"":""Id"",
  ""id"":""testid""
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Id", schema.Description);
            Assert.AreEqual("testid", schema.Id);
        }

        [Test]
        public void Title()
        {
            string json = @"{
  ""description"":""Title"",
  ""title"":""testtitle""
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Title", schema.Description);
            Assert.AreEqual("testtitle", schema.Title);
        }

        [Test]
        public void Pattern()
        {
            string json = @"{
  ""description"":""Pattern"",
  ""pattern"":""testpattern""
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Pattern", schema.Description);
            Assert.AreEqual("testpattern", schema.Pattern);
        }

        [Test]
        public void Format()
        {
            string json = @"{
  ""description"":""Format"",
  ""format"":""testformat""
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Format", schema.Description);
            Assert.AreEqual("testformat", schema.Format);
        }

        [Test]
        public void Dependencies()
        {
            string json = @"{
  ""description"":""Dependencies"",
  ""dependencies"":{""PurpleMonkeyDishwasher"":""a""}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Dependencies", schema.Description);
            Assert.IsTrue(schema.Dependencies.ContainsKey("PurpleMonkeyDishwasher"));
        }

        [Test]
        public void MinimumMaximum()
        {
            string json = @"{
  ""description"":""MinimumMaximum"",
  ""minimum"":1.1,
  ""maximum"":1.2,
  ""minItems"":1,
  ""maxItems"":2,
  ""minLength"":5,
  ""maxLength"":50,
  ""multipleOf"":3,
  ""minProperties"": 10,
  ""maxProperties"": 20
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("MinimumMaximum", schema.Description);
            Assert.AreEqual(1.1, schema.Minimum);
            Assert.AreEqual(1.2, schema.Maximum);
            Assert.AreEqual(1, schema.MinimumItems);
            Assert.AreEqual(2, schema.MaximumItems);
            Assert.AreEqual(5, schema.MinimumLength);
            Assert.AreEqual(50, schema.MaximumLength);
            Assert.AreEqual(3, schema.MultipleOf);
            Assert.AreEqual(10, schema.MinimumProperties);
            Assert.AreEqual(20, schema.MaximumProperties);
        }

        [Test]
        public void NotOfSingleType()
        {
            string json = @"{
  ""description"":""NotSingleType"",
  ""not"":{""type"":""string""}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("NotSingleType", schema.Description);
            Assert.AreEqual(JsonSchemaType.String, schema.NotOf.Type);
        }

        [Test]
        public void NotOfMultipleTypes()
        {
            string json = @"{
  ""description"":""NotMultipleTypes"",
  ""not"":{ ""type"": [""string"",""number""] }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("NotMultipleTypes", schema.Description);
            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Float, schema.NotOf.Type);
        }

        [Test]
        public void DefaultPrimitiveType()
        {
            string json = @"{
  ""description"":""DefaultPrimitiveType"",
  ""default"":1.1
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("DefaultPrimitiveType", schema.Description);
            Assert.AreEqual(1.1, (double)schema.Default);
        }

        [Test]
        public void DefaultComplexType()
        {
            string json = @"{
  ""description"":""DefaultComplexType"",
  ""default"":{""pie"":true}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("DefaultComplexType", schema.Description);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(@"{""pie"":true}"), schema.Default));
        }

        [Test]
        public void Enum()
        {
            string json = @"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""enum"":[""string"",""object"",""array"",""boolean"",""number"",""integer"",""null"",""any""]
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("Type", schema.Description);
            Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Array, schema.Type);

            Assert.AreEqual(8, schema.Enum.Count);
            Assert.AreEqual("string", (string)schema.Enum[0]);
            Assert.AreEqual("any", (string)schema.Enum[schema.Enum.Count - 1]);
        }

        [Test]
        public void CircularReference()
        {
            string json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""CircularReferenceArray""}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("CircularReference", schema.Description);
            Assert.AreEqual("CircularReferenceArray", schema.Id);
            Assert.AreEqual(JsonSchemaType.Array, schema.Type);

            Assert.AreEqual(schema, schema.Items[0]);
        }

        [Test]
        public void UnresolvedReference()
        {
            ExceptionAssert.Throws<Exception>(@"Could not resolve schema reference 'MyUnresolvedReference'.",
                () =>
                {
                    string json = @"{
  ""id"":""CircularReference"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""MyUnresolvedReference""}
}";

                    JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                    JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));
                });
        }

        [Test]
        public void ExternalReference()
        {
            string json = @"{
  ""$ref"": ""http://localhost:1234/subSchemas.json#/refToInteger""
}";
            JsonSchemaSpecTestsResolver resolver = new JsonSchemaSpecTestsResolver();

            resolver.ResolveExternals = false;

            ExceptionAssert.Throws<Exception>(@"Could not resolve schema reference 'http://localhost:1234/subSchemas.json#/refToInteger'.",
            () =>
            {
                JsonSchemaBuilder failBuilder = new JsonSchemaBuilder(resolver);
                JsonSchema failSchema = failBuilder.Read(new JsonTextReader(new StringReader(json)));
            });

#if !PORTABLE40
            resolver.ResolveExternals = true;

            JsonSchemaBuilder builder = new JsonSchemaBuilder(resolver);
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(JsonSchemaType.Integer, schema.Type);
#endif
        }

        [Test]
        public void PatternProperties()
        {
            string json = @"{
  ""patternProperties"": {
    ""[abc]"": { ""id"":""Blah"" }
  }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(schema.PatternProperties);
            Assert.AreEqual(1, schema.PatternProperties.Count);
            Assert.AreEqual("Blah", schema.PatternProperties["[abc]"].Id);
        }

        [Test]
        public void AdditionalItems()
        {
            string json = @"{
    ""items"": [],
    ""additionalItems"": {""type"": ""integer""}
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(schema.AdditionalItems);
            Assert.AreEqual(JsonSchemaType.Integer, schema.AdditionalItems.Type);
            Assert.AreEqual(true, schema.AllowAdditionalItems);
        }

        [Test]
        public void DisallowAdditionalItems()
        {
            string json = @"{
    ""items"": [],
    ""additionalItems"": false
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.IsNull(schema.AdditionalItems);
            Assert.AreEqual(false, schema.AllowAdditionalItems);
        }

        [Test]
        public void AllowAdditionalItems()
        {
            string json = @"{
    ""items"": {},
    ""additionalItems"": false
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.IsNull(schema.AdditionalItems);
            Assert.AreEqual(false, schema.AllowAdditionalItems);
        }

        [Test]
        public void Location()
        {
            string json = @"{
  ""properties"":{
    ""foo"":{
      ""type"":""array"",
      ""items"":[
        {
          ""type"":""integer""
        },
        {
          ""properties"":{
            ""foo"":{
              ""type"":""integer""
            }
          }
        }
      ]
    }
  }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual("#", schema.Location);
            Assert.AreEqual("#/properties/foo", schema.Properties["foo"].Location);
            Assert.AreEqual("#/properties/foo/items/1/properties/foo", schema.Properties["foo"].Items[1].Properties["foo"].Location);
        }

        [Test]
        public void Reference_BackwardsLocation()
        {
            string json = @"{
  ""properties"": {
    ""foo"": {""type"": ""integer""},
    ""bar"": {""$ref"": ""#/properties/foo""}
  }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Test]
        public void Reference_ForwardsLocation()
        {
            string json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/properties/foo""},
    ""foo"": {""type"": ""integer""}
  }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Test]
        public void Reference_NonStandardLocation()
        {
            string json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/common/foo""},
    ""foo"": {""$ref"": ""#/common/foo""}
  },
  ""common"": {
    ""foo"": {""type"": ""integer""}
  }
}";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
        }

        [Test]
        public void EscapedReferences()
        {
            string json = @"{
            ""tilda~field"": {""type"": ""integer""},
            ""slash/field"": {""type"": ""object""},
            ""percent%field"": {""type"": ""array""},
            ""properties"": {
                ""tilda"": {""$ref"": ""#/tilda~0field""},
                ""slash"": {""$ref"": ""#/slash~1field""},
                ""percent"": {""$ref"": ""#/percent%25field""}
            }
        }";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(JsonSchemaType.Integer, schema.Properties["tilda"].Type);
            Assert.AreEqual(JsonSchemaType.Object, schema.Properties["slash"].Type);
            Assert.AreEqual(JsonSchemaType.Array, schema.Properties["percent"].Type);
        }

        [Test]
        public void References_Array()
        {
            string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/1/prop""}
            }
        }";

            JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            JsonSchema schema = builder.Read(new JsonTextReader(new StringReader(json)));

            Assert.AreEqual(JsonSchemaType.Integer, schema.Properties["array"].Type);
            Assert.AreEqual(JsonSchemaType.Object, schema.Properties["arrayprop"].Type);
        }

        [Test]
        public void References_IndexTooBig()
        {
            // JsonException : Could not resolve schema reference '#/array/10'.

            string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/10""}
            }
        }";

            ExceptionAssert.Throws<JsonException>(
                "Could not resolve schema reference '#/array/10'.",
                () =>
                {
                    JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                    builder.Read(new JsonTextReader(new StringReader(json)));
                });
        }

        [Test]
        public void References_IndexNegative()
        {
            string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/-1""}
            }
        }";

            ExceptionAssert.Throws<JsonException>(
                "Could not resolve schema reference '#/array/-1'.",
                () =>
                {
                    JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                    builder.Read(new JsonTextReader(new StringReader(json)));
                });
        }

        [Test]
        public void References_IndexNotInteger()
        {
            string json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/one""}
            }
        }";

            ExceptionAssert.Throws<JsonException>(
                "Could not resolve schema reference '#/array/one'.",
                () =>
                {
                    JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
                    builder.Read(new JsonTextReader(new StringReader(json)));
                });
        }
    }
}