using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json.Schema;

namespace Newtonsoft.Json.Tests
{
  public class JsonValidatingReaderTests : TestFixtureBase
  {
    [Test]
    public void CheckInnerReader()
    {
      string json = "{'name':'James','hobbies':['pie','cake']}";
      JsonReader reader = new JsonTextReader(new StringReader(json));

      JsonValidatingReader validatingReader = new JsonValidatingReader(reader);
      Assert.AreEqual(reader, validatingReader.Reader);
    }

    [Test]
    public void ValidateTypes()
    {
      string schemaJson = @"{
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
}";

      string json = @"{'name':""James"",'hobbies':[""pie"",'cake']}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      JsonSchema schema = JsonSchema.Parse(schemaJson);
      reader.Schema = schema;
      Assert.AreEqual(schema, reader.Schema);

      Assert.AreEqual(0, reader.Depth);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("name", reader.Value.ToString());

      Assert.AreEqual(1, reader.Depth);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("James", reader.Value.ToString());
      Assert.AreEqual(typeof(string), reader.ValueType);
      Assert.AreEqual('"', reader.QuoteChar);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("hobbies", reader.Value.ToString());
      Assert.AreEqual('\'', reader.QuoteChar);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("pie", reader.Value.ToString());
      Assert.AreEqual('"', reader.QuoteChar);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("cake", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsNull(validationEventArgs);
    }

    [Test]
    public void ValidateUnrestrictedArray()
    {
      string schemaJson = @"{
  ""type"":""array""
}";

      string json = "['pie','cake',['nested1','nested2'],{'nestedproperty1':1.1,'nestedproperty2':[null]}]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("pie", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("cake", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("nested1", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("nested2", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("nestedproperty1", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(1.1, reader.Value);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("nestedproperty2", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNull(validationEventArgs);
    }

    [Test]
    public void StringLessThanMinimumLength()
    {
      string schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":50,
}";

      string json = "'pie'";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("String 'pie' is less than minimum length of 5. Line 1, position 5.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void StringGreaterThanMaximumLength()
    {
      string schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":10
}";

      string json = "'The quick brown fox jumps over the lazy dog.'";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 10. Line 1, position 46.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void StringIsNotInEnum()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""string"",
    ""enum"":[""one"",""two""]
  },
  ""maxItems"":3
}";

      string json = "['one','two','THREE']";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(@"Value ""THREE"" is not defined in enum. Line 1, position 20.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void StringDoesNotMatchPattern()
    {
      string schemaJson = @"{
  ""type"":""string"",
  ""pattern"":""foo""
}";

      string json = "'The quick brown fox jumps over the lazy dog.'";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' does not match regex pattern 'foo'. Line 1, position 46.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void IntegerGreaterThanMaximumValue()
    {
      string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

      string json = "10";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual("Integer 10 exceeds maximum value of 5. Line 1, position 2.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    [ExpectedException(typeof(JsonSchemaException), ExpectedMessage = "Integer 10 exceeds maximum value of 5. Line 1, position 2.")]
    public void ThrowExceptionWhenNoValidationEventHandler()
    {
      string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader("10")));
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
    }

    [Test]
    public void IntegerLessThanMinimumValue()
    {
      string schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

      string json = "1";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual("Integer 1 is less than minimum value of 5. Line 1, position 1.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void IntegerIsNotInEnum()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer"",
    ""enum"":[1,2]
  },
  ""maxItems"":3
}";

      string json = "[1,2,3]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual(@"Value 3 is not defined in enum. Line 1, position 7.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void FloatGreaterThanMaximumValue()
    {
      string schemaJson = @"{
  ""type"":""number"",
  ""maximum"":5
}";

      string json = "10.0";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual("Float 10.0 exceeds maximum value of 5. Line 1, position 4.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void FloatLessThanMinimumValue()
    {
      string schemaJson = @"{
  ""type"":""number"",
  ""minimum"":5
}";

      string json = "1.1";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual("Float 1.1 is less than minimum value of 5. Line 1, position 3.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void FloatIsNotInEnum()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""enum"":[1.1,2.2]
  },
  ""maxItems"":3
}";

      string json = "[1.1,2.2,3.0]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(@"Value 3.0 is not defined in enum. Line 1, position 13.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void FloatExceedsMaxDecimalPlaces()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""maxDecimal"":2
  }
}";

      string json = "[1.1,2.2,4.001]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(@"Float 4.001 exceeds the maximum allowed number decimal places of 2. Line 1, position 15.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void NullNotInEnum()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""null"",
    ""enum"":[]
  },
  ""maxItems"":3
}";

      string json = "[null]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.AreEqual(@"Value null is not defined in enum. Line 1, position 6.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void BooleanNotInEnum()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""boolean"",
    ""enum"":[true]
  },
  ""maxItems"":3
}";

      string json = "[true,false]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
      Assert.AreEqual(@"Value false is not defined in enum. Line 1, position 12.", validationEventArgs.Message);
      
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }
    
    [Test]
    public void ArrayCountGreaterThanMaximumItems()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

      string json = "[null,null,null,null]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
      Assert.AreEqual("Array item count 4 exceeds maximum count of 3. Line 1, position 21.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void ArrayCountLessThanMinimumItems()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

      string json = "[null]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
      Assert.AreEqual("Array item count 1 is less than minimum count of 2. Line 1, position 6.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void InvalidDataType()
    {
      string schemaJson = @"{
  ""type"":""string"",
  ""minItems"":2,
  ""maxItems"":3,
  ""items"":{}
}";

      string json = "[null,null,null,null]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
      Assert.AreEqual(@"Invalid type. Expected String but got Array. Line 1, position 1.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void StringDisallowed()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""disallow"":[""number""]
  },
  ""maxItems"":3
}";

      string json = "['pie',1.1]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Float, reader.TokenType);
      Assert.AreEqual(@"Type Float is disallowed. Line 1, position 11.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void MissingNonoptionalProperties()
    {
      string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string""},
    ""age"":{""type"":""integer""}
  }
}";

      string json = "{'name':'James'}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("name", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("James", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
      Assert.AreEqual("Non-optional properties are missing from object: hobbies, age. Line 1, position 16.", validationEventArgs.Message);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void MissingOptionalProperties()
    {
      string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string"",optional:true},
    ""age"":{""type"":""integer"",optional:true}
  }
}";

      string json = "{'name':'James'}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("name", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("James", reader.Value.ToString());
      Assert.IsNull(validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsNull(validationEventArgs);
    }

    [Test]
    public void DisableAdditionalProperties()
    {
      string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

      string json = "{'name':'James','additionalProperty1':null,'additionalProperty2':null}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("name", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("James", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("additionalProperty1", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.AreEqual(null, reader.Value);
      Assert.AreEqual("Property 'additionalProperty1' has not been defined and the schema does not allow additional properties. Line 1, position 38.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("additionalProperty2", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.AreEqual(null, reader.Value);
      Assert.AreEqual("Property 'additionalProperty2' has not been defined and the schema does not allow additional properties. Line 1, position 65.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsNotNull(validationEventArgs);
    }

    [Test]
    public void ExtendsStringGreaterThanMaximumLength()
    {
      string schemaJson = @"{
  ""extends"":{
    ""type"":""string"",
    ""minLength"":5,
    ""maxLength"":10
  },
  ""maxLength"":9
}";

      List<string> errors = new List<string>();
      string json = "'The quick brown fox jumps over the lazy dog.'";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; errors.Add(validationEventArgs.Message); };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 9. Line 1, position 46.", errors[0]);

      Assert.IsNotNull(validationEventArgs);
    }

    private JsonSchema GetExtendedSchema()
    {
      string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string""}
  },
  ""additionalProperties"":{}
}";

      string second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

      JsonSchemaResolver resolver = new JsonSchemaResolver();
      JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
      JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

      return secondSchema;
    }

    [Test]
    public void ExtendsDisallowAdditionProperties()
    {
      string json = "{'firstproperty':'blah','secondproperty':'blah2','additional':'blah3','additional2':'blah4'}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = GetExtendedSchema();

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("firstproperty", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("blah", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("secondproperty", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("blah2", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("additional", reader.Value.ToString());
      Assert.AreEqual("Property 'additional' has not been defined and the schema does not allow additional properties. Line 1, position 62.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("blah3", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("additional2", reader.Value.ToString());
      Assert.AreEqual("Property 'additional2' has not been defined and the schema does not allow additional properties. Line 1, position 84.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("blah4", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void ExtendsMissingNonoptionalProperties()
    {
      string json = "{}";

      List<string> errors = new List<string>();

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { errors.Add(args.Message); };
      reader.Schema = GetExtendedSchema();

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual("Non-optional properties are missing from object: secondproperty, firstproperty. Line 1, position 2.", errors[0]);
    }

    [Test]
    public void sdfsdf()
    {
      string schemaJson = @"{
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""integer""}],
  ""additionalProperties"": false
}";

      string json = @"[1, 'a', null]";

      Json.Schema.ValidationEventArgs validationEventArgs = null;

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
      reader.Schema = JsonSchema.Parse(schemaJson);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Integer, reader.TokenType);
      Assert.AreEqual("Invalid type. Expected String but got Integer. Line 1, position 3.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("Invalid type. Expected Integer but got String. Line 1, position 7.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Null, reader.TokenType);
      Assert.AreEqual("Index 3 has not been defined and the schema does not allow additional items. Line 1, position 14.", validationEventArgs.Message);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

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

      string json = @"{
  'firstproperty':'blahblahblahblahblahblah',
  'secondproperty':'secasecasecasecaseca',
  'thirdproperty':{
    'thirdproperty_firstproperty':'aaa',
    'additional':'three'
  }
}";

      Json.Schema.ValidationEventArgs validationEventArgs = null;
      List<string> errors = new List<string>();

      JsonValidatingReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; errors.Add(validationEventArgs.Message); };
      reader.Schema = secondSchema;

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("firstproperty", reader.Value.ToString());
      Assert.AreEqual(null, validationEventArgs);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("blahblahblahblahblahblah", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("secondproperty", reader.Value.ToString());

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("secasecasecasecaseca", reader.Value.ToString());
      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual("String 'secasecasecasecaseca' exceeds maximum length of 10. Line 3, position 41.", errors[0]);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("thirdproperty", reader.Value.ToString());
      Assert.AreEqual(1, errors.Count);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      Assert.AreEqual(1, errors.Count);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("thirdproperty_firstproperty", reader.Value.ToString());
      Assert.AreEqual(1, errors.Count);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("aaa", reader.Value.ToString());
      Assert.AreEqual(4, errors.Count);
      Assert.AreEqual("String 'aaa' is less than minimum length of 7. Line 5, position 39.", errors[1]);
      Assert.AreEqual("String 'aaa' does not match regex pattern 'hi'. Line 5, position 39.", errors[2]);
      Assert.AreEqual("String 'aaa' does not match regex pattern 'hi2u'. Line 5, position 39.", errors[3]);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
      Assert.AreEqual("additional", reader.Value.ToString());
      Assert.AreEqual(4, errors.Count);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);
      Assert.AreEqual("three", reader.Value.ToString());
      Assert.AreEqual(5, errors.Count);
      Assert.AreEqual("String 'three' is less than minimum length of 6. Line 6, position 24.", errors[4]);
      
      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }
  }
}