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
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Schema
{
    [TestFixture]
    public class JsonSchemaNodeTests : TestFixtureBase
    {
        [Test]
        public void AddSchema()
        {
            string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string"",""maxLength"":10},
    ""secondproperty"":{
      ""type"":""object"",
      ""properties"":
      {
        ""secondproperty_firstproperty"":{""type"":""string"",""maxLength"":10,""minLength"":7}
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
    ""firstproperty"":{""type"":""string""},
    ""secondproperty"":{
      ""extends"":{
        ""properties"":
        {
          ""secondproperty_firstproperty"":{""maxLength"":9,""minLength"":6}
        }
      },
      ""type"":""object"",
      ""properties"":
      {
        ""secondproperty_firstproperty"":{}
      }
    },
    ""thirdproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

            JsonSchemaResolver resolver = new JsonSchemaResolver();
            JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
            JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

            JsonSchemaModelBuilder modelBuilder = new JsonSchemaModelBuilder();

            JsonSchemaNode node = modelBuilder.AddSchema(null, secondSchema);

            Assert.AreEqual(2, node.Schemas.Count);
            Assert.AreEqual(2, node.Properties["firstproperty"].Schemas.Count);
            Assert.AreEqual(3, node.Properties["secondproperty"].Schemas.Count);
            Assert.AreEqual(3, node.Properties["secondproperty"].Properties["secondproperty_firstproperty"].Schemas.Count);
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

            JsonSchemaNode node = modelBuilder.AddSchema(null, schema);

            Assert.AreEqual(1, node.Schemas.Count);

            Assert.AreEqual(node, node.Items[0]);
        }
    }
}