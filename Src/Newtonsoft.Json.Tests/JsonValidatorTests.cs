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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
#if !(NET20 || NET35 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;
using ValidationEventArgs = Newtonsoft.Json.Schema.ValidationEventArgs;

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class JsonValidatorTests : TestFixtureBase
    {
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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            JsonSchema schema = JsonSchema.Parse(schemaJson);
            validator.Validate(JToken.Parse(json), schema);
            
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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Value ""THREE"" is not defined in enum. Line 1, position 20.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' does not match regex pattern 'foo'. Line 1, position 46.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Integer 10 exceeds maximum value of 5. Line 1, position 2.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        [Test]
        public void IntegerGreaterThanMaximumValue_BigInteger()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            string json = "99999999999999999999999999999999999999999999999999999999999999999999";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Integer 99999999999999999999999999999999999999999999999999999999999999999999 exceeds maximum value of 5. Line 1, position 68.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Test]
        public void IntegerLessThanMaximumValue_BigInteger()
        {
            string schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

            JValue v = new JValue(new BigInteger(1));

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            v.Validate(JsonSchema.Parse(schemaJson), (sender, args) => { validationEventArgs = args; });

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Integer 1 is less than minimum value of 5.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);
        }
#endif

        [Test]
        public void ThrowExceptionWhenNoValidationEventHandler()
        {
            ExceptionAssert.Throws<JsonSchemaException>("Integer 10 exceeds maximum value of 5. Line 1, position 2.",
                () =>
                {
                    string schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

                    JsonValidator validator = new JsonValidator();
                    validator.Validate(JToken.Parse("10"), JsonSchema.Parse(schemaJson));
                });
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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Value 3 is not defined in enum. Line 1, position 6.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Value 3.0 is not defined in enum. Line 1, position 12.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Test]
        public void FloatMultipleOf()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""multipleOf"":0.1
  }
}";

            string json = "[1.1,2.2,4.001]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Float 4.001 is not a multiple of 0.1. Line 1, position 14.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        [Test]
        public void BigIntegerMultipleOf_Success()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""multipleOf"":2
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999998]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));
        }

        [Test]
        public void BigIntegerMultipleOf_Failure()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""multipleOf"":2
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999999]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Integer 999999999999999999999999999999999999999999999999999999999 is not a multiple of 2. Line 1, position 58.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Test]
        public void BigIntegerMultipleOf_Fraction()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""multipleOf"":1.1
  }
}";

            string json = "[999999999999999999999999999999999999999999999999999999999]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual(@"Integer 999999999999999999999999999999999999999999999999999999999 is not a multiple of 1.1. Line 1, position 58.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);
        }

        [Test]
        public void BigIntegerMultipleOf_FractionWithZeroValue()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""multipleOf"":1.1
  }
}";

            JArray a = new JArray(new JValue(new BigInteger(0)));

            ValidationEventArgs validationEventArgs = null;

            a.Validate(JsonSchema.Parse(schemaJson), (sender, args) => { validationEventArgs = args; });

            Assert.IsNull(validationEventArgs);
        }
#endif

        [Test]
        public void IntValidForNumber()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number""
  }
}";

            string json = "[1]";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));
            
            Assert.IsNull(validationEventArgs);
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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Value null is not defined in enum. Line 1, position 5.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Value false is not defined in enum. Line 1, position 11.", validationEventArgs.Message);
            Assert.AreEqual("[1]", validationEventArgs.Path);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Array item count 4 exceeds maximum count of 3. Line 1, position 1.", validationEventArgs.Message);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Array item count 1 is less than minimum count of 2. Line 1, position 1.", validationEventArgs.Message);

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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(@"Invalid type. Expected String but got Array. Line 1, position 1.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
        }

        [Test]
        public void MissingRequiredProperties()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string""},
    ""age"":{""type"":""integer""}
  },
  ""required"": [
    ""hobbies"",
    ""age""
  ]
}";

            string json = "{'name':'James'}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Required properties are missing from object: hobbies, age. Line 1, position 1.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Test]
        public void MissingNonRequiredProperties()
        {
            string schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string""},
    ""age"":{""type"":""integer""}
  },
  ""required"": [
    ""name""
  ]
}";

            string json = "{'name':'James'}";

            Json.Schema.ValidationEventArgs validationEventArgs = null;

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.IsNull(validationEventArgs);

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

            IList<Json.Schema.ValidationEventArgs> validationEventArgs = new List<Json.Schema.ValidationEventArgs>();

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs.Add(args); };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Property 'additionalProperty1' has not been defined and the schema does not allow additional properties. Line 1, position 38.", validationEventArgs[0].Message);
            Assert.AreEqual("Property 'additionalProperty2' has not been defined and the schema does not allow additional properties. Line 1, position 65.", validationEventArgs[1].Message);
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

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) =>
            {
                validationEventArgs = args;
                errors.Add(validationEventArgs.Message);
            };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 9. Line 1, position 46.", errors[0]);

            Assert.IsNotNull(validationEventArgs);
        }

        private JsonSchema GetAllOfSchema()
        {
            string first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string""}
  },
  ""required"": [
     ""firstproperty""
  ],
  ""additionalProperties"":{}
}";

            string second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""allOf"":[{""$ref"":""first""}],
  ""properties"":
  {
    ""secondproperty"":{""type"":""string""}
  },
  ""required"": [
     ""secondproperty""
  ],
  ""additionalProperties"":false
}";

            JsonSchemaResolver resolver = new JsonSchemaResolver();
            JsonSchema firstSchema = JsonSchema.Parse(first, resolver);
            JsonSchema secondSchema = JsonSchema.Parse(second, resolver);

            return secondSchema;
        }

        [Test]
        public void NoAdditionalItems()
        {
            string schemaJson = @"{
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""integer""}],
  ""additionalItems"": false
}";

            string json = @"[1, 'a', null]";

            IList<Json.Schema.ValidationEventArgs> validationEventArgs = new List<Json.Schema.ValidationEventArgs>();

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs.Add(args); };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Invalid type. Expected String but got Integer. Line 1, position 2.", validationEventArgs[0].Message);
            Assert.AreEqual("Invalid type. Expected Integer but got String. Line 1, position 7.", validationEventArgs[1].Message);
            Assert.AreEqual("Index 3 has not been defined and the schema does not allow additional items. Line 1, position 13.", validationEventArgs[2].Message);
        }

        [Test]
        public void PatternPropertiesNoAdditionalProperties()
        {
            string schemaJson = @"{
  ""type"":""object"",
  ""patternProperties"": {
     ""hi"": {""type"":""string""},
     ""ho"": {""type"":""string""}
  },
  ""additionalProperties"": false
}";

            string json = @"{
  ""hi"": ""A string!"",
  ""hide"": ""A string!"",
  ""ho"": 1,
  ""hey"": ""A string!""
}";

            IList<Json.Schema.ValidationEventArgs> validationEventArgs = new List<Json.Schema.ValidationEventArgs>();

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs.Add(args); };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schemaJson));

            Assert.AreEqual("Invalid type. Expected String but got Integer. Line 4, position 10.", validationEventArgs[0].Message);
            Assert.AreEqual("Property 'hey' has not been defined and the schema does not allow additional properties. Line 5, position 9.", validationEventArgs[1].Message);
        }

        [Test]
        public void DuplicateErrorsTest()
        {
            string schema = @"{
  ""id"":""ErrorDemo.Database"",
  ""properties"":{
    ""ErrorDemoDatabase"":{
      ""type"":""object"",
      ""properties"":{
        ""URL"":{
          ""type"":""string""
        },
        ""Version"":{
          ""type"":""string""
        },
        ""Date"":{
          ""type"":""string"",
          ""format"":""date-time""
        },
        ""MACLevels"":{
          ""type"":""object"",
          ""properties"":{
            ""MACLevel"":{
              ""type"":""array"",
              ""items"":[
                {
                  ""properties"":{
                    ""IDName"":{
                      ""type"":""string""
                    },
                    ""Order"":{
                      ""type"":""string""
                    },
                    ""IDDesc"":{
                      ""type"":""string""
                    },
                    ""IsActive"":{
                      ""type"":""string""
                    }
                  },
                  ""required"": [
                    ""IDName"",
                    ""Order"",
                    ""IDDesc"",
                    ""IsActive""
                  ]
                }
              ]
            }
          },
          ""required"": [
            ""MACLevel""
          ]
        }
      },
      ""required"": [
        ""URL"",
        ""Version"",
        ""Date"",
        ""MACLevels""
      ]
    }
  },
  ""required"": [
    ""ErrorDemoDatabase""
  ]
}";

            string json = @"{
  ""ErrorDemoDatabase"":{
    ""URL"":""localhost:3164"",
    ""Version"":""1.0"",
    ""Date"":""6.23.2010, 9:35:18.121"",
    ""MACLevels"":{
      ""MACLevel"":[
        {
          ""@IDName"":""Developer"",
          ""Order"":""0"",
          ""IDDesc"":""DeveloperDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Technician"",
          ""Order"":""1"",
          ""IDDesc"":""TechnicianDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Administrator"",
          ""Order"":""2"",
          ""IDDesc"":""AdministratorDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""PowerUser"",
          ""Order"":""3"",
          ""IDDesc"":""PowerUserDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Operator"",
          ""Order"":""4"",
          ""IDDesc"":""OperatorDesc"",
          ""IsActive"":""True""
        }
      ]
    }
  }
}";

            IList<ValidationEventArgs> validationEventArgs = new List<ValidationEventArgs>();

            JsonValidator validator = new JsonValidator();
            validator.ValidationEventHandler += (sender, args) => { validationEventArgs.Add(args); };
            validator.Validate(JToken.Parse(json), JsonSchema.Parse(schema));

            Assert.AreEqual(1, validationEventArgs.Count);
        }
    }
}