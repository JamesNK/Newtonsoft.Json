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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Serialization
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly char _startingWithChar;

        public DynamicContractResolver(char startingWithChar)
#pragma warning disable 612,618
            : base(false)
#pragma warning restore 612,618
        {
            _startingWithChar = startingWithChar;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that start with the specified character
            properties =
                properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

            return properties;
        }
    }

    public class EscapedPropertiesContractResolver : DefaultContractResolver
    {
        public string PropertyPrefix { get; set; }
        public string PropertySuffix { get; set; }

        protected override string ResolvePropertyName(string propertyName)
        {
            return base.ResolvePropertyName(PropertyPrefix + propertyName + PropertySuffix);
        }
    }

    public class Book
    {
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string AuthorName { get; set; }
        public int AuthorAge { get; set; }
        public string AuthorCountry { get; set; }
    }

    public class IPersonContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(Employee))
            {
                objectType = typeof(IPerson);
            }

            return base.CreateContract(objectType);
        }
    }

    public class AddressWithDataMember
    {
#if !NET20
        [DataMember(Name = "CustomerAddress1")]
#endif
            public string AddressLine1 { get; set; }
    }

    [TestFixture]
    public class ContractResolverTests : TestFixtureBase
    {
        [Test]
        public void JsonPropertyDefaultValue()
        {
            JsonProperty p = new JsonProperty();

            Assert.AreEqual(null, p.GetResolvedDefaultValue());
            Assert.AreEqual(null, p.DefaultValue);

            p.PropertyType = typeof(int);

            Assert.AreEqual(0, p.GetResolvedDefaultValue());
            Assert.AreEqual(null, p.DefaultValue);

            p.PropertyType = typeof(DateTime);

            Assert.AreEqual(new DateTime(), p.GetResolvedDefaultValue());
            Assert.AreEqual(null, p.DefaultValue);

            p.PropertyType = null;

            Assert.AreEqual(null, p.GetResolvedDefaultValue());
            Assert.AreEqual(null, p.DefaultValue);

            p.PropertyType = typeof(CompareOptions);

            Assert.AreEqual(CompareOptions.None, (CompareOptions)p.GetResolvedDefaultValue());
            Assert.AreEqual(null, p.DefaultValue);
        }

        [Test]
        public void ListInterface()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(IList<int>));

            Assert.IsTrue(contract.IsInstantiable);
            Assert.AreEqual(typeof(List<int>), contract.CreatedType);
            Assert.IsNotNull(contract.DefaultCreator);
        }

        [Test]
        public void PropertyAttributeProvider()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(Invoice));

            JsonProperty property = contract.Properties["FollowUpDays"];
            Assert.AreEqual(1, property.AttributeProvider.GetAttributes(false).Count);
            Assert.AreEqual(typeof(DefaultValueAttribute), property.AttributeProvider.GetAttributes(false)[0].GetType());
        }

        [Test]
        public void AbstractTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(AbstractTestClass));

            Assert.IsFalse(contract.IsInstantiable);
            Assert.IsNull(contract.DefaultCreator);
            Assert.IsNull(contract.OverrideCreator);

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractTestClass>(@"{Value:'Value!'}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type Newtonsoft.Json.Tests.Serialization.AbstractTestClass. Type is an interface or abstract class and cannot be instantiated. Path 'Value', line 1, position 7.");

            contract.DefaultCreator = () => new AbstractImplementationTestClass();

            var o = JsonConvert.DeserializeObject<AbstractTestClass>(@"{Value:'Value!'}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual("Value!", o.Value);
        }

        [Test]
        public void AbstractListTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(AbstractListTestClass<int>));

            Assert.IsFalse(contract.IsInstantiable);
            Assert.IsNull(contract.DefaultCreator);
            Assert.IsFalse(contract.HasParameterizedCreatorInternal);

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type Newtonsoft.Json.Tests.Serialization.AbstractListTestClass`1[System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path '', line 1, position 1.");

            contract.DefaultCreator = () => new AbstractImplementationListTestClass<int>();

            var l = JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual(2, l.Count);
            Assert.AreEqual(1, l[0]);
            Assert.AreEqual(2, l[1]);
        }

        public class CustomList<T> : List<T>
        {
        }

        [Test]
        public void ListInterfaceDefaultCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonArrayContract)resolver.ResolveContract(typeof(IList<int>));

            Assert.IsTrue(contract.IsInstantiable);
            Assert.IsNotNull(contract.DefaultCreator);

            contract.DefaultCreator = () => new CustomList<int>();

            var l = JsonConvert.DeserializeObject<IList<int>>(@"[1,2,3]", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual(typeof(CustomList<int>), l.GetType());
            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(1, l[0]);
            Assert.AreEqual(2, l[1]);
            Assert.AreEqual(3, l[2]);
        }

        public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
        }

        [Test]
        public void DictionaryInterfaceDefaultCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonDictionaryContract)resolver.ResolveContract(typeof(IDictionary<string, int>));

            Assert.IsTrue(contract.IsInstantiable);
            Assert.IsNotNull(contract.DefaultCreator);

            contract.DefaultCreator = () => new CustomDictionary<string, int>();

            var d = JsonConvert.DeserializeObject<IDictionary<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual(typeof(CustomDictionary<string, int>), d.GetType());
            Assert.AreEqual(2, d.Count);
            Assert.AreEqual(1, d["key1"]);
            Assert.AreEqual(2, d["key2"]);
        }

        [Test]
        public void AbstractDictionaryTestClass()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonDictionaryContract)resolver.ResolveContract(typeof(AbstractDictionaryTestClass<string, int>));

            Assert.IsFalse(contract.IsInstantiable);
            Assert.IsNull(contract.DefaultCreator);
            Assert.IsFalse(contract.HasParameterizedCreatorInternal);

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            }), "Could not create an instance of type Newtonsoft.Json.Tests.Serialization.AbstractDictionaryTestClass`2[System.String,System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path 'key1', line 1, position 6.");

            contract.DefaultCreator = () => new AbstractImplementationDictionaryTestClass<string, int>();

            var d = JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual(2, d.Count);
            Assert.AreEqual(1, d["key1"]);
            Assert.AreEqual(2, d["key2"]);
        }

        [Test]
        public void SerializeWithEscapedPropertyName()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertySuffix = @"-'-""-"
                    }
                });

            Assert.AreEqual(@"{""AddressLine1-'-\""-"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            reader.Read();

            Assert.AreEqual(@"AddressLine1-'-""-", reader.Value);
        }

        [Test]
        public void SerializeWithHtmlEscapedPropertyName()
        {
            string json = JsonConvert.SerializeObject(
                new AddressWithDataMember
                {
                    AddressLine1 = "value!"
                },
                new JsonSerializerSettings
                {
                    ContractResolver = new EscapedPropertiesContractResolver
                    {
                        PropertyPrefix = "<b>",
                        PropertySuffix = "</b>"
                    },
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                });

            Assert.AreEqual(@"{""\u003cb\u003eAddressLine1\u003c/b\u003e"":""value!""}", json);

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            reader.Read();

            Assert.AreEqual(@"<b>AddressLine1</b>", reader.Value);
        }

        [Test]
        public void CalculatingPropertyNameEscapedSkipping()
        {
            JsonProperty p = new JsonProperty { PropertyName = "abc" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "123" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "._-" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "!@#" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "$%^" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "?*(" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = ")_+" };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "=:," };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = null };
            Assert.IsTrue(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "&" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "<" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = ">" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "'" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = @"""" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = Environment.NewLine };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\0" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\n" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\v" };
            Assert.IsFalse(p._skipPropertyNameEscape);

            p = new JsonProperty { PropertyName = "\u00B9" };
            Assert.IsFalse(p._skipPropertyNameEscape);
        }

#if !NET20
        [Test]
        public void DeserializeDataMemberClassWithNoDataContract()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(AddressWithDataMember));

            Assert.AreEqual("AddressLine1", contract.Properties[0].PropertyName);
        }
#endif

        [Test]
        public void ResolveProperties_IgnoreStatic()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(NumberFormatInfo));

            Assert.IsFalse(contract.Properties.Any(c => c.PropertyName == "InvariantInfo"));
        }

        [Test]
        public void ParameterizedCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(PublicParametizedConstructorWithPropertyNameConflictWithAttribute));

            Assert.IsNull(contract.DefaultCreator);
            Assert.IsNotNull(contract.ParameterizedCreator);
#pragma warning disable 618
            Assert.AreEqual(contract.ParametrizedConstructor, typeof(PublicParametizedConstructorWithPropertyNameConflictWithAttribute).GetConstructor(new[] { typeof(string) }));
#pragma warning restore 618
            Assert.AreEqual(1, contract.CreatorParameters.Count);
            Assert.AreEqual("name", contract.CreatorParameters[0].PropertyName);

#pragma warning disable 618
            contract.ParametrizedConstructor = null;
#pragma warning restore 618
            Assert.IsNull(contract.ParameterizedCreator);
        }

        [Test]
        public void OverrideCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParamatrizedConstructorsJsonConstructor));

            Assert.IsNull(contract.DefaultCreator);
            Assert.IsNotNull(contract.OverrideCreator);
#pragma warning disable 618
            Assert.AreEqual(contract.OverrideConstructor, typeof(MultipleParamatrizedConstructorsJsonConstructor).GetConstructor(new[] { typeof(string), typeof(int) }));
#pragma warning restore 618
            Assert.AreEqual(2, contract.CreatorParameters.Count);
            Assert.AreEqual("Value", contract.CreatorParameters[0].PropertyName);
            Assert.AreEqual("Age", contract.CreatorParameters[1].PropertyName);

#pragma warning disable 618
            contract.OverrideConstructor = null;
#pragma warning restore 618
            Assert.IsNull(contract.OverrideCreator);
        }

        [Test]
        public void CustomOverrideCreator()
        {
            var resolver = new DefaultContractResolver();
            var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParamatrizedConstructorsJsonConstructor));

            bool ensureCustomCreatorCalled = false;

            contract.OverrideCreator = args =>
            {
                ensureCustomCreatorCalled = true;
                return new MultipleParamatrizedConstructorsJsonConstructor((string)args[0], (int)args[1]);
            };
#pragma warning disable 618
            Assert.IsNull(contract.OverrideConstructor);
#pragma warning restore 618

            var o = JsonConvert.DeserializeObject<MultipleParamatrizedConstructorsJsonConstructor>("{Value:'value!', Age:1}", new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

            Assert.AreEqual("value!", o.Value);
            Assert.AreEqual(1, o.Age);
            Assert.IsTrue(ensureCustomCreatorCalled);
        }

        [Test]
        public void SerializeInterface()
        {
            Employee employee = new Employee
            {
                BirthDate = new DateTime(1977, 12, 30, 1, 1, 1, DateTimeKind.Utc),
                FirstName = "Maurice",
                LastName = "Moss",
                Department = "IT",
                JobTitle = "Support"
            };

            string iPersonJson = JsonConvert.SerializeObject(employee, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new IPersonContractResolver() });

            StringAssert.AreEqual(@"{
  ""FirstName"": ""Maurice"",
  ""LastName"": ""Moss"",
  ""BirthDate"": ""1977-12-30T01:01:01Z""
}", iPersonJson);
        }

        [Test]
        public void SingleTypeWithMultipleContractResolvers()
        {
            Book book = new Book
            {
                BookName = "The Gathering Storm",
                BookPrice = 16.19m,
                AuthorName = "Brandon Sanderson",
                AuthorAge = 34,
                AuthorCountry = "United States of America"
            };

            string startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

            // {
            //   "AuthorName": "Brandon Sanderson",
            //   "AuthorAge": 34,
            //   "AuthorCountry": "United States of America"
            // }

            string startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

            // {
            //   "BookName": "The Gathering Storm",
            //   "BookPrice": 16.19
            // }

            StringAssert.AreEqual(@"{
  ""AuthorName"": ""Brandon Sanderson"",
  ""AuthorAge"": 34,
  ""AuthorCountry"": ""United States of America""
}", startingWithA);

            StringAssert.AreEqual(@"{
  ""BookName"": ""The Gathering Storm"",
  ""BookPrice"": 16.19
}", startingWithB);
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
#pragma warning disable 618
        [Test]
        public void SerializeCompilerGeneratedMembers()
        {
            StructTest structTest = new StructTest
            {
                IntField = 1,
                IntProperty = 2,
                StringField = "Field",
                StringProperty = "Property"
            };

            DefaultContractResolver skipCompilerGeneratedResolver = new DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            };

            string skipCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = skipCompilerGeneratedResolver });

            StringAssert.AreEqual(@"{
  ""StringField"": ""Field"",
  ""IntField"": 1,
  ""StringProperty"": ""Property"",
  ""IntProperty"": 2
}", skipCompilerGeneratedJson);

            DefaultContractResolver includeCompilerGeneratedResolver = new DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                SerializeCompilerGeneratedMembers = true
            };

            string includeCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = includeCompilerGeneratedResolver });

            JObject o = JObject.Parse(includeCompilerGeneratedJson);

            Console.WriteLine(includeCompilerGeneratedJson);

            Assert.AreEqual("Property", (string)o["<StringProperty>k__BackingField"]);
            Assert.AreEqual(2, (int)o["<IntProperty>k__BackingField"]);
        }
#pragma warning restore 618
#endif

        public class ClassWithExtensionData
        {
            [JsonExtensionData]
            public IDictionary<string, object> Data { get; set; }
        }

        [Test]
        public void ExtensionDataGetterCanBeIteratedMultipleTimes()
        {
            DefaultContractResolver resolver = new DefaultContractResolver();
            JsonObjectContract contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithExtensionData));

            ClassWithExtensionData myClass = new ClassWithExtensionData
            {
                Data = new Dictionary<string, object>
                {
                    { "SomeField", "Field" },
                }
            };

            ExtensionDataGetter getter = contract.ExtensionDataGetter;

            IEnumerable<KeyValuePair<object, object>> dictionaryData = getter(myClass).ToDictionary(kv => kv.Key, kv => kv.Value);
            Assert.IsTrue(dictionaryData.Any());
            Assert.IsTrue(dictionaryData.Any());

            IEnumerable<KeyValuePair<object, object>> extensionData = getter(myClass);
            Assert.IsTrue(extensionData.Any());
            Assert.IsTrue(extensionData.Any()); // second test fails if the enumerator returned isn't reset
        }
    }
}