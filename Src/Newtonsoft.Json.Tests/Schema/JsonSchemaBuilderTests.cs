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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Schema;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Schema
{
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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("AdditionalProperties", schema.Description);
      Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Boolean, schema.AdditionalProperties.Type);
    }

    [Test]
    public void Optional()
    {
      string json = @"{
  ""description"":""Optional"",
  ""optional"":true
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("Optional", schema.Description);
      Assert.AreEqual(true, schema.Optional);
    }

    [Test]
    public void ReadOnly()
    {
      string json = @"{
  ""description"":""ReadOnly"",
  ""readonly"":true
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("Format", schema.Description);
      Assert.AreEqual("testformat", schema.Format);
    }

    [Test]
    public void Requires()
    {
      string json = @"{
  ""description"":""Requires"",
  ""requires"":""PurpleMonkeyDishwasher""
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("Requires", schema.Description);
      Assert.AreEqual("PurpleMonkeyDishwasher", schema.Requires);
    }

    [Test]
    public void IdentitySingle()
    {
      string json = @"{
  ""description"":""Identity"",
  ""identity"":""PurpleMonkeyDishwasher""
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("Identity", schema.Description);
      Assert.AreEqual(1, schema.Identity.Count);
      Assert.AreEqual("PurpleMonkeyDishwasher", schema.Identity[0]);
    }

    [Test]
    public void IdentityMultiple()
    {
      string json = @"{
  ""description"":""Identity"",
  ""identity"":[""PurpleMonkeyDishwasher"",""Antelope""]
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("Identity", schema.Description);
      Assert.AreEqual(2, schema.Identity.Count);
      Assert.AreEqual("PurpleMonkeyDishwasher", schema.Identity[0]);
      Assert.AreEqual("Antelope", schema.Identity[1]);
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
  ""maxDecimal"":3,
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("MinimumMaximum", schema.Description);
      Assert.AreEqual(1.1, schema.Minimum);
      Assert.AreEqual(1.2, schema.Maximum);
      Assert.AreEqual(1, schema.MinimumItems);
      Assert.AreEqual(2, schema.MaximumItems);
      Assert.AreEqual(5, schema.MinimumLength);
      Assert.AreEqual(50, schema.MaximumLength);
      Assert.AreEqual(3, schema.MaximumDecimals);
    }

    [Test]
    public void DisallowSingleType()
    {
      string json = @"{
  ""description"":""DisallowSingleType"",
  ""disallow"":""string""
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("DisallowSingleType", schema.Description);
      Assert.AreEqual(JsonSchemaType.String, schema.Disallow);
    }

    [Test]
    public void DisallowMultipleTypes()
    {
      string json = @"{
  ""description"":""DisallowMultipleTypes"",
  ""disallow"":[""string"",""number""]
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("DisallowMultipleTypes", schema.Description);
      Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Float, schema.Disallow);
    }

    [Test]
    public void DefaultPrimitiveType()
    {
      string json = @"{
  ""description"":""DefaultPrimitiveType"",
  ""default"":1.1
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("DefaultComplexType", schema.Description);
      Assert.IsTrue(JToken.DeepEquals(JObject.Parse(@"{""pie"":true}"), schema.Default));
    }

    [Test]
    public void Options()
    {
      string json = @"{
  ""description"":""NZ Island"",
  ""type"":""string"",
  ""options"":
  [
    {""value"":""NI"",""label"":""North Island""},
    {""value"":""SI"",""label"":""South Island""}
  ]
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("NZ Island", schema.Description);
      Assert.AreEqual(JsonSchemaType.String, schema.Type);

      Assert.AreEqual(2, schema.Options.Count);
      Assert.AreEqual("North Island", schema.Options[new JValue("NI")]);
      Assert.AreEqual("South Island", schema.Options[new JValue("SI")]);
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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

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
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));

      Assert.AreEqual("CircularReference", schema.Description);
      Assert.AreEqual("CircularReferenceArray", schema.Id);
      Assert.AreEqual(JsonSchemaType.Array, schema.Type);

      Assert.AreEqual(schema, schema.Items[0]);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Could not resolve schema reference for Id 'MyUnresolvedReference'.")]
    public void UnresolvedReference()
    {
      string json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""MyUnresolvedReference""}
}";

      JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      JsonSchema schema = builder.Parse(new JsonTextReader(new StringReader(json)));
    }
  }
}
