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
using System.Globalization;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Schema;

namespace Newtonsoft.Json.Tests.Schema
{
  public class JsonSchemaGeneratorTests : TestFixtureBase
  {
    [Test]
    public void Generate_GenericDictionary()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof (Dictionary<string, List<string>>));

      string json = schema.ToString();

      Assert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""array"",
    ""items"": {
      ""type"": ""string""
    }
  }
}", json);
    }
    
#if !PocketPC
    [Test]
    public void Generate_DefaultValueAttributeTestClass()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(DefaultValueAttributeTestClass));

      string json = schema.ToString();

      Assert.AreEqual(@"{
  ""description"": ""DefaultValueAttributeTestClass description!"",
  ""type"": ""object"",
  ""properties"": {
    ""TestField1"": {
      ""type"": ""integer"",
      ""default"": 21
    },
    ""TestProperty1"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""TestProperty1Value""
    }
  }
}", json);
    }
#endif

    [Test]
    public void Generate_Person()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(Person));

      string json = schema.ToString();

      Assert.AreEqual(@"{
  ""id"": ""Person"",
  ""title"": ""Title!"",
  ""description"": ""JsonObjectAttribute description!"",
  ""type"": ""object"",
  ""properties"": {
    ""Name"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""BirthDate"": {
      ""type"": ""string""
    },
    ""LastModified"": {
      ""type"": ""string""
    }
  }
}", json);
    }

    [Test]
    public void Generate_UserNullable()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(UserNullable));

      string json = schema.ToString();

      Assert.AreEqual(@"{
  ""type"": ""object"",
  ""properties"": {
    ""Id"": {
      ""type"": ""string""
    },
    ""FName"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""LName"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""RoleId"": {
      ""type"": ""integer""
    },
    ""NullableRoleId"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""NullRoleId"": {
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""Active"": {
      ""type"": [
        ""boolean"",
        ""null""
      ]
    }
  }
}", json);
    }

    [Test]
    public void Generate_RequiredMembersClass()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(RequiredMembersClass));

      Assert.AreEqual(JsonSchemaType.String, schema.Properties["FirstName"].Type);
      Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["MiddleName"].Type);
      Assert.AreEqual(JsonSchemaType.String, schema.Properties["LastName"].Type);
      Assert.AreEqual(JsonSchemaType.String, schema.Properties["BirthDate"].Type);
    }

    [Test]
    public void Generate_Store()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(Store));

      Assert.AreEqual(11, schema.Properties.Count);

      JsonSchema productArraySchema = schema.Properties["product"];
      JsonSchema productSchema = productArraySchema.Items[0];

      Assert.AreEqual(4, productSchema.Properties.Count);
    }

    [Test]
    public void MissingSchemaIdHandlingTest()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();

      JsonSchema schema = generator.Generate(typeof(Store));
      Assert.AreEqual(null, schema.Id);

      generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
      schema = generator.Generate(typeof (Store));
      Assert.AreEqual(typeof(Store).FullName, schema.Id);

      generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
      schema = generator.Generate(typeof(Store));
      Assert.AreEqual(typeof(Store).AssemblyQualifiedName, schema.Id);
    }

    [Test]
    public void Generate_NumberFormatInfo()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      JsonSchema schema = generator.Generate(typeof(NumberFormatInfo));

      string json = schema.ToString();

      Console.WriteLine(json);

      //      Assert.AreEqual(@"{
      //  ""type"": ""object"",
      //  ""additionalProperties"": {
      //    ""type"": ""array"",
      //    ""items"": {
      //      ""type"": ""string""
      //    }
      //  }
      //}", json);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = @"Unresolved circular reference for type 'Newtonsoft.Json.Tests.TestObjects.CircularReferenceClass'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.")]
    public void CircularReferenceError()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      generator.Generate(typeof(CircularReferenceClass));
    }

    [Test]
    public void CircularReferenceWithTypeNameId()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

      JsonSchema schema = generator.Generate(typeof(CircularReferenceClass), true);

      Assert.AreEqual(JsonSchemaType.String, schema.Properties["Name"].Type);
      Assert.AreEqual(typeof(CircularReferenceClass).FullName, schema.Id);
      Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
      Assert.AreEqual(schema, schema.Properties["Child"]);
    }

    [Test]
    public void CircularReferenceWithExplicitId()
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();

      JsonSchema schema = generator.Generate(typeof(CircularReferenceWithIdClass));

      Assert.AreEqual(JsonSchemaType.String, schema.Properties["Name"].Type);
      Assert.AreEqual("MyExplicitId", schema.Id);
      Assert.AreEqual(JsonSchemaType.Object, schema.Properties["Child"].Type);
      Assert.AreEqual(schema, schema.Properties["Child"]);
    }
  }
}
