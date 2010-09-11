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
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json.Tests.TestObjects;
#if !SILVERLIGHT
using System.Data;
#endif

namespace Newtonsoft.Json.Tests.Schema
{
  public class ExtensionsTests : TestFixtureBase
  {
    [Test]
    public void IsValid()
    {
      JsonSchema schema = JsonSchema.Parse("{'type':'integer'}");
      JToken stringToken = JToken.FromObject("pie");
      JToken integerToken = JToken.FromObject(1);

      Assert.AreEqual(false, stringToken.IsValid(schema));
      Assert.AreEqual(true, integerToken.IsValid(schema));
    }

    [Test]
    public void ValidateWithEventHandler()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie lol");

      List<string> errors = new List<string>();
      stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
      Assert.AreEqual(0, errors.Count);

      stringToken = JToken.FromObject("pie");

      stringToken.Validate(schema, (sender, args) => errors.Add(args.Message));
      Assert.AreEqual(1, errors.Count);

      Assert.AreEqual("String 'pie' does not match regex pattern 'lol'.", errors[0]);
    }

    [Test]
    [ExpectedException(typeof(JsonSchemaException), ExpectedMessage = @"String 'pie' does not match regex pattern 'lol'.")]
    public void ValidateWithOutEventHandlerFailure()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie");
      stringToken.Validate(schema);
    }

    [Test]
    public void ValidateWithOutEventHandlerSuccess()
    {
      JsonSchema schema = JsonSchema.Parse("{'pattern':'lol'}");
      JToken stringToken = JToken.FromObject("pie lol");
      stringToken.Validate(schema);
    }

    [Test]
    public void ValidateFailureWithOutLineInfoBecauseOfEndToken()
    {
      JsonSchema schema = JsonSchema.Parse("{'properties':{'lol':{}}}");
      JObject o = JObject.Parse("{}");

      List<string> errors = new List<string>();
      o.Validate(schema, (sender, args) => errors.Add(args.Message));

      Assert.AreEqual("Non-optional properties are missing from object: lol.", errors[0]);
      Assert.AreEqual(1, errors.Count);
    }

    [Test]
    public void ValidateFailureWithLineInfo()
    {
      JsonSchema schema = JsonSchema.Parse("{'properties':{'lol':{'type':'string'}}}");
      JObject o = JObject.Parse("{'lol':1}");

      List<string> errors = new List<string>();
      o.Validate(schema, (sender, args) => errors.Add(args.Message));

      Assert.AreEqual("Invalid type. Expected String but got Integer. Line 1, position 9.", errors[0]);
      Assert.AreEqual(1, errors.Count);
    }

    [Test]
    public void Blog()
    {
      string schemaJson = @"
{
  ""description"": ""A person schema"",
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

      //JsonSchema schema;

      //using (JsonTextReader reader = new JsonTextReader(new StringReader(schemaJson)))
      //{
      //  JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
      //  schema = builder.Parse(reader);
      //}

      JsonSchema schema = JsonSchema.Parse(schemaJson);

      JObject person = JObject.Parse(@"{
        ""name"": ""James"",
        ""hobbies"": ["".NET"", ""Blogging"", ""Reading"", ""Xbox"", ""LOLCATS""]
      }");

      bool valid = person.IsValid(schema);
      // true
    }

    private void GenerateSchemaAndSerializeFromType<T>(T value)
    {
      JsonSchemaGenerator generator = new JsonSchemaGenerator();
      generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
      JsonSchema typeSchema = generator.Generate(typeof (T));
      string schema = typeSchema.ToString();

      string json = JsonConvert.SerializeObject(value, Formatting.Indented);
      JToken token = JToken.ReadFrom(new JsonTextReader(new StringReader(json)));

      List<string> errors = new List<string>();

      token.Validate(typeSchema, (sender, args) =>
                                   {
                                     errors.Add(args.Message);
                                   });

      if (errors.Count > 0)
        Assert.Fail("Schema generated for type '{0}' is not valid." + Environment.NewLine + string.Join(Environment.NewLine, errors.ToArray()), typeof(T));
    }

    [Test]
    public void GenerateSchemaAndSerializeFromTypeTests()
    {
      GenerateSchemaAndSerializeFromType(new List<string> { "1", "Two", "III" });
      GenerateSchemaAndSerializeFromType(new List<int> { 1 });
      GenerateSchemaAndSerializeFromType(new Version("1.2.3.4"));
      GenerateSchemaAndSerializeFromType(new Store());
      GenerateSchemaAndSerializeFromType(new Person());
      GenerateSchemaAndSerializeFromType(new PersonRaw());
      GenerateSchemaAndSerializeFromType(new CircularReferenceClass() { Name = "I'm required" });
      GenerateSchemaAndSerializeFromType(new CircularReferenceWithIdClass());
      GenerateSchemaAndSerializeFromType(new ClassWithArray());
      GenerateSchemaAndSerializeFromType(new ClassWithGuid());
#if !NET20 && !PocketPC
      GenerateSchemaAndSerializeFromType(new NullableDateTimeTestClass());
#endif
#if !SILVERLIGHT
      GenerateSchemaAndSerializeFromType(new DataSet());
#endif
      GenerateSchemaAndSerializeFromType(new object());
      GenerateSchemaAndSerializeFromType(1);
      GenerateSchemaAndSerializeFromType("Hi");
      GenerateSchemaAndSerializeFromType(new DateTime(2000, 12, 29, 23, 59, 0, DateTimeKind.Utc));
      GenerateSchemaAndSerializeFromType(TimeSpan.FromTicks(1000000));
      GenerateSchemaAndSerializeFromType(DBNull.Value);
      GenerateSchemaAndSerializeFromType(new JsonPropertyWithHandlingValues());
    }

    [Test]
    public void UndefinedPropertyOnNoPropertySchema()
    {
      JsonSchema schema = JsonSchema.Parse(@"{
  ""description"": ""test"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
  }
}");

      JObject o = JObject.Parse("{'g':1}");

      List<string> errors = new List<string>();
      o.Validate(schema, (sender, args) => errors.Add(args.Message));

      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual("Property 'g' has not been defined and the schema does not allow additional properties. Line 1, position 5.", errors[0]);
    }
  }
}