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
    public class JsonSchemaModelBuilderTests : TestFixtureBase
    {
        [Test]
        public void ExtendedComplex()
        {
            string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string""},
    ""secondproperty"":{""type"":""string"",""maxLength"":10},
    ""thirdproperty"":{
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""type"":""string"",""maxLength"":10,""minLength"":7}
      }
    }
  },
  ""additionalProperties"":{}
}";

            string second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""any""},
    ""thirdproperty"":{
      ""extends"":{
        ""properties"":
        {
          ""thirdproperty_firstproperty"":{""maxLength"":9,""minLength"":6,""pattern"":""hi2u""}
        },
        ""additionalProperties"":{""maxLength"":9,""minLength"":6,""enum"":[""one"",""two""]}
      },
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""pattern"":""hi""}
      },
      ""additionalProperties"":{""type"":""string"",""enum"":[""two"",""three""]}
    },
    ""fourthproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

            JsonSchemaResolver resolver = new JsonSchemaResolver();
            JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
            JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

            JsonSchemaModelBuilder modelBuilder = new JsonSchemaModelBuilder();

            JsonSchemaModel model = modelBuilder.Build(secondSchema);

            Assert.AreEqual(4, model.Properties.Count);

            Assert.AreEqual(JsonSchemaType.String, model.Properties["firstproperty"].Type);

            Assert.AreEqual(JsonSchemaType.String, model.Properties["secondproperty"].Type);
            Assert.AreEqual(10, model.Properties["secondproperty"].MaximumLength);
            Assert.AreEqual(null, model.Properties["secondproperty"].Enum);
            Assert.AreEqual(null, model.Properties["secondproperty"].Patterns);

            Assert.AreEqual(JsonSchemaType.Object, model.Properties["thirdproperty"].Type);
            Assert.AreEqual(3, model.Properties["thirdproperty"].AdditionalProperties.Enum.Count);
            Assert.AreEqual("two", (string)model.Properties["thirdproperty"].AdditionalProperties.Enum[0]);
            Assert.AreEqual("three", (string)model.Properties["thirdproperty"].AdditionalProperties.Enum[1]);
            Assert.AreEqual("one", (string)model.Properties["thirdproperty"].AdditionalProperties.Enum[2]);

            Assert.AreEqual(JsonSchemaType.String, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Type);
            Assert.AreEqual(9, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].MaximumLength);
            Assert.AreEqual(7, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].MinimumLength);
            Assert.AreEqual(2, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns.Count);
            Assert.AreEqual("hi", model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns[0]);
            Assert.AreEqual("hi2u", model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns[1]);
            Assert.AreEqual(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Properties);
            Assert.AreEqual(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Items);
            Assert.AreEqual(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].AdditionalProperties);
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

            JsonSchema schema = JsonSchema.Parse(json);

            JsonSchemaModelBuilder modelBuilder = new JsonSchemaModelBuilder();

            JsonSchemaModel model = modelBuilder.Build(schema);

            Assert.AreEqual(JsonSchemaType.Array, model.Type);

            Assert.AreEqual(model, model.Items[0]);
        }

        [Test]
        public void Required()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string"",required:true},
    ""age"":{""type"":""integer"",required:true}
  }
}";

            JsonSchema schema = JsonSchema.Parse(schemaJson);
            JsonSchemaModelBuilder modelBuilder = new JsonSchemaModelBuilder();
            JsonSchemaModel model = modelBuilder.Build(schema);

            Assert.AreEqual(JsonSchemaType.Object, model.Type);
            Assert.AreEqual(3, model.Properties.Count);
            Assert.AreEqual(false, model.Properties["name"].Required);
            Assert.AreEqual(true, model.Properties["hobbies"].Required);
            Assert.AreEqual(true, model.Properties["age"].Required);
        }
    }
}