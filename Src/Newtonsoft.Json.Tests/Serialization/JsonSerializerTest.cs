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
using System.ComponentModel;
#if !(NET35 || NET20)
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
#if !(NET20 || NET35 || PORTABLE)
using System.Numerics;
#endif
#if !(NET20 || NETFX_CORE || DNXCORE50)
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Web.Script.Serialization;
#endif
using System.Text;
using System.Text.RegularExpressions;
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
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#if !(NET20 || NET35)
using System.Runtime.Serialization.Json;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using System.Runtime.Serialization;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Reflection;
#if !NET20
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Linq.Expressions;
#endif
#if !(NET35 || NET20)
using System.Dynamic;
#endif
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if !(NETFX_CORE || DNXCORE50)
using System.Drawing;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class JsonSerializerTest : TestFixtureBase
    {
        public class ErroringClass
        {
            public DateTime Tags { get; set; }
        }

        [Test]
        public void DontCloseInputOnDeserializeError()
        {
            using (var s = System.IO.File.OpenRead("large.json"))
            {
                try
                {
                    using (JsonTextReader reader = new JsonTextReader(new StreamReader(s)))
                    {
                        reader.SupportMultipleContent = true;
                        reader.CloseInput = false;

                        // read into array
                        reader.Read();

                        var ser = new JsonSerializer();
                        ser.CheckAdditionalContent = false;

                        ser.Deserialize<IList<ErroringClass>>(reader);
                    }

                    Assert.Fail();
                }
                catch (Exception)
                {
                    Assert.IsTrue(s.Position > 0);

                    s.Seek(0, SeekOrigin.Begin);

                    Assert.AreEqual(0, s.Position);
                }
            }
        }

        public interface ISubclassBase
        {
            int ID { get; set; }
            string Name { get; set; }
            bool P1 { get; }
        }

        public interface ISubclass : ISubclassBase
        {
            new bool P1 { get; set; }
            int P2 { get; set; }
        }

        public interface IMainClass
        {
            int ID { get; set; }
            string Name { get; set; }
            ISubclass Subclass { get; set; }
        }

        public class Subclass : ISubclass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool P1 { get; set; }
            public int P2 { get; set; }
        }

        public class MainClass : IMainClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public ISubclass Subclass { get; set; }
        }

        public class MyFactory
        {
            public static ISubclass InstantiateSubclass()
            {
                return new Subclass
                {
                    ID = 123,
                    Name = "ABC",
                    P1 = true,
                    P2 = 44
                };
            }

            public static IMainClass InstantiateManiClass()
            {
                return new MainClass
                {
                    ID = 567,
                    Name = "XYZ",
                    Subclass = InstantiateSubclass()
                };
            }
        }

        [Test]
        public void SerializeInterfaceWithHiddenProperties()
        {
            var mySubclass = MyFactory.InstantiateSubclass();
            var myMainClass = MyFactory.InstantiateManiClass();

            //Class implementing interface with hidden members - flat object. 
            var strJsonSubclass = JsonConvert.SerializeObject(mySubclass, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""ID"": 123,
  ""Name"": ""ABC"",
  ""P1"": true,
  ""P2"": 44
}", strJsonSubclass);

            //Class implementing interface with hidden members - member of another class. 
            var strJsonMainClass = JsonConvert.SerializeObject(myMainClass, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""ID"": 567,
  ""Name"": ""XYZ"",
  ""Subclass"": {
    ""ID"": 123,
    ""Name"": ""ABC"",
    ""P1"": true,
    ""P2"": 44
  }
}", strJsonMainClass);
        }

#if !(PORTABLE || PORTABLE40 || NET20 || NET35)
        [Test]
        public void LargeIntegerAsString()
        {
            var largeBrokenNumber = JsonConvert.DeserializeObject<Foo64>("{\"Blah\": 43443333222211111117 }");
            Assert.AreEqual("43443333222211111117", largeBrokenNumber.Blah);

            var largeOddWorkingNumber = JsonConvert.DeserializeObject<Foo64>("{\"Blah\": 53443333222211111117 }");
            Assert.AreEqual("53443333222211111117", largeOddWorkingNumber.Blah);
        }

        public class Foo64
        {
            public string Blah { get; set; }
        }
#endif

#if !NET20
        [Test]
        public void DeserializeMSDateTimeOffset()
        {
            DateTimeOffset d = JsonConvert.DeserializeObject<DateTimeOffset>(@"""/Date(1418924498000+0800)/""");
            long initialTicks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(d.DateTime, d.Offset);

            Assert.AreEqual(1418924498000, initialTicks);
            Assert.AreEqual(8, d.Offset.Hours);
        }
#endif

        [Test]
        public void DeserializeBoolean_Null()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IList<bool>>(@"[null]"),
                "Error converting value {null} to type 'System.Boolean'. Path '[0]', line 1, position 5.");
        }

        [Test]
        public void DeserializeBoolean_DateTime()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<IList<bool>>(@"['2000-12-20T10:55:55Z']"),
                "Could not convert string to boolean: 2000-12-20T10:55:55Z. Path '[0]', line 1, position 23.");
        }

        [Test]
        public void DeserializeBoolean_BadString()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<IList<bool>>(@"['pie']"),
                @"Could not convert string to boolean: pie. Path '[0]', line 1, position 6.");
        }

        [Test]
        public void DeserializeBoolean_EmptyString()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IList<bool>>(@"['']"),
                @"Error converting value {null} to type 'System.Boolean'. Path '[0]', line 1, position 3.");
        }

#if !(PORTABLE || PORTABLE40 || NET35 || NET20)
        [Test]
        public void DeserializeBooleans()
        {
            IList<bool> l = JsonConvert.DeserializeObject<IList<bool>>(@"[
  1,
  0,
  1.1,
  0.0,
  0.000000000001,
  9999999999,
  -9999999999,
  9999999999999999999999999999999999999999999999999999999999999999999999,
  -9999999999999999999999999999999999999999999999999999999999999999999999,
  'true',
  'TRUE',
  'false',
  'FALSE'
]");

            int i = 0;
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(false, l[i++]);
        }

        [Test]
        public void DeserializeNullableBooleans()
        {
            IList<bool?> l = JsonConvert.DeserializeObject<IList<bool?>>(@"[
  1,
  0,
  1.1,
  0.0,
  0.000000000001,
  9999999999,
  -9999999999,
  9999999999999999999999999999999999999999999999999999999999999999999999,
  -9999999999999999999999999999999999999999999999999999999999999999999999,
  'true',
  'TRUE',
  'false',
  'FALSE',
  '',
  null
]");

            int i = 0;
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(true, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(false, l[i++]);
            Assert.AreEqual(null, l[i++]);
            Assert.AreEqual(null, l[i++]);
        }
#endif

        [Test]
        public void CaseInsensitiveRequiredPropertyConstructorCreation()
        {
            FooRequired foo1 = new FooRequired(new[] { "A", "B", "C" });
            string json = JsonConvert.SerializeObject(foo1);

            StringAssert.AreEqual(@"{""Bars"":[""A"",""B"",""C""]}", json);

            FooRequired foo2 = JsonConvert.DeserializeObject<FooRequired>(json);
            Assert.AreEqual(foo1.Bars.Count, foo2.Bars.Count);
            Assert.AreEqual(foo1.Bars[0], foo2.Bars[0]);
            Assert.AreEqual(foo1.Bars[1], foo2.Bars[1]);
            Assert.AreEqual(foo1.Bars[2], foo2.Bars[2]);
        }

        public class FooRequired
        {
            [JsonProperty(Required = Required.Always)]
            public List<string> Bars { get; private set; }

            public FooRequired(IEnumerable<string> bars)
            {
                Bars = new List<string>();
                if (bars != null)
                {
                    Bars.AddRange(bars);
                }
            }
        }

        [Test]
        public void CoercedEmptyStringWithRequired()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Binding>("{requiredProperty:''}"); }, "Required property 'RequiredProperty' expects a value but got null. Path '', line 1, position 21.");
        }

        [Test]
        public void CoercedEmptyStringWithRequired_DisallowNull()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Binding_DisallowNull>("{requiredProperty:''}"); }, "Required property 'RequiredProperty' expects a non-null value. Path '', line 1, position 21.");
        }

        [Test]
        public void DisallowNull_NoValue()
        {
            Binding_DisallowNull o = JsonConvert.DeserializeObject<Binding_DisallowNull>("{}");
            Assert.IsNull(o.RequiredProperty);
        }

        [Test]
        public void CoercedEmptyStringWithRequiredConstructor()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<FooRequired>("{Bars:''}"); }, "Required property 'Bars' expects a value but got null. Path '', line 1, position 9.");
        }

        public class IgnoredProperty
        {
            [JsonIgnore]
            [JsonProperty(Required = Required.Always)]
            public string StringProp1 { get; set; }

            [JsonIgnore]
            public string StringProp2 { get; set; }
        }

        [Test]
        public void NoErrorWhenValueDoesNotMatchIgnoredProperty()
        {
            IgnoredProperty p = JsonConvert.DeserializeObject<IgnoredProperty>("{'StringProp1':[1,2,3],'StringProp2':{}}");
            Assert.IsNull(p.StringProp1);
            Assert.IsNull(p.StringProp2);
        }

        public class Binding
        {
            [JsonProperty(Required = Required.Always)]
            public Binding RequiredProperty { get; set; }
        }

        public class Binding_DisallowNull
        {
            [JsonProperty(Required = Required.DisallowNull)]
            public Binding RequiredProperty { get; set; }
        }

        [Test]
        public void Serialize_Required_DisallowedNull()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(new Binding_DisallowNull()); }, "Cannot write a null value for property 'RequiredProperty'. Property requires a non-null value. Path ''.");
        }

        [Test]
        public void Serialize_Required_DisallowedNull_NullValueHandlingIgnore()
        {
            string json = JsonConvert.SerializeObject(new Binding_DisallowNull(), new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Assert.AreEqual("{}", json);
        }

        [JsonObject(ItemRequired = Required.DisallowNull)]
        public class DictionaryWithNoNull
        {
            public string Name { get; set; }
        }

        [Test]
        public void Serialize_ItemRequired_DisallowedNull()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(new DictionaryWithNoNull()); }, "Cannot write a null value for property 'Name'. Property requires a non-null value. Path ''.");
        }

        public class DictionaryKeyContractResolver : DefaultContractResolver
        {
            protected override string ResolveDictionaryKey(string dictionaryKey)
            {
                return dictionaryKey;
            }

            protected override string ResolvePropertyName(string propertyName)
            {
#if DNXCORE50
                return propertyName.ToUpperInvariant();
#else
                return propertyName.ToUpper(CultureInfo.InvariantCulture);
#endif
            }
        }

        [Test]
        public void DictionaryKeyContractResolverTest()
        {
            var person = new
            {
                Name = "James",
                Age = 1,
                RoleNames = new Dictionary<string, bool>
                {
                    { "IsAdmin", true },
                    { "IsModerator", false }
                }
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new DictionaryKeyContractResolver()
            });

            Assert.AreEqual(@"{
  ""NAME"": ""James"",
  ""AGE"": 1,
  ""ROLENAMES"": {
    ""IsAdmin"": true,
    ""IsModerator"": false
  }
}", json);
        }

        [Test]
        public void IncompleteContainers()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                 () => JsonConvert.DeserializeObject<IList<object>>("[1,"),
                 "Unexpected end when deserializing array. Path '[0]', line 1, position 3.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IList<int>>("[1,"),
                "Unexpected end when deserializing array. Path '[0]', line 1, position 3.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IList<int>>("[1"),
                "Unexpected end when deserializing array. Path '[0]', line 1, position 2.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IDictionary<string, int>>("{'key':1,"),
                "Unexpected end when deserializing object. Path 'key', line 1, position 9.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IDictionary<string, int>>("{'key':1"),
                "Unexpected end when deserializing object. Path 'key', line 1, position 8.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IncompleteTestClass>("{'key':1,"),
                "Unexpected end when deserializing object. Path 'key', line 1, position 9.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<IncompleteTestClass>("{'key':1"),
                "Unexpected end when deserializing object. Path 'key', line 1, position 8.");
        }

        public class IncompleteTestClass
        {
            public int Key { get; set; }
        }

#if !NET20
        public enum EnumA
        {
            [EnumMember(Value = "value_a")]
            ValueA
        }

        [Test]
        public void DeserializeEnumsByName()
        {
            var e1 = JsonConvert.DeserializeObject<EnumA>("'ValueA'");
            Assert.AreEqual(EnumA.ValueA, e1);

            var e2 = JsonConvert.DeserializeObject<EnumA>("'value_a'", new StringEnumConverter());
            Assert.AreEqual(EnumA.ValueA, e2);
        }
#endif

        public class RequiredPropertyTestClass
        {
            [JsonRequired]
            internal string Name { get; set; }
        }

        [Test]
        public void RequiredPropertyTest()
        {
            RequiredPropertyTestClass c1 = new RequiredPropertyTestClass();

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.SerializeObject(c1),
                "Cannot write a null value for property 'Name'. Property requires a value. Path ''.");

            RequiredPropertyTestClass c2 = new RequiredPropertyTestClass
            {
                Name = "Name!"
            };

            string json = JsonConvert.SerializeObject(c2);

            Assert.AreEqual(@"{""Name"":""Name!""}", json);

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RequiredPropertyTestClass>(@"{}"),
                "Required property 'Name' not found in JSON. Path '', line 1, position 2.");

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RequiredPropertyTestClass>(@"{""Name"":null}"),
                "Required property 'Name' expects a value but got null. Path '', line 1, position 13.");

            RequiredPropertyTestClass c3 = JsonConvert.DeserializeObject<RequiredPropertyTestClass>(@"{""Name"":""Name!""}");

            Assert.AreEqual("Name!", c3.Name);
        }

        public class RequiredPropertyConstructorTestClass
        {
            public RequiredPropertyConstructorTestClass(string name)
            {
                Name = name;
            }

            [JsonRequired]
            internal string Name { get; set; }
        }

        [Test]
        public void RequiredPropertyConstructorTest()
        {
            RequiredPropertyConstructorTestClass c1 = new RequiredPropertyConstructorTestClass(null);

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.SerializeObject(c1),
                "Cannot write a null value for property 'Name'. Property requires a value. Path ''.");

            RequiredPropertyConstructorTestClass c2 = new RequiredPropertyConstructorTestClass("Name!");

            string json = JsonConvert.SerializeObject(c2);

            Assert.AreEqual(@"{""Name"":""Name!""}", json);

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RequiredPropertyConstructorTestClass>(@"{}"),
                "Required property 'Name' not found in JSON. Path '', line 1, position 2.");

            RequiredPropertyConstructorTestClass c3 = JsonConvert.DeserializeObject<RequiredPropertyConstructorTestClass>(@"{""Name"":""Name!""}");

            Assert.AreEqual("Name!", c3.Name);
        }

        public class IgnoredPropertiesTestClass
        {
            [JsonIgnore]
            public Version IgnoredProperty { get; set; }

            [JsonIgnore]
            public List<Version> IgnoredList { get; set; }

            [JsonIgnore]
            public Dictionary<string, Version> IgnoredDictionary { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string Name { get; set; }
        }

        public class IgnoredPropertiesContractResolver : DefaultContractResolver
        {
            public override JsonContract ResolveContract(Type type)
            {
                if (type == typeof(Version))
                {
                    throw new Exception("Error!");
                }

                return base.ResolveContract(type);
            }
        }

        [Test]
        public void NeverResolveIgnoredPropertyTypes()
        {
            Version v = new Version(1, 2, 3, 4);

            IgnoredPropertiesTestClass c1 = new IgnoredPropertiesTestClass
            {
                IgnoredProperty = v,
                IgnoredList = new List<Version>
                {
                    v
                },
                IgnoredDictionary = new Dictionary<string, Version>
                {
                    { "Value", v }
                },
                Name = "Name!"
            };

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new IgnoredPropertiesContractResolver()
            });

            Assert.AreEqual(@"{
  ""Name"": ""Name!""
}", json);

            string deserializeJson = @"{
  ""IgnoredList"": [
    {
      ""Major"": 1,
      ""Minor"": 2,
      ""Build"": 3,
      ""Revision"": 4,
      ""MajorRevision"": 0,
      ""MinorRevision"": 4
    }
  ],
  ""IgnoredDictionary"": {
    ""Value"": {
      ""Major"": 1,
      ""Minor"": 2,
      ""Build"": 3,
      ""Revision"": 4,
      ""MajorRevision"": 0,
      ""MinorRevision"": 4
    }
  },
  ""Name"": ""Name!""
}";

            IgnoredPropertiesTestClass c2 = JsonConvert.DeserializeObject<IgnoredPropertiesTestClass>(deserializeJson, new JsonSerializerSettings
            {
                ContractResolver = new IgnoredPropertiesContractResolver()
            });

            Assert.AreEqual("Name!", c2.Name);
        }

#if !(NETFX_CORE || DNXCORE50 || NET20)
        [MetadataType(typeof(CustomerValidation))]
        public partial class CustomerWithMetadataType
        {
            public System.Guid UpdatedBy_Id { get; set; }

            public class CustomerValidation
            {
                [JsonIgnore]
                public System.Guid UpdatedBy_Id { get; set; }
            }
        }

        [Test]
        public void SerializeMetadataType()
        {
            CustomerWithMetadataType c = new CustomerWithMetadataType();
            c.UpdatedBy_Id = Guid.NewGuid();

            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual("{}", json);

            CustomerWithMetadataType c2 = JsonConvert.DeserializeObject<CustomerWithMetadataType>("{'UpdatedBy_Id':'F6E0666D-13C7-4745-B486-800812C8F6DE'}");

            Assert.AreEqual(Guid.Empty, c2.UpdatedBy_Id);
        }

        [Serializable]
        public partial class FaqItem
        {
            public FaqItem()
            {
                this.Sections = new HashSet<FaqSection>();
            }

            public int FaqId { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; }

            public virtual ICollection<FaqSection> Sections { get; set; }
        }

        [MetadataType(typeof(FaqItemMetadata))]
        partial class FaqItem
        {
            [JsonProperty("FullSectionsProp")]
            public ICollection<FaqSection> FullSections
            {
                get { return Sections; }
            }
        }

        public class FaqItemMetadata
        {
            [JsonIgnore]
            public virtual ICollection<FaqSection> Sections { get; set; }
        }

        public class FaqSection
        {
        }

        public class FaqItemProxy : FaqItem
        {
            public bool IsProxy { get; set; }

            public override ICollection<FaqSection> Sections
            {
                get { return base.Sections; }
                set { base.Sections = value; }
            }
        }

        [Test]
        public void SerializeMetadataType2()
        {
            FaqItem c = new FaqItem();
            c.FaqId = 1;
            c.Sections.Add(new FaqSection());

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""FaqId"": 1,
  ""Name"": null,
  ""IsDeleted"": false,
  ""FullSectionsProp"": [
    {}
  ]
}", json);

            FaqItem c2 = JsonConvert.DeserializeObject<FaqItem>(json);

            Assert.AreEqual(1, c2.FaqId);
            Assert.AreEqual(1, c2.Sections.Count);
        }

        [Test]
        public void SerializeMetadataTypeInheritance()
        {
            FaqItemProxy c = new FaqItemProxy();
            c.FaqId = 1;
            c.Sections.Add(new FaqSection());
            c.IsProxy = true;

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""IsProxy"": true,
  ""FaqId"": 1,
  ""Name"": null,
  ""IsDeleted"": false,
  ""FullSectionsProp"": [
    {}
  ]
}", json);

            FaqItemProxy c2 = JsonConvert.DeserializeObject<FaqItemProxy>(json);

            Assert.AreEqual(1, c2.FaqId);
            Assert.AreEqual(1, c2.Sections.Count);
        }
#endif

        public class NullTestClass
        {
            public JObject Value1 { get; set; }
            public JValue Value2 { get; set; }
            public JRaw Value3 { get; set; }
            public JToken Value4 { get; set; }
            public object Value5 { get; set; }
        }

        [Test]
        public void DeserializeNullToJTokenProperty()
        {
            NullTestClass otc = JsonConvert.DeserializeObject<NullTestClass>(@"{
    ""Value1"": null,
    ""Value2"": null,
    ""Value3"": null,
    ""Value4"": null,
    ""Value5"": null
}");
            Assert.IsNull(otc.Value1);
            Assert.AreEqual(JTokenType.Null, otc.Value2.Type);
            Assert.AreEqual(JTokenType.Raw, otc.Value3.Type);
            Assert.AreEqual(JTokenType.Null, otc.Value4.Type);
            Assert.IsNull(otc.Value5);
        }

        public class Link
        {
            /// <summary>
            /// The unique identifier.
            /// </summary>
            public int Id;

            /// <summary>
            /// The parent information identifier.
            /// </summary>
            public int ParentId;

            /// <summary>
            /// The child information identifier.
            /// </summary>
            public int ChildId;
        }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
        [Test]
        public void ReadIntegerWithError()
        {
            string json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333
}";

            Link l = JsonConvert.DeserializeObject<Link>(json, new JsonSerializerSettings
            {
                Error = (s, a) => a.ErrorContext.Handled = true
            });

            Assert.AreEqual(0, l.ChildId);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void DeserializeObservableCollection()
        {
            ObservableCollection<string> s = JsonConvert.DeserializeObject<ObservableCollection<string>>("['1','2']");
            Assert.AreEqual(2, s.Count);
            Assert.AreEqual("1", s[0]);
            Assert.AreEqual("2", s[1]);
        }
#endif

        [Test]
        public void DeserializeBoolAsStringInDictionary()
        {
            Dictionary<string, string> d = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"Test1\":false}");
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual("false", d["Test1"]);
        }

#if !NET20
        [Test]
        public void PopulateResetSettings()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""2000-01-01T01:01:01+00:00""]"));
            Assert.AreEqual(DateParseHandling.DateTime, reader.DateParseHandling);

            JsonSerializer serializer = new JsonSerializer();
            serializer.DateParseHandling = DateParseHandling.DateTimeOffset;

            IList<object> l = new List<object>();
            serializer.Populate(reader, l);

            Assert.AreEqual(typeof(DateTimeOffset), l[0].GetType());
            Assert.AreEqual(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero), l[0]);

            Assert.AreEqual(DateParseHandling.DateTime, reader.DateParseHandling);
        }
#endif

        public class BaseClass
        {
            internal bool IsTransient { get; set; }
        }

        public class ChildClass : BaseClass
        {
            public new bool IsTransient { get; set; }
        }

        [Test]
        public void NewProperty()
        {
            Assert.AreEqual(@"{""IsTransient"":true}", JsonConvert.SerializeObject(new ChildClass { IsTransient = true }));

            var childClass = JsonConvert.DeserializeObject<ChildClass>(@"{""IsTransient"":true}");
            Assert.AreEqual(true, childClass.IsTransient);
        }

        public class BaseClassVirtual
        {
            internal virtual bool IsTransient { get; set; }
        }

        public class ChildClassVirtual : BaseClassVirtual
        {
            public new virtual bool IsTransient { get; set; }
        }

        [Test]
        public void NewPropertyVirtual()
        {
            Assert.AreEqual(@"{""IsTransient"":true}", JsonConvert.SerializeObject(new ChildClassVirtual { IsTransient = true }));

            var childClass = JsonConvert.DeserializeObject<ChildClassVirtual>(@"{""IsTransient"":true}");
            Assert.AreEqual(true, childClass.IsTransient);
        }

        public class ResponseWithNewGenericProperty<T> : SimpleResponse
        {
            public new T Data { get; set; }
        }

        public class ResponseWithNewGenericPropertyVirtual<T> : SimpleResponse
        {
            public new virtual T Data { get; set; }
        }

        public class ResponseWithNewGenericPropertyOverride<T> : ResponseWithNewGenericPropertyVirtual<T>
        {
            public override T Data { get; set; }
        }

        public abstract class SimpleResponse
        {
            public string Result { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }

            protected SimpleResponse()
            {
            }

            protected SimpleResponse(string message)
            {
                Message = message;
            }
        }

        [Test]
        public void CanSerializeWithBuiltInTypeAsGenericArgument()
        {
            var input = new ResponseWithNewGenericProperty<int>()
            {
                Message = "Trying out integer as type parameter",
                Data = 25,
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericProperty<int>>(json);

            Assert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }

        [Test]
        public void CanSerializeWithBuiltInTypeAsGenericArgumentVirtual()
        {
            var input = new ResponseWithNewGenericPropertyVirtual<int>()
            {
                Message = "Trying out integer as type parameter",
                Data = 25,
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericPropertyVirtual<int>>(json);

            Assert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }

        [Test]
        public void CanSerializeWithBuiltInTypeAsGenericArgumentOverride()
        {
            var input = new ResponseWithNewGenericPropertyOverride<int>()
            {
                Message = "Trying out integer as type parameter",
                Data = 25,
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericPropertyOverride<int>>(json);

            Assert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }

        [Test]
        public void CanSerializedWithGenericClosedTypeAsArgument()
        {
            var input = new ResponseWithNewGenericProperty<List<int>>()
            {
                Message = "More complex case - generic list of int",
                Data = Enumerable.Range(50, 70).ToList(),
                Result = "This should be fine too"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericProperty<List<int>>>(json);

            CollectionAssert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }

        [Test]
        public void DeserializeVersionString()
        {
            string json = "['1.2.3.4']";
            List<Version> deserialized = JsonConvert.DeserializeObject<List<Version>>(json);

            Assert.AreEqual(1, deserialized[0].Major);
            Assert.AreEqual(2, deserialized[0].Minor);
            Assert.AreEqual(3, deserialized[0].Build);
            Assert.AreEqual(4, deserialized[0].Revision);
        }

        [Test]
        public void DeserializeVersionString_Fail()
        {
            string json = "['1.2.3.4444444444444444444444']";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<List<Version>>(json); }, @"Error converting value ""1.2.3.4444444444444444444444"" to type 'System.Version'. Path '[0]', line 1, position 31.");
        }

        [Test]
        public void DeserializeJObjectWithComments()
        {
            string json = @"/* Test */
            {
                /*Test*/""A"":/* Test */true/* Test */,
                /* Test */""B"":/* Test */false/* Test */,
                /* Test */""C"":/* Test */[
                    /* Test */
                    1/* Test */
                ]/* Test */
            }
            /* Test */";
            JObject o = (JObject)JsonConvert.DeserializeObject(json);
            Assert.AreEqual(3, o.Count);
            Assert.AreEqual(true, (bool)o["A"]);
            Assert.AreEqual(false, (bool)o["B"]);
            Assert.AreEqual(1, o["C"].Count());
            Assert.AreEqual(1, (int)o["C"][0]);

            Assert.IsTrue(JToken.DeepEquals(o, JObject.Parse(json)));

            json = @"{/* Test */}";
            o = (JObject)JsonConvert.DeserializeObject(json);
            Assert.AreEqual(0, o.Count);
            Assert.IsTrue(JToken.DeepEquals(o, JObject.Parse(json)));

            json = @"{""A"": true/* Test */}";
            o = (JObject)JsonConvert.DeserializeObject(json);
            Assert.AreEqual(1, o.Count);
            Assert.AreEqual(true, (bool)o["A"]);
            Assert.IsTrue(JToken.DeepEquals(o, JObject.Parse(json)));
        }

        public class CommentTestObject
        {
            public bool? A { get; set; }
        }

        [Test]
        public void DeserializeCommentTestObjectWithComments()
        {
            CommentTestObject o = JsonConvert.DeserializeObject<CommentTestObject>(@"{/* Test */}");
            Assert.AreEqual(null, o.A);

            o = JsonConvert.DeserializeObject<CommentTestObject>(@"{""A"": true/* Test */}");
            Assert.AreEqual(true, o.A);
        }

        [Test]
        public void JsonSerializerProperties()
        {
            JsonSerializer serializer = new JsonSerializer();

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
            serializer.Binder = customBinder;
            Assert.AreEqual(customBinder, serializer.Binder);

            serializer.CheckAdditionalContent = true;
            Assert.AreEqual(true, serializer.CheckAdditionalContent);

            serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, serializer.ConstructorHandling);

#if !(NETFX_CORE || DNXCORE50)
            serializer.Context = new StreamingContext(StreamingContextStates.Other);
            Assert.AreEqual(new StreamingContext(StreamingContextStates.Other), serializer.Context);
#endif

            CamelCasePropertyNamesContractResolver resolver = new CamelCasePropertyNamesContractResolver();
            serializer.ContractResolver = resolver;
            Assert.AreEqual(resolver, serializer.ContractResolver);

            serializer.Converters.Add(new StringEnumConverter());
            Assert.AreEqual(1, serializer.Converters.Count);

            serializer.Culture = new CultureInfo("en-nz");
            Assert.AreEqual("en-NZ", serializer.Culture.ToString());

            serializer.EqualityComparer = EqualityComparer<object>.Default;
            Assert.AreEqual(EqualityComparer<object>.Default, serializer.EqualityComparer);

            serializer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            Assert.AreEqual(DateFormatHandling.MicrosoftDateFormat, serializer.DateFormatHandling);

            serializer.DateFormatString = "yyyy";
            Assert.AreEqual("yyyy", serializer.DateFormatString);

            serializer.DateParseHandling = DateParseHandling.None;
            Assert.AreEqual(DateParseHandling.None, serializer.DateParseHandling);

            serializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            Assert.AreEqual(DateTimeZoneHandling.Utc, serializer.DateTimeZoneHandling);

            serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            Assert.AreEqual(DefaultValueHandling.IgnoreAndPopulate, serializer.DefaultValueHandling);

            serializer.FloatFormatHandling = FloatFormatHandling.Symbol;
            Assert.AreEqual(FloatFormatHandling.Symbol, serializer.FloatFormatHandling);

            serializer.FloatParseHandling = FloatParseHandling.Decimal;
            Assert.AreEqual(FloatParseHandling.Decimal, serializer.FloatParseHandling);

            serializer.Formatting = Formatting.Indented;
            Assert.AreEqual(Formatting.Indented, serializer.Formatting);

            serializer.MaxDepth = 9001;
            Assert.AreEqual(9001, serializer.MaxDepth);

            serializer.MissingMemberHandling = MissingMemberHandling.Error;
            Assert.AreEqual(MissingMemberHandling.Error, serializer.MissingMemberHandling);

            serializer.NullValueHandling = NullValueHandling.Ignore;
            Assert.AreEqual(NullValueHandling.Ignore, serializer.NullValueHandling);

            serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
            Assert.AreEqual(ObjectCreationHandling.Replace, serializer.ObjectCreationHandling);

            serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;
            Assert.AreEqual(PreserveReferencesHandling.All, serializer.PreserveReferencesHandling);

            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Assert.AreEqual(ReferenceLoopHandling.Ignore, serializer.ReferenceLoopHandling);

            IdReferenceResolver referenceResolver = new IdReferenceResolver();
            serializer.ReferenceResolver = referenceResolver;
            Assert.AreEqual(referenceResolver, serializer.ReferenceResolver);

            serializer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            Assert.AreEqual(StringEscapeHandling.EscapeNonAscii, serializer.StringEscapeHandling);

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            serializer.TraceWriter = traceWriter;
            Assert.AreEqual(traceWriter, serializer.TraceWriter);

#if !(PORTABLE || PORTABLE40 || NETFX_CORE || NET20 || DNXCORE50)
            serializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, serializer.TypeNameAssemblyFormat);
#endif

            serializer.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, serializer.TypeNameHandling);
        }

        [Test]
        public void JsonSerializerSettingsProperties()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
            settings.Binder = customBinder;
            Assert.AreEqual(customBinder, settings.Binder);

            settings.CheckAdditionalContent = true;
            Assert.AreEqual(true, settings.CheckAdditionalContent);

            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, settings.ConstructorHandling);

#if !(NETFX_CORE || DNXCORE50)
            settings.Context = new StreamingContext(StreamingContextStates.Other);
            Assert.AreEqual(new StreamingContext(StreamingContextStates.Other), settings.Context);
#endif

            CamelCasePropertyNamesContractResolver resolver = new CamelCasePropertyNamesContractResolver();
            settings.ContractResolver = resolver;
            Assert.AreEqual(resolver, settings.ContractResolver);

            settings.Converters.Add(new StringEnumConverter());
            Assert.AreEqual(1, settings.Converters.Count);

            settings.Culture = new CultureInfo("en-nz");
            Assert.AreEqual("en-NZ", settings.Culture.ToString());

            settings.EqualityComparer = EqualityComparer<object>.Default;
            Assert.AreEqual(EqualityComparer<object>.Default, settings.EqualityComparer);

            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            Assert.AreEqual(DateFormatHandling.MicrosoftDateFormat, settings.DateFormatHandling);

            settings.DateFormatString = "yyyy";
            Assert.AreEqual("yyyy", settings.DateFormatString);

            settings.DateParseHandling = DateParseHandling.None;
            Assert.AreEqual(DateParseHandling.None, settings.DateParseHandling);

            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            Assert.AreEqual(DateTimeZoneHandling.Utc, settings.DateTimeZoneHandling);

            settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            Assert.AreEqual(DefaultValueHandling.IgnoreAndPopulate, settings.DefaultValueHandling);

            settings.FloatFormatHandling = FloatFormatHandling.Symbol;
            Assert.AreEqual(FloatFormatHandling.Symbol, settings.FloatFormatHandling);

            settings.FloatParseHandling = FloatParseHandling.Decimal;
            Assert.AreEqual(FloatParseHandling.Decimal, settings.FloatParseHandling);

            settings.Formatting = Formatting.Indented;
            Assert.AreEqual(Formatting.Indented, settings.Formatting);

            settings.MaxDepth = 9001;
            Assert.AreEqual(9001, settings.MaxDepth);

            settings.MissingMemberHandling = MissingMemberHandling.Error;
            Assert.AreEqual(MissingMemberHandling.Error, settings.MissingMemberHandling);

            settings.NullValueHandling = NullValueHandling.Ignore;
            Assert.AreEqual(NullValueHandling.Ignore, settings.NullValueHandling);

            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            Assert.AreEqual(ObjectCreationHandling.Replace, settings.ObjectCreationHandling);

            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            Assert.AreEqual(PreserveReferencesHandling.All, settings.PreserveReferencesHandling);

            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Assert.AreEqual(ReferenceLoopHandling.Ignore, settings.ReferenceLoopHandling);

            IdReferenceResolver referenceResolver = new IdReferenceResolver();
#pragma warning disable 618
            settings.ReferenceResolver = referenceResolver;
            Assert.AreEqual(referenceResolver, settings.ReferenceResolver);
#pragma warning restore 618
            Assert.AreEqual(referenceResolver, settings.ReferenceResolverProvider());

            settings.ReferenceResolverProvider = () => referenceResolver;
            Assert.AreEqual(referenceResolver, settings.ReferenceResolverProvider());

            settings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            Assert.AreEqual(StringEscapeHandling.EscapeNonAscii, settings.StringEscapeHandling);

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            settings.TraceWriter = traceWriter;
            Assert.AreEqual(traceWriter, settings.TraceWriter);

#if !(PORTABLE || PORTABLE40 || NETFX_CORE || NET20 || DNXCORE50)
            settings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, settings.TypeNameAssemblyFormat);
#endif

            settings.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, settings.TypeNameHandling);
        }

        [Test]
        public void JsonSerializerProxyProperties()
        {
            JsonSerializerProxy serializerProxy = new JsonSerializerProxy(new JsonSerializerInternalReader(new JsonSerializer()));

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
            serializerProxy.Binder = customBinder;
            Assert.AreEqual(customBinder, serializerProxy.Binder);

            serializerProxy.CheckAdditionalContent = true;
            Assert.AreEqual(true, serializerProxy.CheckAdditionalContent);

            serializerProxy.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, serializerProxy.ConstructorHandling);

#if !(NETFX_CORE || DNXCORE50)
            serializerProxy.Context = new StreamingContext(StreamingContextStates.Other);
            Assert.AreEqual(new StreamingContext(StreamingContextStates.Other), serializerProxy.Context);
#endif

            CamelCasePropertyNamesContractResolver resolver = new CamelCasePropertyNamesContractResolver();
            serializerProxy.ContractResolver = resolver;
            Assert.AreEqual(resolver, serializerProxy.ContractResolver);

            serializerProxy.Converters.Add(new StringEnumConverter());
            Assert.AreEqual(1, serializerProxy.Converters.Count);

            serializerProxy.Culture = new CultureInfo("en-nz");
            Assert.AreEqual("en-NZ", serializerProxy.Culture.ToString());

            serializerProxy.EqualityComparer = EqualityComparer<object>.Default;
            Assert.AreEqual(EqualityComparer<object>.Default, serializerProxy.EqualityComparer);

            serializerProxy.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            Assert.AreEqual(DateFormatHandling.MicrosoftDateFormat, serializerProxy.DateFormatHandling);

            serializerProxy.DateFormatString = "yyyy";
            Assert.AreEqual("yyyy", serializerProxy.DateFormatString);

            serializerProxy.DateParseHandling = DateParseHandling.None;
            Assert.AreEqual(DateParseHandling.None, serializerProxy.DateParseHandling);

            serializerProxy.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            Assert.AreEqual(DateTimeZoneHandling.Utc, serializerProxy.DateTimeZoneHandling);

            serializerProxy.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            Assert.AreEqual(DefaultValueHandling.IgnoreAndPopulate, serializerProxy.DefaultValueHandling);

            serializerProxy.FloatFormatHandling = FloatFormatHandling.Symbol;
            Assert.AreEqual(FloatFormatHandling.Symbol, serializerProxy.FloatFormatHandling);

            serializerProxy.FloatParseHandling = FloatParseHandling.Decimal;
            Assert.AreEqual(FloatParseHandling.Decimal, serializerProxy.FloatParseHandling);

            serializerProxy.Formatting = Formatting.Indented;
            Assert.AreEqual(Formatting.Indented, serializerProxy.Formatting);

            serializerProxy.MaxDepth = 9001;
            Assert.AreEqual(9001, serializerProxy.MaxDepth);

            serializerProxy.MissingMemberHandling = MissingMemberHandling.Error;
            Assert.AreEqual(MissingMemberHandling.Error, serializerProxy.MissingMemberHandling);

            serializerProxy.NullValueHandling = NullValueHandling.Ignore;
            Assert.AreEqual(NullValueHandling.Ignore, serializerProxy.NullValueHandling);

            serializerProxy.ObjectCreationHandling = ObjectCreationHandling.Replace;
            Assert.AreEqual(ObjectCreationHandling.Replace, serializerProxy.ObjectCreationHandling);

            serializerProxy.PreserveReferencesHandling = PreserveReferencesHandling.All;
            Assert.AreEqual(PreserveReferencesHandling.All, serializerProxy.PreserveReferencesHandling);

            serializerProxy.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Assert.AreEqual(ReferenceLoopHandling.Ignore, serializerProxy.ReferenceLoopHandling);

            IdReferenceResolver referenceResolver = new IdReferenceResolver();
            serializerProxy.ReferenceResolver = referenceResolver;
            Assert.AreEqual(referenceResolver, serializerProxy.ReferenceResolver);

            serializerProxy.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
            Assert.AreEqual(StringEscapeHandling.EscapeNonAscii, serializerProxy.StringEscapeHandling);

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            serializerProxy.TraceWriter = traceWriter;
            Assert.AreEqual(traceWriter, serializerProxy.TraceWriter);

#if !(PORTABLE || PORTABLE40 || NETFX_CORE || NET20 || DNXCORE50)
            serializerProxy.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, serializerProxy.TypeNameAssemblyFormat);
#endif

            serializerProxy.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, serializerProxy.TypeNameHandling);
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40 || DNXCORE50)
        [Test]
        public void DeserializeISerializableIConvertible()
        {
            Ratio ratio = new Ratio(2, 1);
            string json = JsonConvert.SerializeObject(ratio);

            Assert.AreEqual(@"{""n"":2,""d"":1}", json);

            Ratio ratio2 = JsonConvert.DeserializeObject<Ratio>(json);

            Assert.AreEqual(ratio.Denominator, ratio2.Denominator);
            Assert.AreEqual(ratio.Numerator, ratio2.Numerator);
        }

        public class PreserveReferencesCallbackTestObject : ISerializable
        {
            internal string _stringValue;
            internal int _intValue;
            internal PersonReference _person1;
            internal PersonReference _person2;
            internal PersonReference _person3;
            internal PreserveReferencesCallbackTestObject _parent;
            internal SerializationInfo _serializationInfo;

            public PreserveReferencesCallbackTestObject(string stringValue, int intValue, PersonReference p1, PersonReference p2, PersonReference p3)
            {
                _stringValue = stringValue;
                _intValue = intValue;
                _person1 = p1;
                _person2 = p2;
                _person3 = p3;
            }

            protected PreserveReferencesCallbackTestObject(SerializationInfo info, StreamingContext context)
            {
                _serializationInfo = info;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("stringValue", _stringValue);
                info.AddValue("intValue", _intValue);
                info.AddValue("person1", _person1, typeof(PersonReference));
                info.AddValue("person2", _person2, typeof(PersonReference));
                info.AddValue("person3", _person3, typeof(PersonReference));
                info.AddValue("parent", _parent, typeof(PreserveReferencesCallbackTestObject));
            }

            [OnDeserialized]
            private void OnDeserializedMethod(StreamingContext context)
            {
                if (_serializationInfo == null)
                {
                    return;
                }

                _stringValue = _serializationInfo.GetString("stringValue");
                _intValue = _serializationInfo.GetInt32("intValue");
                _person1 = (PersonReference)_serializationInfo.GetValue("person1", typeof(PersonReference));
                _person2 = (PersonReference)_serializationInfo.GetValue("person2", typeof(PersonReference));
                _person3 = (PersonReference)_serializationInfo.GetValue("person3", typeof(PersonReference));
                _parent = (PreserveReferencesCallbackTestObject)_serializationInfo.GetValue("parent", typeof(PreserveReferencesCallbackTestObject));

                _serializationInfo = null;
            }
        }

        [Test]
        public void PreserveReferencesCallbackTest()
        {
            var p1 = new PersonReference
            {
                Name = "John Smith"
            };
            var p2 = new PersonReference
            {
                Name = "Mary Sue",
            };

            p1.Spouse = p2;
            p2.Spouse = p1;

            var obj = new PreserveReferencesCallbackTestObject("string!", 42, p1, p2, p1);
            obj._parent = obj;

            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(obj, settings);

            StringAssert.AreEqual(json, @"{
  ""$id"": ""1"",
  ""stringValue"": ""string!"",
  ""intValue"": 42,
  ""person1"": {
    ""$id"": ""2"",
    ""Name"": ""John Smith"",
    ""Spouse"": {
      ""$id"": ""3"",
      ""Name"": ""Mary Sue"",
      ""Spouse"": {
        ""$ref"": ""2""
      }
    }
  },
  ""person2"": {
    ""$ref"": ""3""
  },
  ""person3"": {
    ""$ref"": ""2""
  },
  ""parent"": {
    ""$ref"": ""1""
  }
}");

            PreserveReferencesCallbackTestObject obj2 = JsonConvert.DeserializeObject<PreserveReferencesCallbackTestObject>(json);

            Assert.AreEqual(obj._stringValue, obj2._stringValue);
            Assert.AreEqual(obj._intValue, obj2._intValue);
            Assert.AreEqual(obj._person1.Name, obj2._person1.Name);
            Assert.AreEqual(obj._person2.Name, obj2._person2.Name);
            Assert.AreEqual(obj._person3.Name, obj2._person3.Name);
            Assert.AreEqual(obj2._person1, obj2._person3);
            Assert.AreEqual(obj2._person1.Spouse, obj2._person2);
            Assert.AreEqual(obj2._person2.Spouse, obj2._person1);
            Assert.AreEqual(obj2._parent, obj2);
        }
#endif

        [Test]
        public void DeserializeLargeFloat()
        {
            object o = JsonConvert.DeserializeObject("100000000000000000000000000000000000000.0");

            CustomAssert.IsInstanceOfType(typeof(double), o);

            Assert.IsTrue(MathUtils.ApproxEquals(1E+38, (double)o));
        }

        [Test]
        public void SerializeDeserializeRegex()
        {
            Regex regex = new Regex("(hi)", RegexOptions.CultureInvariant);

            string json = JsonConvert.SerializeObject(regex, Formatting.Indented);

            Regex r2 = JsonConvert.DeserializeObject<Regex>(json);

            Assert.AreEqual("(hi)", r2.ToString());
            Assert.AreEqual(RegexOptions.CultureInvariant, r2.Options);
        }

        [Test]
        public void EmbedJValueStringInNewJObject()
        {
            string s = null;
            var v = new JValue(s);
            var o = JObject.FromObject(new { title = v });

            JObject oo = new JObject
            {
                { "title", v }
            };

            string output = o.ToString();

            Assert.AreEqual(null, v.Value);
            Assert.AreEqual(JTokenType.String, v.Type);

            StringAssert.AreEqual(@"{
  ""title"": null
}", output);
        }

        // bug: the generic member (T) that hides the base member will not
        // be used when serializing and deserializing the object,
        // resulting in unexpected behavior during serialization and deserialization.

        public class Foo1
        {
            public object foo { get; set; }
        }

        public class Bar1
        {
            public object bar { get; set; }
        }

        public class Foo1<T> : Foo1
        {
            public new T foo { get; set; }

            public T foo2 { get; set; }
        }

        public class FooBar1 : Foo1
        {
            public new Bar1 foo { get; set; }
        }

        [Test]
        public void BaseClassSerializesAsExpected()
        {
            var original = new Foo1 { foo = "value" };
            var json = JsonConvert.SerializeObject(original);
            var expectedJson = @"{""foo"":""value""}";
            Assert.AreEqual(expectedJson, json); // passes
        }

        [Test]
        public void BaseClassDeserializesAsExpected()
        {
            var json = @"{""foo"":""value""}";
            var deserialized = JsonConvert.DeserializeObject<Foo1>(json);
            Assert.AreEqual("value", deserialized.foo); // passes
        }

        [Test]
        public void DerivedClassHidingBasePropertySerializesAsExpected()
        {
            var original = new FooBar1 { foo = new Bar1 { bar = "value" } };
            var json = JsonConvert.SerializeObject(original);
            var expectedJson = @"{""foo"":{""bar"":""value""}}";
            Assert.AreEqual(expectedJson, json); // passes
        }

        [Test]
        public void DerivedClassHidingBasePropertyDeserializesAsExpected()
        {
            var json = @"{""foo"":{""bar"":""value""}}";
            var deserialized = JsonConvert.DeserializeObject<FooBar1>(json);
            Assert.IsNotNull(deserialized.foo); // passes
            Assert.AreEqual("value", deserialized.foo.bar); // passes
        }

        [Test]
        public void DerivedGenericClassHidingBasePropertySerializesAsExpected()
        {
            var original = new Foo1<Bar1> { foo = new Bar1 { bar = "value" }, foo2 = new Bar1 { bar = "value2" } };
            var json = JsonConvert.SerializeObject(original);
            var expectedJson = @"{""foo"":{""bar"":""value""},""foo2"":{""bar"":""value2""}}";
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void DerivedGenericClassHidingBasePropertyDeserializesAsExpected()
        {
            var json = @"{""foo"":{""bar"":""value""},""foo2"":{""bar"":""value2""}}";
            var deserialized = JsonConvert.DeserializeObject<Foo1<Bar1>>(json);
            Assert.IsNotNull(deserialized.foo2); // passes (bug only occurs for generics that /hide/ another property)
            Assert.AreEqual("value2", deserialized.foo2.bar); // also passes, with no issue
            Assert.IsNotNull(deserialized.foo);
            Assert.AreEqual("value", deserialized.foo.bar);
        }

#if !(NETFX_CORE)
        [Test]
        public void ConversionOperator()
        {
            // Creating a simple dictionary that has a non-string key
            var dictStore = new Dictionary<DictionaryKeyCast, int>();
            for (var i = 0; i < 800; i++)
            {
                dictStore.Add(new DictionaryKeyCast(i.ToString(CultureInfo.InvariantCulture), i), i);
            }
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            var jsonSerializer = JsonSerializer.Create(settings);
            var ms = new MemoryStream();

            var streamWriter = new StreamWriter(ms);
            jsonSerializer.Serialize(streamWriter, dictStore);
            streamWriter.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            var stopWatch = Stopwatch.StartNew();
            var deserialize = jsonSerializer.Deserialize(new StreamReader(ms), typeof(Dictionary<DictionaryKeyCast, int>));
            stopWatch.Stop();
        }
#endif

        internal class DictionaryKeyCast
        {
            private String _name;
            private int _number;

            public DictionaryKeyCast(String name, int number)
            {
                _name = name;
                _number = number;
            }

            public override string ToString()
            {
                return _name + " " + _number;
            }

            public static implicit operator DictionaryKeyCast(string dictionaryKey)
            {
                var strings = dictionaryKey.Split(' ');
                return new DictionaryKeyCast(strings[0], Convert.ToInt32(strings[1]));
            }
        }

#if !(NET20 || NET35)
        [DataContract]
        public class BaseDataContractWithHidden
        {
            [DataMember(Name = "virtualMember")]
            public virtual string VirtualMember { get; set; }

            [DataMember(Name = "nonVirtualMember")]
            public string NonVirtualMember { get; set; }

            public virtual object NewMember { get; set; }
        }

        public class ChildDataContractWithHidden : BaseDataContractWithHidden
        {
            [DataMember(Name = "NewMember")]
            public new virtual string NewMember { get; set; }

            public override string VirtualMember { get; set; }
            public string AddedMember { get; set; }
        }

        [Test]
        public void ChildDataContractTestWithHidden()
        {
            var cc = new ChildDataContractWithHidden
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!",
                NewMember = "NewMember!"
            };

            string result = JsonConvert.SerializeObject(cc);
            Assert.AreEqual(@"{""NewMember"":""NewMember!"",""virtualMember"":""VirtualMember!"",""nonVirtualMember"":""NonVirtualMember!""}", result);
        }

        // ignore hiding members compiler warning
#pragma warning disable 108, 114
        [DataContract]
        public class BaseWithContract
        {
            [DataMember(Name = "VirtualWithDataMemberBase")]
            public virtual string VirtualWithDataMember { get; set; }

            [DataMember]
            public virtual string Virtual { get; set; }

            [DataMember(Name = "WithDataMemberBase")]
            public string WithDataMember { get; set; }

            [DataMember]
            public string JustAProperty { get; set; }
        }

        [DataContract]
        public class BaseWithoutContract
        {
            [DataMember(Name = "VirtualWithDataMemberBase")]
            public virtual string VirtualWithDataMember { get; set; }

            [DataMember]
            public virtual string Virtual { get; set; }

            [DataMember(Name = "WithDataMemberBase")]
            public string WithDataMember { get; set; }

            [DataMember]
            public string JustAProperty { get; set; }
        }

        [DataContract]
        public class SubWithoutContractNewProperties : BaseWithContract
        {
            [DataMember(Name = "VirtualWithDataMemberSub")]
            public string VirtualWithDataMember { get; set; }

            public string Virtual { get; set; }

            [DataMember(Name = "WithDataMemberSub")]
            public string WithDataMember { get; set; }

            public string JustAProperty { get; set; }
        }

        [DataContract]
        public class SubWithoutContractVirtualProperties : BaseWithContract
        {
            public override string VirtualWithDataMember { get; set; }

            [DataMember(Name = "VirtualSub")]
            public override string Virtual { get; set; }
        }

        [DataContract]
        public class SubWithContractNewProperties : BaseWithContract
        {
            [DataMember(Name = "VirtualWithDataMemberSub")]
            public string VirtualWithDataMember { get; set; }

            [DataMember(Name = "Virtual2")]
            public string Virtual { get; set; }

            [DataMember(Name = "WithDataMemberSub")]
            public string WithDataMember { get; set; }

            [DataMember(Name = "JustAProperty2")]
            public string JustAProperty { get; set; }
        }

        [DataContract]
        public class SubWithContractVirtualProperties : BaseWithContract
        {
            [DataMember(Name = "VirtualWithDataMemberSub")]
            public virtual string VirtualWithDataMember { get; set; }
        }
#pragma warning restore 108, 114

        [Test]
        public void SubWithoutContractNewPropertiesTest()
        {
            BaseWithContract baseWith = new SubWithoutContractNewProperties
            {
                JustAProperty = "JustAProperty!",
                Virtual = "Virtual!",
                VirtualWithDataMember = "VirtualWithDataMember!",
                WithDataMember = "WithDataMember!"
            };

            baseWith.JustAProperty = "JustAProperty2!";
            baseWith.Virtual = "Virtual2!";
            baseWith.VirtualWithDataMember = "VirtualWithDataMember2!";
            baseWith.WithDataMember = "WithDataMember2!";

            string json = AssertSerializeDeserializeEqual(baseWith);

            StringAssert.AreEqual(@"{
  ""JustAProperty"": ""JustAProperty2!"",
  ""Virtual"": ""Virtual2!"",
  ""VirtualWithDataMemberBase"": ""VirtualWithDataMember2!"",
  ""VirtualWithDataMemberSub"": ""VirtualWithDataMember!"",
  ""WithDataMemberBase"": ""WithDataMember2!"",
  ""WithDataMemberSub"": ""WithDataMember!""
}", json);
        }

        [Test]
        public void SubWithoutContractVirtualPropertiesTest()
        {
            BaseWithContract baseWith = new SubWithoutContractVirtualProperties
            {
                JustAProperty = "JustAProperty!",
                Virtual = "Virtual!",
                VirtualWithDataMember = "VirtualWithDataMember!",
                WithDataMember = "WithDataMember!"
            };

            baseWith.JustAProperty = "JustAProperty2!";
            baseWith.Virtual = "Virtual2!";
            baseWith.VirtualWithDataMember = "VirtualWithDataMember2!";
            baseWith.WithDataMember = "WithDataMember2!";

            string json = JsonConvert.SerializeObject(baseWith, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""VirtualWithDataMemberBase"": ""VirtualWithDataMember2!"",
  ""VirtualSub"": ""Virtual2!"",
  ""WithDataMemberBase"": ""WithDataMember2!"",
  ""JustAProperty"": ""JustAProperty2!""
}", json);
        }

        [Test]
        public void SubWithContractNewPropertiesTest()
        {
            BaseWithContract baseWith = new SubWithContractNewProperties
            {
                JustAProperty = "JustAProperty!",
                Virtual = "Virtual!",
                VirtualWithDataMember = "VirtualWithDataMember!",
                WithDataMember = "WithDataMember!"
            };

            baseWith.JustAProperty = "JustAProperty2!";
            baseWith.Virtual = "Virtual2!";
            baseWith.VirtualWithDataMember = "VirtualWithDataMember2!";
            baseWith.WithDataMember = "WithDataMember2!";

            string json = AssertSerializeDeserializeEqual(baseWith);

            StringAssert.AreEqual(@"{
  ""JustAProperty"": ""JustAProperty2!"",
  ""JustAProperty2"": ""JustAProperty!"",
  ""Virtual"": ""Virtual2!"",
  ""Virtual2"": ""Virtual!"",
  ""VirtualWithDataMemberBase"": ""VirtualWithDataMember2!"",
  ""VirtualWithDataMemberSub"": ""VirtualWithDataMember!"",
  ""WithDataMemberBase"": ""WithDataMember2!"",
  ""WithDataMemberSub"": ""WithDataMember!""
}", json);
        }

        [Test]
        public void SubWithContractVirtualPropertiesTest()
        {
            BaseWithContract baseWith = new SubWithContractVirtualProperties
            {
                JustAProperty = "JustAProperty!",
                Virtual = "Virtual!",
                VirtualWithDataMember = "VirtualWithDataMember!",
                WithDataMember = "WithDataMember!"
            };

            baseWith.JustAProperty = "JustAProperty2!";
            baseWith.Virtual = "Virtual2!";
            baseWith.VirtualWithDataMember = "VirtualWithDataMember2!";
            baseWith.WithDataMember = "WithDataMember2!";

            string json = AssertSerializeDeserializeEqual(baseWith);

            StringAssert.AreEqual(@"{
  ""JustAProperty"": ""JustAProperty2!"",
  ""Virtual"": ""Virtual2!"",
  ""VirtualWithDataMemberBase"": ""VirtualWithDataMember2!"",
  ""VirtualWithDataMemberSub"": ""VirtualWithDataMember!"",
  ""WithDataMemberBase"": ""WithDataMember2!""
}", json);
        }

        private string AssertSerializeDeserializeEqual(object o)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer s = new DataContractJsonSerializer(o.GetType());
            s.WriteObject(ms, o);

            var data = ms.ToArray();
            JObject dataContractJson = JObject.Parse(Encoding.UTF8.GetString(data, 0, data.Length));
            dataContractJson = new JObject(dataContractJson.Properties().OrderBy(p => p.Name));

            JObject jsonNetJson = JObject.Parse(JsonConvert.SerializeObject(o));
            jsonNetJson = new JObject(jsonNetJson.Properties().OrderBy(p => p.Name));

            //Console.WriteLine("Results for " + o.GetType().Name);
            //Console.WriteLine("DataContractJsonSerializer: " + dataContractJson);
            //Console.WriteLine("JsonDotNetSerializer      : " + jsonNetJson);

            Assert.AreEqual(dataContractJson.Count, jsonNetJson.Count);
            foreach (KeyValuePair<string, JToken> property in dataContractJson)
            {
                Assert.IsTrue(JToken.DeepEquals(jsonNetJson[property.Key], property.Value), "Property not equal: " + property.Key);
            }

            return jsonNetJson.ToString();
        }
#endif

        [Test]
        public void PersonTypedObjectDeserialization()
        {
            Store store = new Store();

            string jsonText = JsonConvert.SerializeObject(store);

            Store deserializedStore = (Store)JsonConvert.DeserializeObject(jsonText, typeof(Store));

            Assert.AreEqual(store.Establised, deserializedStore.Establised);
            Assert.AreEqual(store.product.Count, deserializedStore.product.Count);

            Console.WriteLine(jsonText);
        }

        [Test]
        public void TypedObjectDeserialization()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small", "Medium", "Large" };

            string output = JsonConvert.SerializeObject(product);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": "\/Date(1230375600000+1300)\/",
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            Product deserializedProduct = (Product)JsonConvert.DeserializeObject(output, typeof(Product));

            Assert.AreEqual("Apple", deserializedProduct.Name);
            Assert.AreEqual(new DateTime(2008, 12, 28), deserializedProduct.ExpiryDate);
            Assert.AreEqual(3.99m, deserializedProduct.Price);
            Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
            Assert.AreEqual("Medium", deserializedProduct.Sizes[1]);
            Assert.AreEqual("Large", deserializedProduct.Sizes[2]);
        }

        //[Test]
        //public void Advanced()
        //{
        //  Product product = new Product();
        //  product.ExpiryDate = new DateTime(2008, 12, 28);

        //  JsonSerializer serializer = new JsonSerializer();
        //  serializer.Converters.Add(new JavaScriptDateTimeConverter());
        //  serializer.NullValueHandling = NullValueHandling.Ignore;

        //  using (StreamWriter sw = new StreamWriter(@"c:\json.txt"))
        //  using (JsonWriter writer = new JsonTextWriter(sw))
        //  {
        //    serializer.Serialize(writer, product);
        //    // {"ExpiryDate":new Date(1230375600000),"Price":0}
        //  }
        //}

        [Test]
        public void JsonConvertSerializer()
        {
            string value = @"{""Name"":""Orange"", ""Price"":3.99, ""ExpiryDate"":""01/24/2010 12:00:00""}";

            Product p = JsonConvert.DeserializeObject(value, typeof(Product)) as Product;

            Assert.AreEqual("Orange", p.Name);
            Assert.AreEqual(new DateTime(2010, 1, 24, 12, 0, 0), p.ExpiryDate);
            Assert.AreEqual(3.99m, p.Price);
        }

        [Test]
        public void DeserializeJavaScriptDate()
        {
            DateTime dateValue = new DateTime(2010, 3, 30);
            Dictionary<string, object> testDictionary = new Dictionary<string, object>();
            testDictionary["date"] = dateValue;

            string jsonText = JsonConvert.SerializeObject(testDictionary);

#if !(NET20 || NET35)
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>));
            serializer.WriteObject(ms, testDictionary);

            byte[] data = ms.ToArray();
            string output = Encoding.UTF8.GetString(data, 0, data.Length);
#endif

            Dictionary<string, object> deserializedDictionary = (Dictionary<string, object>)JsonConvert.DeserializeObject(jsonText, typeof(Dictionary<string, object>));
            DateTime deserializedDate = (DateTime)deserializedDictionary["date"];

            Assert.AreEqual(dateValue, deserializedDate);
        }

        [Test]
        public void TestMethodExecutorObject()
        {
            MethodExecutorObject executorObject = new MethodExecutorObject();
            executorObject.serverClassName = "BanSubs";
            executorObject.serverMethodParams = new object[] { "21321546", "101", "1236", "D:\\1.txt" };
            executorObject.clientGetResultFunction = "ClientBanSubsCB";

            string output = JsonConvert.SerializeObject(executorObject);

            MethodExecutorObject executorObject2 = JsonConvert.DeserializeObject(output, typeof(MethodExecutorObject)) as MethodExecutorObject;

            Assert.AreNotSame(executorObject, executorObject2);
            Assert.AreEqual(executorObject2.serverClassName, "BanSubs");
            Assert.AreEqual(executorObject2.serverMethodParams.Length, 4);
            CustomAssert.Contains(executorObject2.serverMethodParams, "101");
            Assert.AreEqual(executorObject2.clientGetResultFunction, "ClientBanSubsCB");
        }

#if !(NETFX_CORE || DNXCORE50)
        [Test]
        public void HashtableDeserialization()
        {
            string value = @"{""Name"":""Orange"", ""Price"":3.99, ""ExpiryDate"":""01/24/2010 12:00:00""}";

            Hashtable p = JsonConvert.DeserializeObject(value, typeof(Hashtable)) as Hashtable;

            Assert.AreEqual("Orange", p["Name"].ToString());
        }

        [Test]
        public void TypedHashtableDeserialization()
        {
            string value = @"{""Name"":""Orange"", ""Hash"":{""ExpiryDate"":""01/24/2010 12:00:00"",""UntypedArray"":[""01/24/2010 12:00:00""]}}";

            TypedSubHashtable p = JsonConvert.DeserializeObject(value, typeof(TypedSubHashtable)) as TypedSubHashtable;

            Assert.AreEqual("01/24/2010 12:00:00", p.Hash["ExpiryDate"].ToString());
            StringAssert.AreEqual(@"[
  ""01/24/2010 12:00:00""
]", p.Hash["UntypedArray"].ToString());
        }
#endif

        [Test]
        public void SerializeDeserializeGetOnlyProperty()
        {
            string value = JsonConvert.SerializeObject(new GetOnlyPropertyClass());

            GetOnlyPropertyClass c = JsonConvert.DeserializeObject<GetOnlyPropertyClass>(value);

            Assert.AreEqual(c.Field, "Field");
            Assert.AreEqual(c.GetOnlyProperty, "GetOnlyProperty");
        }

        [Test]
        public void SerializeDeserializeSetOnlyProperty()
        {
            string value = JsonConvert.SerializeObject(new SetOnlyPropertyClass());

            SetOnlyPropertyClass c = JsonConvert.DeserializeObject<SetOnlyPropertyClass>(value);

            Assert.AreEqual(c.Field, "Field");
        }

        [Test]
        public void JsonIgnoreAttributeTest()
        {
            string json = JsonConvert.SerializeObject(new JsonIgnoreAttributeTestClass());

            Assert.AreEqual(@"{""Field"":0,""Property"":21}", json);

            JsonIgnoreAttributeTestClass c = JsonConvert.DeserializeObject<JsonIgnoreAttributeTestClass>(@"{""Field"":99,""Property"":-1,""IgnoredField"":-1,""IgnoredObject"":[1,2,3,4,5]}");

            Assert.AreEqual(0, c.IgnoredField);
            Assert.AreEqual(99, c.Field);
        }

        [Test]
        public void GoogleSearchAPI()
        {
            string json = @"{
    results:
        [
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://www.google.com/"",
                url : ""http://www.google.com/"",
                visibleUrl : ""www.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:zhool8dxBV4J:www.google.com"",
                title : ""Google"",
                titleNoFormatting : ""Google"",
                content : ""Enables users to search the Web, Usenet, and 
images. Features include PageRank,   caching and translation of 
results, and an option to find similar pages.""
            },
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://news.google.com/"",
                url : ""http://news.google.com/"",
                visibleUrl : ""news.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:Va_XShOz_twJ:news.google.com"",
                title : ""Google News"",
                titleNoFormatting : ""Google News"",
                content : ""Aggregated headlines and a search engine of many of the world's news sources.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://groups.google.com/"",
                url : ""http://groups.google.com/"",
                visibleUrl : ""groups.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:x2uPD3hfkn0J:groups.google.com"",
                title : ""Google Groups"",
                titleNoFormatting : ""Google Groups"",
                content : ""Enables users to search and browse the Usenet 
archives which consist of over 700   million messages, and post new 
comments.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://maps.google.com/"",
                url : ""http://maps.google.com/"",
                visibleUrl : ""maps.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:dkf5u2twBXIJ:maps.google.com"",
                title : ""Google Maps"",
                titleNoFormatting : ""Google Maps"",
                content : ""Provides directions, interactive maps, and 
satellite/aerial imagery of the United   States. Can also search by 
keyword such as type of business.""
            }
        ],
        
    adResults:
        [
            {
                GsearchResultClass:""GwebSearch.ad"",
                title : ""Gartner Symposium/ITxpo"",
                content1 : ""Meet brilliant Gartner IT analysts"",
                content2 : ""20-23 May 2007- Barcelona, Spain"",
                url : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                impressionUrl : 
""http://www.google.com/uds/css/ad-indicator-on.gif?ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB"", 

                unescapedUrl : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                visibleUrl : ""www.gartner.com""
            }
        ]
}
";
            object o = JsonConvert.DeserializeObject(json);
            string s = string.Empty;
            s += s;
        }

        [Test]
        public void TorrentDeserializeTest()
        {
            string jsonText = @"{
"""":"""",
""label"": [
       [""SomeName"",6]
],
""torrents"": [
       [""192D99A5C943555CB7F00A852821CF6D6DB3008A"",201,""filename.avi"",178311826,1000,178311826,72815250,408,1603,7,121430,""NameOfLabelPrevioslyDefined"",3,6,0,8,128954,-1,0],
],
""torrentc"": ""1816000723""
}";

            JObject o = (JObject)JsonConvert.DeserializeObject(jsonText);
            Assert.AreEqual(4, o.Children().Count());

            JToken torrentsArray = (JToken)o["torrents"];
            JToken nestedTorrentsArray = (JToken)torrentsArray[0];
            Assert.AreEqual(nestedTorrentsArray.Children().Count(), 19);
        }

        [Test]
        public void JsonPropertyClassSerialize()
        {
            JsonPropertyClass test = new JsonPropertyClass();
            test.Pie = "Delicious";
            test.SweetCakesCount = int.MaxValue;

            string jsonText = JsonConvert.SerializeObject(test);

            Assert.AreEqual(@"{""pie"":""Delicious"",""pie1"":""PieChart!"",""sweet_cakes_count"":2147483647}", jsonText);

            JsonPropertyClass test2 = JsonConvert.DeserializeObject<JsonPropertyClass>(jsonText);

            Assert.AreEqual(test.Pie, test2.Pie);
            Assert.AreEqual(test.SweetCakesCount, test2.SweetCakesCount);
        }

        [Test]
        public void BadJsonPropertyClassSerialize()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(new BadJsonPropertyClass()); }, @"A member with the name 'pie' already exists on 'Newtonsoft.Json.Tests.TestObjects.BadJsonPropertyClass'. Use the JsonPropertyAttribute to specify another name.");
        }

        [Test]
        public void InvalidBackslash()
        {
            string json = @"[""vvv\jvvv""]";

            ExceptionAssert.Throws<JsonReaderException>(() => { JsonConvert.DeserializeObject<List<string>>(json); }, @"Bad JSON escape sequence: \j. Path '', line 1, position 7.");
        }

#if !(NET20 || NET35)
        [Test]
        public void Unicode()
        {
            string json = @"[""PRE\u003cPOST""]";

            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
            List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

            List<string> jsonNetResult = JsonConvert.DeserializeObject<List<string>>(json);

            Assert.AreEqual(1, jsonNetResult.Count);
            Assert.AreEqual(dataContractResult[0], jsonNetResult[0]);
        }

        [Test]
        public void BackslashEqivilence()
        {
            string json = @"[""vvv\/vvv\tvvv\""vvv\bvvv\nvvv\rvvv\\vvv\fvvv""]";

#if !(NETFX_CORE || DNXCORE50)
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<string> javaScriptSerializerResult = javaScriptSerializer.Deserialize<List<string>>(json);
#endif

            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
            List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

            List<string> jsonNetResult = JsonConvert.DeserializeObject<List<string>>(json);

            Assert.AreEqual(1, jsonNetResult.Count);
            Assert.AreEqual(dataContractResult[0], jsonNetResult[0]);
#if !(NETFX_CORE || DNXCORE50)
            Assert.AreEqual(javaScriptSerializerResult[0], jsonNetResult[0]);
#endif
        }

        [Test]
        public void DateTimeTest()
        {
            List<DateTime> testDates = new List<DateTime>
            {
                new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Local),
                new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
                new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Local),
                new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
                new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            };

            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<DateTime>));
            s.WriteObject(ms, testDates);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);

            string expected = sr.ReadToEnd();

            string result = JsonConvert.SerializeObject(testDates, new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DateTimeOffsetIso()
        {
            List<DateTimeOffset> testDates = new List<DateTimeOffset>
            {
                new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
            };

            string result = JsonConvert.SerializeObject(testDates);
            Assert.AreEqual(@"[""0100-01-01T01:01:01+00:00"",""2000-01-01T01:01:01+00:00"",""2000-01-01T01:01:01+13:00"",""2000-01-01T01:01:01-03:30""]", result);
        }

        [Test]
        public void DateTimeOffsetMsAjax()
        {
            List<DateTimeOffset> testDates = new List<DateTimeOffset>
            {
                new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
                new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
            };

            string result = JsonConvert.SerializeObject(testDates, new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat });
            Assert.AreEqual(@"[""\/Date(-59011455539000+0000)\/"",""\/Date(946688461000+0000)\/"",""\/Date(946641661000+1300)\/"",""\/Date(946701061000-0330)\/""]", result);
        }
#endif

        [Test]
        public void NonStringKeyDictionary()
        {
            Dictionary<int, int> values = new Dictionary<int, int>();
            values.Add(-5, 6);
            values.Add(int.MinValue, int.MaxValue);

            string json = JsonConvert.SerializeObject(values);

            Assert.AreEqual(@"{""-5"":6,""-2147483648"":2147483647}", json);

            Dictionary<int, int> newValues = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);

            CollectionAssert.AreEqual(values, newValues);
        }

        [Test]
        public void AnonymousObjectSerialization()
        {
            var anonymous =
                new
                {
                    StringValue = "I am a string",
                    IntValue = int.MaxValue,
                    NestedAnonymous = new { NestedValue = byte.MaxValue },
                    NestedArray = new[] { 1, 2 },
                    Product = new Product() { Name = "TestProduct" }
                };

            string json = JsonConvert.SerializeObject(anonymous);
            Assert.AreEqual(@"{""StringValue"":""I am a string"",""IntValue"":2147483647,""NestedAnonymous"":{""NestedValue"":255},""NestedArray"":[1,2],""Product"":{""Name"":""TestProduct"",""ExpiryDate"":""2000-01-01T00:00:00Z"",""Price"":0.0,""Sizes"":null}}", json);

            anonymous = JsonConvert.DeserializeAnonymousType(json, anonymous);
            Assert.AreEqual("I am a string", anonymous.StringValue);
            Assert.AreEqual(int.MaxValue, anonymous.IntValue);
            Assert.AreEqual(255, anonymous.NestedAnonymous.NestedValue);
            Assert.AreEqual(2, anonymous.NestedArray.Length);
            Assert.AreEqual(1, anonymous.NestedArray[0]);
            Assert.AreEqual(2, anonymous.NestedArray[1]);
            Assert.AreEqual("TestProduct", anonymous.Product.Name);
        }

        [Test]
        public void AnonymousObjectSerializationWithSetting()
        {
            DateTime d = new DateTime(2000, 1, 1);

            var anonymous =
                new
                {
                    DateValue = d
                };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy"
            });

            string json = JsonConvert.SerializeObject(anonymous, settings);
            Assert.AreEqual(@"{""DateValue"":""2000""}", json);

            anonymous = JsonConvert.DeserializeAnonymousType(json, anonymous, settings);
            Assert.AreEqual(d, anonymous.DateValue);
        }

        [Test]
        public void SerializeObject()
        {
            string json = JsonConvert.SerializeObject(new object());
            Assert.AreEqual("{}", json);
        }

        [Test]
        public void SerializeNull()
        {
            string json = JsonConvert.SerializeObject(null);
            Assert.AreEqual("null", json);
        }

        [Test]
        public void CanDeserializeIntArrayWhenNotFirstPropertyInJson()
        {
            string json = "{foo:'hello',bar:[1,2,3]}";
            ClassWithArray wibble = JsonConvert.DeserializeObject<ClassWithArray>(json);
            Assert.AreEqual("hello", wibble.Foo);

            Assert.AreEqual(4, wibble.Bar.Count);
            Assert.AreEqual(int.MaxValue, wibble.Bar[0]);
            Assert.AreEqual(1, wibble.Bar[1]);
            Assert.AreEqual(2, wibble.Bar[2]);
            Assert.AreEqual(3, wibble.Bar[3]);
        }

        [Test]
        public void CanDeserializeIntArray_WhenArrayIsFirstPropertyInJson()
        {
            string json = "{bar:[1,2,3], foo:'hello'}";
            ClassWithArray wibble = JsonConvert.DeserializeObject<ClassWithArray>(json);
            Assert.AreEqual("hello", wibble.Foo);

            Assert.AreEqual(4, wibble.Bar.Count);
            Assert.AreEqual(int.MaxValue, wibble.Bar[0]);
            Assert.AreEqual(1, wibble.Bar[1]);
            Assert.AreEqual(2, wibble.Bar[2]);
            Assert.AreEqual(3, wibble.Bar[3]);
        }

        [Test]
        public void ObjectCreationHandlingReplace()
        {
            string json = "{bar:[1,2,3], foo:'hello'}";

            JsonSerializer s = new JsonSerializer();
            s.ObjectCreationHandling = ObjectCreationHandling.Replace;

            ClassWithArray wibble = (ClassWithArray)s.Deserialize(new StringReader(json), typeof(ClassWithArray));

            Assert.AreEqual("hello", wibble.Foo);

            Assert.AreEqual(1, wibble.Bar.Count);
        }

        [Test]
        public void CanDeserializeSerializedJson()
        {
            ClassWithArray wibble = new ClassWithArray();
            wibble.Foo = "hello";
            wibble.Bar.Add(1);
            wibble.Bar.Add(2);
            wibble.Bar.Add(3);
            string json = JsonConvert.SerializeObject(wibble);

            ClassWithArray wibbleOut = JsonConvert.DeserializeObject<ClassWithArray>(json);
            Assert.AreEqual("hello", wibbleOut.Foo);

            Assert.AreEqual(5, wibbleOut.Bar.Count);
            Assert.AreEqual(int.MaxValue, wibbleOut.Bar[0]);
            Assert.AreEqual(int.MaxValue, wibbleOut.Bar[1]);
            Assert.AreEqual(1, wibbleOut.Bar[2]);
            Assert.AreEqual(2, wibbleOut.Bar[3]);
            Assert.AreEqual(3, wibbleOut.Bar[4]);
        }

        [Test]
        public void SerializeConverableObjects()
        {
            string json = JsonConvert.SerializeObject(new ConverableMembers(), Formatting.Indented);

            string expected = null;
#if !(NETFX_CORE || PORTABLE || DNXCORE50)
            expected = @"{
  ""String"": ""string"",
  ""Int32"": 2147483647,
  ""UInt32"": 4294967295,
  ""Byte"": 255,
  ""SByte"": 127,
  ""Short"": 32767,
  ""UShort"": 65535,
  ""Long"": 9223372036854775807,
  ""ULong"": 9223372036854775807,
  ""Double"": 1.7976931348623157E+308,
  ""Float"": 3.40282347E+38,
  ""DBNull"": null,
  ""Bool"": true,
  ""Char"": ""\u0000""
}";
#else
      expected = @"{
  ""String"": ""string"",
  ""Int32"": 2147483647,
  ""UInt32"": 4294967295,
  ""Byte"": 255,
  ""SByte"": 127,
  ""Short"": 32767,
  ""UShort"": 65535,
  ""Long"": 9223372036854775807,
  ""ULong"": 9223372036854775807,
  ""Double"": 1.7976931348623157E+308,
  ""Float"": 3.40282347E+38,
  ""Bool"": true,
  ""Char"": ""\u0000""
}";
#endif

            StringAssert.AreEqual(expected, json);

            ConverableMembers c = JsonConvert.DeserializeObject<ConverableMembers>(json);
            Assert.AreEqual("string", c.String);
            Assert.AreEqual(double.MaxValue, c.Double);
#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
            Assert.AreEqual(DBNull.Value, c.DBNull);
#endif
        }

        [Test]
        public void SerializeStack()
        {
            Stack<object> s = new Stack<object>();
            s.Push(1);
            s.Push(2);
            s.Push(3);

            string json = JsonConvert.SerializeObject(s);
            Assert.AreEqual("[3,2,1]", json);
        }

        [Test]
        public void FormattingOverride()
        {
            var obj = new { Formatting = "test" };

            JsonSerializerSettings settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            string indented = JsonConvert.SerializeObject(obj, settings);

            string none = JsonConvert.SerializeObject(obj, Formatting.None, settings);
            Assert.AreNotEqual(indented, none);
        }

        [Test]
        public void DateTimeTimeZone()
        {
            var date = new DateTime(2001, 4, 4, 0, 0, 0, DateTimeKind.Utc);

            string json = JsonConvert.SerializeObject(date);
            Assert.AreEqual(@"""2001-04-04T00:00:00Z""", json);
        }

        [Test]
        public void GuidTest()
        {
            Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");

            string json = JsonConvert.SerializeObject(new ClassWithGuid { GuidField = guid });
            Assert.AreEqual(@"{""GuidField"":""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""}", json);

            ClassWithGuid c = JsonConvert.DeserializeObject<ClassWithGuid>(json);
            Assert.AreEqual(guid, c.GuidField);
        }

        [Test]
        public void EnumTest()
        {
            string json = JsonConvert.SerializeObject(StringComparison.CurrentCultureIgnoreCase);
            Assert.AreEqual(@"1", json);

            StringComparison s = JsonConvert.DeserializeObject<StringComparison>(json);
            Assert.AreEqual(StringComparison.CurrentCultureIgnoreCase, s);
        }

        public class ClassWithTimeSpan
        {
            public TimeSpan TimeSpanField;
        }

        [Test]
        public void TimeSpanTest()
        {
            TimeSpan ts = new TimeSpan(00, 23, 59, 1);

            string json = JsonConvert.SerializeObject(new ClassWithTimeSpan { TimeSpanField = ts }, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""TimeSpanField"": ""23:59:01""
}", json);

            ClassWithTimeSpan c = JsonConvert.DeserializeObject<ClassWithTimeSpan>(json);
            Assert.AreEqual(ts, c.TimeSpanField);
        }

        [Test]
        public void JsonIgnoreAttributeOnClassTest()
        {
            string json = JsonConvert.SerializeObject(new JsonIgnoreAttributeOnClassTestClass());

            Assert.AreEqual(@"{""TheField"":0,""Property"":21}", json);

            JsonIgnoreAttributeOnClassTestClass c = JsonConvert.DeserializeObject<JsonIgnoreAttributeOnClassTestClass>(@"{""TheField"":99,""Property"":-1,""IgnoredField"":-1}");

            Assert.AreEqual(0, c.IgnoredField);
            Assert.AreEqual(99, c.Field);
        }

        [Test]
        public void ConstructorCaseSensitivity()
        {
            ConstructorCaseSensitivityClass c = new ConstructorCaseSensitivityClass("param1", "Param1", "Param2");

            string json = JsonConvert.SerializeObject(c);

            ConstructorCaseSensitivityClass deserialized = JsonConvert.DeserializeObject<ConstructorCaseSensitivityClass>(json);

            Assert.AreEqual("param1", deserialized.param1);
            Assert.AreEqual("Param1", deserialized.Param1);
            Assert.AreEqual("Param2", deserialized.Param2);
        }

        [Test]
        public void SerializerShouldUseClassConverter()
        {
            ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

            string json = JsonConvert.SerializeObject(c1);
            Assert.AreEqual(@"[""Class"",""!Test!""]", json);

            ConverterPrecedenceClass c2 = JsonConvert.DeserializeObject<ConverterPrecedenceClass>(json);

            Assert.AreEqual("!Test!", c2.TestValue);
        }

        [Test]
        public void SerializerShouldUseClassConverterOverArgumentConverter()
        {
            ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

            string json = JsonConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
            Assert.AreEqual(@"[""Class"",""!Test!""]", json);

            ConverterPrecedenceClass c2 = JsonConvert.DeserializeObject<ConverterPrecedenceClass>(json, new ArgumentConverterPrecedenceClassConverter());

            Assert.AreEqual("!Test!", c2.TestValue);
        }

        [Test]
        public void SerializerShouldUseMemberConverter_IsoDate()
        {
            DateTime testDate = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
            MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

            string json = JsonConvert.SerializeObject(m1);
            Assert.AreEqual(@"{""DefaultConverter"":""1970-01-01T00:00:00Z"",""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

            MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json);

            Assert.AreEqual(testDate, m2.DefaultConverter);
            Assert.AreEqual(testDate, m2.MemberConverter);
        }

        [Test]
        public void SerializerShouldUseMemberConverter_MsDate()
        {
            DateTime testDate = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
            MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

            string json = JsonConvert.SerializeObject(m1, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });
            Assert.AreEqual(@"{""DefaultConverter"":""\/Date(0)\/"",""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

            MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json);

            Assert.AreEqual(testDate, m2.DefaultConverter);
            Assert.AreEqual(testDate, m2.MemberConverter);
        }

        [Test]
        public void SerializerShouldUseMemberConverter_MsDate_DateParseNone()
        {
            DateTime testDate = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
            MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

            string json = JsonConvert.SerializeObject(m1, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            });
            Assert.AreEqual(@"{""DefaultConverter"":""\/Date(0)\/"",""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

            var m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });

            Assert.AreEqual(new DateTime(1970, 1, 1), m2.DefaultConverter);
            Assert.AreEqual(new DateTime(1970, 1, 1), m2.MemberConverter);
        }

        [Test]
        public void SerializerShouldUseMemberConverter_IsoDate_DateParseNone()
        {
            DateTime testDate = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
            MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

            string json = JsonConvert.SerializeObject(m1, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
            });
            Assert.AreEqual(@"{""DefaultConverter"":""1970-01-01T00:00:00Z"",""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

            MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json);

            Assert.AreEqual(testDate, m2.DefaultConverter);
            Assert.AreEqual(testDate, m2.MemberConverter);
        }

        [Test]
        public void SerializerShouldUseMemberConverterOverArgumentConverter()
        {
            DateTime testDate = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
            MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

            string json = JsonConvert.SerializeObject(m1, new JavaScriptDateTimeConverter());
            Assert.AreEqual(@"{""DefaultConverter"":new Date(0),""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

            MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json, new JavaScriptDateTimeConverter());

            Assert.AreEqual(testDate, m2.DefaultConverter);
            Assert.AreEqual(testDate, m2.MemberConverter);
        }

        [Test]
        public void ConverterAttributeExample()
        {
            DateTime date = Convert.ToDateTime("1970-01-01T00:00:00Z").ToUniversalTime();

            MemberConverterClass c = new MemberConverterClass
            {
                DefaultConverter = date,
                MemberConverter = date
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""DefaultConverter"": ""1970-01-01T00:00:00Z"",
  ""MemberConverter"": ""1970-01-01T00:00:00Z""
}", json);
        }

        [Test]
        public void SerializerShouldUseMemberConverterOverClassAndArgumentConverter()
        {
            ClassAndMemberConverterClass c1 = new ClassAndMemberConverterClass();
            c1.DefaultConverter = new ConverterPrecedenceClass("DefaultConverterValue");
            c1.MemberConverter = new ConverterPrecedenceClass("MemberConverterValue");

            string json = JsonConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
            Assert.AreEqual(@"{""DefaultConverter"":[""Class"",""DefaultConverterValue""],""MemberConverter"":[""Member"",""MemberConverterValue""]}", json);

            ClassAndMemberConverterClass c2 = JsonConvert.DeserializeObject<ClassAndMemberConverterClass>(json, new ArgumentConverterPrecedenceClassConverter());

            Assert.AreEqual("DefaultConverterValue", c2.DefaultConverter.TestValue);
            Assert.AreEqual("MemberConverterValue", c2.MemberConverter.TestValue);
        }

        [Test]
        public void IncompatibleJsonAttributeShouldThrow()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                IncompatibleJsonAttributeClass c = new IncompatibleJsonAttributeClass();
                JsonConvert.SerializeObject(c);
            }, "Unexpected value when converting date. Expected DateTime or DateTimeOffset, got Newtonsoft.Json.Tests.TestObjects.IncompatibleJsonAttributeClass.");
        }

        [Test]
        public void GenericAbstractProperty()
        {
            string json = JsonConvert.SerializeObject(new GenericImpl());
            Assert.AreEqual(@"{""Id"":0}", json);
        }

        [Test]
        public void DeserializeNullable()
        {
            string json;

            json = JsonConvert.SerializeObject((int?)null);
            Assert.AreEqual("null", json);

            json = JsonConvert.SerializeObject((int?)1);
            Assert.AreEqual("1", json);
        }

        [Test]
        public void SerializeJsonRaw()
        {
            PersonRaw personRaw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            string json;

            json = JsonConvert.SerializeObject(personRaw);
            Assert.AreEqual(@"{""first_name"":""FirstNameValue"",""RawContent"":[1,2,3,4,5],""last_name"":""LastNameValue""}", json);
        }

        [Test]
        public void DeserializeJsonRaw()
        {
            string json = @"{""first_name"":""FirstNameValue"",""RawContent"":[1,2,3,4,5],""last_name"":""LastNameValue""}";

            PersonRaw personRaw = JsonConvert.DeserializeObject<PersonRaw>(json);

            Assert.AreEqual("FirstNameValue", personRaw.FirstName);
            Assert.AreEqual("[1,2,3,4,5]", personRaw.RawContent.ToString());
            Assert.AreEqual("LastNameValue", personRaw.LastName);
        }

        [Test]
        public void DeserializeNullableMember()
        {
            UserNullable userNullablle = new UserNullable
            {
                Id = new Guid("AD6205E8-0DF4-465d-AEA6-8BA18E93A7E7"),
                FName = "FirstValue",
                LName = "LastValue",
                RoleId = 5,
                NullableRoleId = 6,
                NullRoleId = null,
                Active = true
            };

            string json = JsonConvert.SerializeObject(userNullablle);

            Assert.AreEqual(@"{""Id"":""ad6205e8-0df4-465d-aea6-8ba18e93a7e7"",""FName"":""FirstValue"",""LName"":""LastValue"",""RoleId"":5,""NullableRoleId"":6,""NullRoleId"":null,""Active"":true}", json);

            UserNullable userNullablleDeserialized = JsonConvert.DeserializeObject<UserNullable>(json);

            Assert.AreEqual(new Guid("AD6205E8-0DF4-465d-AEA6-8BA18E93A7E7"), userNullablleDeserialized.Id);
            Assert.AreEqual("FirstValue", userNullablleDeserialized.FName);
            Assert.AreEqual("LastValue", userNullablleDeserialized.LName);
            Assert.AreEqual(5, userNullablleDeserialized.RoleId);
            Assert.AreEqual(6, userNullablleDeserialized.NullableRoleId);
            Assert.AreEqual(null, userNullablleDeserialized.NullRoleId);
            Assert.AreEqual(true, userNullablleDeserialized.Active);
        }

        [Test]
        public void DeserializeInt64ToNullableDouble()
        {
            string json = @"{""Height"":1}";

            DoubleClass c = JsonConvert.DeserializeObject<DoubleClass>(json);
            Assert.AreEqual(1, c.Height);
        }

        [Test]
        public void SerializeTypeProperty()
        {
            string boolRef = typeof(bool).AssemblyQualifiedName;
            TypeClass typeClass = new TypeClass { TypeProperty = typeof(bool) };

            string json = JsonConvert.SerializeObject(typeClass);
            Assert.AreEqual(@"{""TypeProperty"":""" + boolRef + @"""}", json);

            TypeClass typeClass2 = JsonConvert.DeserializeObject<TypeClass>(json);
            Assert.AreEqual(typeof(bool), typeClass2.TypeProperty);

            string jsonSerializerTestRef = typeof(JsonSerializerTest).AssemblyQualifiedName;
            typeClass = new TypeClass { TypeProperty = typeof(JsonSerializerTest) };

            json = JsonConvert.SerializeObject(typeClass);
            Assert.AreEqual(@"{""TypeProperty"":""" + jsonSerializerTestRef + @"""}", json);

            typeClass2 = JsonConvert.DeserializeObject<TypeClass>(json);
            Assert.AreEqual(typeof(JsonSerializerTest), typeClass2.TypeProperty);
        }

        [Test]
        public void RequiredMembersClass()
        {
            RequiredMembersClass c = new RequiredMembersClass()
            {
                BirthDate = new DateTime(2000, 12, 20, 10, 55, 55, DateTimeKind.Utc),
                FirstName = "Bob",
                LastName = "Smith",
                MiddleName = "Cosmo"
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""FirstName"": ""Bob"",
  ""MiddleName"": ""Cosmo"",
  ""LastName"": ""Smith"",
  ""BirthDate"": ""2000-12-20T10:55:55Z""
}", json);

            RequiredMembersClass c2 = JsonConvert.DeserializeObject<RequiredMembersClass>(json);

            Assert.AreEqual("Bob", c2.FirstName);
            Assert.AreEqual(new DateTime(2000, 12, 20, 10, 55, 55, DateTimeKind.Utc), c2.BirthDate);
        }

        [Test]
        public void DeserializeRequiredMembersClassWithNullValues()
        {
            string json = @"{
  ""FirstName"": ""I can't be null bro!"",
  ""MiddleName"": null,
  ""LastName"": null,
  ""BirthDate"": ""\/Date(977309755000)\/""
}";

            RequiredMembersClass c = JsonConvert.DeserializeObject<RequiredMembersClass>(json);

            Assert.AreEqual("I can't be null bro!", c.FirstName);
            Assert.AreEqual(null, c.MiddleName);
            Assert.AreEqual(null, c.LastName);
        }

        [Test]
        public void DeserializeRequiredMembersClassNullRequiredValueProperty()
        {
            try
            {
                string json = @"{
  ""FirstName"": null,
  ""MiddleName"": null,
  ""LastName"": null,
  ""BirthDate"": ""\/Date(977309755000)\/""
}";

                JsonConvert.DeserializeObject<RequiredMembersClass>(json);
                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Required property 'FirstName' expects a value but got null. Path ''"));
            }
        }

        [Test]
        public void SerializeRequiredMembersClassNullRequiredValueProperty()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                RequiredMembersClass requiredMembersClass = new RequiredMembersClass
                {
                    FirstName = null,
                    BirthDate = new DateTime(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc),
                    LastName = null,
                    MiddleName = null
                };

                string json = JsonConvert.SerializeObject(requiredMembersClass);
            }, "Cannot write a null value for property 'FirstName'. Property requires a value. Path ''.");
        }

        [Test]
        public void RequiredMembersClassMissingRequiredProperty()
        {
            try
            {
                string json = @"{
  ""FirstName"": ""Bob""
}";

                JsonConvert.DeserializeObject<RequiredMembersClass>(json);
                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Required property 'LastName' not found in JSON. Path ''"));
            }
        }

        [Test]
        public void SerializeJaggedArray()
        {
            JaggedArray aa = new JaggedArray();
            aa.Before = "Before!";
            aa.After = "After!";
            aa.Coordinates = new[] { new[] { 1, 1 }, new[] { 1, 2 }, new[] { 2, 1 }, new[] { 2, 2 } };

            string json = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}", json);
        }

        [Test]
        public void DeserializeJaggedArray()
        {
            string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}";

            JaggedArray aa = JsonConvert.DeserializeObject<JaggedArray>(json);

            Assert.AreEqual("Before!", aa.Before);
            Assert.AreEqual("After!", aa.After);
            Assert.AreEqual(4, aa.Coordinates.Length);
            Assert.AreEqual(2, aa.Coordinates[0].Length);
            Assert.AreEqual(1, aa.Coordinates[0][0]);
            Assert.AreEqual(2, aa.Coordinates[1][1]);

            string after = JsonConvert.SerializeObject(aa);

            Assert.AreEqual(json, after);
        }

        [Test]
        public void DeserializeGoogleGeoCode()
        {
            string json = @"{
  ""name"": ""1600 Amphitheatre Parkway, Mountain View, CA, USA"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [
    {
      ""address"": ""1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA"",
      ""AddressDetails"": {
        ""Country"": {
          ""CountryNameCode"": ""US"",
          ""AdministrativeArea"": {
            ""AdministrativeAreaName"": ""CA"",
            ""SubAdministrativeArea"": {
              ""SubAdministrativeAreaName"": ""Santa Clara"",
              ""Locality"": {
                ""LocalityName"": ""Mountain View"",
                ""Thoroughfare"": {
                  ""ThoroughfareName"": ""1600 Amphitheatre Pkwy""
                },
                ""PostalCode"": {
                  ""PostalCodeNumber"": ""94043""
                }
              }
            }
          }
        },
        ""Accuracy"": 8
      },
      ""Point"": {
        ""coordinates"": [-122.083739, 37.423021, 0]
      }
    }
  ]
}";

            GoogleMapGeocoderStructure jsonGoogleMapGeocoder = JsonConvert.DeserializeObject<GoogleMapGeocoderStructure>(json);
        }

        [Test]
        public void DeserializeInterfaceProperty()
        {
            InterfacePropertyTestClass testClass = new InterfacePropertyTestClass();
            testClass.co = new Co();
            String strFromTest = JsonConvert.SerializeObject(testClass);

            ExceptionAssert.Throws<JsonSerializationException>(() => { InterfacePropertyTestClass testFromDe = (InterfacePropertyTestClass)JsonConvert.DeserializeObject(strFromTest, typeof(InterfacePropertyTestClass)); }, @"Could not create an instance of type Newtonsoft.Json.Tests.TestObjects.ICo. Type is an interface or abstract class and cannot be instantiated. Path 'co.Name', line 1, position 14.");
        }

        private Person GetPerson()
        {
            Person person = new Person
            {
                Name = "Mike Manager",
                BirthDate = new DateTime(1983, 8, 3, 0, 0, 0, DateTimeKind.Utc),
                Department = "IT",
                LastModified = new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            };
            return person;
        }

        [Test]
        public void WriteJsonDates()
        {
            LogEntry entry = new LogEntry
            {
                LogDate = new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                Details = "Application started."
            };

            string defaultJson = JsonConvert.SerializeObject(entry);
            // {"Details":"Application started.","LogDate":"\/Date(1234656000000)\/"}

            string isoJson = JsonConvert.SerializeObject(entry, new IsoDateTimeConverter());
            // {"Details":"Application started.","LogDate":"2009-02-15T00:00:00.0000000Z"}

            string javascriptJson = JsonConvert.SerializeObject(entry, new JavaScriptDateTimeConverter());
            // {"Details":"Application started.","LogDate":new Date(1234656000000)}

            Assert.AreEqual(@"{""Details"":""Application started."",""LogDate"":""2009-02-15T00:00:00Z""}", defaultJson);
            Assert.AreEqual(@"{""Details"":""Application started."",""LogDate"":""2009-02-15T00:00:00Z""}", isoJson);
            Assert.AreEqual(@"{""Details"":""Application started."",""LogDate"":new Date(1234656000000)}", javascriptJson);
        }

        public void GenericListAndDictionaryInterfaceProperties()
        {
            GenericListAndDictionaryInterfaceProperties o = new GenericListAndDictionaryInterfaceProperties();
            o.IDictionaryProperty = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };
            o.IListProperty = new List<int>
            {
                1, 2, 3
            };
            o.IEnumerableProperty = new List<int>
            {
                4, 5, 6
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);

            Assert.AreEqual(@"{
  ""IEnumerableProperty"": [
    4,
    5,
    6
  ],
  ""IListProperty"": [
    1,
    2,
    3
  ],
  ""IDictionaryProperty"": {
    ""one"": 1,
    ""two"": 2,
    ""three"": 3
  }
}", json);

            GenericListAndDictionaryInterfaceProperties deserializedObject = JsonConvert.DeserializeObject<GenericListAndDictionaryInterfaceProperties>(json);
            Assert.IsNotNull(deserializedObject);

            CollectionAssert.AreEqual(o.IListProperty.ToArray(), deserializedObject.IListProperty.ToArray());
            CollectionAssert.AreEqual(o.IEnumerableProperty.ToArray(), deserializedObject.IEnumerableProperty.ToArray());
            CollectionAssert.AreEqual(o.IDictionaryProperty.ToArray(), deserializedObject.IDictionaryProperty.ToArray());
        }

        [Test]
        public void DeserializeBestMatchPropertyCase()
        {
            string json = @"{
  ""firstName"": ""firstName"",
  ""FirstName"": ""FirstName"",
  ""LastName"": ""LastName"",
  ""lastName"": ""lastName"",
}";

            PropertyCase o = JsonConvert.DeserializeObject<PropertyCase>(json);
            Assert.IsNotNull(o);

            Assert.AreEqual("firstName", o.firstName);
            Assert.AreEqual("FirstName", o.FirstName);
            Assert.AreEqual("LastName", o.LastName);
            Assert.AreEqual("lastName", o.lastName);
        }

        public sealed class ConstructorAndDefaultValueAttributeTestClass
        {
            public ConstructorAndDefaultValueAttributeTestClass(string testProperty1)
            {
                TestProperty1 = testProperty1;
            }

            public string TestProperty1 { get; set; }

            [DefaultValue(21)]
            public int TestProperty2 { get; set; }
        }

        [Test]
        public void PopulateDefaultValueWhenUsingConstructor()
        {
            string json = "{ 'testProperty1': 'value' }";

            ConstructorAndDefaultValueAttributeTestClass c = JsonConvert.DeserializeObject<ConstructorAndDefaultValueAttributeTestClass>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            });
            Assert.AreEqual("value", c.TestProperty1);
            Assert.AreEqual(21, c.TestProperty2);

            c = JsonConvert.DeserializeObject<ConstructorAndDefaultValueAttributeTestClass>(json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
            });
            Assert.AreEqual("value", c.TestProperty1);
            Assert.AreEqual(21, c.TestProperty2);
        }

        public sealed class ConstructorAndRequiredTestClass
        {
            public ConstructorAndRequiredTestClass(string testProperty1)
            {
                TestProperty1 = testProperty1;
            }

            public string TestProperty1 { get; set; }

            [JsonProperty(Required = Required.AllowNull)]
            public int TestProperty2 { get; set; }
        }

        [Test]
        public void RequiredWhenUsingConstructor()
        {
            try
            {
                string json = "{ 'testProperty1': 'value' }";
                JsonConvert.DeserializeObject<ConstructorAndRequiredTestClass>(json);

                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Required property 'TestProperty2' not found in JSON. Path ''"));
            }
        }

        [Test]
        public void DeserializePropertiesOnToNonDefaultConstructor()
        {
            SubKlass i = new SubKlass("my subprop");
            i.SuperProp = "overrided superprop";

            string json = JsonConvert.SerializeObject(i);
            Assert.AreEqual(@"{""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", json);

            SubKlass ii = JsonConvert.DeserializeObject<SubKlass>(json);

            string newJson = JsonConvert.SerializeObject(ii);
            Assert.AreEqual(@"{""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", newJson);
        }

        [Test]
        public void DeserializePropertiesOnToNonDefaultConstructorWithReferenceTracking()
        {
            SubKlass i = new SubKlass("my subprop");
            i.SuperProp = "overrided superprop";

            string json = JsonConvert.SerializeObject(i, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            Assert.AreEqual(@"{""$id"":""1"",""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", json);

            SubKlass ii = JsonConvert.DeserializeObject<SubKlass>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            string newJson = JsonConvert.SerializeObject(ii, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            Assert.AreEqual(@"{""$id"":""1"",""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", newJson);
        }

        [Test]
        public void SerializeJsonPropertyWithHandlingValues()
        {
            JsonPropertyWithHandlingValues o = new JsonPropertyWithHandlingValues();
            o.DefaultValueHandlingIgnoreProperty = "Default!";
            o.DefaultValueHandlingIncludeProperty = "Default!";
            o.DefaultValueHandlingPopulateProperty = "Default!";
            o.DefaultValueHandlingIgnoreAndPopulateProperty = "Default!";

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""DefaultValueHandlingIncludeProperty"": ""Default!"",
  ""DefaultValueHandlingPopulateProperty"": ""Default!"",
  ""NullValueHandlingIncludeProperty"": null,
  ""ReferenceLoopHandlingErrorProperty"": null,
  ""ReferenceLoopHandlingIgnoreProperty"": null,
  ""ReferenceLoopHandlingSerializeProperty"": null
}", json);

            json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            StringAssert.AreEqual(@"{
  ""DefaultValueHandlingIncludeProperty"": ""Default!"",
  ""DefaultValueHandlingPopulateProperty"": ""Default!"",
  ""NullValueHandlingIncludeProperty"": null
}", json);
        }

        [Test]
        public void DeserializeJsonPropertyWithHandlingValues()
        {
            string json = "{}";

            JsonPropertyWithHandlingValues o = JsonConvert.DeserializeObject<JsonPropertyWithHandlingValues>(json);
            Assert.AreEqual("Default!", o.DefaultValueHandlingIgnoreAndPopulateProperty);
            Assert.AreEqual("Default!", o.DefaultValueHandlingPopulateProperty);
            Assert.AreEqual(null, o.DefaultValueHandlingIgnoreProperty);
            Assert.AreEqual(null, o.DefaultValueHandlingIncludeProperty);
        }

        [Test]
        public void JsonPropertyWithHandlingValues_ReferenceLoopError()
        {
            string classRef = typeof(JsonPropertyWithHandlingValues).FullName;

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonPropertyWithHandlingValues o = new JsonPropertyWithHandlingValues();
                o.ReferenceLoopHandlingErrorProperty = o;

                JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }, "Self referencing loop detected for property 'ReferenceLoopHandlingErrorProperty' with type '" + classRef + "'. Path ''.");
        }

        [Test]
        public void PartialClassDeserialize()
        {
            string json = @"{
    ""request"": ""ux.settings.update"",
    ""sid"": ""14c561bd-32a8-457e-b4e5-4bba0832897f"",
    ""uid"": ""30c39065-0f31-de11-9442-001e3786a8ec"",
    ""fidOrder"": [
        ""id"",
        ""andytest_name"",
        ""andytest_age"",
        ""andytest_address"",
        ""andytest_phone"",
        ""date"",
        ""title"",
        ""titleId""
    ],
    ""entityName"": ""Andy Test"",
    ""setting"": ""entity.field.order""
}";

            RequestOnly r = JsonConvert.DeserializeObject<RequestOnly>(json);
            Assert.AreEqual("ux.settings.update", r.Request);

            NonRequest n = JsonConvert.DeserializeObject<NonRequest>(json);
            Assert.AreEqual(new Guid("14c561bd-32a8-457e-b4e5-4bba0832897f"), n.Sid);
            Assert.AreEqual(new Guid("30c39065-0f31-de11-9442-001e3786a8ec"), n.Uid);
            Assert.AreEqual(8, n.FidOrder.Count);
            Assert.AreEqual("id", n.FidOrder[0]);
            Assert.AreEqual("titleId", n.FidOrder[n.FidOrder.Count - 1]);
        }

#if !(NET20 || NETFX_CORE || DNXCORE50)
        [MetadataType(typeof(OptInClassMetadata))]
        public class OptInClass
        {
            [DataContract]
            public class OptInClassMetadata
            {
                [DataMember]
                public string Name { get; set; }

                [DataMember]
                public int Age { get; set; }

                public string NotIncluded { get; set; }
            }

            public string Name { get; set; }
            public int Age { get; set; }
            public string NotIncluded { get; set; }
        }

        [Test]
        public void OptInClassMetadataSerialization()
        {
            OptInClass optInClass = new OptInClass();
            optInClass.Age = 26;
            optInClass.Name = "James NK";
            optInClass.NotIncluded = "Poor me :(";

            string json = JsonConvert.SerializeObject(optInClass, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""James NK"",
  ""Age"": 26
}", json);

            OptInClass newOptInClass = JsonConvert.DeserializeObject<OptInClass>(@"{
  ""Name"": ""James NK"",
  ""NotIncluded"": ""Ignore me!"",
  ""Age"": 26
}");
            Assert.AreEqual(26, newOptInClass.Age);
            Assert.AreEqual("James NK", newOptInClass.Name);
            Assert.AreEqual(null, newOptInClass.NotIncluded);
        }
#endif

#if !NET20
        [DataContract]
        public class DataContractPrivateMembers
        {
            public DataContractPrivateMembers()
            {
            }

            public DataContractPrivateMembers(string name, int age, int rank, string title)
            {
                _name = name;
                Age = age;
                Rank = rank;
                Title = title;
            }

            [DataMember]
            private string _name;

            [DataMember(Name = "_age")]
            private int Age { get; set; }

            [JsonProperty]
            private int Rank { get; set; }

            [JsonProperty(PropertyName = "JsonTitle")]
            [DataMember(Name = "DataTitle")]
            private string Title { get; set; }

            public string NotIncluded { get; set; }

            public override string ToString()
            {
                return "_name: " + _name + ", _age: " + Age + ", Rank: " + Rank + ", JsonTitle: " + Title;
            }
        }

        [Test]
        public void SerializeDataContractPrivateMembers()
        {
            DataContractPrivateMembers c = new DataContractPrivateMembers("Jeff", 26, 10, "Dr");
            c.NotIncluded = "Hi";
            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""_name"": ""Jeff"",
  ""_age"": 26,
  ""Rank"": 10,
  ""JsonTitle"": ""Dr""
}", json);

            DataContractPrivateMembers cc = JsonConvert.DeserializeObject<DataContractPrivateMembers>(json);
            Assert.AreEqual("_name: Jeff, _age: 26, Rank: 10, JsonTitle: Dr", cc.ToString());
        }
#endif

        [Test]
        public void DeserializeDictionaryInterface()
        {
            string json = @"{
  ""Name"": ""Name!"",
  ""Dictionary"": {
    ""Item"": 11
  }
}";

            DictionaryInterfaceClass c = JsonConvert.DeserializeObject<DictionaryInterfaceClass>(
                json,
                new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            Assert.AreEqual("Name!", c.Name);
            Assert.AreEqual(1, c.Dictionary.Count);
            Assert.AreEqual(11, c.Dictionary["Item"]);
        }

        [Test]
        public void DeserializeDictionaryInterfaceWithExistingValues()
        {
            string json = @"{
  ""Random"": {
    ""blah"": 1
  },
  ""Name"": ""Name!"",
  ""Dictionary"": {
    ""Item"": 11,
    ""Item1"": 12
  },
  ""Collection"": [
    999
  ],
  ""Employee"": {
    ""Manager"": {
      ""Name"": ""ManagerName!""
    }
  }
}";

            DictionaryInterfaceClass c = JsonConvert.DeserializeObject<DictionaryInterfaceClass>(json,
                new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Reuse });

            Assert.AreEqual("Name!", c.Name);
            Assert.AreEqual(3, c.Dictionary.Count);
            Assert.AreEqual(11, c.Dictionary["Item"]);
            Assert.AreEqual(1, c.Dictionary["existing"]);
            Assert.AreEqual(4, c.Collection.Count);
            Assert.AreEqual(1, c.Collection.ElementAt(0));
            Assert.AreEqual(999, c.Collection.ElementAt(3));
            Assert.AreEqual("EmployeeName!", c.Employee.Name);
            Assert.AreEqual("ManagerName!", c.Employee.Manager.Name);
            Assert.IsNotNull(c.Random);
        }

        [Test]
        public void TypedObjectDeserializationWithComments()
        {
            string json = @"/*comment1*/ { /*comment2*/
        ""Name"": /*comment3*/ ""Apple"" /*comment4*/, /*comment5*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment6*/ [ /*comment7*/
          ""Small"", /*comment8*/
          ""Medium"" /*comment9*/,
          /*comment10*/ ""Large""
        /*comment11*/ ] /*comment12*/
      } /*comment13*/";

            Product deserializedProduct = (Product)JsonConvert.DeserializeObject(json, typeof(Product));

            Assert.AreEqual("Apple", deserializedProduct.Name);
            Assert.AreEqual(new DateTime(2008, 12, 28, 0, 0, 0, DateTimeKind.Utc), deserializedProduct.ExpiryDate);
            Assert.AreEqual(3.99m, deserializedProduct.Price);
            Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
            Assert.AreEqual("Medium", deserializedProduct.Sizes[1]);
            Assert.AreEqual("Large", deserializedProduct.Sizes[2]);
        }

        [Test]
        public void NestedInsideOuterObject()
        {
            string json = @"{
  ""short"": {
    ""original"": ""http://www.contrast.ie/blog/online&#45;marketing&#45;2009/"",
    ""short"": ""m2sqc6"",
    ""shortened"": ""http://short.ie/m2sqc6"",
    ""error"": {
      ""code"": 0,
      ""msg"": ""No action taken""
    }
  }
}";

            JObject o = JObject.Parse(json);

            Shortie s = JsonConvert.DeserializeObject<Shortie>(o["short"].ToString());
            Assert.IsNotNull(s);

            Assert.AreEqual(s.Original, "http://www.contrast.ie/blog/online&#45;marketing&#45;2009/");
            Assert.AreEqual(s.Short, "m2sqc6");
            Assert.AreEqual(s.Shortened, "http://short.ie/m2sqc6");
        }

        [Test]
        public void UriSerialization()
        {
            Uri uri = new Uri("http://codeplex.com");
            string json = JsonConvert.SerializeObject(uri);

            Assert.AreEqual("http://codeplex.com/", uri.ToString());

            Uri newUri = JsonConvert.DeserializeObject<Uri>(json);
            Assert.AreEqual(uri, newUri);
        }

        [Test]
        public void AnonymousPlusLinqToSql()
        {
            var value = new
            {
                bar = new JObject(new JProperty("baz", 13))
            };

            string json = JsonConvert.SerializeObject(value);

            Assert.AreEqual(@"{""bar"":{""baz"":13}}", json);
        }

        [Test]
        public void SerializeEnumerableAsObject()
        {
            Content content = new Content
            {
                Text = "Blah, blah, blah",
                Children = new List<Content>
                {
                    new Content { Text = "First" },
                    new Content { Text = "Second" }
                }
            };

            string json = JsonConvert.SerializeObject(content, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Children"": [
    {
      ""Children"": null,
      ""Text"": ""First""
    },
    {
      ""Children"": null,
      ""Text"": ""Second""
    }
  ],
  ""Text"": ""Blah, blah, blah""
}", json);
        }

        [Test]
        public void DeserializeEnumerableAsObject()
        {
            string json = @"{
  ""Children"": [
    {
      ""Children"": null,
      ""Text"": ""First""
    },
    {
      ""Children"": null,
      ""Text"": ""Second""
    }
  ],
  ""Text"": ""Blah, blah, blah""
}";

            Content content = JsonConvert.DeserializeObject<Content>(json);

            Assert.AreEqual("Blah, blah, blah", content.Text);
            Assert.AreEqual(2, content.Children.Count);
            Assert.AreEqual("First", content.Children[0].Text);
            Assert.AreEqual("Second", content.Children[1].Text);
        }

        [Test]
        public void RoleTransferTest()
        {
            string json = @"{""Operation"":""1"",""RoleName"":""Admin"",""Direction"":""0""}";

            RoleTransfer r = JsonConvert.DeserializeObject<RoleTransfer>(json);

            Assert.AreEqual(RoleTransferOperation.Second, r.Operation);
            Assert.AreEqual("Admin", r.RoleName);
            Assert.AreEqual(RoleTransferDirection.First, r.Direction);
        }

        [Test]
        public void DeserializeGenericDictionary()
        {
            string json = @"{""key1"":""value1"",""key2"":""value2""}";

            Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("value1", values["key1"]);
            Assert.AreEqual("value2", values["key2"]);
        }

#if !NET20
        [Test]
        public void DeserializeEmptyStringToNullableDateTime()
        {
            string json = @"{""DateTimeField"":""""}";

            NullableDateTimeTestClass c = JsonConvert.DeserializeObject<NullableDateTimeTestClass>(json);
            Assert.AreEqual(null, c.DateTimeField);
        }
#endif

        [Test]
        public void FailWhenClassWithNoDefaultConstructorHasMultipleConstructorsWithArguments()
        {
            string json = @"{""sublocation"":""AlertEmailSender.Program.Main"",""userId"":0,""type"":0,""summary"":""Loading settings variables"",""details"":null,""stackTrace"":""   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\r\n   at System.Environment.get_StackTrace()\r\n   at mr.Logging.Event..ctor(String summary) in C:\\Projects\\MRUtils\\Logging\\Event.vb:line 71\r\n   at AlertEmailSender.Program.Main(String[] args) in C:\\Projects\\AlertEmailSender\\AlertEmailSender\\Program.cs:line 25"",""tag"":null,""time"":""\/Date(1249591032026-0400)\/""}";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<TestObjects.Event>(json); }, @"Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.Event. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'sublocation', line 1, position 15.");
        }

        [Test]
        public void DeserializeObjectSetOnlyProperty()
        {
            string json = @"{'SetOnlyProperty':[1,2,3,4,5]}";

            SetOnlyPropertyClass2 setOnly = JsonConvert.DeserializeObject<SetOnlyPropertyClass2>(json);
            JArray a = (JArray)setOnly.GetValue();
            Assert.AreEqual(5, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(5, (int)a[a.Count - 1]);
        }

        [Test]
        public void DeserializeOptInClasses()
        {
            string json = @"{id: ""12"", name: ""test"", items: [{id: ""112"", name: ""testing""}]}";

            ListTestClass l = JsonConvert.DeserializeObject<ListTestClass>(json);
        }

        [Test]
        public void DeserializeNullableListWithNulls()
        {
            List<decimal?> l = JsonConvert.DeserializeObject<List<decimal?>>("[ 3.3, null, 1.1 ] ");
            Assert.AreEqual(3, l.Count);

            Assert.AreEqual(3.3m, l[0]);
            Assert.AreEqual(null, l[1]);
            Assert.AreEqual(1.1m, l[2]);
        }

        [Test]
        public void CannotDeserializeArrayIntoObject()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Person>(json); }, @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'Newtonsoft.Json.Tests.TestObjects.Person' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.
To fix this error either change the JSON to a JSON object (e.g. {""name"":""value""}) or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
Path '', line 1, position 1.");
        }

        [Test]
        public void CannotDeserializeArrayIntoDictionary()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Dictionary<string, string>>(json); }, @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'System.Collections.Generic.Dictionary`2[System.String,System.String]' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.
To fix this error either change the JSON to a JSON object (e.g. {""name"":""value""}) or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
Path '', line 1, position 1.");
        }

#if !(NETFX_CORE || PORTABLE || DNXCORE50)
        [Test]
        public void CannotDeserializeArrayIntoSerializable()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Exception>(json); }, @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'System.Exception' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.
To fix this error either change the JSON to a JSON object (e.g. {""name"":""value""}) or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
Path '', line 1, position 1.");
        }
#endif

        [Test]
        public void CannotDeserializeArrayIntoDouble()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonReaderException>(
                () => { JsonConvert.DeserializeObject<double>(json); },
                @"Unexpected character encountered while parsing value: [. Path '', line 1, position 1.");
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void CannotDeserializeArrayIntoDynamic()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeObject<DynamicDictionary>(json); },
                @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'Newtonsoft.Json.Tests.Linq.DynamicDictionary' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.
To fix this error either change the JSON to a JSON object (e.g. {""name"":""value""}) or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
Path '', line 1, position 1.");
        }
#endif

        [Test]
        public void CannotDeserializeArrayIntoLinqToJson()
        {
            string json = @"[]";

            ExceptionAssert.Throws<InvalidCastException>(
                () => { JsonConvert.DeserializeObject<JObject>(json); },
                new[]
                {
                    "Unable to cast object of type 'Newtonsoft.Json.Linq.JArray' to type 'Newtonsoft.Json.Linq.JObject'.",
                    "Cannot cast from source type to destination type." // mono
                });
        }

        [Test]
        public void CannotDeserializeConstructorIntoObject()
        {
            string json = @"new Constructor(123)";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Person>(json); }, @"Error converting value ""Constructor"" to type 'Newtonsoft.Json.Tests.TestObjects.Person'. Path '', line 1, position 16.");
        }

        [Test]
        public void CannotDeserializeConstructorIntoObjectNested()
        {
            string json = @"[new Constructor(123)]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<List<Person>>(json); }, @"Error converting value ""Constructor"" to type 'Newtonsoft.Json.Tests.TestObjects.Person'. Path '[0]', line 1, position 17.");
        }

        [Test]
        public void CannotDeserializeObjectIntoArray()
        {
            string json = @"{}";

            try
            {
                JsonConvert.DeserializeObject<List<Person>>(json);
                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith(@"Cannot deserialize the current JSON object (e.g. {""name"":""value""}) into type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Person]' because the type requires a JSON array (e.g. [1,2,3]) to deserialize correctly." + Environment.NewLine +
                                                    @"To fix this error either change the JSON to a JSON array (e.g. [1,2,3]) or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine +
                                                    @"Path ''"));
            }
        }

        [Test]
        public void CannotPopulateArrayIntoObject()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject(json, new Person()); }, @"Cannot populate JSON array onto type 'Newtonsoft.Json.Tests.TestObjects.Person'. Path '', line 1, position 1.");
        }

        [Test]
        public void CannotPopulateObjectIntoArray()
        {
            string json = @"{}";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject(json, new List<Person>()); }, @"Cannot populate JSON object onto type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Person]'. Path '', line 1, position 2.");
        }

        [Test]
        public void DeserializeEmptyString()
        {
            string json = @"{""Name"":""""}";

            Person p = JsonConvert.DeserializeObject<Person>(json);
            Assert.AreEqual("", p.Name);
        }

        [Test]
        public void SerializePropertyGetError()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.SerializeObject(new MemoryStream(), new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
                        IgnoreSerializableAttribute = true
#endif
                    }
                });
            }, @"Error getting value from 'ReadTimeout' on 'System.IO.MemoryStream'.");
        }

        [Test]
        public void DeserializePropertySetError()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:0}", new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
                        IgnoreSerializableAttribute = true
#endif
                    }
                });
            }, @"Error setting value to 'ReadTimeout' on 'System.IO.MemoryStream'.");
        }

        [Test]
        public void DeserializeEnsureTypeEmptyStringToIntError()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:''}", new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
                        IgnoreSerializableAttribute = true
#endif
                    }
                });
            }, @"Error converting value {null} to type 'System.Int32'. Path 'ReadTimeout', line 1, position 15.");
        }

        [Test]
        public void DeserializeEnsureTypeNullToIntError()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:null}", new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
                        IgnoreSerializableAttribute = true
#endif
                    }
                });
            }, @"Error converting value {null} to type 'System.Int32'. Path 'ReadTimeout', line 1, position 17.");
        }

        [Test]
        public void SerializeGenericListOfStrings()
        {
            List<String> strings = new List<String>();

            strings.Add("str_1");
            strings.Add("str_2");
            strings.Add("str_3");

            string json = JsonConvert.SerializeObject(strings);
            Assert.AreEqual(@"[""str_1"",""str_2"",""str_3""]", json);
        }

        [Test]
        public void ConstructorReadonlyFieldsTest()
        {
            ConstructorReadonlyFields c1 = new ConstructorReadonlyFields("String!", int.MaxValue);
            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""A"": ""String!"",
  ""B"": 2147483647
}", json);

            ConstructorReadonlyFields c2 = JsonConvert.DeserializeObject<ConstructorReadonlyFields>(json);
            Assert.AreEqual("String!", c2.A);
            Assert.AreEqual(int.MaxValue, c2.B);
        }

        [Test]
        public void SerializeStruct()
        {
            StructTest structTest = new StructTest
            {
                StringProperty = "StringProperty!",
                StringField = "StringField",
                IntProperty = 5,
                IntField = 10
            };

            string json = JsonConvert.SerializeObject(structTest, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""StringField"": ""StringField"",
  ""IntField"": 10,
  ""StringProperty"": ""StringProperty!"",
  ""IntProperty"": 5
}", json);

            StructTest deserialized = JsonConvert.DeserializeObject<StructTest>(json);
            Assert.AreEqual(structTest.StringProperty, deserialized.StringProperty);
            Assert.AreEqual(structTest.StringField, deserialized.StringField);
            Assert.AreEqual(structTest.IntProperty, deserialized.IntProperty);
            Assert.AreEqual(structTest.IntField, deserialized.IntField);
        }

        [Test]
        public void SerializeListWithJsonConverter()
        {
            Foo f = new Foo();
            f.Bars.Add(new Bar { Id = 0 });
            f.Bars.Add(new Bar { Id = 1 });
            f.Bars.Add(new Bar { Id = 2 });

            string json = JsonConvert.SerializeObject(f, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Bars"": [
    0,
    1,
    2
  ]
}", json);

            Foo newFoo = JsonConvert.DeserializeObject<Foo>(json);
            Assert.AreEqual(3, newFoo.Bars.Count);
            Assert.AreEqual(0, newFoo.Bars[0].Id);
            Assert.AreEqual(1, newFoo.Bars[1].Id);
            Assert.AreEqual(2, newFoo.Bars[2].Id);
        }

        [Test]
        public void SerializeGuidKeyedDictionary()
        {
            Dictionary<Guid, int> dictionary = new Dictionary<Guid, int>();
            dictionary.Add(new Guid("F60EAEE0-AE47-488E-B330-59527B742D77"), 1);
            dictionary.Add(new Guid("C2594C02-EBA1-426A-AA87-8DD8871350B0"), 2);

            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""f60eaee0-ae47-488e-b330-59527b742d77"": 1,
  ""c2594c02-eba1-426a-aa87-8dd8871350b0"": 2
}", json);
        }

        [Test]
        public void SerializePersonKeyedDictionary()
        {
            Dictionary<Person, int> dictionary = new Dictionary<Person, int>();
            dictionary.Add(new Person { Name = "p1" }, 1);
            dictionary.Add(new Person { Name = "p2" }, 2);

            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 2
}", json);
        }

        [Test]
        public void DeserializePersonKeyedDictionary()
        {
            try
            {
                string json =
                    @"{
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 2
}";

                JsonConvert.DeserializeObject<Dictionary<Person, int>>(json);
                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Could not convert string 'Newtonsoft.Json.Tests.TestObjects.Person' to dictionary key type 'Newtonsoft.Json.Tests.TestObjects.Person'. Create a TypeConverter to convert from the string to the key type object. Path '['Newtonsoft.Json.Tests.TestObjects.Person']'"));
            }
        }

        [Test]
        public void SerializeFragment()
        {
            string googleSearchText = @"{
        ""responseData"": {
          ""results"": [
            {
              ""GsearchResultClass"": ""GwebSearch"",
              ""unescapedUrl"": ""http://en.wikipedia.org/wiki/Paris_Hilton"",
              ""url"": ""http://en.wikipedia.org/wiki/Paris_Hilton"",
              ""visibleUrl"": ""en.wikipedia.org"",
              ""cacheUrl"": ""http://www.google.com/search?q=cache:TwrPfhd22hYJ:en.wikipedia.org"",
              ""title"": ""<b>Paris Hilton</b> - Wikipedia, the free encyclopedia"",
              ""titleNoFormatting"": ""Paris Hilton - Wikipedia, the free encyclopedia"",
              ""content"": ""[1] In 2006, she released her debut album...""
            },
            {
              ""GsearchResultClass"": ""GwebSearch"",
              ""unescapedUrl"": ""http://www.imdb.com/name/nm0385296/"",
              ""url"": ""http://www.imdb.com/name/nm0385296/"",
              ""visibleUrl"": ""www.imdb.com"",
              ""cacheUrl"": ""http://www.google.com/search?q=cache:1i34KkqnsooJ:www.imdb.com"",
              ""title"": ""<b>Paris Hilton</b>"",
              ""titleNoFormatting"": ""Paris Hilton"",
              ""content"": ""Self: Zoolander. Socialite <b>Paris Hilton</b>...""
            }
          ],
          ""cursor"": {
            ""pages"": [
              {
                ""start"": ""0"",
                ""label"": 1
              },
              {
                ""start"": ""4"",
                ""label"": 2
              },
              {
                ""start"": ""8"",
                ""label"": 3
              },
              {
                ""start"": ""12"",
                ""label"": 4
              }
            ],
            ""estimatedResultCount"": ""59600000"",
            ""currentPageIndex"": 0,
            ""moreResultsUrl"": ""http://www.google.com/search?oe=utf8&ie=utf8...""
          }
        },
        ""responseDetails"": null,
        ""responseStatus"": 200
      }";

            JObject googleSearch = JObject.Parse(googleSearchText);

            // get JSON result objects into a list
            IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

            // serialize JSON results into .NET objects
            IList<SearchResult> searchResults = new List<SearchResult>();
            foreach (JToken result in results)
            {
                SearchResult searchResult = JsonConvert.DeserializeObject<SearchResult>(result.ToString());
                searchResults.Add(searchResult);
            }

            // Title = <b>Paris Hilton</b> - Wikipedia, the free encyclopedia
            // Content = [1] In 2006, she released her debut album...
            // Url = http://en.wikipedia.org/wiki/Paris_Hilton

            // Title = <b>Paris Hilton</b>
            // Content = Self: Zoolander. Socialite <b>Paris Hilton</b>...
            // Url = http://www.imdb.com/name/nm0385296/

            Assert.AreEqual(2, searchResults.Count);
            Assert.AreEqual("<b>Paris Hilton</b> - Wikipedia, the free encyclopedia", searchResults[0].Title);
            Assert.AreEqual("<b>Paris Hilton</b>", searchResults[1].Title);
        }

        [Test]
        public void DeserializeBaseReferenceWithDerivedValue()
        {
            PersonPropertyClass personPropertyClass = new PersonPropertyClass();
            WagePerson wagePerson = (WagePerson)personPropertyClass.Person;

            wagePerson.BirthDate = new DateTime(2000, 11, 29, 23, 59, 59, DateTimeKind.Utc);
            wagePerson.Department = "McDees";
            wagePerson.HourlyWage = 12.50m;
            wagePerson.LastModified = new DateTime(2000, 11, 29, 23, 59, 59, DateTimeKind.Utc);
            wagePerson.Name = "Jim Bob";

            string json = JsonConvert.SerializeObject(personPropertyClass, Formatting.Indented);
            StringAssert.AreEqual(
                @"{
  ""Person"": {
    ""HourlyWage"": 12.50,
    ""Name"": ""Jim Bob"",
    ""BirthDate"": ""2000-11-29T23:59:59Z"",
    ""LastModified"": ""2000-11-29T23:59:59Z""
  }
}",
                json);

            PersonPropertyClass newPersonPropertyClass = JsonConvert.DeserializeObject<PersonPropertyClass>(json);
            Assert.AreEqual(wagePerson.HourlyWage, ((WagePerson)newPersonPropertyClass.Person).HourlyWage);
        }

        public class ExistingValueClass
        {
            public Dictionary<string, string> Dictionary { get; set; }
            public List<string> List { get; set; }

            public ExistingValueClass()
            {
                Dictionary = new Dictionary<string, string>
                {
                    { "existing", "yup" }
                };
                List = new List<string>
                {
                    "existing"
                };
            }
        }

        [Test]
        public void DeserializePopulateDictionaryAndList()
        {
            ExistingValueClass d = JsonConvert.DeserializeObject<ExistingValueClass>(@"{'Dictionary':{appended:'appended',existing:'new'}}");

            Assert.IsNotNull(d);
            Assert.IsNotNull(d.Dictionary);
            Assert.AreEqual(typeof(Dictionary<string, string>), d.Dictionary.GetType());
            Assert.AreEqual(typeof(List<string>), d.List.GetType());
            Assert.AreEqual(2, d.Dictionary.Count);
            Assert.AreEqual("new", d.Dictionary["existing"]);
            Assert.AreEqual("appended", d.Dictionary["appended"]);
            Assert.AreEqual(1, d.List.Count);
            Assert.AreEqual("existing", d.List[0]);
        }

        public interface IKeyValueId
        {
            int Id { get; set; }
            string Key { get; set; }
            string Value { get; set; }
        }

        public class KeyValueId : IKeyValueId
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class ThisGenericTest<T> where T : IKeyValueId
        {
            private Dictionary<string, T> _dict1 = new Dictionary<string, T>();

            public string MyProperty { get; set; }

            public void Add(T item)
            {
                _dict1.Add(item.Key, item);
            }

            public T this[string key]
            {
                get { return _dict1[key]; }
                set { _dict1[key] = value; }
            }

            public T this[int id]
            {
                get { return _dict1.Values.FirstOrDefault(x => x.Id == id); }
                set
                {
                    var item = this[id];

                    if (item == null)
                    {
                        Add(value);
                    }
                    else
                    {
                        _dict1[item.Key] = value;
                    }
                }
            }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }

            public T[] TheItems
            {
                get { return _dict1.Values.ToArray<T>(); }
                set
                {
                    foreach (var item in value)
                    {
                        Add(item);
                    }
                }
            }
        }

        [Test]
        public void IgnoreIndexedProperties()
        {
            ThisGenericTest<KeyValueId> g = new ThisGenericTest<KeyValueId>();

            g.Add(new KeyValueId { Id = 1, Key = "key1", Value = "value1" });
            g.Add(new KeyValueId { Id = 2, Key = "key2", Value = "value2" });

            g.MyProperty = "some value";

            string json = g.ToJson();

            StringAssert.AreEqual(@"{
  ""MyProperty"": ""some value"",
  ""TheItems"": [
    {
      ""Id"": 1,
      ""Key"": ""key1"",
      ""Value"": ""value1""
    },
    {
      ""Id"": 2,
      ""Key"": ""key2"",
      ""Value"": ""value2""
    }
  ]
}", json);

            ThisGenericTest<KeyValueId> gen = JsonConvert.DeserializeObject<ThisGenericTest<KeyValueId>>(json);
            Assert.AreEqual("some value", gen.MyProperty);
        }

        public class JRawValueTestObject
        {
            public JRaw Value { get; set; }
        }

        [Test]
        public void JRawValue()
        {
            JRawValueTestObject deserialized = JsonConvert.DeserializeObject<JRawValueTestObject>("{value:3}");
            Assert.AreEqual("3", deserialized.Value.ToString());

            deserialized = JsonConvert.DeserializeObject<JRawValueTestObject>("{value:'3'}");
            Assert.AreEqual(@"""3""", deserialized.Value.ToString());
        }

        [Test]
        public void DeserializeDictionaryWithNoDefaultConstructor()
        {
            string json = "{key1:'value1',key2:'value2',key3:'value3'}";

            var dic = JsonConvert.DeserializeObject<DictionaryWithNoDefaultConstructor>(json);

            Assert.AreEqual(3, dic.Count);
            Assert.AreEqual("value1", dic["key1"]);
            Assert.AreEqual("value2", dic["key2"]);
            Assert.AreEqual("value3", dic["key3"]);
        }

        [Test]
        public void DeserializeDictionaryWithNoDefaultConstructor_PreserveReferences()
        {
            string json = "{'$id':'1',key1:'value1',key2:'value2',key3:'value3'}";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<DictionaryWithNoDefaultConstructor>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            }), "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+DictionaryWithNoDefaultConstructor. Path 'key1', line 1, position 16.");
        }

        public class DictionaryWithNoDefaultConstructor : Dictionary<string, string>
        {
            public DictionaryWithNoDefaultConstructor(IEnumerable<KeyValuePair<string, string>> initial)
            {
                foreach (KeyValuePair<string, string> pair in initial)
                {
                    Add(pair.Key, pair.Value);
                }
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class A
        {
            [JsonProperty("A1")]
            private string _A1;

            public string A1
            {
                get { return _A1; }
                set { _A1 = value; }
            }

            [JsonProperty("A2")]
            private string A2 { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class B : A
        {
            public string B1 { get; set; }

            [JsonProperty("B2")]
            private string _B2;

            public string B2
            {
                get { return _B2; }
                set { _B2 = value; }
            }

            [JsonProperty("B3")]
            private string B3 { get; set; }
        }

        [Test]
        public void SerializeNonPublicBaseJsonProperties()
        {
            B value = new B();
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""B2"": null,
  ""A1"": null,
  ""B3"": null,
  ""A2"": null
}", json);
        }

#if !NET20
        public class DateTimeOffsetWrapper
        {
            public DateTimeOffset DateTimeOffsetValue { get; set; }

            public DateTime DateTimeValue { get; set; }
        }

        [Test]
        public void DeserializeDateTimeOffsetAndDateTime()
        {
            string jsonIsoText =
                @"{""DateTimeOffsetValue"":""2012-02-25T19:55:50.6095676+00:00"", ""DateTimeValue"":""2012-02-25T19:55:50.6095676+00:00""}";

            DateTimeOffsetWrapper cISO = JsonConvert.DeserializeObject<DateTimeOffsetWrapper>(jsonIsoText, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Converters =
                {
                    new IsoDateTimeConverter()
                }
            });
            DateTimeOffsetWrapper c = JsonConvert.DeserializeObject<DateTimeOffsetWrapper>(jsonIsoText, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            });

            Assert.AreEqual(c.DateTimeOffsetValue, cISO.DateTimeOffsetValue);
        }
#endif

        [Test]
        public void CircularConstructorDeserialize()
        {
            CircularConstructor1 c1 = new CircularConstructor1(null)
            {
                StringProperty = "Value!"
            };

            CircularConstructor2 c2 = new CircularConstructor2(null)
            {
                IntProperty = 1
            };

            c1.C2 = c2;
            c2.C1 = c1;

            string json = JsonConvert.SerializeObject(c1, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"{
  ""C2"": {
    ""IntProperty"": 1
  },
  ""StringProperty"": ""Value!""
}", json);

            CircularConstructor1 newC1 = JsonConvert.DeserializeObject<CircularConstructor1>(@"{
  ""C2"": {
    ""IntProperty"": 1,
    ""C1"": {}
  },
  ""StringProperty"": ""Value!""
}");

            Assert.AreEqual("Value!", newC1.StringProperty);
            Assert.AreEqual(1, newC1.C2.IntProperty);
            Assert.AreEqual(null, newC1.C2.C1.StringProperty);
            Assert.AreEqual(null, newC1.C2.C1.C2);
        }

        public class CircularConstructor1
        {
            public CircularConstructor2 C2 { get; internal set; }
            public string StringProperty { get; set; }

            public CircularConstructor1(CircularConstructor2 c2)
            {
                C2 = c2;
            }
        }

        public class CircularConstructor2
        {
            public CircularConstructor1 C1 { get; internal set; }
            public int IntProperty { get; set; }

            public CircularConstructor2(CircularConstructor1 c1)
            {
                C1 = c1;
            }
        }

        public class TestClass
        {
            public string Key { get; set; }
            public object Value { get; set; }
        }

        [Test]
        public void DeserializeToObjectProperty()
        {
            var json = "{ Key: 'abc', Value: 123 }";
            var item = JsonConvert.DeserializeObject<TestClass>(json);

            Assert.AreEqual(123L, item.Value);
        }

        public abstract class Animal
        {
            public abstract string Name { get; }
        }

        public class Human : Animal
        {
            public override string Name
            {
                get { return typeof(Human).Name; }
            }

            public string Ethnicity { get; set; }
        }

#if !(NET20 || NET35)
        public class DataContractJsonSerializerTestClass
        {
            public TimeSpan TimeSpanProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public Animal AnimalProperty { get; set; }
        }

        [Test]
        public void DataContractJsonSerializerTest()
        {
            DataContractJsonSerializerTestClass c = new DataContractJsonSerializerTestClass();
            c.TimeSpanProperty = new TimeSpan(200, 20, 59, 30, 900);
            c.GuidProperty = new Guid("66143115-BE2A-4a59-AF0A-348E1EA15B1E");
            c.AnimalProperty = new Human() { Ethnicity = "European" };

            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                typeof(DataContractJsonSerializerTestClass),
                new Type[] { typeof(Human) });
            serializer.WriteObject(ms, c);

            byte[] jsonBytes = ms.ToArray();
            string json = Encoding.UTF8.GetString(jsonBytes, 0, jsonBytes.Length);

            //Console.WriteLine(JObject.Parse(json).ToString());
            //Console.WriteLine();

            //Console.WriteLine(JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            //  {
            //    //               TypeNameHandling = TypeNameHandling.Objects
            //  }));
        }
#endif

        public class ModelStateDictionary<T> : IDictionary<string, T>
        {
            private readonly Dictionary<string, T> _innerDictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            public ModelStateDictionary()
            {
            }

            public ModelStateDictionary(ModelStateDictionary<T> dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException(nameof(dictionary));
                }

                foreach (var entry in dictionary)
                {
                    _innerDictionary.Add(entry.Key, entry.Value);
                }
            }

            public int Count
            {
                get { return _innerDictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return ((IDictionary<string, T>)_innerDictionary).IsReadOnly; }
            }

            public ICollection<string> Keys
            {
                get { return _innerDictionary.Keys; }
            }

            public T this[string key]
            {
                get
                {
                    T value;
                    _innerDictionary.TryGetValue(key, out value);
                    return value;
                }
                set { _innerDictionary[key] = value; }
            }

            public ICollection<T> Values
            {
                get { return _innerDictionary.Values; }
            }

            public void Add(KeyValuePair<string, T> item)
            {
                ((IDictionary<string, T>)_innerDictionary).Add(item);
            }

            public void Add(string key, T value)
            {
                _innerDictionary.Add(key, value);
            }

            public void Clear()
            {
                _innerDictionary.Clear();
            }

            public bool Contains(KeyValuePair<string, T> item)
            {
                return ((IDictionary<string, T>)_innerDictionary).Contains(item);
            }

            public bool ContainsKey(string key)
            {
                return _innerDictionary.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
            {
                ((IDictionary<string, T>)_innerDictionary).CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                return _innerDictionary.GetEnumerator();
            }

            public void Merge(ModelStateDictionary<T> dictionary)
            {
                if (dictionary == null)
                {
                    return;
                }

                foreach (var entry in dictionary)
                {
                    this[entry.Key] = entry.Value;
                }
            }

            public bool Remove(KeyValuePair<string, T> item)
            {
                return ((IDictionary<string, T>)_innerDictionary).Remove(item);
            }

            public bool Remove(string key)
            {
                return _innerDictionary.Remove(key);
            }

            public bool TryGetValue(string key, out T value)
            {
                return _innerDictionary.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_innerDictionary).GetEnumerator();
            }
        }

        [Test]
        public void SerializeNonIDictionary()
        {
            ModelStateDictionary<string> modelStateDictionary = new ModelStateDictionary<string>();
            modelStateDictionary.Add("key", "value");

            string json = JsonConvert.SerializeObject(modelStateDictionary);

            Assert.AreEqual(@"{""key"":""value""}", json);

            ModelStateDictionary<string> newModelStateDictionary = JsonConvert.DeserializeObject<ModelStateDictionary<string>>(json);
            Assert.AreEqual(1, newModelStateDictionary.Count);
            Assert.AreEqual("value", newModelStateDictionary["key"]);
        }

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
        public class ISerializableTestObject : ISerializable
        {
            internal string _stringValue;
            internal int _intValue;
            internal DateTimeOffset _dateTimeOffsetValue;
            internal Person _personValue;
            internal Person _nullPersonValue;
            internal int? _nullableInt;
            internal bool _booleanValue;
            internal byte _byteValue;
            internal char _charValue;
            internal DateTime _dateTimeValue;
            internal decimal _decimalValue;
            internal short _shortValue;
            internal long _longValue;
            internal sbyte _sbyteValue;
            internal float _floatValue;
            internal ushort _ushortValue;
            internal uint _uintValue;
            internal ulong _ulongValue;

            public ISerializableTestObject(string stringValue, int intValue, DateTimeOffset dateTimeOffset, Person personValue)
            {
                _stringValue = stringValue;
                _intValue = intValue;
                _dateTimeOffsetValue = dateTimeOffset;
                _personValue = personValue;
                _dateTimeValue = new DateTime(0, DateTimeKind.Utc);
            }

            protected ISerializableTestObject(SerializationInfo info, StreamingContext context)
            {
                _stringValue = info.GetString("stringValue");
                _intValue = info.GetInt32("intValue");
                _dateTimeOffsetValue = (DateTimeOffset)info.GetValue("dateTimeOffsetValue", typeof(DateTimeOffset));
                _personValue = (Person)info.GetValue("personValue", typeof(Person));
                _nullPersonValue = (Person)info.GetValue("nullPersonValue", typeof(Person));
                _nullableInt = (int?)info.GetValue("nullableInt", typeof(int?));

                _booleanValue = info.GetBoolean("booleanValue");
                _byteValue = info.GetByte("byteValue");
                _charValue = info.GetChar("charValue");
                _dateTimeValue = info.GetDateTime("dateTimeValue");
                _decimalValue = info.GetDecimal("decimalValue");
                _shortValue = info.GetInt16("shortValue");
                _longValue = info.GetInt64("longValue");
                _sbyteValue = info.GetSByte("sbyteValue");
                _floatValue = info.GetSingle("floatValue");
                _ushortValue = info.GetUInt16("ushortValue");
                _uintValue = info.GetUInt32("uintValue");
                _ulongValue = info.GetUInt64("ulongValue");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("stringValue", _stringValue);
                info.AddValue("intValue", _intValue);
                info.AddValue("dateTimeOffsetValue", _dateTimeOffsetValue);
                info.AddValue("personValue", _personValue);
                info.AddValue("nullPersonValue", _nullPersonValue);
                info.AddValue("nullableInt", null);

                info.AddValue("booleanValue", _booleanValue);
                info.AddValue("byteValue", _byteValue);
                info.AddValue("charValue", _charValue);
                info.AddValue("dateTimeValue", _dateTimeValue);
                info.AddValue("decimalValue", _decimalValue);
                info.AddValue("shortValue", _shortValue);
                info.AddValue("longValue", _longValue);
                info.AddValue("sbyteValue", _sbyteValue);
                info.AddValue("floatValue", _floatValue);
                info.AddValue("ushortValue", _ushortValue);
                info.AddValue("uintValue", _uintValue);
                info.AddValue("ulongValue", _ulongValue);
            }
        }

#if DEBUG
        [Test]
        public void SerializeISerializableInPartialTrustWithIgnoreInterface()
        {
            try
            {
                JsonTypeReflector.SetFullyTrusted(false);
                ISerializableTestObject value = new ISerializableTestObject("string!", 0, default(DateTimeOffset), null);

                string json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        IgnoreSerializableInterface = true
                    }
                });

                Assert.AreEqual("{}", json);

                value = JsonConvert.DeserializeObject<ISerializableTestObject>("{booleanValue:true}", new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        IgnoreSerializableInterface = true
                    }
                });

                Assert.IsNotNull(value);
                Assert.AreEqual(false, value._booleanValue);
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(true);
            }
        }

        [Test]
        public void SerializeISerializableInPartialTrust()
        {
            try
            {
                ExceptionAssert.Throws<JsonSerializationException>(() =>
                {
                    JsonTypeReflector.SetFullyTrusted(false);

                    JsonConvert.DeserializeObject<ISerializableTestObject>("{booleanValue:true}");
                }, @"Type 'Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+ISerializableTestObject' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                   @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine +
                   @"Path 'booleanValue', line 1, position 14.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(true);
            }
        }

        [Test]
        public void DeserializeISerializableInPartialTrust()
        {
            try
            {
                ExceptionAssert.Throws<JsonSerializationException>(() =>
                {
                    JsonTypeReflector.SetFullyTrusted(false);
                    ISerializableTestObject value = new ISerializableTestObject("string!", 0, default(DateTimeOffset), null);

                    JsonConvert.SerializeObject(value);
                }, @"Type 'Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+ISerializableTestObject' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                   @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine +
                   @"Path ''.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(true);
            }
        }
#endif

        [Test]
        public void SerializeISerializableTestObject_IsoDate()
        {
            Person person = new Person();
            person.BirthDate = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            person.LastModified = person.BirthDate;
            person.Department = "Department!";
            person.Name = "Name!";

            DateTimeOffset dateTimeOffset = new DateTimeOffset(2000, 12, 20, 22, 59, 59, TimeSpan.FromHours(2));
            string dateTimeOffsetText;
#if !NET20
            dateTimeOffsetText = @"2000-12-20T22:59:59+02:00";
#else
            dateTimeOffsetText = @"12/20/2000 22:59:59 +02:00";
#endif

            ISerializableTestObject o = new ISerializableTestObject("String!", int.MinValue, dateTimeOffset, person);

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""stringValue"": ""String!"",
  ""intValue"": -2147483648,
  ""dateTimeOffsetValue"": """ + dateTimeOffsetText + @""",
  ""personValue"": {
    ""Name"": ""Name!"",
    ""BirthDate"": ""2000-01-01T01:01:01Z"",
    ""LastModified"": ""2000-01-01T01:01:01Z""
  },
  ""nullPersonValue"": null,
  ""nullableInt"": null,
  ""booleanValue"": false,
  ""byteValue"": 0,
  ""charValue"": ""\u0000"",
  ""dateTimeValue"": ""0001-01-01T00:00:00Z"",
  ""decimalValue"": 0.0,
  ""shortValue"": 0,
  ""longValue"": 0,
  ""sbyteValue"": 0,
  ""floatValue"": 0.0,
  ""ushortValue"": 0,
  ""uintValue"": 0,
  ""ulongValue"": 0
}", json);

            ISerializableTestObject o2 = JsonConvert.DeserializeObject<ISerializableTestObject>(json);
            Assert.AreEqual("String!", o2._stringValue);
            Assert.AreEqual(int.MinValue, o2._intValue);
            Assert.AreEqual(dateTimeOffset, o2._dateTimeOffsetValue);
            Assert.AreEqual("Name!", o2._personValue.Name);
            Assert.AreEqual(null, o2._nullPersonValue);
            Assert.AreEqual(null, o2._nullableInt);
        }

        [Test]
        public void SerializeISerializableTestObject_MsAjax()
        {
            Person person = new Person();
            person.BirthDate = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            person.LastModified = person.BirthDate;
            person.Department = "Department!";
            person.Name = "Name!";

            DateTimeOffset dateTimeOffset = new DateTimeOffset(2000, 12, 20, 22, 59, 59, TimeSpan.FromHours(2));
            string dateTimeOffsetText;
#if !NET20
            dateTimeOffsetText = @"\/Date(977345999000+0200)\/";
#else
      dateTimeOffsetText = @"12/20/2000 22:59:59 +02:00";
#endif

            ISerializableTestObject o = new ISerializableTestObject("String!", int.MinValue, dateTimeOffset, person);

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });
            StringAssert.AreEqual(@"{
  ""stringValue"": ""String!"",
  ""intValue"": -2147483648,
  ""dateTimeOffsetValue"": """ + dateTimeOffsetText + @""",
  ""personValue"": {
    ""Name"": ""Name!"",
    ""BirthDate"": ""\/Date(946688461000)\/"",
    ""LastModified"": ""\/Date(946688461000)\/""
  },
  ""nullPersonValue"": null,
  ""nullableInt"": null,
  ""booleanValue"": false,
  ""byteValue"": 0,
  ""charValue"": ""\u0000"",
  ""dateTimeValue"": ""\/Date(-62135596800000)\/"",
  ""decimalValue"": 0.0,
  ""shortValue"": 0,
  ""longValue"": 0,
  ""sbyteValue"": 0,
  ""floatValue"": 0.0,
  ""ushortValue"": 0,
  ""uintValue"": 0,
  ""ulongValue"": 0
}", json);

            ISerializableTestObject o2 = JsonConvert.DeserializeObject<ISerializableTestObject>(json);
            Assert.AreEqual("String!", o2._stringValue);
            Assert.AreEqual(int.MinValue, o2._intValue);
            Assert.AreEqual(dateTimeOffset, o2._dateTimeOffsetValue);
            Assert.AreEqual("Name!", o2._personValue.Name);
            Assert.AreEqual(null, o2._nullPersonValue);
            Assert.AreEqual(null, o2._nullableInt);
        }
#endif

        public class KVPair<TKey, TValue>
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }

            public KVPair(TKey k, TValue v)
            {
                Key = k;
                Value = v;
            }
        }

        [Test]
        public void DeserializeUsingNonDefaultConstructorWithLeftOverValues()
        {
            List<KVPair<string, string>> kvPairs =
                JsonConvert.DeserializeObject<List<KVPair<string, string>>>(
                    "[{\"Key\":\"Two\",\"Value\":\"2\"},{\"Key\":\"One\",\"Value\":\"1\"}]");

            Assert.AreEqual(2, kvPairs.Count);
            Assert.AreEqual("Two", kvPairs[0].Key);
            Assert.AreEqual("2", kvPairs[0].Value);
            Assert.AreEqual("One", kvPairs[1].Key);
            Assert.AreEqual("1", kvPairs[1].Value);
        }

        [Test]
        public void SerializeClassWithInheritedProtectedMember()
        {
            AA myA = new AA(2);
            string json = JsonConvert.SerializeObject(myA, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""AA_field1"": 2,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2
}", json);

            BB myB = new BB(3, 4);
            json = JsonConvert.SerializeObject(myB, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""BB_field1"": 4,
  ""BB_field2"": 4,
  ""AA_field1"": 3,
  ""BB_property1"": 4,
  ""BB_property2"": 4,
  ""BB_property3"": 4,
  ""BB_property4"": 4,
  ""BB_property5"": 4,
  ""BB_property7"": 4,
  ""AA_property1"": 3,
  ""AA_property2"": 3,
  ""AA_property3"": 3,
  ""AA_property4"": 3
}", json);
        }

#if !(PORTABLE)
        [Test]
        public void DeserializeClassWithInheritedProtectedMember()
        {
            AA myA = JsonConvert.DeserializeObject<AA>(
                @"{
  ""AA_field1"": 2,
  ""AA_field2"": 2,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2,
  ""AA_property5"": 2,
  ""AA_property6"": 2
}");

            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myA));

            BB myB = JsonConvert.DeserializeObject<BB>(
                @"{
  ""BB_field1"": 4,
  ""BB_field2"": 4,
  ""AA_field1"": 3,
  ""AA_field2"": 3,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2,
  ""AA_property5"": 2,
  ""AA_property6"": 2,
  ""BB_property1"": 3,
  ""BB_property2"": 3,
  ""BB_property3"": 3,
  ""BB_property4"": 3,
  ""BB_property5"": 3,
  ""BB_property6"": 3,
  ""BB_property7"": 3,
  ""BB_property8"": 3
}");

            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myB));

            Assert.AreEqual(4, myB.BB_field1);
            Assert.AreEqual(4, myB.BB_field2);
            Assert.AreEqual(3, myB.BB_property1);
            Assert.AreEqual(3, myB.BB_property2);
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property3", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, myB.BB_property5);
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property6", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property7", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property8", BindingFlags.Instance | BindingFlags.Public), myB));
        }
#endif

        public class AA
        {
            [JsonProperty]
            protected int AA_field1;

            protected int AA_field2;

            [JsonProperty]
            protected int AA_property1 { get; set; }

            [JsonProperty]
            protected int AA_property2 { get; private set; }

            [JsonProperty]
            protected int AA_property3 { private get; set; }

            [JsonProperty]
            private int AA_property4 { get; set; }

            protected int AA_property5 { get; private set; }
            protected int AA_property6 { private get; set; }

            public AA()
            {
            }

            public AA(int f)
            {
                AA_field1 = f;
                AA_field2 = f;
                AA_property1 = f;
                AA_property2 = f;
                AA_property3 = f;
                AA_property4 = f;
                AA_property5 = f;
                AA_property6 = f;
            }
        }

        public class BB : AA
        {
            [JsonProperty]
            public int BB_field1;

            public int BB_field2;

            [JsonProperty]
            public int BB_property1 { get; set; }

            [JsonProperty]
            public int BB_property2 { get; private set; }

            [JsonProperty]
            public int BB_property3 { private get; set; }

            [JsonProperty]
            private int BB_property4 { get; set; }

            public int BB_property5 { get; private set; }
            public int BB_property6 { private get; set; }

            [JsonProperty]
            public int BB_property7 { protected get; set; }

            public int BB_property8 { protected get; set; }

            public BB()
            {
            }

            public BB(int f, int g)
                : base(f)
            {
                BB_field1 = g;
                BB_field2 = g;
                BB_property1 = g;
                BB_property2 = g;
                BB_property3 = g;
                BB_property4 = g;
                BB_property5 = g;
                BB_property6 = g;
                BB_property7 = g;
                BB_property8 = g;
            }
        }

#if !NET20
        public class XNodeTestObject
        {
            public XDocument Document { get; set; }
            public XElement Element { get; set; }
        }
#endif

#if !(NETFX_CORE || DNXCORE50)
        public class XmlNodeTestObject
        {
            public XmlDocument Document { get; set; }
        }
#endif

#if !(NET20 || PORTABLE40)
        [Test]
        public void SerializeDeserializeXNodeProperties()
        {
            XNodeTestObject testObject = new XNodeTestObject();
            testObject.Document = XDocument.Parse("<root>hehe, root</root>");
            testObject.Element = XElement.Parse(@"<fifth xmlns:json=""http://json.org"" json:Awesome=""true"">element</fifth>");

            string json = JsonConvert.SerializeObject(testObject, Formatting.Indented);
            string expected = @"{
  ""Document"": {
    ""root"": ""hehe, root""
  },
  ""Element"": {
    ""fifth"": {
      ""@xmlns:json"": ""http://json.org"",
      ""@json:Awesome"": ""true"",
      ""#text"": ""element""
    }
  }
}";
            StringAssert.AreEqual(expected, json);

            XNodeTestObject newTestObject = JsonConvert.DeserializeObject<XNodeTestObject>(json);
            Assert.AreEqual(testObject.Document.ToString(), newTestObject.Document.ToString());
            Assert.AreEqual(testObject.Element.ToString(), newTestObject.Element.ToString());

            Assert.IsNull(newTestObject.Element.Parent);
        }
#endif

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
        [Test]
        public void SerializeDeserializeXmlNodeProperties()
        {
            XmlNodeTestObject testObject = new XmlNodeTestObject();
            XmlDocument document = new XmlDocument();
            document.LoadXml("<root>hehe, root</root>");
            testObject.Document = document;

            string json = JsonConvert.SerializeObject(testObject, Formatting.Indented);
            string expected = @"{
  ""Document"": {
    ""root"": ""hehe, root""
  }
}";
            StringAssert.AreEqual(expected, json);

            XmlNodeTestObject newTestObject = JsonConvert.DeserializeObject<XmlNodeTestObject>(json);
            Assert.AreEqual(testObject.Document.InnerXml, newTestObject.Document.InnerXml);
        }
#endif

        [Test]
        public void FullClientMapSerialization()
        {
            ClientMap source = new ClientMap()
            {
                position = new Pos() { X = 100, Y = 200 },
                center = new PosDouble() { X = 251.6, Y = 361.3 }
            };

            string json = JsonConvert.SerializeObject(source, new PosConverter(), new PosDoubleConverter());
            Assert.AreEqual("{\"position\":new Pos(100,200),\"center\":new PosD(251.6,361.3)}", json);
        }

        public class ClientMap
        {
            public Pos position { get; set; }
            public PosDouble center { get; set; }
        }

        public class Pos
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class PosDouble
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class PosConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Pos p = (Pos)value;

                if (p != null)
                {
                    writer.WriteRawValue(String.Format("new Pos({0},{1})", p.X, p.Y));
                }
                else
                {
                    writer.WriteNull();
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Pos);
            }
        }

        public class PosDoubleConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                PosDouble p = (PosDouble)value;

                if (p != null)
                {
                    writer.WriteRawValue(String.Format(CultureInfo.InvariantCulture, "new PosD({0},{1})", p.X, p.Y));
                }
                else
                {
                    writer.WriteNull();
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(PosDouble);
            }
        }

        [Test]
        public void SerializeRefAdditionalContent()
        {
            //Additional text found in JSON string after finishing deserializing object.
            //Test 1
            var reference = new Dictionary<string, object>();
            reference.Add("$ref", "Persons");
            reference.Add("$id", 1);

            var child = new Dictionary<string, object>();
            child.Add("_id", 2);
            child.Add("Name", "Isabell");
            child.Add("Father", reference);

            var json = JsonConvert.SerializeObject(child, Formatting.Indented);

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Dictionary<string, object>>(json); }, "Additional content found in JSON reference object. A JSON reference object should only have a $ref property. Path 'Father.$id', line 6, position 10.");
        }

        [Test]
        public void SerializeRefBadType()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                //Additional text found in JSON string after finishing deserializing object.
                //Test 1
                var reference = new Dictionary<string, object>();
                reference.Add("$ref", 1);
                reference.Add("$id", 1);

                var child = new Dictionary<string, object>();
                child.Add("_id", 2);
                child.Add("Name", "Isabell");
                child.Add("Father", reference);

                var json = JsonConvert.SerializeObject(child, Formatting.Indented);
                JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }, "JSON reference $ref property must have a string or null value. Path 'Father.$ref', line 5, position 13.");
        }

        [Test]
        public void SerializeRefNull()
        {
            var reference = new Dictionary<string, object>();
            reference.Add("$ref", null);
            reference.Add("$id", null);
            reference.Add("blah", "blah!");

            var child = new Dictionary<string, object>();
            child.Add("_id", 2);
            child.Add("Name", "Isabell");
            child.Add("Father", reference);

            string json = JsonConvert.SerializeObject(child);

            Assert.AreEqual(@"{""_id"":2,""Name"":""Isabell"",""Father"":{""$ref"":null,""$id"":null,""blah"":""blah!""}}", json);

            Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, ((JObject)result["Father"]).Count);
            Assert.AreEqual("blah!", (string)((JObject)result["Father"])["blah"]);
        }

        public class ConstructorCompexIgnoredProperty
        {
            [JsonIgnore]
            public Product Ignored { get; set; }

            public string First { get; set; }
            public int Second { get; set; }

            public ConstructorCompexIgnoredProperty(string first, int second)
            {
                First = first;
                Second = second;
            }
        }

        [Test]
        public void DeserializeIgnoredPropertyInConstructor()
        {
            string json = @"{""First"":""First"",""Second"":2,""Ignored"":{""Name"":""James""},""AdditionalContent"":{""LOL"":true}}";

            ConstructorCompexIgnoredProperty cc = JsonConvert.DeserializeObject<ConstructorCompexIgnoredProperty>(json);
            Assert.AreEqual("First", cc.First);
            Assert.AreEqual(2, cc.Second);
            Assert.AreEqual(null, cc.Ignored);
        }

        [Test]
        public void DeserializeFloatAsDecimal()
        {
            string json = @"{'value':9.9}";

            var dic = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                json, new JsonSerializerSettings
                {
                    FloatParseHandling = FloatParseHandling.Decimal
                });

            Assert.AreEqual(typeof(decimal), dic["value"].GetType());
            Assert.AreEqual(9.9m, dic["value"]);
        }

        public class DictionaryKey
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }

            public static implicit operator DictionaryKey(string value)
            {
                return new DictionaryKey() { Value = value };
            }
        }

        [Test]
        public void SerializeDeserializeDictionaryKey()
        {
            Dictionary<DictionaryKey, string> dictionary = new Dictionary<DictionaryKey, string>();

            dictionary.Add(new DictionaryKey() { Value = "First!" }, "First");
            dictionary.Add(new DictionaryKey() { Value = "Second!" }, "Second");

            string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""First!"": ""First"",
  ""Second!"": ""Second""
}", json);

            Dictionary<DictionaryKey, string> newDictionary =
                JsonConvert.DeserializeObject<Dictionary<DictionaryKey, string>>(json);

            Assert.AreEqual(2, newDictionary.Count);
        }

        [Test]
        public void SerializeNullableArray()
        {
            string jsonText = JsonConvert.SerializeObject(new double?[] { 2.4, 4.3, null }, Formatting.Indented);

            StringAssert.AreEqual(@"[
  2.4,
  4.3,
  null
]", jsonText);
        }

        [Test]
        public void DeserializeNullableArray()
        {
            double?[] d = (double?[])JsonConvert.DeserializeObject(@"[
  2.4,
  4.3,
  null
]", typeof(double?[]));

            Assert.AreEqual(3, d.Length);
            Assert.AreEqual(2.4, d[0]);
            Assert.AreEqual(4.3, d[1]);
            Assert.AreEqual(null, d[2]);
        }

#if !NET20
        [Test]
        public void SerializeHashSet()
        {
            string jsonText = JsonConvert.SerializeObject(new HashSet<string>()
            {
                "One",
                "2",
                "III"
            }, Formatting.Indented);

            StringAssert.AreEqual(@"[
  ""One"",
  ""2"",
  ""III""
]", jsonText);

            HashSet<string> d = JsonConvert.DeserializeObject<HashSet<string>>(jsonText);

            Assert.AreEqual(3, d.Count);
            Assert.IsTrue(d.Contains("One"));
            Assert.IsTrue(d.Contains("2"));
            Assert.IsTrue(d.Contains("III"));
        }
#endif

        private class MyClass
        {
            public byte[] Prop1 { get; set; }

            public MyClass()
            {
                Prop1 = new byte[0];
            }
        }

        [Test]
        public void DeserializeByteArray()
        {
            JsonSerializer serializer1 = new JsonSerializer();
            serializer1.Converters.Add(new IsoDateTimeConverter());
            serializer1.NullValueHandling = NullValueHandling.Ignore;

            string json = @"[{""Prop1"":""""},{""Prop1"":""""}]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            MyClass[] z = (MyClass[])serializer1.Deserialize(reader, typeof(MyClass[]));
            Assert.AreEqual(2, z.Length);
            Assert.AreEqual(0, z[0].Prop1.Length);
            Assert.AreEqual(0, z[1].Prop1.Length);
        }

#if !(NET20 || NETFX_CORE || DNXCORE50)
        public class StringDictionaryTestClass
        {
            public StringDictionary StringDictionaryProperty { get; set; }
        }

        [Test]
        public void StringDictionaryTest()
        {
            string classRef = typeof(StringDictionary).FullName;

            StringDictionaryTestClass s1 = new StringDictionaryTestClass()
            {
                StringDictionaryProperty = new StringDictionary()
                {
                    { "1", "One" },
                    { "2", "II" },
                    { "3", "3" }
                }
            };

            string json = JsonConvert.SerializeObject(s1, Formatting.Indented);

            // .NET 4.5.3 added IDictionary<string, string> to StringDictionary
            if (s1.StringDictionaryProperty is IDictionary<string, string>)
            {
                StringDictionaryTestClass d = JsonConvert.DeserializeObject<StringDictionaryTestClass>(json);

                Assert.AreEqual(3, d.StringDictionaryProperty.Count);
                Assert.AreEqual("One", d.StringDictionaryProperty["1"]);
                Assert.AreEqual("II", d.StringDictionaryProperty["2"]);
                Assert.AreEqual("3", d.StringDictionaryProperty["3"]);
            }
            else
            {
                ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<StringDictionaryTestClass>(json); }, "Cannot create and populate list type " + classRef + ". Path 'StringDictionaryProperty', line 2, position 31.");
            }
        }
#endif

        [JsonObject(MemberSerialization.OptIn)]
        public struct StructWithAttribute
        {
            public string MyString { get; set; }

            [JsonProperty]
            public int MyInt { get; set; }
        }

        [Test]
        public void SerializeStructWithJsonObjectAttribute()
        {
            StructWithAttribute testStruct = new StructWithAttribute
            {
                MyInt = int.MaxValue
            };

            string json = JsonConvert.SerializeObject(testStruct, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""MyInt"": 2147483647
}", json);

            StructWithAttribute newStruct = JsonConvert.DeserializeObject<StructWithAttribute>(json);

            Assert.AreEqual(int.MaxValue, newStruct.MyInt);
        }

        public class TimeZoneOffsetObject
        {
            public DateTimeOffset Offset { get; set; }
        }

#if !NET20
        [Test]
        public void ReadWriteTimeZoneOffsetIso()
        {
            var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
            {
                Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
            });

            Assert.AreEqual("{\"Offset\":\"2000-01-01T00:00:00+06:00\"}", serializeObject);

            JsonTextReader reader = new JsonTextReader(new StringReader(serializeObject));
            reader.DateParseHandling = DateParseHandling.None;

            JsonSerializer serializer = new JsonSerializer();

            var deserializeObject = serializer.Deserialize<TimeZoneOffsetObject>(reader);

            Assert.AreEqual(TimeSpan.FromHours(6), deserializeObject.Offset.Offset);
            Assert.AreEqual(new DateTime(2000, 1, 1), deserializeObject.Offset.Date);
        }

        [Test]
        public void DeserializePropertyNullableDateTimeOffsetExactIso()
        {
            NullableDateTimeTestClass d = JsonConvert.DeserializeObject<NullableDateTimeTestClass>("{\"DateTimeOffsetField\":\"2000-01-01T00:00:00+06:00\"}");
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), d.DateTimeOffsetField);
        }

        [Test]
        public void ReadWriteTimeZoneOffsetMsAjax()
        {
            var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
            {
                Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
            }, Formatting.None, new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat });

            Assert.AreEqual("{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}", serializeObject);

            JsonTextReader reader = new JsonTextReader(new StringReader(serializeObject));

            JsonSerializer serializer = new JsonSerializer();
            serializer.DateParseHandling = DateParseHandling.None;

            var deserializeObject = serializer.Deserialize<TimeZoneOffsetObject>(reader);

            Assert.AreEqual(TimeSpan.FromHours(6), deserializeObject.Offset.Offset);
            Assert.AreEqual(new DateTime(2000, 1, 1), deserializeObject.Offset.Date);
        }

        [Test]
        public void DeserializePropertyNullableDateTimeOffsetExactMsAjax()
        {
            NullableDateTimeTestClass d = JsonConvert.DeserializeObject<NullableDateTimeTestClass>("{\"DateTimeOffsetField\":\"\\/Date(946663200000+0600)\\/\"}");
            Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), d.DateTimeOffsetField);
        }
#endif

        public abstract class LogEvent
        {
            [JsonProperty("event")]
            public abstract string EventName { get; }
        }

        public class DerivedEvent : LogEvent
        {
            public override string EventName
            {
                get { return "derived"; }
            }
        }

        [Test]
        public void OverridenPropertyMembers()
        {
            string json = JsonConvert.SerializeObject(new DerivedEvent(), Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""event"": ""derived""
}", json);
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void SerializeExpandoObject()
        {
            dynamic expando = new ExpandoObject();
            expando.Int = 1;
            expando.Decimal = 99.9d;
            expando.Complex = new ExpandoObject();
            expando.Complex.String = "I am a string";
            expando.Complex.DateTime = new DateTime(2000, 12, 20, 18, 55, 0, DateTimeKind.Utc);

            string json = JsonConvert.SerializeObject(expando, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Int"": 1,
  ""Decimal"": 99.9,
  ""Complex"": {
    ""String"": ""I am a string"",
    ""DateTime"": ""2000-12-20T18:55:00Z""
  }
}", json);

            IDictionary<string, object> newExpando = JsonConvert.DeserializeObject<ExpandoObject>(json);

            CustomAssert.IsInstanceOfType(typeof(long), newExpando["Int"]);
            Assert.AreEqual((long)expando.Int, newExpando["Int"]);

            CustomAssert.IsInstanceOfType(typeof(double), newExpando["Decimal"]);
            Assert.AreEqual(expando.Decimal, newExpando["Decimal"]);

            CustomAssert.IsInstanceOfType(typeof(ExpandoObject), newExpando["Complex"]);
            IDictionary<string, object> o = (ExpandoObject)newExpando["Complex"];

            CustomAssert.IsInstanceOfType(typeof(string), o["String"]);
            Assert.AreEqual(expando.Complex.String, o["String"]);

            CustomAssert.IsInstanceOfType(typeof(DateTime), o["DateTime"]);
            Assert.AreEqual(expando.Complex.DateTime, o["DateTime"]);
        }
#endif

        [Test]
        public void DeserializeDecimalExact()
        {
            decimal d = JsonConvert.DeserializeObject<decimal>("123456789876543.21");
            Assert.AreEqual(123456789876543.21m, d);
        }

        [Test]
        public void DeserializeNullableDecimalExact()
        {
            decimal? d = JsonConvert.DeserializeObject<decimal?>("123456789876543.21");
            Assert.AreEqual(123456789876543.21m, d);
        }

        [Test]
        public void DeserializeDecimalPropertyExact()
        {
            string json = "{Amount:123456789876543.21}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            JsonSerializer serializer = new JsonSerializer();

            Invoice i = serializer.Deserialize<Invoice>(reader);
            Assert.AreEqual(123456789876543.21m, i.Amount);
        }

        [Test]
        public void DeserializeDecimalArrayExact()
        {
            string json = "[123456789876543.21]";
            IList<decimal> a = JsonConvert.DeserializeObject<IList<decimal>>(json);
            Assert.AreEqual(123456789876543.21m, a[0]);
        }

        [Test]
        public void DeserializeDecimalDictionaryExact()
        {
            string json = "{'Value':123456789876543.21}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            JsonSerializer serializer = new JsonSerializer();

            IDictionary<string, decimal> d = serializer.Deserialize<IDictionary<string, decimal>>(reader);
            Assert.AreEqual(123456789876543.21m, d["Value"]);
        }

        public struct Vector
        {
            public float X;
            public float Y;
            public float Z;

            public override string ToString()
            {
                return string.Format("({0},{1},{2})", X, Y, Z);
            }
        }

        public class VectorParent
        {
            public Vector Position;
        }

        [Test]
        public void DeserializeStructProperty()
        {
            VectorParent obj = new VectorParent();
            obj.Position = new Vector { X = 1, Y = 2, Z = 3 };

            string str = JsonConvert.SerializeObject(obj);

            obj = JsonConvert.DeserializeObject<VectorParent>(str);

            Assert.AreEqual(1, obj.Position.X);
            Assert.AreEqual(2, obj.Position.Y);
            Assert.AreEqual(3, obj.Position.Z);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Derived : Base
        {
            [JsonProperty]
            public string IDoWork { get; private set; }

            private Derived()
            {
            }

            internal Derived(string dontWork, string doWork)
                : base(dontWork)
            {
                IDoWork = doWork;
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Base
        {
            [JsonProperty]
            public string IDontWork { get; private set; }

            protected Base()
            {
            }

            internal Base(string dontWork)
            {
                IDontWork = dontWork;
            }
        }

        [Test]
        public void PrivateSetterOnBaseClassProperty()
        {
            var derived = new Derived("meh", "woo");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            string json = JsonConvert.SerializeObject(derived, Formatting.Indented, settings);

            var meh = JsonConvert.DeserializeObject<Base>(json, settings);

            Assert.AreEqual(((Derived)meh).IDoWork, "woo");
            Assert.AreEqual(meh.IDontWork, "meh");
        }

#if !(NET20 || NETFX_CORE || DNXCORE50)
        [DataContract]
        public struct StructISerializable : ISerializable
        {
            private string _name;

            public StructISerializable(SerializationInfo info, StreamingContext context)
            {
                _name = info.GetString("Name");
            }

            [DataMember]
            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Name", _name);
            }
        }

        [DataContract]
        public class NullableStructPropertyClass
        {
            private StructISerializable _foo1;
            private StructISerializable? _foo2;

            [DataMember]
            public StructISerializable Foo1
            {
                get { return _foo1; }
                set { _foo1 = value; }
            }

            [DataMember]
            public StructISerializable? Foo2
            {
                get { return _foo2; }
                set { _foo2 = value; }
            }
        }

        [Test]
        public void DeserializeNullableStruct()
        {
            NullableStructPropertyClass nullableStructPropertyClass = new NullableStructPropertyClass();
            nullableStructPropertyClass.Foo1 = new StructISerializable() { Name = "foo 1" };
            nullableStructPropertyClass.Foo2 = new StructISerializable() { Name = "foo 2" };

            NullableStructPropertyClass barWithNull = new NullableStructPropertyClass();
            barWithNull.Foo1 = new StructISerializable() { Name = "foo 1" };
            barWithNull.Foo2 = null;

            //throws error on deserialization because bar1.Foo2 is of type Foo?
            string s = JsonConvert.SerializeObject(nullableStructPropertyClass);
            NullableStructPropertyClass deserialized = deserialize(s);
            Assert.AreEqual(deserialized.Foo1.Name, "foo 1");
            Assert.AreEqual(deserialized.Foo2.Value.Name, "foo 2");

            //no error Foo2 is null
            s = JsonConvert.SerializeObject(barWithNull);
            deserialized = deserialize(s);
            Assert.AreEqual(deserialized.Foo1.Name, "foo 1");
            Assert.AreEqual(deserialized.Foo2, null);
        }

        private static NullableStructPropertyClass deserialize(string serStr)
        {
            return JsonConvert.DeserializeObject<NullableStructPropertyClass>(
                serStr,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });
        }
#endif

        public class Response
        {
            public string Name { get; set; }
            public JToken Data { get; set; }
        }

        [Test]
        public void DeserializeJToken()
        {
            Response response = new Response
            {
                Name = "Success",
                Data = new JObject(new JProperty("First", "Value1"), new JProperty("Second", "Value2"))
            };

            string json = JsonConvert.SerializeObject(response, Formatting.Indented);

            Response deserializedResponse = JsonConvert.DeserializeObject<Response>(json);

            Assert.AreEqual("Success", deserializedResponse.Name);
            Assert.IsTrue(deserializedResponse.Data.DeepEquals(response.Data));
        }

        [Test]
        public void DeserializeMinValueDecimal()
        {
            var data = new DecimalTest(decimal.MinValue);
            var json = JsonConvert.SerializeObject(data);
            var obj = JsonConvert.DeserializeObject<DecimalTest>(json, new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Default });

            Assert.AreEqual(decimal.MinValue, obj.Value);
        }

        [Test]
        public void NonPublicConstructorWithJsonConstructorTest()
        {
            NonPublicConstructorWithJsonConstructor c = JsonConvert.DeserializeObject<NonPublicConstructorWithJsonConstructor>("{}");
            Assert.AreEqual("NonPublic", c.Constructor);
        }

        [Test]
        public void PublicConstructorOverridenByJsonConstructorTest()
        {
            PublicConstructorOverridenByJsonConstructor c = JsonConvert.DeserializeObject<PublicConstructorOverridenByJsonConstructor>("{Value:'value!'}");
            Assert.AreEqual("Public Parameterized", c.Constructor);
            Assert.AreEqual("value!", c.Value);
        }

        [Test]
        public void MultipleParametrizedConstructorsJsonConstructorTest()
        {
            MultipleParametrizedConstructorsJsonConstructor c = JsonConvert.DeserializeObject<MultipleParametrizedConstructorsJsonConstructor>("{Value:'value!', Age:1}");
            Assert.AreEqual("Public Parameterized 2", c.Constructor);
            Assert.AreEqual("value!", c.Value);
            Assert.AreEqual(1, c.Age);
        }

        [Test]
        public void DeserializeEnumerable()
        {
            EnumerableClass c = new EnumerableClass
            {
                Enumerable = new List<string> { "One", "Two", "Three" }
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Enumerable"": [
    ""One"",
    ""Two"",
    ""Three""
  ]
}", json);

            EnumerableClass c2 = JsonConvert.DeserializeObject<EnumerableClass>(json);

            Assert.AreEqual("One", c2.Enumerable.ElementAt(0));
            Assert.AreEqual("Two", c2.Enumerable.ElementAt(1));
            Assert.AreEqual("Three", c2.Enumerable.ElementAt(2));
        }

        [Test]
        public void SerializeAttributesOnBase()
        {
            ComplexItem i = new ComplexItem();

            string json = JsonConvert.SerializeObject(i, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": null
}", json);
        }

        [Test]
        public void DeserializeStringEnglish()
        {
            string json = @"{
  'Name': 'James Hughes',
  'Age': '40',
  'Height': '44.4',
  'Price': '4'
}";

            DeserializeStringConvert p = JsonConvert.DeserializeObject<DeserializeStringConvert>(json);
            Assert.AreEqual(40, p.Age);
            Assert.AreEqual(44.4, p.Height);
            Assert.AreEqual(4m, p.Price);
        }

        [Test]
        public void DeserializeNullDateTimeValueTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject("null", typeof(DateTime)); }, "Error converting value {null} to type 'System.DateTime'. Path '', line 1, position 4.");
        }

        [Test]
        public void DeserializeNullNullableDateTimeValueTest()
        {
            object dateTime = JsonConvert.DeserializeObject("null", typeof(DateTime?));

            Assert.IsNull(dateTime);
        }

        [Test]
        public void MultiIndexSuperTest()
        {
            MultiIndexSuper e = new MultiIndexSuper();

            string json = JsonConvert.SerializeObject(e, Formatting.Indented);

            Assert.AreEqual(@"{}", json);
        }

        public class MultiIndexSuper : MultiIndexBase
        {
        }

        public abstract class MultiIndexBase
        {
            protected internal object this[string propertyName]
            {
                get { return null; }
                set { }
            }

            protected internal object this[object property]
            {
                get { return null; }
                set { }
            }
        }

        public class CommentTestClass
        {
            public bool Indexed { get; set; }
            public int StartYear { get; set; }
            public IList<decimal> Values { get; set; }
        }

        [Test]
        public void CommentTestClassTest()
        {
            string json = @"{""indexed"":true, ""startYear"":1939, ""values"":
                            [  3000,  /* 1940-1949 */
                               3000,   3600,   3600,   3600,   3600,   4200,   4200,   4200,   4200,   4800,  /* 1950-1959 */
                               4800,   4800,   4800,   4800,   4800,   4800,   6600,   6600,   7800,   7800,  /* 1960-1969 */
                               7800,   7800,   9000,  10800,  13200,  14100,  15300,  16500,  17700,  22900,  /* 1970-1979 */
                              25900,  29700,  32400,  35700,  37800,  39600,  42000,  43800,  45000,  48000,  /* 1980-1989 */
                              51300,  53400,  55500,  57600,  60600,  61200,  62700,  65400,  68400,  72600,  /* 1990-1999 */
                              76200,  80400,  84900,  87000,  87900,  90000,  94200,  97500, 102000, 106800,  /* 2000-2009 */
                             106800, 106800]  /* 2010-2011 */
                                }";

            CommentTestClass commentTestClass = JsonConvert.DeserializeObject<CommentTestClass>(json);

            Assert.AreEqual(true, commentTestClass.Indexed);
            Assert.AreEqual(1939, commentTestClass.StartYear);
            Assert.AreEqual(63, commentTestClass.Values.Count);
        }

        private class DTOWithParameterisedConstructor
        {
            public DTOWithParameterisedConstructor(string A)
            {
                this.A = A;
                B = 2;
            }

            public string A { get; set; }
            public int? B { get; set; }
        }

        private class DTOWithoutParameterisedConstructor
        {
            public DTOWithoutParameterisedConstructor()
            {
                B = 2;
            }

            public string A { get; set; }
            public int? B { get; set; }
        }

        [Test]
        public void PopulationBehaviourForOmittedPropertiesIsTheSameForParameterisedConstructorAsForDefaultConstructor()
        {
            string json = @"{A:""Test""}";

            var withoutParameterisedConstructor = JsonConvert.DeserializeObject<DTOWithoutParameterisedConstructor>(json);
            var withParameterisedConstructor = JsonConvert.DeserializeObject<DTOWithParameterisedConstructor>(json);
            Assert.AreEqual(withoutParameterisedConstructor.B, withParameterisedConstructor.B);
        }

        public class EnumerableArrayPropertyClass
        {
            public IEnumerable<int> Numbers
            {
                get
                {
                    return new[] { 1, 2, 3 }; //fails
                    //return new List<int>(new[] { 1, 2, 3 }); //works
                }
            }
        }

        [Test]
        public void SkipPopulatingArrayPropertyClass()
        {
            string json = JsonConvert.SerializeObject(new EnumerableArrayPropertyClass());
            JsonConvert.DeserializeObject<EnumerableArrayPropertyClass>(json);
        }

#if !(NET20)
        [DataContract]
        public class BaseDataContract
        {
            [DataMember(Name = "virtualMember")]
            public virtual string VirtualMember { get; set; }

            [DataMember(Name = "nonVirtualMember")]
            public string NonVirtualMember { get; set; }
        }

        public class ChildDataContract : BaseDataContract
        {
            public override string VirtualMember { get; set; }
            public string NewMember { get; set; }
        }

        [Test]
        public void ChildDataContractTest()
        {
            ChildDataContract cc = new ChildDataContract
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!"
            };

            string result = JsonConvert.SerializeObject(cc, Formatting.Indented);
            //      Assert.AreEqual(@"{
            //  ""VirtualMember"": ""VirtualMember!"",
            //  ""NewMember"": null,
            //  ""nonVirtualMember"": ""NonVirtualMember!""
            //}", result);

            StringAssert.AreEqual(@"{
  ""virtualMember"": ""VirtualMember!"",
  ""nonVirtualMember"": ""NonVirtualMember!""
}", result);
        }

        [Test]
        public void ChildDataContractTestWithDataContractSerializer()
        {
            ChildDataContract cc = new ChildDataContract
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!"
            };

            DataContractSerializer serializer = new DataContractSerializer(typeof(ChildDataContract));

            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, cc);

            string xml = Encoding.UTF8.GetString(ms.ToArray(), 0, Convert.ToInt32(ms.Length));

            Assert.AreEqual(@"<JsonSerializerTest.ChildDataContract xmlns=""http://schemas.datacontract.org/2004/07/Newtonsoft.Json.Tests.Serialization"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><nonVirtualMember>NonVirtualMember!</nonVirtualMember><virtualMember>VirtualMember!</virtualMember><NewMember i:nil=""true""/></JsonSerializerTest.ChildDataContract>", xml);
        }
#endif

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class BaseObject
        {
            [JsonProperty(PropertyName = "virtualMember")]
            public virtual string VirtualMember { get; set; }

            [JsonProperty(PropertyName = "nonVirtualMember")]
            public string NonVirtualMember { get; set; }
        }

        public class ChildObject : BaseObject
        {
            public override string VirtualMember { get; set; }
            public string NewMember { get; set; }
        }

        public class ChildWithDifferentOverrideObject : BaseObject
        {
            [JsonProperty(PropertyName = "differentVirtualMember")]
            public override string VirtualMember { get; set; }
        }

        [Test]
        public void ChildObjectTest()
        {
            ChildObject cc = new ChildObject
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!"
            };

            string result = JsonConvert.SerializeObject(cc);
            Assert.AreEqual(@"{""virtualMember"":""VirtualMember!"",""nonVirtualMember"":""NonVirtualMember!""}", result);
        }

        [Test]
        public void ChildWithDifferentOverrideObjectTest()
        {
            ChildWithDifferentOverrideObject cc = new ChildWithDifferentOverrideObject
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!"
            };

            string result = JsonConvert.SerializeObject(cc);
            Assert.AreEqual(@"{""differentVirtualMember"":""VirtualMember!"",""nonVirtualMember"":""NonVirtualMember!""}", result);
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public interface IInterfaceObject
        {
            [JsonProperty(PropertyName = "virtualMember")]
            [JsonConverter(typeof(IsoDateTimeConverter))]
            DateTime InterfaceMember { get; set; }
        }

        public class ImplementInterfaceObject : IInterfaceObject
        {
            public DateTime InterfaceMember { get; set; }
            public string NewMember { get; set; }

            [JsonProperty(PropertyName = "newMemberWithProperty")]
            public string NewMemberWithProperty { get; set; }
        }

        [Test]
        public void ImplementInterfaceObjectTest()
        {
            ImplementInterfaceObject cc = new ImplementInterfaceObject
            {
                InterfaceMember = new DateTime(2010, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                NewMember = "NewMember!"
            };

            string result = JsonConvert.SerializeObject(cc, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""virtualMember"": ""2010-12-31T00:00:00Z"",
  ""newMemberWithProperty"": null
}", result);
        }

        public class NonDefaultConstructorWithReadOnlyCollectionProperty
        {
            public string Title { get; set; }
            public IList<string> Categories { get; private set; }

            public NonDefaultConstructorWithReadOnlyCollectionProperty(string title)
            {
                Title = title;
                Categories = new List<string>();
            }
        }

        [Test]
        public void NonDefaultConstructorWithReadOnlyCollectionPropertyTest()
        {
            NonDefaultConstructorWithReadOnlyCollectionProperty c1 = new NonDefaultConstructorWithReadOnlyCollectionProperty("blah");
            c1.Categories.Add("one");
            c1.Categories.Add("two");

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Title"": ""blah"",
  ""Categories"": [
    ""one"",
    ""two""
  ]
}", json);

            NonDefaultConstructorWithReadOnlyCollectionProperty c2 = JsonConvert.DeserializeObject<NonDefaultConstructorWithReadOnlyCollectionProperty>(json);
            Assert.AreEqual(c1.Title, c2.Title);
            Assert.AreEqual(c1.Categories.Count, c2.Categories.Count);
            Assert.AreEqual("one", c2.Categories[0]);
            Assert.AreEqual("two", c2.Categories[1]);
        }

        public class NonDefaultConstructorWithReadOnlyDictionaryProperty
        {
            public string Title { get; set; }
            public IDictionary<string, int> Categories { get; private set; }

            public NonDefaultConstructorWithReadOnlyDictionaryProperty(string title)
            {
                Title = title;
                Categories = new Dictionary<string, int>();
            }
        }

        [Test]
        public void NonDefaultConstructorWithReadOnlyDictionaryPropertyTest()
        {
            NonDefaultConstructorWithReadOnlyDictionaryProperty c1 = new NonDefaultConstructorWithReadOnlyDictionaryProperty("blah");
            c1.Categories.Add("one", 1);
            c1.Categories.Add("two", 2);

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Title"": ""blah"",
  ""Categories"": {
    ""one"": 1,
    ""two"": 2
  }
}", json);

            NonDefaultConstructorWithReadOnlyDictionaryProperty c2 = JsonConvert.DeserializeObject<NonDefaultConstructorWithReadOnlyDictionaryProperty>(json);
            Assert.AreEqual(c1.Title, c2.Title);
            Assert.AreEqual(c1.Categories.Count, c2.Categories.Count);
            Assert.AreEqual(1, c2.Categories["one"]);
            Assert.AreEqual(2, c2.Categories["two"]);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ClassAttributeBase
        {
            [JsonProperty]
            public string BaseClassValue { get; set; }
        }

        public class ClassAttributeDerived : ClassAttributeBase
        {
            [JsonProperty]
            public string DerivedClassValue { get; set; }

            public string NonSerialized { get; set; }
        }

        public class CollectionClassAttributeDerived : ClassAttributeBase, ICollection<object>
        {
            [JsonProperty]
            public string CollectionDerivedClassValue { get; set; }

            public void Add(object item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(object item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<object> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ClassAttributesInheritance()
        {
            string json = JsonConvert.SerializeObject(new ClassAttributeDerived
            {
                BaseClassValue = "BaseClassValue!",
                DerivedClassValue = "DerivedClassValue!",
                NonSerialized = "NonSerialized!"
            }, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""DerivedClassValue"": ""DerivedClassValue!"",
  ""BaseClassValue"": ""BaseClassValue!""
}", json);

            json = JsonConvert.SerializeObject(new CollectionClassAttributeDerived
            {
                BaseClassValue = "BaseClassValue!",
                CollectionDerivedClassValue = "CollectionDerivedClassValue!"
            }, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""CollectionDerivedClassValue"": ""CollectionDerivedClassValue!"",
  ""BaseClassValue"": ""BaseClassValue!""
}", json);
        }

        public class PrivateMembersClassWithAttributes
        {
            public PrivateMembersClassWithAttributes(string privateString, string internalString, string readonlyString)
            {
                _privateString = privateString;
                _readonlyString = readonlyString;
                _internalString = internalString;
            }

            public PrivateMembersClassWithAttributes()
            {
                _readonlyString = "default!";
            }

            [JsonProperty]
            private string _privateString;

            [JsonProperty]
            private readonly string _readonlyString;

            [JsonProperty]
            internal string _internalString;

            public string UseValue()
            {
                return _readonlyString;
            }
        }

        [Test]
        public void PrivateMembersClassWithAttributesTest()
        {
            PrivateMembersClassWithAttributes c1 = new PrivateMembersClassWithAttributes("privateString!", "internalString!", "readonlyString!");

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""_privateString"": ""privateString!"",
  ""_readonlyString"": ""readonlyString!"",
  ""_internalString"": ""internalString!""
}", json);

            PrivateMembersClassWithAttributes c2 = JsonConvert.DeserializeObject<PrivateMembersClassWithAttributes>(json);
            Assert.AreEqual("readonlyString!", c2.UseValue());
        }

        public partial class BusRun
        {
            public IEnumerable<Nullable<DateTime>> Departures { get; set; }
            public Boolean WheelchairAccessible { get; set; }
        }

        [Test]
        public void DeserializeGenericEnumerableProperty()
        {
            BusRun r = JsonConvert.DeserializeObject<BusRun>("{\"Departures\":[\"\\/Date(1309874148734-0400)\\/\",\"\\/Date(1309874148739-0400)\\/\",null],\"WheelchairAccessible\":true}");

            Assert.AreEqual(typeof(List<DateTime?>), r.Departures.GetType());
            Assert.AreEqual(3, r.Departures.Count());
            Assert.IsNotNull(r.Departures.ElementAt(0));
            Assert.IsNotNull(r.Departures.ElementAt(1));
            Assert.IsNull(r.Departures.ElementAt(2));
        }

#if !(NET20)
        [DataContract]
        public class BaseType
        {
            [DataMember]
            public string zebra;
        }

        [DataContract]
        public class DerivedType : BaseType
        {
            [DataMember(Order = 0)]
            public string bird;

            [DataMember(Order = 1)]
            public string parrot;

            [DataMember]
            public string dog;

            [DataMember(Order = 3)]
            public string antelope;

            [DataMember]
            public string cat;

            [JsonProperty(Order = 1)]
            public string albatross;

            [JsonProperty(Order = -2)]
            public string dinosaur;
        }

        [Test]
        public void JsonPropertyDataMemberOrder()
        {
            DerivedType d = new DerivedType();
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""dinosaur"": null,
  ""dog"": null,
  ""cat"": null,
  ""zebra"": null,
  ""bird"": null,
  ""parrot"": null,
  ""albatross"": null,
  ""antelope"": null
}", json);
        }
#endif

        public class ClassWithException
        {
            public IList<Exception> Exceptions { get; set; }

            public ClassWithException()
            {
                Exceptions = new List<Exception>();
            }
        }

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
        [Test]
        public void SerializeException1()
        {
            ClassWithException classWithException = new ClassWithException();
            try
            {
                throw new Exception("Test Exception");
            }
            catch (Exception ex)
            {
                classWithException.Exceptions.Add(ex);
            }
            string sex = JsonConvert.SerializeObject(classWithException);
            ClassWithException dex = JsonConvert.DeserializeObject<ClassWithException>(sex);
            Assert.AreEqual(dex.Exceptions[0].ToString(), dex.Exceptions[0].ToString());

            sex = JsonConvert.SerializeObject(classWithException, Formatting.Indented);

            dex = JsonConvert.DeserializeObject<ClassWithException>(sex); // this fails!
            Assert.AreEqual(dex.Exceptions[0].ToString(), dex.Exceptions[0].ToString());
        }
#endif

        [Test]
        public void UriGuidTimeSpanTestClassEmptyTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass();
            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Guid"": ""00000000-0000-0000-0000-000000000000"",
  ""NullableGuid"": null,
  ""TimeSpan"": ""00:00:00"",
  ""NullableTimeSpan"": null,
  ""Uri"": null
}", json);

            UriGuidTimeSpanTestClass c2 = JsonConvert.DeserializeObject<UriGuidTimeSpanTestClass>(json);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public void UriGuidTimeSpanTestClassValuesTest()
        {
            UriGuidTimeSpanTestClass c1 = new UriGuidTimeSpanTestClass
            {
                Guid = new Guid("1924129C-F7E0-40F3-9607-9939C531395A"),
                NullableGuid = new Guid("9E9F3ADF-E017-4F72-91E0-617EBE85967D"),
                TimeSpan = TimeSpan.FromDays(1),
                NullableTimeSpan = TimeSpan.FromHours(1),
                Uri = new Uri("http://testuri.com")
            };
            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Guid"": ""1924129c-f7e0-40f3-9607-9939c531395a"",
  ""NullableGuid"": ""9e9f3adf-e017-4f72-91e0-617ebe85967d"",
  ""TimeSpan"": ""1.00:00:00"",
  ""NullableTimeSpan"": ""01:00:00"",
  ""Uri"": ""http://testuri.com""
}", json);

            UriGuidTimeSpanTestClass c2 = JsonConvert.DeserializeObject<UriGuidTimeSpanTestClass>(json);
            Assert.AreEqual(c1.Guid, c2.Guid);
            Assert.AreEqual(c1.NullableGuid, c2.NullableGuid);
            Assert.AreEqual(c1.TimeSpan, c2.TimeSpan);
            Assert.AreEqual(c1.NullableTimeSpan, c2.NullableTimeSpan);
            Assert.AreEqual(c1.Uri, c2.Uri);
        }

        [Test]
        public void UsingJsonTextWriter()
        {
            // The property of the object has to be a number for the cast exception to occure
            object o = new { p = 1 };

            var json = JObject.FromObject(o);

            using (var sw = new StringWriter())
            using (var jw = new JsonTextWriter(sw))
            {
                jw.WriteToken(json.CreateReader());
                jw.Flush();

                string result = sw.ToString();
                Assert.AreEqual(@"{""p"":1}", result);
            }
        }

        [Test]
        public void SerializeUriWithQuotes()
        {
            string input = "http://test.com/%22foo+bar%22";
            Uri uri = new Uri(input);
            string json = JsonConvert.SerializeObject(uri);
            Uri output = JsonConvert.DeserializeObject<Uri>(json);

            Assert.AreEqual(uri, output);
        }

        [Test]
        public void SerializeUriWithSlashes()
        {
            string input = @"http://tes/?a=b\\c&d=e\";
            Uri uri = new Uri(input);
            string json = JsonConvert.SerializeObject(uri);
            Uri output = JsonConvert.DeserializeObject<Uri>(json);

            Assert.AreEqual(uri, output);
        }

        [Test]
        public void DeserializeByteArrayWithTypeNameHandling()
        {
            TestObject test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            byte[] objectBytes;
            using (MemoryStream stream = new MemoryStream())
            using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
            {
                serializer.Serialize(jsonWriter, test);
                jsonWriter.Flush();

                objectBytes = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream(objectBytes))
            using (JsonReader jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                // Get exception here
                TestObject newObject = (TestObject)serializer.Deserialize(jsonReader);

                Assert.AreEqual("Test", newObject.Name);
                CollectionAssert.AreEquivalent(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
            }
        }

        public class ReflectionContractResolver : DefaultContractResolver
        {
            protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
            {
                return new ReflectionValueProvider(member);
            }
        }

        [Test]
        public void SerializeStaticDefault()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver();

            StaticTestClass c = new StaticTestClass
            {
                x = int.MaxValue
            };
            StaticTestClass.y = 2;
            StaticTestClass.z = 3;
            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            StringAssert.AreEqual(@"{
  ""x"": 2147483647,
  ""y"": 2,
  ""z"": 3
}", json);

            StaticTestClass c2 = JsonConvert.DeserializeObject<StaticTestClass>(@"{
  ""x"": -1,
  ""y"": -2,
  ""z"": -3
}",
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            Assert.AreEqual(-1, c2.x);
            Assert.AreEqual(-2, StaticTestClass.y);
            Assert.AreEqual(-3, StaticTestClass.z);
        }

        [Test]
        public void SerializeStaticReflection()
        {
            ReflectionContractResolver contractResolver = new ReflectionContractResolver();

            StaticTestClass c = new StaticTestClass
            {
                x = int.MaxValue
            };
            StaticTestClass.y = 2;
            StaticTestClass.z = 3;
            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            StringAssert.AreEqual(@"{
  ""x"": 2147483647,
  ""y"": 2,
  ""z"": 3
}", json);

            StaticTestClass c2 = JsonConvert.DeserializeObject<StaticTestClass>(@"{
  ""x"": -1,
  ""y"": -2,
  ""z"": -3
}",
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            Assert.AreEqual(-1, c2.x);
            Assert.AreEqual(-2, StaticTestClass.y);
            Assert.AreEqual(-3, StaticTestClass.z);
        }

#if !(NET20 || NETFX_CORE || DNXCORE50)
        [Test]
        public void DeserializeDecimalsWithCulture()
        {
            CultureInfo initialCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                CultureInfo testCulture = CultureInfo.CreateSpecificCulture("nb-NO");

                Thread.CurrentThread.CurrentCulture = testCulture;
                Thread.CurrentThread.CurrentUICulture = testCulture;

                string json = @"{ 'Quantity': '1.5', 'OptionalQuantity': '2.2' }";

                DecimalTestClass c = JsonConvert.DeserializeObject<DecimalTestClass>(json);

                Assert.AreEqual(1.5m, c.Quantity);
                Assert.AreEqual(2.2d, c.OptionalQuantity);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = initialCulture;
                Thread.CurrentThread.CurrentUICulture = initialCulture;
            }
        }
#endif

        [Test]
        public void ReadForTypeHackFixDecimal()
        {
            IList<decimal> d1 = new List<decimal> { 1.1m };

            string json = JsonConvert.SerializeObject(d1);

            IList<decimal> d2 = JsonConvert.DeserializeObject<IList<decimal>>(json);

            Assert.AreEqual(d1.Count, d2.Count);
            Assert.AreEqual(d1[0], d2[0]);
        }

        [Test]
        public void ReadForTypeHackFixDateTimeOffset()
        {
            IList<DateTimeOffset?> d1 = new List<DateTimeOffset?> { null };

            string json = JsonConvert.SerializeObject(d1);

            IList<DateTimeOffset?> d2 = JsonConvert.DeserializeObject<IList<DateTimeOffset?>>(json);

            Assert.AreEqual(d1.Count, d2.Count);
            Assert.AreEqual(d1[0], d2[0]);
        }

        [Test]
        public void ReadForTypeHackFixByteArray()
        {
            IList<byte[]> d1 = new List<byte[]> { null };

            string json = JsonConvert.SerializeObject(d1);

            IList<byte[]> d2 = JsonConvert.DeserializeObject<IList<byte[]>>(json);

            Assert.AreEqual(d1.Count, d2.Count);
            Assert.AreEqual(d1[0], d2[0]);
        }

        internal class HasByteArray
        {
            public byte[] EncryptedPassword { get; set; }
        }

        [Test]
        public void DeserializeByteArrayWithTypeName()
        {
            string json = @"{
  ""$type"": ""Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+HasByteArray, Newtonsoft.Json.Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ=""
  }
}";
            HasByteArray value = JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            CollectionAssert.AreEquivalent(Convert.FromBase64String("cGFzc3dvcmQ="), value.EncryptedPassword);
        }

        [Test]
        public void DeserializeByteArrayWithTypeName_BadAdditionalContent()
        {
            string json = @"{
  ""$type"": ""Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+HasByteArray, Newtonsoft.Json.Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ="",
    ""$value"": ""cGFzc3dvcmQ=""
  }
}";

            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
            }, "Error reading bytes. Unexpected token: PropertyName. Path 'EncryptedPassword.$value', line 6, position 13.");
        }

        [Test]
        public void DeserializeByteArrayWithTypeName_ExtraProperty()
        {
            string json = @"{
  ""$type"": ""Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+HasByteArray, Newtonsoft.Json.Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ=""
  },
  ""Pie"": null
}";
            HasByteArray value = JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Assert.IsNotNull(value.EncryptedPassword);
            CollectionAssert.AreEquivalent(Convert.FromBase64String("cGFzc3dvcmQ="), value.EncryptedPassword);
        }

        [Test]
        public void SerializeInheritanceHierarchyWithDuplicateProperty()
        {
            Bb b = new Bb();
            b.no = true;
            Aa a = b;
            a.no = int.MaxValue;

            string json = JsonConvert.SerializeObject(b);

            Assert.AreEqual(@"{""no"":true}", json);

            Bb b2 = JsonConvert.DeserializeObject<Bb>(json);

            Assert.AreEqual(true, b2.no);
        }

        [Test]
        public void DeserializeNullInt()
        {
            string json = @"[
  1,
  2,
  3,
  null
]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { List<int> numbers = JsonConvert.DeserializeObject<List<int>>(json); }, "Error converting value {null} to type 'System.Int32'. Path '[3]', line 5, position 6.");
        }

#if !(PORTABLE || NETFX_CORE)
        public class ConvertableIntTestClass
        {
            public ConvertibleInt Integer { get; set; }
            public ConvertibleInt? NullableInteger1 { get; set; }
            public ConvertibleInt? NullableInteger2 { get; set; }
        }

        [Test]
        public void SerializeIConvertible()
        {
            ConvertableIntTestClass c = new ConvertableIntTestClass
            {
                Integer = new ConvertibleInt(1),
                NullableInteger1 = new ConvertibleInt(2),
                NullableInteger2 = null
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Integer"": 1,
  ""NullableInteger1"": 2,
  ""NullableInteger2"": null
}", json);
        }

        [Test]
        public void DeserializeIConvertible()
        {
            string json = @"{
  ""Integer"": 1,
  ""NullableInteger1"": 2,
  ""NullableInteger2"": null
}";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ConvertableIntTestClass>(json), "Error converting value 1 to type 'Newtonsoft.Json.Tests.ConvertibleInt'. Path 'Integer', line 2, position 14.");
        }
#endif

        [Test]
        public void SerializeNullableWidgetStruct()
        {
            Widget widget = new Widget { Id = new WidgetId { Value = "id" } };

            string json = JsonConvert.SerializeObject(widget);

            Assert.AreEqual(@"{""Id"":{""Value"":""id""}}", json);
        }

        [Test]
        public void DeserializeNullableWidgetStruct()
        {
            string json = @"{""Id"":{""Value"":""id""}}";

            Widget w = JsonConvert.DeserializeObject<Widget>(json);

            Assert.AreEqual(new WidgetId { Value = "id" }, w.Id);
            Assert.AreEqual(new WidgetId { Value = "id" }, w.Id.Value);
            Assert.AreEqual("id", w.Id.Value.Value);
        }

        [Test]
        public void DeserializeBoolInt()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                string json = @"{
  ""PreProperty"": true,
  ""PostProperty"": ""-1""
}";

                JsonConvert.DeserializeObject<TestObjects.MyClass>(json);
            }, "Unexpected character encountered while parsing value: t. Path 'PreProperty', line 2, position 18.");
        }

        [Test]
        public void DeserializeUnexpectedEndInt()
        {
            ExceptionAssert.Throws<JsonException>(() =>
            {
                string json = @"{
  ""PreProperty"": ";

                JsonConvert.DeserializeObject<TestObjects.MyClass>(json);
            });
        }

        [Test]
        public void DeserializeNullableGuid()
        {
            string json = @"{""Id"":null}";
            var c = JsonConvert.DeserializeObject<NullableGuid>(json);

            Assert.AreEqual(null, c.Id);

            json = @"{""Id"":""d8220a4b-75b1-4b7a-8112-b7bdae956a45""}";
            c = JsonConvert.DeserializeObject<NullableGuid>(json);

            Assert.AreEqual(new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"), c.Id);
        }

        [Test]
        public void DeserializeGuid()
        {
            Item expected = new Item()
            {
                SourceTypeID = new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"),
                BrokerID = new Guid("951663c4-924e-4c86-a57a-7ed737501dbd"),
                Latitude = 33.657145,
                Longitude = -117.766684,
                TimeStamp = new DateTime(2000, 3, 1, 23, 59, 59, DateTimeKind.Utc),
                Payload = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            };

            string jsonString = JsonConvert.SerializeObject(expected, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""SourceTypeID"": ""d8220a4b-75b1-4b7a-8112-b7bdae956a45"",
  ""BrokerID"": ""951663c4-924e-4c86-a57a-7ed737501dbd"",
  ""Latitude"": 33.657145,
  ""Longitude"": -117.766684,
  ""TimeStamp"": ""2000-03-01T23:59:59Z"",
  ""Payload"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""AAECAwQFBgcICQ==""
  }
}", jsonString);

            Item actual = JsonConvert.DeserializeObject<Item>(jsonString);

            Assert.AreEqual(new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"), actual.SourceTypeID);
            Assert.AreEqual(new Guid("951663c4-924e-4c86-a57a-7ed737501dbd"), actual.BrokerID);
            byte[] bytes = (byte[])actual.Payload;
            CollectionAssert.AreEquivalent((new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }).ToList(), bytes.ToList());
        }

        [Test]
        public void DeserializeObjectDictionary()
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());
            var dict = serializer.Deserialize<Dictionary<string, string>>(new JsonTextReader(new StringReader("{'k1':'','k2':'v2'}")));

            Assert.AreEqual("", dict["k1"]);
            Assert.AreEqual("v2", dict["k2"]);
        }

        [Test]
        public void DeserializeNullableEnum()
        {
            string json = JsonConvert.SerializeObject(new WithEnums
            {
                Id = 7,
                NullableEnum = null
            });

            Assert.AreEqual(@"{""Id"":7,""NullableEnum"":null}", json);

            WithEnums e = JsonConvert.DeserializeObject<WithEnums>(json);

            Assert.AreEqual(null, e.NullableEnum);

            json = JsonConvert.SerializeObject(new WithEnums
            {
                Id = 7,
                NullableEnum = MyEnum.Value2
            });

            Assert.AreEqual(@"{""Id"":7,""NullableEnum"":1}", json);

            e = JsonConvert.DeserializeObject<WithEnums>(json);

            Assert.AreEqual(MyEnum.Value2, e.NullableEnum);
        }

        [Test]
        public void NullableStructWithConverter()
        {
            string json = JsonConvert.SerializeObject(new Widget1 { Id = new WidgetId1 { Value = 1234 } });

            Assert.AreEqual(@"{""Id"":""1234""}", json);

            Widget1 w = JsonConvert.DeserializeObject<Widget1>(@"{""Id"":""1234""}");

            Assert.AreEqual(new WidgetId1 { Value = 1234 }, w.Id);
        }

        [Test]
        public void SerializeDictionaryStringStringAndStringObject()
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());
            var dict = serializer.Deserialize<Dictionary<string, string>>(new JsonTextReader(new StringReader("{'k1':'','k2':'v2'}")));

            var reader = new JsonTextReader(new StringReader("{'k1':'','k2':'v2'}"));
            var dict2 = serializer.Deserialize<Dictionary<string, object>>(reader);

            Assert.AreEqual(dict["k1"], dict2["k1"]);
        }

        [Test]
        public void DeserializeEmptyStrings()
        {
            object v = JsonConvert.DeserializeObject<double?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<char?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<int?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<decimal?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<DateTime?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<DateTimeOffset?>("");
            Assert.IsNull(v);

            v = JsonConvert.DeserializeObject<byte[]>("");
            Assert.IsNull(v);
        }

        public class Sdfsdf
        {
            public double Id { get; set; }
        }

        [Test]
        public void DeserializeDoubleFromEmptyString()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<double>(""); }, "No JSON content found and type 'System.Double' is not nullable. Path '', line 0, position 0.");
        }

        [Test]
        public void DeserializeEnumFromEmptyString()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<StringComparison>(""); }, "No JSON content found and type 'System.StringComparison' is not nullable. Path '', line 0, position 0.");
        }

        [Test]
        public void DeserializeInt32FromEmptyString()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<int>(""); }, "No JSON content found and type 'System.Int32' is not nullable. Path '', line 0, position 0.");
        }

        [Test]
        public void DeserializeByteArrayFromEmptyString()
        {
            byte[] b = JsonConvert.DeserializeObject<byte[]>("");
            Assert.IsNull(b);
        }

        [Test]
        public void DeserializeDoubleFromNullString()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                () => { JsonConvert.DeserializeObject<double>(null); },
                new[]
                {
                    "Value cannot be null." + Environment.NewLine + "Parameter name: value",
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: value" // mono
                });
        }

        [Test]
        public void DeserializeFromNullString()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                () => { JsonConvert.DeserializeObject(null); },
                new[]
                {
                    "Value cannot be null." + Environment.NewLine + "Parameter name: value",
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: value" // mono
                });
        }

        [Test]
        public void DeserializeIsoDatesWithIsoConverter()
        {
            string jsonIsoText =
                @"{""Value"":""2012-02-25T19:55:50.6095676+13:00""}";

            DateTimeWrapper c = JsonConvert.DeserializeObject<DateTimeWrapper>(jsonIsoText, new IsoDateTimeConverter());
            Assert.AreEqual(DateTimeKind.Local, c.Value.Kind);
        }

#if !NET20
        [Test]
        public void DeserializeUTC()
        {
            DateTimeTestClass c =
                JsonConvert.DeserializeObject<DateTimeTestClass>(
                    @"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}",
                    new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Local
                    });

            Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
            Assert.AreEqual(new DateTimeOffset(2008, 12, 12, 12, 12, 12, 0, TimeSpan.Zero), c.DateTimeOffsetField);
            Assert.AreEqual("Pre", c.PreField);
            Assert.AreEqual("Post", c.PostField);

            DateTimeTestClass c2 =
                JsonConvert.DeserializeObject<DateTimeTestClass>(
                    @"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01Z"",""PostField"":""Post""}",
                    new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Local
                    });

            Assert.AreEqual(new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime(), c2.DateTimeField);
            Assert.AreEqual(new DateTimeOffset(2008, 1, 1, 1, 1, 1, 0, TimeSpan.Zero), c2.DateTimeOffsetField);
            Assert.AreEqual("Pre", c2.PreField);
            Assert.AreEqual("Post", c2.PostField);
        }

        [Test]
        public void NullableDeserializeUTC()
        {
            NullableDateTimeTestClass c =
                JsonConvert.DeserializeObject<NullableDateTimeTestClass>(
                    @"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12Z"",""PostField"":""Post""}",
                    new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Local
                    });

            Assert.AreEqual(new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(), c.DateTimeField);
            Assert.AreEqual(new DateTimeOffset(2008, 12, 12, 12, 12, 12, 0, TimeSpan.Zero), c.DateTimeOffsetField);
            Assert.AreEqual("Pre", c.PreField);
            Assert.AreEqual("Post", c.PostField);

            NullableDateTimeTestClass c2 =
                JsonConvert.DeserializeObject<NullableDateTimeTestClass>(
                    @"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}");

            Assert.AreEqual(null, c2.DateTimeField);
            Assert.AreEqual(null, c2.DateTimeOffsetField);
            Assert.AreEqual("Pre", c2.PreField);
            Assert.AreEqual("Post", c2.PostField);
        }

        [Test]
        public void PrivateConstructor()
        {
            var person = PersonWithPrivateConstructor.CreatePerson();
            person.Name = "John Doe";
            person.Age = 25;

            var serializedPerson = JsonConvert.SerializeObject(person);
            var roundtrippedPerson = JsonConvert.DeserializeObject<PersonWithPrivateConstructor>(serializedPerson);

            Assert.AreEqual(person.Name, roundtrippedPerson.Name);
        }
#endif

#if !(NETFX_CORE || DNXCORE50)
        [Test]
        public void MetroBlogPost()
        {
            Product product = new Product();
            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2012, 4, 1);
            product.Price = 3.99M;
            product.Sizes = new[] { "Small", "Medium", "Large" };

            string json = JsonConvert.SerializeObject(product);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": "2012-04-01T00:00:00",
            //  "Price": 3.99,
            //  "Sizes": [ "Small", "Medium", "Large" ]
            //}

            string metroJson = JsonConvert.SerializeObject(product, new JsonSerializerSettings
            {
                ContractResolver = new MetroPropertyNameResolver(),
                Converters = { new MetroStringConverter() },
                Formatting = Formatting.Indented
            });
            StringAssert.AreEqual(@"{
  "":::NAME:::"": "":::APPLE:::"",
  "":::EXPIRYDATE:::"": ""2012-04-01T00:00:00"",
  "":::PRICE:::"": 3.99,
  "":::SIZES:::"": [
    "":::SMALL:::"",
    "":::MEDIUM:::"",
    "":::LARGE:::""
  ]
}", metroJson);
            //{
            //  ":::NAME:::": ":::APPLE:::",
            //  ":::EXPIRYDATE:::": "2012-04-01T00:00:00",
            //  ":::PRICE:::": 3.99,
            //  ":::SIZES:::": [ ":::SMALL:::", ":::MEDIUM:::", ":::LARGE:::" ]
            //}

            Color[] colors = new[] { Color.Blue, Color.Red, Color.Yellow, Color.Green, Color.Black, Color.Brown };

            string json2 = JsonConvert.SerializeObject(colors, new JsonSerializerSettings
            {
                ContractResolver = new MetroPropertyNameResolver(),
                Converters = { new MetroStringConverter(), new MetroColorConverter() },
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"[
  "":::GRAY:::"",
  "":::GRAY:::"",
  "":::GRAY:::"",
  "":::GRAY:::"",
  "":::BLACK:::"",
  "":::GRAY:::""
]", json2);
        }

        public class MetroColorConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Color color = (Color)value;
                Color fixedColor = (color == Color.White || color == Color.Black) ? color : Color.Gray;

                writer.WriteValue(":::" + fixedColor.ToKnownColor().ToString().ToUpper() + ":::");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return Enum.Parse(typeof(Color), reader.Value.ToString());
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Color);
            }
        }
#endif

        public class MultipleItemsClass
        {
            public string Name { get; set; }
        }

        [Test]
        public void MultipleItems()
        {
            IList<MultipleItemsClass> values = new List<MultipleItemsClass>();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"{ ""name"": ""bar"" }{ ""name"": ""baz"" }"));
            reader.SupportMultipleContent = true;

            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }

                JsonSerializer serializer = new JsonSerializer();
                MultipleItemsClass foo = serializer.Deserialize<MultipleItemsClass>(reader);

                values.Add(foo);
            }

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual("bar", values[0].Name);
            Assert.AreEqual("baz", values[1].Name);
        }

        private class FooBar
        {
            public DateTimeOffset Foo { get; set; }
        }

        [Test]
        public void TokenFromBson()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartArray();
            writer.WriteValue("2000-01-02T03:04:05+06:00");
            writer.WriteEndArray();

            byte[] data = ms.ToArray();
            BsonReader reader = new BsonReader(new MemoryStream(data))
            {
                ReadRootValueAsArray = true
            };

            JArray a = (JArray)JArray.ReadFrom(reader);
            JValue v = (JValue)a[0];

            Assert.AreEqual(typeof(string), v.Value.GetType());
            StringAssert.AreEqual(@"[
  ""2000-01-02T03:04:05+06:00""
]", a.ToString());
        }

        [Test]
        public void ObjectRequiredDeserializeMissing()
        {
            string json = "{}";
            IList<string> errors = new List<string>();

            EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> error = (s, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            var o = JsonConvert.DeserializeObject<RequiredObject>(json, new JsonSerializerSettings
            {
                Error = error
            });

            Assert.IsNotNull(o);
            Assert.AreEqual(4, errors.Count);
            Assert.IsTrue(errors[0].StartsWith("Required property 'NonAttributeProperty' not found in JSON. Path ''"));
            Assert.IsTrue(errors[1].StartsWith("Required property 'UnsetProperty' not found in JSON. Path ''"));
            Assert.IsTrue(errors[2].StartsWith("Required property 'AllowNullProperty' not found in JSON. Path ''"));
            Assert.IsTrue(errors[3].StartsWith("Required property 'AlwaysProperty' not found in JSON. Path ''"));
        }

        [Test]
        public void ObjectRequiredDeserializeNull()
        {
            string json = "{'NonAttributeProperty':null,'UnsetProperty':null,'AllowNullProperty':null,'AlwaysProperty':null}";
            IList<string> errors = new List<string>();

            EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> error = (s, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            var o = JsonConvert.DeserializeObject<RequiredObject>(json, new JsonSerializerSettings
            {
                Error = error
            });

            Assert.IsNotNull(o);
            Assert.AreEqual(3, errors.Count);
            Assert.IsTrue(errors[0].StartsWith("Required property 'NonAttributeProperty' expects a value but got null. Path ''"));
            Assert.IsTrue(errors[1].StartsWith("Required property 'UnsetProperty' expects a value but got null. Path ''"));
            Assert.IsTrue(errors[2].StartsWith("Required property 'AlwaysProperty' expects a value but got null. Path ''"));
        }

        [Test]
        public void ObjectRequiredSerialize()
        {
            IList<string> errors = new List<string>();

            EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> error = (s, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            string json = JsonConvert.SerializeObject(new RequiredObject(), new JsonSerializerSettings
            {
                Error = error,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"{
  ""DefaultProperty"": null,
  ""AllowNullProperty"": null
}", json);

            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual("Cannot write a null value for property 'NonAttributeProperty'. Property requires a value. Path ''.", errors[0]);
            Assert.AreEqual("Cannot write a null value for property 'UnsetProperty'. Property requires a value. Path ''.", errors[1]);
            Assert.AreEqual("Cannot write a null value for property 'AlwaysProperty'. Property requires a value. Path ''.", errors[2]);
        }

        [Test]
        public void DeserializeCollectionItemConverter()
        {
            PropertyItemConverter c = new PropertyItemConverter
            {
                Data =
                    new[]
                    {
                        "one",
                        "two",
                        "three"
                    }
            };

            var c2 = JsonConvert.DeserializeObject<PropertyItemConverter>("{'Data':['::ONE::','::TWO::']}");

            Assert.IsNotNull(c2);
            Assert.AreEqual(2, c2.Data.Count);
            Assert.AreEqual("one", c2.Data[0]);
            Assert.AreEqual("two", c2.Data[1]);
        }

        [Test]
        public void SerializeCollectionItemConverter()
        {
            PropertyItemConverter c = new PropertyItemConverter
            {
                Data = new[]
                {
                    "one",
                    "two",
                    "three"
                }
            };

            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual(@"{""Data"":["":::ONE:::"","":::TWO:::"","":::THREE:::""]}", json);
        }

#if !NET20
        [Test]
        public void DateTimeDictionaryKey_DateTimeOffset_Iso()
        {
            IDictionary<DateTimeOffset, int> dic1 = new Dictionary<DateTimeOffset, int>
            {
                { new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), 1 },
                { new DateTimeOffset(2013, 12, 12, 12, 12, 12, TimeSpan.Zero), 2 }
            };

            string json = JsonConvert.SerializeObject(dic1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""2000-12-12T12:12:12+00:00"": 1,
  ""2013-12-12T12:12:12+00:00"": 2
}", json);

            IDictionary<DateTimeOffset, int> dic2 = JsonConvert.DeserializeObject<IDictionary<DateTimeOffset, int>>(json);

            Assert.AreEqual(2, dic2.Count);
            Assert.AreEqual(1, dic2[new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero)]);
            Assert.AreEqual(2, dic2[new DateTimeOffset(2013, 12, 12, 12, 12, 12, TimeSpan.Zero)]);
        }

        [Test]
        public void DateTimeDictionaryKey_DateTimeOffset_MS()
        {
            IDictionary<DateTimeOffset?, int> dic1 = new Dictionary<DateTimeOffset?, int>
            {
                { new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), 1 },
                { new DateTimeOffset(2013, 12, 12, 12, 12, 12, TimeSpan.Zero), 2 }
            };

            string json = JsonConvert.SerializeObject(dic1, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });

            StringAssert.AreEqual(@"{
  ""\/Date(976623132000+0000)\/"": 1,
  ""\/Date(1386850332000+0000)\/"": 2
}", json);

            IDictionary<DateTimeOffset?, int> dic2 = JsonConvert.DeserializeObject<IDictionary<DateTimeOffset?, int>>(json);

            Assert.AreEqual(2, dic2.Count);
            Assert.AreEqual(1, dic2[new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero)]);
            Assert.AreEqual(2, dic2[new DateTimeOffset(2013, 12, 12, 12, 12, 12, TimeSpan.Zero)]);
        }
#endif

        [Test]
        public void DateTimeDictionaryKey_DateTime_Iso()
        {
            IDictionary<DateTime, int> dic1 = new Dictionary<DateTime, int>
            {
                { new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), 1 },
                { new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc), 2 }
            };

            string json = JsonConvert.SerializeObject(dic1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""2000-12-12T12:12:12Z"": 1,
  ""2013-12-12T12:12:12Z"": 2
}", json);

            IDictionary<DateTime, int> dic2 = JsonConvert.DeserializeObject<IDictionary<DateTime, int>>(json);

            Assert.AreEqual(2, dic2.Count);
            Assert.AreEqual(1, dic2[new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
            Assert.AreEqual(2, dic2[new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
        }

        [Test]
        public void DateTimeDictionaryKey_DateTime_Iso_Local()
        {
            IDictionary<DateTime, int> dic1 = new Dictionary<DateTime, int>
            {
                { new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), 1 },
                { new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc), 2 }
            };

            string json = JsonConvert.SerializeObject(dic1, Formatting.Indented, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });

            JObject o = JObject.Parse(json);
            Assert.IsFalse(o.Properties().ElementAt(0).Name.Contains("Z"));
            Assert.IsFalse(o.Properties().ElementAt(1).Name.Contains("Z"));

            IDictionary<DateTime, int> dic2 = JsonConvert.DeserializeObject<IDictionary<DateTime, int>>(json, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

            Assert.AreEqual(2, dic2.Count);
            Assert.AreEqual(1, dic2[new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
            Assert.AreEqual(2, dic2[new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
        }

        [Test]
        public void DateTimeDictionaryKey_DateTime_MS()
        {
            IDictionary<DateTime, int> dic1 = new Dictionary<DateTime, int>
            {
                { new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), 1 },
                { new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc), 2 }
            };

            string json = JsonConvert.SerializeObject(dic1, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });

            StringAssert.AreEqual(@"{
  ""\/Date(976623132000)\/"": 1,
  ""\/Date(1386850332000)\/"": 2
}", json);

            IDictionary<DateTime, int> dic2 = JsonConvert.DeserializeObject<IDictionary<DateTime, int>>(json);

            Assert.AreEqual(2, dic2.Count);
            Assert.AreEqual(1, dic2[new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
            Assert.AreEqual(2, dic2[new DateTime(2013, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
        }

        [Test]
        public void DeserializeEmptyJsonString()
        {
            string s = (string)new JsonSerializer().Deserialize(new JsonTextReader(new StringReader("''")));
            Assert.AreEqual("", s);
        }

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
        [Test]
        public void SerializeAndDeserializeWithAttributes()
        {
            var testObj = new PersonSerializable() { Name = "John Doe", Age = 28 };
            var objDeserialized = SerializeAndDeserialize<PersonSerializable>(testObj);

            Assert.AreEqual(testObj.Name, objDeserialized.Name);
            Assert.AreEqual(0, objDeserialized.Age);
        }

        private T SerializeAndDeserialize<T>(T obj)
            where T : class
        {
            var json = Serialize(obj);
            return Deserialize<T>(json);
        }

        private string Serialize<T>(T obj)
            where T : class
        {
            var stringWriter = new StringWriter();
            var serializer = new JsonSerializer();
            serializer.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            serializer.Serialize(stringWriter, obj);

            return stringWriter.ToString();
        }

        private T Deserialize<T>(string json)
            where T : class
        {
            var jsonReader = new JsonTextReader(new StringReader(json));
            var serializer = new JsonSerializer();
            serializer.ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = false
            };

            return serializer.Deserialize(jsonReader, typeof(T)) as T;
        }
#endif

        [Test]
        public void PropertyItemConverter()
        {
            Event1 e = new Event1
            {
                EventName = "Blackadder III",
                Venue = "Gryphon Theatre",
                Performances = new List<DateTime>
                {
                    DateTimeUtils.ConvertJavaScriptTicksToDateTime(1336458600000),
                    DateTimeUtils.ConvertJavaScriptTicksToDateTime(1336545000000),
                    DateTimeUtils.ConvertJavaScriptTicksToDateTime(1336636800000)
                }
            };

            string json = JsonConvert.SerializeObject(e, Formatting.Indented);
            //{
            //  "EventName": "Blackadder III",
            //  "Venue": "Gryphon Theatre",
            //  "Performances": [
            //    new Date(1336458600000),
            //    new Date(1336545000000),
            //    new Date(1336636800000)
            //  ]
            //}

            StringAssert.AreEqual(@"{
  ""EventName"": ""Blackadder III"",
  ""Venue"": ""Gryphon Theatre"",
  ""Performances"": [
    new Date(
      1336458600000
    ),
    new Date(
      1336545000000
    ),
    new Date(
      1336636800000
    )
  ]
}", json);
        }

#if !(NET20 || NET35)
        public class IgnoreDataMemberTestClass
        {
            [IgnoreDataMember]
            public int Ignored { get; set; }
        }

        [Test]
        public void IgnoreDataMemberTest()
        {
            string json = JsonConvert.SerializeObject(new IgnoreDataMemberTestClass() { Ignored = int.MaxValue }, Formatting.Indented);
            Assert.AreEqual(@"{}", json);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void SerializeDataContractSerializationAttributes()
        {
            DataContractSerializationAttributesClass dataContract = new DataContractSerializationAttributesClass
            {
                NoAttribute = "Value!",
                IgnoreDataMemberAttribute = "Value!",
                DataMemberAttribute = "Value!",
                IgnoreDataMemberAndDataMemberAttribute = "Value!"
            };

            //MemoryStream ms = new MemoryStream();
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DataContractSerializationAttributesClass));
            //serializer.WriteObject(ms, dataContract);

            //Console.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));

            string json = JsonConvert.SerializeObject(dataContract, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""DataMemberAttribute"": ""Value!"",
  ""IgnoreDataMemberAndDataMemberAttribute"": ""Value!""
}", json);

            PocoDataContractSerializationAttributesClass poco = new PocoDataContractSerializationAttributesClass
            {
                NoAttribute = "Value!",
                IgnoreDataMemberAttribute = "Value!",
                DataMemberAttribute = "Value!",
                IgnoreDataMemberAndDataMemberAttribute = "Value!"
            };

            json = JsonConvert.SerializeObject(poco, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""NoAttribute"": ""Value!"",
  ""DataMemberAttribute"": ""Value!""
}", json);
        }
#endif

        [Test]
        public void CheckAdditionalContent()
        {
            string json = "{one:1}{}";

            JsonSerializerSettings settings = new JsonSerializerSettings();
            JsonSerializer s = JsonSerializer.Create(settings);
            IDictionary<string, int> o = s.Deserialize<Dictionary<string, int>>(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(o);
            Assert.AreEqual(1, o["one"]);

            settings.CheckAdditionalContent = true;
            s = JsonSerializer.Create(settings);
            ExceptionAssert.Throws<JsonReaderException>(() => { s.Deserialize<Dictionary<string, int>>(new JsonTextReader(new StringReader(json))); }, "Additional text encountered after finished reading JSON content: {. Path '', line 1, position 7.");
        }

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
        [Test]
        public void DeserializeException()
        {
            string json = @"{ ""ClassName"" : ""System.InvalidOperationException"",
  ""Data"" : null,
  ""ExceptionMethod"" : ""8\nLogin\nAppBiz, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null\nMyApp.LoginBiz\nMyApp.User Login()"",
  ""HResult"" : -2146233079,
  ""HelpURL"" : null,
  ""InnerException"" : { ""ClassName"" : ""System.Exception"",
      ""Data"" : null,
      ""ExceptionMethod"" : null,
      ""HResult"" : -2146233088,
      ""HelpURL"" : null,
      ""InnerException"" : null,
      ""Message"" : ""Inner exception..."",
      ""RemoteStackIndex"" : 0,
      ""RemoteStackTraceString"" : null,
      ""Source"" : null,
      ""StackTraceString"" : null,
      ""WatsonBuckets"" : null
    },
  ""Message"" : ""Outter exception..."",
  ""RemoteStackIndex"" : 0,
  ""RemoteStackTraceString"" : null,
  ""Source"" : ""AppBiz"",
  ""StackTraceString"" : "" at MyApp.LoginBiz.Login() in C:\\MyApp\\LoginBiz.cs:line 44\r\n at MyApp.LoginSvc.Login() in C:\\MyApp\\LoginSvc.cs:line 71\r\n at SyncInvokeLogin(Object , Object[] , Object[] )\r\n at System.ServiceModel.Dispatcher.SyncMethodInvoker.Invoke(Object instance, Object[] inputs, Object[]& outputs)\r\n at System.ServiceModel.Dispatcher.DispatchOperationRuntime.InvokeBegin(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage5(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage41(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage4(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage31(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage3(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage2(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage11(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.ImmutableDispatchRuntime.ProcessMessage1(MessageRpc& rpc)\r\n at System.ServiceModel.Dispatcher.MessageRpc.Process(Boolean isOperationContextSet)"",
  ""WatsonBuckets"" : null
}";

            InvalidOperationException exception = JsonConvert.DeserializeObject<InvalidOperationException>(json);
            Assert.IsNotNull(exception);
            CustomAssert.IsInstanceOfType(typeof(InvalidOperationException), exception);

            Assert.AreEqual("Outter exception...", exception.Message);
        }
#endif

        [Test]
        public void AdditionalContentAfterFinish()
        {
            ExceptionAssert.Throws<JsonException>(() =>
            {
                string json = "[{},1]";

                JsonSerializer serializer = new JsonSerializer();
                serializer.CheckAdditionalContent = true;

                var reader = new JsonTextReader(new StringReader(json));
                reader.Read();
                reader.Read();

                serializer.Deserialize(reader, typeof(MyType));
            }, "Additional text found in JSON string after finishing deserializing object.");
        }

        [Test]
        public void DeserializeRelativeUri()
        {
            IList<Uri> uris = JsonConvert.DeserializeObject<IList<Uri>>(@"[""http://localhost/path?query#hash""]");
            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(new Uri("http://localhost/path?query#hash"), uris[0]);

            Uri uri = JsonConvert.DeserializeObject<Uri>(@"""http://localhost/path?query#hash""");
            Assert.IsNotNull(uri);

            Uri i1 = new Uri("http://localhost/path?query#hash", UriKind.RelativeOrAbsolute);
            Uri i2 = new Uri("http://localhost/path?query#hash");
            Assert.AreEqual(i1, i2);

            uri = JsonConvert.DeserializeObject<Uri>(@"""/path?query#hash""");
            Assert.IsNotNull(uri);
            Assert.AreEqual(new Uri("/path?query#hash", UriKind.RelativeOrAbsolute), uri);
        }

        public class MyConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue("X");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return "X";
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }

        public class MyType
        {
            [JsonProperty(ItemConverterType = typeof(MyConverter))]
            public Dictionary<string, object> MyProperty { get; set; }
        }

        [Test]
        public void DeserializeDictionaryItemConverter()
        {
            var actual = JsonConvert.DeserializeObject<MyType>(@"{ ""MyProperty"":{""Key"":""Y""}}");
            Assert.AreEqual("X", actual.MyProperty["Key"]);
        }

        [Test]
        public void DeserializeCaseInsensitiveKeyValuePairConverter()
        {
            KeyValuePair<int, string> result =
                JsonConvert.DeserializeObject<KeyValuePair<int, string>>(
                    "{key: 123, \"VALUE\": \"test value\"}"
                    );

            Assert.AreEqual(123, result.Key);
            Assert.AreEqual("test value", result.Value);
        }

        [Test]
        public void SerializeKeyValuePairConverterWithCamelCase()
        {
            string json =
                JsonConvert.SerializeObject(new KeyValuePair<int, string>(123, "test value"), Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            StringAssert.AreEqual(@"{
  ""key"": 123,
  ""value"": ""test value""
}", json);
        }

        [JsonObject(MemberSerialization.Fields)]
        public class MyTuple<T1>
        {
            private readonly T1 m_Item1;

            public MyTuple(T1 item1)
            {
                m_Item1 = item1;
            }

            public T1 Item1
            {
                get { return m_Item1; }
            }
        }

        [JsonObject(MemberSerialization.Fields)]
        public class MyTuplePartial<T1>
        {
            private readonly T1 m_Item1;

            public MyTuplePartial(T1 item1)
            {
                m_Item1 = item1;
            }

            public T1 Item1
            {
                get { return m_Item1; }
            }
        }

        [Test]
        public void SerializeFloatingPointHandling()
        {
            string json;
            IList<double> d = new List<double> { 1.1, double.NaN, double.PositiveInfinity };

            json = JsonConvert.SerializeObject(d);
            // [1.1,"NaN","Infinity"]

            json = JsonConvert.SerializeObject(d, new JsonSerializerSettings { FloatFormatHandling = FloatFormatHandling.Symbol });
            // [1.1,NaN,Infinity]

            json = JsonConvert.SerializeObject(d, new JsonSerializerSettings { FloatFormatHandling = FloatFormatHandling.DefaultValue });
            // [1.1,0.0,0.0]

            Assert.AreEqual("[1.1,0.0,0.0]", json);
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
#if !PORTABLE
        [Test]
        public void DeserializeReadOnlyListWithBigInteger()
        {
            string json = @"[
        9000000000000000000000000000000000000000000000000
      ]";

            var l = JsonConvert.DeserializeObject<IReadOnlyList<BigInteger>>(json);

            BigInteger nineQuindecillion = l[0];
            // 9000000000000000000000000000000000000000000000000

            Assert.AreEqual(BigInteger.Parse("9000000000000000000000000000000000000000000000000"), nineQuindecillion);
        }
#endif

        [Test]
        public void DeserializeReadOnlyListWithInt()
        {
            string json = @"[
        900
      ]";

            var l = JsonConvert.DeserializeObject<IReadOnlyList<int>>(json);

            int i = l[0];
            // 900

            Assert.AreEqual(900, i);
        }

        [Test]
        public void DeserializeReadOnlyListWithNullableType()
        {
            string json = @"[
        1,
        null
      ]";

            var l = JsonConvert.DeserializeObject<IReadOnlyList<int?>>(json);

            Assert.AreEqual(1, l[0]);
            Assert.AreEqual(null, l[1]);
        }
#endif

        [Test]
        public void SerializeCustomTupleWithSerializableAttribute()
        {
            var tuple = new MyTuple<int>(500);
            var json = JsonConvert.SerializeObject(tuple);
            Assert.AreEqual(@"{""m_Item1"":500}", json);

            MyTuple<int> obj = null;

            Action doStuff = () => { obj = JsonConvert.DeserializeObject<MyTuple<int>>(json); };

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
            doStuff();
            Assert.AreEqual(500, obj.Item1);
#else
            ExceptionAssert.Throws<JsonSerializationException>(
                doStuff,
                "Unable to find a constructor to use for type Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+MyTuple`1[System.Int32]. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'm_Item1', line 1, position 11.");
#endif
        }

#if DEBUG
        [Test]
        public void SerializeCustomTupleWithSerializableAttributeInPartialTrust()
        {
            try
            {
                JsonTypeReflector.SetFullyTrusted(false);

                var tuple = new MyTuplePartial<int>(500);
                var json = JsonConvert.SerializeObject(tuple);
                Assert.AreEqual(@"{""m_Item1"":500}", json);

                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<MyTuplePartial<int>>(json), "Unable to find a constructor to use for type Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+MyTuplePartial`1[System.Int32]. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'm_Item1', line 1, position 11.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(true);
            }
        }
#endif

#if !(NETFX_CORE || PORTABLE || NET35 || NET20 || PORTABLE40 || DNXCORE50)
        [Test]
        public void SerializeTupleWithSerializableAttribute()
        {
            var tuple = Tuple.Create(500);
            var json = JsonConvert.SerializeObject(tuple, new JsonSerializerSettings
            {
                ContractResolver = new SerializableContractResolver()
            });
            Assert.AreEqual(@"{""m_Item1"":500}", json);

            var obj = JsonConvert.DeserializeObject<Tuple<int>>(json, new JsonSerializerSettings
            {
                ContractResolver = new SerializableContractResolver()
            });
            Assert.AreEqual(500, obj.Item1);
        }

        public class SerializableContractResolver : DefaultContractResolver
        {
            public SerializableContractResolver()
            {
                IgnoreSerializableAttribute = false;
            }
        }
#endif

#if NETFX_CORE
    [Test]
    public void SerializeWinRTJsonObject()
    {
      var o = Windows.Data.Json.JsonObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");

      string json = JsonConvert.SerializeObject(o, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ],
  ""CPU"": ""Intel""
}", json);
    }
#endif

#if !NET20
        [Test]
        public void RoundtripOfDateTimeOffset()
        {
            var content = @"{""startDateTime"":""2012-07-19T14:30:00+09:30""}";

            var jsonSerializerSettings = new JsonSerializerSettings() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateParseHandling = DateParseHandling.DateTimeOffset, DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind };

            var obj = (JObject)JsonConvert.DeserializeObject(content, jsonSerializerSettings);

            var dateTimeOffset = (DateTimeOffset)((JValue)obj["startDateTime"]).Value;

            Assert.AreEqual(TimeSpan.FromHours(9.5), dateTimeOffset.Offset);
            Assert.AreEqual("07/19/2012 14:30:00 +09:30", dateTimeOffset.ToString(CultureInfo.InvariantCulture));
        }

        public class NullableFloats
        {
            public object Object { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public float? NullableFloat { get; set; }
            public double? NullableDouble { get; set; }
            public object ObjectNull { get; set; }
        }

        [Test]
        public void NullableFloatingPoint()
        {
            NullableFloats floats = new NullableFloats
            {
                Object = double.NaN,
                ObjectNull = null,
                Float = float.NaN,
                NullableDouble = double.NaN,
                NullableFloat = null
            };

            string json = JsonConvert.SerializeObject(floats, Formatting.Indented, new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue
            });

            StringAssert.AreEqual(@"{
  ""Object"": 0.0,
  ""Float"": 0.0,
  ""Double"": 0.0,
  ""NullableFloat"": null,
  ""NullableDouble"": null,
  ""ObjectNull"": null
}", json);
        }

        [Test]
        public void DateFormatString()
        {
            CultureInfo culture = new CultureInfo("en-NZ");
            culture.DateTimeFormat.AMDesignator = "a.m.";
            culture.DateTimeFormat.PMDesignator = "p.m.";

            IList<object> dates = new List<object>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1))
            };

            string json = JsonConvert.SerializeObject(dates, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatString = "yyyy tt",
                Culture = culture
            });

            StringAssert.AreEqual(@"[
  ""2000 p.m."",
  ""2000 p.m.""
]", json);
        }

        [Test]
        public void DateFormatStringForInternetExplorer()
        {
            IList<object> dates = new List<object>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1))
            };

            string json = JsonConvert.SerializeObject(dates, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"
            });

            StringAssert.AreEqual(@"[
  ""2000-12-12T12:12:12.000Z"",
  ""2000-12-12T12:12:12.000+01:00""
]", json);
        }

        [Test]
        public void JsonSerializerDateFormatString()
        {
            CultureInfo culture = new CultureInfo("en-NZ");
            culture.DateTimeFormat.AMDesignator = "a.m.";
            culture.DateTimeFormat.PMDesignator = "p.m.";

            IList<object> dates = new List<object>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1))
            };

            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                DateFormatString = "yyyy tt",
                Culture = culture,
                Formatting = Formatting.Indented
            });
            serializer.Serialize(jsonWriter, dates);

            Assert.IsNull(jsonWriter.DateFormatString);
            Assert.AreEqual(CultureInfo.InvariantCulture, jsonWriter.Culture);
            Assert.AreEqual(Formatting.None, jsonWriter.Formatting);

            string json = sw.ToString();

            StringAssert.AreEqual(@"[
  ""2000 p.m."",
  ""2000 p.m.""
]", json);
        }

#if !(NET20 || NET35)
        [Test]
        public void SerializeDeserializeTuple()
        {
            Tuple<int, int> tuple = Tuple.Create(500, 20);
            string json = JsonConvert.SerializeObject(tuple);
            Assert.AreEqual(@"{""Item1"":500,""Item2"":20}", json);

            Tuple<int, int> tuple2 = JsonConvert.DeserializeObject<Tuple<int, int>>(json);
            Assert.AreEqual(500, tuple2.Item1);
            Assert.AreEqual(20, tuple2.Item2);
        }
#endif

        public class MessageWithIsoDate
        {
            public String IsoDate { get; set; }
        }

        [Test]
        public void JsonSerializerStringEscapeHandling()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
                Formatting = Formatting.Indented
            });
            serializer.Serialize(jsonWriter, new { html = "<html></html>" });

            Assert.AreEqual(StringEscapeHandling.Default, jsonWriter.StringEscapeHandling);

            string json = sw.ToString();

            StringAssert.AreEqual(@"{
  ""html"": ""\u003chtml\u003e\u003c/html\u003e""
}", json);
        }

        public class NoConstructorReadOnlyCollection<T> : ReadOnlyCollection<T>
        {
            public NoConstructorReadOnlyCollection() : base(new List<T>())
            {
            }
        }

        [Test]
        public void NoConstructorReadOnlyCollectionTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<NoConstructorReadOnlyCollection<int>>("[1]"), "Cannot deserialize readonly or fixed size list: Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+NoConstructorReadOnlyCollection`1[System.Int32]. Path '', line 1, position 1.");
        }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
        public class NoConstructorReadOnlyDictionary<TKey, TValue> : ReadOnlyDictionary<TKey, TValue>
        {
            public NoConstructorReadOnlyDictionary()
                : base(new Dictionary<TKey, TValue>())
            {
            }
        }

        [Test]
        public void NoConstructorReadOnlyDictionaryTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<NoConstructorReadOnlyDictionary<int, int>>("{'1':1}"), "Cannot deserialize readonly or fixed size dictionary: Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+NoConstructorReadOnlyDictionary`2[System.Int32,System.Int32]. Path '1', line 1, position 5.");
        }
#endif

#if !(PORTABLE || NET35 || NET20 || PORTABLE40)
        [Test]
        public void ReadTooLargeInteger()
        {
            string json = @"[999999999999999999999999999999999999999999999999]";

            IList<BigInteger> l = JsonConvert.DeserializeObject<IList<BigInteger>>(json);

            Assert.AreEqual(BigInteger.Parse("999999999999999999999999999999999999999999999999"), l[0]);

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<IList<long>>(json), "Error converting value 999999999999999999999999999999999999999999999999 to type 'System.Int64'. Path '[0]', line 1, position 49.");
        }
#endif

#if !(NETFX_CORE || DNXCORE50)
        [Serializable]
#endif
        [DataContract]
        public struct Pair<TFirst, TSecond>
        {
            public Pair(TFirst first, TSecond second)
                : this()
            {
                this.First = first;
                this.Second = second;
            }

            [DataMember]
            public TFirst First { get; set; }

            [DataMember]
            public TSecond Second { get; set; }
        }

        [Test]
        public void SerializeStructWithSerializableAndDataContract()
        {
            Pair<string, int> p = new Pair<string, int>("One", 2);

            string json = JsonConvert.SerializeObject(p);

            Assert.AreEqual(@"{""First"":""One"",""Second"":2}", json);

#if !(NETFX_CORE || PORTABLE || DNXCORE50 || PORTABLE40)
            DefaultContractResolver r = new DefaultContractResolver();
            r.IgnoreSerializableAttribute = false;

            json = JsonConvert.SerializeObject(p, new JsonSerializerSettings
            {
                ContractResolver = r
            });

            Assert.AreEqual(@"{""First"":""One"",""Second"":2}", json);
#endif
        }

        [Test]
        public void ReadStringFloatingPointSymbols()
        {
            string json = @"[
  ""NaN"",
  ""Infinity"",
  ""-Infinity""
]";

            IList<float> floats = JsonConvert.DeserializeObject<IList<float>>(json);
            Assert.AreEqual(float.NaN, floats[0]);
            Assert.AreEqual(float.PositiveInfinity, floats[1]);
            Assert.AreEqual(float.NegativeInfinity, floats[2]);

            IList<double> doubles = JsonConvert.DeserializeObject<IList<double>>(json);
            Assert.AreEqual(float.NaN, doubles[0]);
            Assert.AreEqual(float.PositiveInfinity, doubles[1]);
            Assert.AreEqual(float.NegativeInfinity, doubles[2]);
        }

        [Test]
        public void DefaultDateStringFormatVsUnsetDateStringFormat()
        {
            IDictionary<string, object> dates = new Dictionary<string, object>
            {
                { "DateTime-Unspecified", new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Unspecified) },
                { "DateTime-Utc", new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc) },
                { "DateTime-Local", new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Local) },
                { "DateTimeOffset-Zero", new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero) },
                { "DateTimeOffset-Plus1", new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1)) },
                { "DateTimeOffset-Plus15", new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(1.5)) }
            };

            string expected = JsonConvert.SerializeObject(dates, Formatting.Indented);

            string actual = JsonConvert.SerializeObject(dates, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatString = JsonSerializerSettings.DefaultDateFormatString
            });

            Assert.AreEqual(expected, actual);
        }
#endif

#if !NET20
        public class NullableTestClass
        {
            public bool? MyNullableBool { get; set; }
            public int? MyNullableInteger { get; set; }
            public DateTime? MyNullableDateTime { get; set; }
            public DateTimeOffset? MyNullableDateTimeOffset { get; set; }
            public Decimal? MyNullableDecimal { get; set; }
        }

        [Test]
        public void TestStringToNullableDeserialization()
        {
            string json = @"{
  ""MyNullableBool"": """",
  ""MyNullableInteger"": """",
  ""MyNullableDateTime"": """",
  ""MyNullableDateTimeOffset"": """",
  ""MyNullableDecimal"": """"
}";

            NullableTestClass c2 = JsonConvert.DeserializeObject<NullableTestClass>(json);
            Assert.IsNull(c2.MyNullableBool);
            Assert.IsNull(c2.MyNullableInteger);
            Assert.IsNull(c2.MyNullableDateTime);
            Assert.IsNull(c2.MyNullableDateTimeOffset);
            Assert.IsNull(c2.MyNullableDecimal);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void HashSetInterface()
        {
            ISet<string> s1 = new HashSet<string>(new[] { "1", "two", "III" });

            string json = JsonConvert.SerializeObject(s1);

            ISet<string> s2 = JsonConvert.DeserializeObject<ISet<string>>(json);

            Assert.AreEqual(s1.Count, s2.Count);
            foreach (string s in s1)
            {
                Assert.IsTrue(s2.Contains(s));
            }
        }
#endif

        [Test]
        public void DeserializeDecimal()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("1234567890.123456"));
            var settings = new JsonSerializerSettings();
            var serialiser = JsonSerializer.Create(settings);
            decimal? d = serialiser.Deserialize<decimal?>(reader);

            Assert.AreEqual(1234567890.123456m, d);
        }

#if !(PORTABLE || DNXCORE50 || NETFX_CORE || PORTABLE40)
        [Test]
        public void DontSerializeStaticFields()
        {
            string json =
                JsonConvert.SerializeObject(new AnswerFilterModel(), Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        IgnoreSerializableAttribute = false
                    }
                });

            StringAssert.AreEqual(@"{
  ""<Active>k__BackingField"": false,
  ""<Ja>k__BackingField"": false,
  ""<Handlungsbedarf>k__BackingField"": false,
  ""<Beratungsbedarf>k__BackingField"": false,
  ""<Unzutreffend>k__BackingField"": false,
  ""<Unbeantwortet>k__BackingField"": false
}", json);
        }
#endif

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        [Test]
        public void SerializeBigInteger()
        {
            BigInteger i = BigInteger.Parse("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990");

            string json = JsonConvert.SerializeObject(new[] { i }, Formatting.Indented);

            StringAssert.AreEqual(@"[
  123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990
]", json);
        }
#endif

        public class FooConstructor
        {
            [JsonProperty(PropertyName = "something_else")]
            public readonly string Bar;

            public FooConstructor(string bar)
            {
                if (bar == null)
                {
                    throw new ArgumentNullException(nameof(bar));
                }

                Bar = bar;
            }
        }

        [Test]
        public void DeserializeWithConstructor()
        {
            const string json = @"{""something_else"":""my value""}";
            var foo = JsonConvert.DeserializeObject<FooConstructor>(json);
            Assert.AreEqual("my value", foo.Bar);
        }

        [Test]
        public void SerializeCustomReferenceResolver()
        {
            PersonReference john = new PersonReference
            {
                Id = new Guid("0B64FFDF-D155-44AD-9689-58D9ADB137F3"),
                Name = "John Smith"
            };

            PersonReference jane = new PersonReference
            {
                Id = new Guid("AE3C399C-058D-431D-91B0-A36C266441B9"),
                Name = "Jane Smith"
            };

            john.Spouse = jane;
            jane.Spouse = john;

            IList<PersonReference> people = new List<PersonReference>
            {
                john,
                jane
            };

            string json = JsonConvert.SerializeObject(people, new JsonSerializerSettings
            {
#pragma warning disable 618
                ReferenceResolver = new IdReferenceResolver(),
#pragma warning restore 618
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"[
  {
    ""$id"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3"",
    ""Name"": ""John Smith"",
    ""Spouse"": {
      ""$id"": ""ae3c399c-058d-431d-91b0-a36c266441b9"",
      ""Name"": ""Jane Smith"",
      ""Spouse"": {
        ""$ref"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3""
      }
    }
  },
  {
    ""$ref"": ""ae3c399c-058d-431d-91b0-a36c266441b9""
  }
]", json);
        }

        [Test]
        public void NullReferenceResolver()
        {
            PersonReference john = new PersonReference
            {
                Id = new Guid("0B64FFDF-D155-44AD-9689-58D9ADB137F3"),
                Name = "John Smith"
            };

            PersonReference jane = new PersonReference
            {
                Id = new Guid("AE3C399C-058D-431D-91B0-A36C266441B9"),
                Name = "Jane Smith"
            };

            john.Spouse = jane;
            jane.Spouse = john;

            IList<PersonReference> people = new List<PersonReference>
            {
                john,
                jane
            };

            string json = JsonConvert.SerializeObject(people, new JsonSerializerSettings
            {
#pragma warning disable 618
                ReferenceResolver = null,
#pragma warning restore 618
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""Name"": ""John Smith"",
    ""Spouse"": {
      ""$id"": ""2"",
      ""Name"": ""Jane Smith"",
      ""Spouse"": {
        ""$ref"": ""1""
      }
    }
  },
  {
    ""$ref"": ""2""
  }
]", json);
        }

#if !(PORTABLE || PORTABLE40 || NETFX_CORE || DNXCORE50)
        [Test]
        public void SerializeDictionaryWithStructKey()
        {
            string json = JsonConvert.SerializeObject(
                new Dictionary<Size, Size> { { new Size(1, 2), new Size(3, 4) } }
                );

            Assert.AreEqual(@"{""1, 2"":""3, 4""}", json);

            Dictionary<Size, Size> d = JsonConvert.DeserializeObject<Dictionary<Size, Size>>(json);

            Assert.AreEqual(new Size(1, 2), d.Keys.First());
            Assert.AreEqual(new Size(3, 4), d.Values.First());
        }
#endif

        [Test]
        public void DeserializeCustomReferenceResolver()
        {
            string json = @"[
  {
    ""$id"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3"",
    ""Name"": ""John Smith"",
    ""Spouse"": {
      ""$id"": ""ae3c399c-058d-431d-91b0-a36c266441b9"",
      ""Name"": ""Jane Smith"",
      ""Spouse"": {
        ""$ref"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3""
      }
    }
  },
  {
    ""$ref"": ""ae3c399c-058d-431d-91b0-a36c266441b9""
  }
]";

            IList<PersonReference> people = JsonConvert.DeserializeObject<IList<PersonReference>>(json, new JsonSerializerSettings
            {
#pragma warning disable 618
                ReferenceResolver = new IdReferenceResolver(),
#pragma warning restore 618
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });

            Assert.AreEqual(2, people.Count);

            PersonReference john = people[0];
            PersonReference jane = people[1];

            Assert.AreEqual(john, jane.Spouse);
            Assert.AreEqual(jane, john.Spouse);
        }

        [Test]
        public void DeserializeCustomReferenceResolver_ViaProvider()
        {
            string json = @"[
  {
    ""$id"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3"",
    ""Name"": ""John Smith"",
    ""Spouse"": {
      ""$id"": ""ae3c399c-058d-431d-91b0-a36c266441b9"",
      ""Name"": ""Jane Smith"",
      ""Spouse"": {
        ""$ref"": ""0b64ffdf-d155-44ad-9689-58d9adb137f3""
      }
    }
  },
  {
    ""$ref"": ""ae3c399c-058d-431d-91b0-a36c266441b9""
  }
]";

            IList<PersonReference> people = JsonConvert.DeserializeObject<IList<PersonReference>>(json, new JsonSerializerSettings
            {
                ReferenceResolverProvider = () => new IdReferenceResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });

            Assert.AreEqual(2, people.Count);

            PersonReference john = people[0];
            PersonReference jane = people[1];

            Assert.AreEqual(john, jane.Spouse);
            Assert.AreEqual(jane, john.Spouse);
        }

#if !(NETFX_CORE || NET35 || NET20 || PORTABLE || PORTABLE40)
        [Test]
        public void TypeConverterOnInterface()
        {
            var consoleWriter = new ConsoleWriter();

            // If dynamic type handling is enabled, case 1 and 3 work fine
            var options = new JsonSerializerSettings
            {
                Converters = new JsonConverterCollection { new TypeConverterJsonConverter() },
                //TypeNameHandling = TypeNameHandling.All
            };

            //
            // Case 1: Serialize the concrete value and restore it from the interface
            // Therefore we need dynamic handling of type information if the type is not serialized with the type converter directly
            //
            var text1 = JsonConvert.SerializeObject(consoleWriter, Formatting.Indented, options);
            Assert.AreEqual(@"""Console Writer""", text1);

            var restoredWriter = JsonConvert.DeserializeObject<IMyInterface>(text1, options);
            Assert.AreEqual("ConsoleWriter", restoredWriter.PrintTest());

            //
            // Case 2: Serialize a dictionary where the interface is the key
            // The key is always serialized with its ToString() method and therefore needs a mechanism to be restored from that (using the type converter)
            //
            var dict2 = new Dictionary<IMyInterface, string>();
            dict2.Add(consoleWriter, "Console");

            var text2 = JsonConvert.SerializeObject(dict2, Formatting.Indented, options);
            StringAssert.AreEqual(@"{
  ""Console Writer"": ""Console""
}", text2);

            var restoredObject = JsonConvert.DeserializeObject<Dictionary<IMyInterface, string>>(text2, options);
            Assert.AreEqual("ConsoleWriter", restoredObject.First().Key.PrintTest());

            //
            // Case 3 Serialize a dictionary where the interface is the value
            // The key is always serialized with its ToString() method and therefore needs a mechanism to be restored from that (using the type converter)
            //
            var dict3 = new Dictionary<string, IMyInterface>();
            dict3.Add("Console", consoleWriter);

            var text3 = JsonConvert.SerializeObject(dict3, Formatting.Indented, options);
            StringAssert.AreEqual(@"{
  ""Console"": ""Console Writer""
}", text3);

            var restoredDict2 = JsonConvert.DeserializeObject<Dictionary<string, IMyInterface>>(text3, options);
            Assert.AreEqual("ConsoleWriter", restoredDict2.First().Value.PrintTest());
        }
#endif

        [Test]
        public void Main()
        {
            ParticipantEntity product = new ParticipantEntity();
            product.Properties = new Dictionary<string, string> { { "s", "d" } };
            string json = JsonConvert.SerializeObject(product);

            Assert.AreEqual(@"{""pa_info"":{""s"":""d""}}", json);
            ParticipantEntity deserializedProduct = JsonConvert.DeserializeObject<ParticipantEntity>(json);
        }

#if !(PORTABLE || NETFX_CORE)
        public class ConvertibleId : IConvertible
        {
            public int Value;

            TypeCode IConvertible.GetTypeCode()
            {
                return TypeCode.Object;
            }

            object IConvertible.ToType(Type conversionType, IFormatProvider provider)
            {
                if (conversionType == typeof(object))
                {
                    return this;
                }
                if (conversionType == typeof(int))
                {
                    return (int)Value;
                }
                if (conversionType == typeof(long))
                {
                    return (long)Value;
                }
                if (conversionType == typeof(string))
                {
                    return Value.ToString(CultureInfo.InvariantCulture);
                }
                throw new InvalidCastException();
            }

            bool IConvertible.ToBoolean(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            byte IConvertible.ToByte(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            char IConvertible.ToChar(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            DateTime IConvertible.ToDateTime(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            decimal IConvertible.ToDecimal(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            double IConvertible.ToDouble(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            short IConvertible.ToInt16(IFormatProvider provider)
            {
                return (short)Value;
            }

            int IConvertible.ToInt32(IFormatProvider provider)
            {
                return Value;
            }

            long IConvertible.ToInt64(IFormatProvider provider)
            {
                return (long)Value;
            }

            sbyte IConvertible.ToSByte(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            float IConvertible.ToSingle(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            string IConvertible.ToString(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            ushort IConvertible.ToUInt16(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            uint IConvertible.ToUInt32(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }

            ulong IConvertible.ToUInt64(IFormatProvider provider)
            {
                throw new InvalidCastException();
            }
        }

        public class TestClassConvertable
        {
            public ConvertibleId Id;
            public int X;
        }

        [Test]
        public void ConvertibleIdTest()
        {
            var c = new TestClassConvertable { Id = new ConvertibleId { Value = 1 }, X = 2 };
            var s = JsonConvert.SerializeObject(c, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Id"": ""1"",
  ""X"": 2
}", s);
        }
#endif

        [Test]
        public void DuplicatePropertiesInNestedObject()
        {
            string content = @"{""result"":{""time"":1408188592,""time"":1408188593},""error"":null,""id"":""1""}";
            JObject o = JsonConvert.DeserializeObject<JObject>(content);
            int time = (int)o["result"]["time"];

            Assert.AreEqual(1408188593, time);
        }

        [Test]
        public void RoundtripUriOriginalString()
        {
            string originalUri = "https://test.com?m=a%2bb";

            Uri uriWithPlus = new Uri(originalUri);

            string jsonWithPlus = JsonConvert.SerializeObject(uriWithPlus);

            Uri uriWithPlus2 = JsonConvert.DeserializeObject<Uri>(jsonWithPlus);

            Assert.AreEqual(originalUri, uriWithPlus2.OriginalString);
        }

        [Test]
        public void DateFormatStringWithDateTime()
        {
            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Dec-Friday-22""", json);

            DateTime dt1 = JsonConvert.DeserializeObject<DateTime>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString
            };
            JValue v = (JValue)JToken.ReadFrom(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTime), v.Value.GetType());
            Assert.AreEqual(dt, (DateTime)v.Value);

            reader = new JsonTextReader(new StringReader(@"""abc"""))
            {
                DateFormatString = dateFormatString
            };
            v = (JValue)JToken.ReadFrom(reader);

            Assert.AreEqual(JTokenType.String, v.Type);
            Assert.AreEqual(typeof(string), v.Value.GetType());
            Assert.AreEqual("abc", v.Value);
        }

        [Test]
        public void DateFormatStringWithDateTimeAndCulture()
        {
            CultureInfo culture = new CultureInfo("tr-TR");

            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Ara-Cuma-22""", json);

            DateTime dt1 = JsonConvert.DeserializeObject<DateTime>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };
            JValue v = (JValue)JToken.ReadFrom(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTime), v.Value.GetType());
            Assert.AreEqual(dt, (DateTime)v.Value);

            reader = new JsonTextReader(new StringReader(@"""2000-pie-Dec-Friday-22"""))
            {
                DateFormatString = dateFormatString,
                Culture = culture
            };
            v = (JValue)JToken.ReadFrom(reader);

            Assert.AreEqual(JTokenType.String, v.Type);
            Assert.AreEqual(typeof(string), v.Value.GetType());
            Assert.AreEqual("2000-pie-Dec-Friday-22", v.Value);
        }

        [Test]
        public void DateFormatStringWithDictionaryKey_DateTime()
        {
            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(new Dictionary<DateTime, string>
            {
                { dt, "123" }
            }, settings);

            StringAssert.AreEqual(@"{
  ""2000-pie-Dec-Friday-22"": ""123""
}", json);

            Dictionary<DateTime, string> d = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(json, settings);

            Assert.AreEqual(dt, d.Keys.ElementAt(0));
        }

        [Test]
        public void DateFormatStringWithDictionaryKey_DateTime_ReadAhead()
        {
            DateTime dt = new DateTime(2000, 12, 22);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(new Dictionary<DateTime, string>
            {
                { dt, "123" }
            }, settings);

            StringAssert.AreEqual(@"{
  ""2000-pie-Dec-Friday-22"": ""123""
}", json);

            Dictionary<DateTime, string> d = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(json, settings);

            Assert.AreEqual(dt, d.Keys.ElementAt(0));
        }

#if !NET20
        [Test]
        public void DateFormatStringWithDictionaryKey_DateTimeOffset()
        {
            DateTimeOffset dt = new DateTimeOffset(2000, 12, 22, 0, 0, 0, TimeSpan.Zero);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd'!'K";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(new Dictionary<DateTimeOffset, string>
            {
                { dt, "123" }
            }, settings);

            StringAssert.AreEqual(@"{
  ""2000-pie-Dec-Friday-22!+00:00"": ""123""
}", json);

            Dictionary<DateTimeOffset, string> d = JsonConvert.DeserializeObject<Dictionary<DateTimeOffset, string>>(json, settings);

            Assert.AreEqual(dt, d.Keys.ElementAt(0));
        }

        [Test]
        public void DateFormatStringWithDictionaryKey_DateTimeOffset_ReadAhead()
        {
            DateTimeOffset dt = new DateTimeOffset(2000, 12, 22, 0, 0, 0, TimeSpan.Zero);
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd'!'K";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(new Dictionary<DateTimeOffset, string>
            {
                { dt, "123" }
            }, settings);

            StringAssert.AreEqual(@"{
  ""2000-pie-Dec-Friday-22!+00:00"": ""123""
}", json);

            Dictionary<DateTimeOffset, string> d = JsonConvert.DeserializeObject<Dictionary<DateTimeOffset, string>>(json, settings);

            Assert.AreEqual(dt, d.Keys.ElementAt(0));
        }

        [Test]
        public void DateFormatStringWithDateTimeOffset()
        {
            DateTimeOffset dt = new DateTimeOffset(new DateTime(2000, 12, 22));
            string dateFormatString = "yyyy'-pie-'MMM'-'dddd'-'dd";
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = dateFormatString
            };

            string json = JsonConvert.SerializeObject(dt, settings);

            Assert.AreEqual(@"""2000-pie-Dec-Friday-22""", json);

            DateTimeOffset dt1 = JsonConvert.DeserializeObject<DateTimeOffset>(json, settings);

            Assert.AreEqual(dt, dt1);

            JsonTextReader reader = new JsonTextReader(new StringReader(json))
            {
                DateFormatString = dateFormatString,
                DateParseHandling = DateParseHandling.DateTimeOffset
            };
            JValue v = (JValue)JToken.ReadFrom(reader);

            Assert.AreEqual(JTokenType.Date, v.Type);
            Assert.AreEqual(typeof(DateTimeOffset), v.Value.GetType());
            Assert.AreEqual(dt, (DateTimeOffset)v.Value);
        }

        [DataContract]
        public class ConstantTestClass
        {
            [DataMember]
            public const char MY_CONSTANT = '.';
        }

        [Test]
        public void DeserializeConstantProperty()
        {
            ConstantTestClass c1 = new ConstantTestClass();

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""MY_CONSTANT"": "".""
}", json);

            JsonConvert.DeserializeObject<ConstantTestClass>(json);
        }
#endif

        [Test]
        public void SerializeObjectWithEvent()
        {
            MyObservableObject o = new MyObservableObject
            {
                TestString = "Test string"
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""PropertyChanged"": null,
  ""TestString"": ""Test string""
}", json);

            MyObservableObject o2 = JsonConvert.DeserializeObject<MyObservableObject>(json);
            Assert.AreEqual("Test string", o2.TestString);
        }

        public class MyObservableObject : ObservableObject
        {
            public new string PropertyChanged;

            public string TestString { get; set; }
        }

        public class ObservableObject : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected PropertyChangedEventHandler PropertyChangedHandler
            {
                get { return PropertyChanged; }
            }
        }

        [Test]
        public void ParameterizedConstructorWithBasePrivateProperties()
        {
            var original = new DerivedConstructorType("Base", "Derived");

            var serializerSettings = new JsonSerializerSettings();
            var jsonCopy = JsonConvert.SerializeObject(original, serializerSettings);

            var clonedObject = JsonConvert.DeserializeObject<DerivedConstructorType>(jsonCopy, serializerSettings);

            Assert.AreEqual("Base", clonedObject.BaseProperty);
            Assert.AreEqual("Derived", clonedObject.DerivedProperty);
        }

        public class DerivedConstructorType : BaseConstructorType
        {
            public DerivedConstructorType(string baseProperty, string derivedProperty)
                : base(baseProperty)
            {
                DerivedProperty = derivedProperty;
            }

            [JsonProperty]
            public string DerivedProperty { get; private set; }
        }

        public class BaseConstructorType
        {
            [JsonProperty]
            public string BaseProperty { get; private set; }

            public BaseConstructorType(string baseProperty)
            {
                BaseProperty = baseProperty;
            }
        }

        public class ErroringJsonConverter : JsonConverter
        {
            public ErroringJsonConverter(string s)
            {
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }
        }

        [JsonConverter(typeof(ErroringJsonConverter))]
        public class ErroringTestClass
        {
        }

        [Test]
        public void ErrorCreatingJsonConverter()
        {
            ExceptionAssert.Throws<JsonException>(() => JsonConvert.SerializeObject(new ErroringTestClass()), "Error creating 'Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+ErroringJsonConverter'.");
        }

        [Test]
        public void DeserializeInvalidOctalRootError()
        {
            ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<string>("020474068"), "Input string '020474068' is not a valid number. Path '', line 1, position 9.");
        }

        [Test]
        public void DeserializedDerivedWithPrivate()
        {
            string json = @"{
  ""DerivedProperty"": ""derived"",
  ""BaseProperty"": ""base""
}";

            var d = JsonConvert.DeserializeObject<DerivedWithPrivate>(json);

            Assert.AreEqual("base", d.BaseProperty);
            Assert.AreEqual("derived", d.DerivedProperty);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        [Test]
        public void DeserializeNullableUnsignedLong()
        {
            NullableLongTestClass instance = new NullableLongTestClass
            {
                Value = ulong.MaxValue
            };
            string output = JsonConvert.SerializeObject(instance);
            NullableLongTestClass result = JsonConvert.DeserializeObject<NullableLongTestClass>(output);

            Assert.AreEqual(ulong.MaxValue, result.Value);
        }

        public class NullableLongTestClass
        {
            public ulong? Value { get; set; }
        }
#endif

#if !(NETFX_CORE || DNXCORE50)
        [Test]
        public void MailMessageConverterTest()
        {
            const string JsonMessage = @"{
  ""From"": {
    ""Address"": ""askywalker@theEmpire.gov"",
    ""DisplayName"": ""Darth Vader""
  },
  ""Sender"": null,
  ""ReplyTo"": null,
  ""ReplyToList"": [],
  ""To"": [
    {
      ""Address"": ""lskywalker@theRebellion.org"",
      ""DisplayName"": ""Luke Skywalker""
    }
  ],
  ""Bcc"": [],
  ""CC"": [
    {
      ""Address"": ""lorgana@alderaan.gov"",
      ""DisplayName"": ""Princess Leia""
    }
  ],
  ""Priority"": 0,
  ""DeliveryNotificationOptions"": 0,
  ""Subject"": ""Family tree"",
  ""SubjectEncoding"": null,
  ""Headers"": [],
  ""HeadersEncoding"": null,
  ""Body"": ""<strong>I am your father!</strong>"",
  ""BodyEncoding"": ""US-ASCII"",
  ""BodyTransferEncoding"": -1,
  ""IsBodyHtml"": true,
  ""Attachments"": [
    {
      ""FileName"": ""skywalker family tree.jpg"",
      ""ContentBase64"": ""AQIDBAU=""
    }
  ],
  ""AlternateViews"": []
}";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<System.Net.Mail.MailMessage>(
                    JsonMessage,
                    new MailAddressReadConverter(),
                    new AttachmentReadConverter(),
                    new EncodingReadConverter());
            },
                "Cannot populate list type System.Net.Mime.HeaderCollection. Path 'Headers', line 26, position 14.");
        }

        public class MailAddressReadConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(System.Net.Mail.MailAddress);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var messageJObject = serializer.Deserialize<JObject>(reader);
                if (messageJObject == null)
                {
                    return null;
                }

                var address = messageJObject.GetValue("Address", StringComparison.OrdinalIgnoreCase).ToObject<string>();

                JToken displayNameToken;
                string displayName;
                if (messageJObject.TryGetValue("DisplayName", StringComparison.OrdinalIgnoreCase, out displayNameToken)
                    && !string.IsNullOrEmpty(displayName = displayNameToken.ToObject<string>()))
                {
                    return new System.Net.Mail.MailAddress(address, displayName);
                }

                return new System.Net.Mail.MailAddress(address);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class AttachmentReadConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(System.Net.Mail.Attachment);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var info = serializer.Deserialize<AttachmentInfo>(reader);

                var attachment = info != null
                    ? new System.Net.Mail.Attachment(new MemoryStream(Convert.FromBase64String(info.ContentBase64)), "application/octet-stream")
                    {
                        ContentDisposition = { FileName = info.FileName }
                    }
                    : null;
                return attachment;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            private class AttachmentInfo
            {
                [JsonProperty(Required = Required.Always)]
                public string FileName { get; set; }

                [JsonProperty(Required = Required.Always)]
                public string ContentBase64 { get; set; }
            }
        }

        public class EncodingReadConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(Encoding).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var encodingName = serializer.Deserialize<string>(reader);
                if (encodingName == null)
                {
                    return null;
                }

                return Encoding.GetEncoding(encodingName);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
#endif
    }

    public class DerivedWithPrivate : BaseWithPrivate
    {
        [JsonProperty]
        public string DerivedProperty { get; private set; }
    }

    public class BaseWithPrivate
    {
        [JsonProperty]
        public string BaseProperty { get; private set; }
    }

    public abstract class Test<T>
    {
        public abstract T Value { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DecimalTest : Test<decimal>
    {
        protected DecimalTest()
        {
        }

        public DecimalTest(decimal val)
        {
            Value = val;
        }

        [JsonProperty]
        public override decimal Value { get; set; }
    }

    public class NonPublicConstructorWithJsonConstructor
    {
        public string Value { get; private set; }
        public string Constructor { get; private set; }

        [JsonConstructor]
        private NonPublicConstructorWithJsonConstructor()
        {
            Constructor = "NonPublic";
        }

        public NonPublicConstructorWithJsonConstructor(string value)
        {
            Value = value;
            Constructor = "Public Parameterized";
        }
    }

    public abstract class AbstractTestClass
    {
        public string Value { get; set; }
    }

    public class AbstractImplementationTestClass : AbstractTestClass
    {
    }

    public abstract class AbstractListTestClass<T> : List<T>
    {
    }

    public class AbstractImplementationListTestClass<T> : AbstractListTestClass<T>
    {
    }

    public abstract class AbstractDictionaryTestClass<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }

    public class AbstractImplementationDictionaryTestClass<TKey, TValue> : AbstractDictionaryTestClass<TKey, TValue>
    {
    }

    public class PublicConstructorOverridenByJsonConstructor
    {
        public string Value { get; private set; }
        public string Constructor { get; private set; }

        public PublicConstructorOverridenByJsonConstructor()
        {
            Constructor = "NonPublic";
        }

        [JsonConstructor]
        public PublicConstructorOverridenByJsonConstructor(string value)
        {
            Value = value;
            Constructor = "Public Parameterized";
        }
    }

    public class MultipleParametrizedConstructorsJsonConstructor
    {
        public string Value { get; private set; }
        public int Age { get; private set; }
        public string Constructor { get; private set; }

        public MultipleParametrizedConstructorsJsonConstructor(string value)
        {
            Value = value;
            Constructor = "Public Parameterized 1";
        }

        [JsonConstructor]
        public MultipleParametrizedConstructorsJsonConstructor(string value, int age)
        {
            Value = value;
            Age = age;
            Constructor = "Public Parameterized 2";
        }
    }

    public class EnumerableClass
    {
        public IEnumerable<string> Enumerable { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ItemBase
    {
        [JsonProperty]
        public string Name { get; set; }
    }

    public class ComplexItem : ItemBase
    {
        public Stream Source { get; set; }
    }

    public class DeserializeStringConvert
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public decimal Price { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class StaticTestClass
    {
        [JsonProperty]
        public int x = 1;

        [JsonProperty]
        public static int y = 2;

        [JsonProperty]
        public static int z { get; set; }

        static StaticTestClass()
        {
            z = 3;
        }
    }
}