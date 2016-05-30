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
using Newtonsoft.Json.Serialization;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class SnakeCaseNamingStrategyTests : TestFixtureBase
    {
        [Test]
        public void JsonConvertSerializerSettings()
        {
            Person person = new Person();
            person.BirthDate = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.LastModified = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.Name = "Name!";

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            StringAssert.AreEqual(@"{
  ""name"": ""Name!"",
  ""birth_date"": ""2000-11-20T23:55:44Z"",
  ""last_modified"": ""2000-11-20T23:55:44Z""
}", json);

            Person deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            Assert.AreEqual(person.BirthDate, deserializedPerson.BirthDate);
            Assert.AreEqual(person.LastModified, deserializedPerson.LastModified);
            Assert.AreEqual(person.Name, deserializedPerson.Name);

            json = JsonConvert.SerializeObject(person, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
        }

        [Test]
        public void JTokenWriter_OverrideSpecifiedName()
        {
            JsonIgnoreAttributeOnClassTestClass ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass();
            ignoreAttributeOnClassTestClass.Field = int.MinValue;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy
                {
                    OverrideSpecifiedNames = true
                }
            };

            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = contractResolver;

            JTokenWriter writer = new JTokenWriter();

            serializer.Serialize(writer, ignoreAttributeOnClassTestClass);

            JObject o = (JObject)writer.Token;
            JProperty p = o.Property("the_field");

            Assert.IsNotNull(p);
            Assert.AreEqual(int.MinValue, (int)p.Value);
        }

        [Test]
        public void BlogPostExample()
        {
            Product product = new Product
            {
                ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                Name = "Widget",
                Price = 9.99m,
                Sizes = new[] { "Small", "Medium", "Large" }
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            string json =
                JsonConvert.SerializeObject(
                    product,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = contractResolver }
                    );

            //{
            //  "name": "Widget",
            //  "expiryDate": "\/Date(1292868060000)\/",
            //  "price": 9.99,
            //  "sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            StringAssert.AreEqual(@"{
  ""name"": ""Widget"",
  ""expiry_date"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void DynamicSnakeCasePropertyNames()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Integer = int.MaxValue;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""explicit"": false,
  ""text"": ""Text!"",
  ""integer"": 2147483647,
  ""int"": 0,
  ""child_object"": null
}", json);
        }
#endif

        [Test]
        public void DictionarySnakeCasePropertyNames_Disabled()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "First", "Value1!" },
                { "Second", "Value2!" }
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""First"": ""Value1!"",
  ""Second"": ""Value2!""
}", json);
        }

        [Test]
        public void DictionarySnakeCasePropertyNames_Enabled()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "First", "Value1!" },
                { "Second", "Value2!" }
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""first"": ""Value1!"",
  ""second"": ""Value2!""
}", json);
        }

        public class PropertyAttributeNamingStrategyTestClass
        {
            [JsonProperty]
            public string HasNoAttributeNamingStrategy { get; set; }

            [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
            public string HasAttributeNamingStrategy { get; set; }
        }

        [Test]
        public void JsonPropertyAttribute_NamingStrategyType()
        {
            PropertyAttributeNamingStrategyTestClass c = new PropertyAttributeNamingStrategyTestClass
            {
                HasNoAttributeNamingStrategy = "Value1!",
                HasAttributeNamingStrategy = "Value2!"
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""HasNoAttributeNamingStrategy"": ""Value1!"",
  ""has_attribute_naming_strategy"": ""Value2!""
}", json);
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public class ContainerAttributeNamingStrategyTestClass
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            [JsonProperty(NamingStrategyType = typeof(DefaultNamingStrategy))]
            public string HasAttributeNamingStrategy { get; set; }
        }

        [Test]
        public void JsonObjectAttribute_NamingStrategyType()
        {
            ContainerAttributeNamingStrategyTestClass c = new ContainerAttributeNamingStrategyTestClass
            {
                Prop1 = "Value1!",
                Prop2 = "Value2!"
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""prop1"": ""Value1!"",
  ""prop2"": ""Value2!"",
  ""HasAttributeNamingStrategy"": null
}", json);
        }

        [JsonDictionary(NamingStrategyType = typeof(SnakeCaseNamingStrategy), NamingStrategyParameters = new object[] { true, true })]
        public class DictionaryAttributeNamingStrategyTestClass : Dictionary<string, string>
        {
        }

        [Test]
        public void JsonDictionaryAttribute_NamingStrategyType()
        {
            DictionaryAttributeNamingStrategyTestClass c = new DictionaryAttributeNamingStrategyTestClass
            {
                ["Key1"] = "Value1!",
                ["Key2"] = "Value2!"
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""key1"": ""Value1!"",
  ""key2"": ""Value2!""
}", json);
        }
    }
}