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
#if !(NET20 || NET35 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
using System.Numerics;
#endif
#if !(NET20 || DNXCORE50) || NETSTANDARD2_0
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Threading;
#endif
#if !(NET20 || DNXCORE50)
using System.Web.Script.Serialization;
#endif
using System.Text;
using System.Text.RegularExpressions;
#if DNXCORE50
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
using Newtonsoft.Json.Tests.TestObjects.Events;
using Newtonsoft.Json.Tests.TestObjects.GeoCoding;
using Newtonsoft.Json.Tests.TestObjects.Organization;
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
using Action = Newtonsoft.Json.Serialization.Action;
#else
using System.Linq;
#endif
#if !(DNXCORE50)
using System.Drawing;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class JsonSerializerTest : TestFixtureBase
    {
        [Test]
        public void ListSourceSerialize()
        {
            ListSourceTest c = new ListSourceTest();
            c.strprop = "test";
            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual(@"{""strprop"":""test""}", json);

            ListSourceTest c2 = JsonConvert.DeserializeObject<ListSourceTest>(json);

            Assert.AreEqual("test", c2.strprop);
        }

        [Test]
        public void DeserializeImmutableStruct()
        {
            var result = JsonConvert.DeserializeObject<ImmutableStruct>("{ \"Value\": \"working\", \"Value2\": 2 }");

            Assert.AreEqual("working", result.Value);
            Assert.AreEqual(2, result.Value2);
        }

        public struct AlmostImmutableStruct
        {
            public AlmostImmutableStruct(string value, int value2)
            {
                Value = value;
                Value2 = value2;
            }

            public string Value { get; }
            public int Value2 { get; set; }
        }

        [Test]
        public void DeserializeAlmostImmutableStruct()
        {
            var result = JsonConvert.DeserializeObject<AlmostImmutableStruct>("{ \"Value\": \"working\", \"Value2\": 2 }");

            Assert.AreEqual(null, result.Value);
            Assert.AreEqual(2, result.Value2);
        }

        [Test]
        public void DontCloseInputOnDeserializeError()
        {
            using (var s = System.IO.File.OpenRead(ResolvePath("large.json")))
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

        [Test]
        public void DeserializeGenericIEnumerableWithImplicitConversion()
        {
            string deserialized = @"{
  ""Enumerable"": [ ""abc"", ""def"" ] 
}";
            var enumerableClass = JsonConvert.DeserializeObject<GenericIEnumerableWithImplicitConversion>(deserialized);
            var enumerableObject = enumerableClass.Enumerable.ToArray();
            Assert.AreEqual(2, enumerableObject.Length);
            Assert.AreEqual("abc", enumerableObject[0].Value);
            Assert.AreEqual("def", enumerableObject[1].Value);
        }

#if !(PORTABLE || PORTABLE40 || NET20 || NET35) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void LargeIntegerAsString()
        {
            var largeBrokenNumber = JsonConvert.DeserializeObject<Foo64>("{\"Blah\": 43443333222211111117 }");
            Assert.AreEqual("43443333222211111117", largeBrokenNumber.Blah);

            var largeOddWorkingNumber = JsonConvert.DeserializeObject<Foo64>("{\"Blah\": 53443333222211111117 }");
            Assert.AreEqual("53443333222211111117", largeOddWorkingNumber.Blah);
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

#if !(PORTABLE || PORTABLE40 || NET35 || NET20) || NETSTANDARD1_3 || NETSTANDARD2_0
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

        [Test]
        public void NoErrorWhenValueDoesNotMatchIgnoredProperty()
        {
            IgnoredProperty p = JsonConvert.DeserializeObject<IgnoredProperty>("{'StringProp1':[1,2,3],'StringProp2':{}}");
            Assert.IsNull(p.StringProp1);
            Assert.IsNull(p.StringProp2);
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

        [Test]
        public void Serialize_ItemRequired_DisallowedNull()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(new DictionaryWithNoNull()); }, "Cannot write a null value for property 'Name'. Property requires a non-null value. Path ''.");
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

            StringAssert.AreEqual(@"{
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

#if !NET20
        [Test]
        public void DeserializeEnumsByName()
        {
            var e1 = JsonConvert.DeserializeObject<EnumA>("'ValueA'");
            Assert.AreEqual(EnumA.ValueA, e1);

            var e2 = JsonConvert.DeserializeObject<EnumA>("'value_a'", new StringEnumConverter());
            Assert.AreEqual(EnumA.ValueA, e2);
        }
#endif

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

            StringAssert.AreEqual(@"{
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

#if !(NET20 || NET35 || PORTABLE40)
        [Test]
        public void SerializeValueTuple()
        {
            ValueTuple<int, int, string> t = ValueTuple.Create(1, 2, "string");

            string json = JsonConvert.SerializeObject(t, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Item1"": 1,
  ""Item2"": 2,
  ""Item3"": ""string""
}", json);

            ValueTuple<int, int, string> t2 = JsonConvert.DeserializeObject<ValueTuple<int, int, string>>(json);

            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            Assert.AreEqual("string", t2.Item3);
        }
#endif

        [Test]
        public void DeserializeStructWithConstructorAttribute()
        {
            ImmutableStructWithConstructorAttribute result = JsonConvert.DeserializeObject<ImmutableStructWithConstructorAttribute>("{ \"Value\": \"working\" }");

            Assert.AreEqual("working", result.Value);
        }

        public struct ImmutableStructWithConstructorAttribute
        {
            [JsonConstructor]
            public ImmutableStructWithConstructorAttribute(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

#if !(DNXCORE50 || NET20)
        [Test]
        public void SerializeMetadataType()
        {
            CustomerWithMetadataType c = new CustomerWithMetadataType()
            {
                UpdatedBy_Id = Guid.NewGuid()
            };
            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual("{}", json);

            CustomerWithMetadataType c2 = JsonConvert.DeserializeObject<CustomerWithMetadataType>("{'UpdatedBy_Id':'F6E0666D-13C7-4745-B486-800812C8F6DE'}");

            Assert.AreEqual(Guid.Empty, c2.UpdatedBy_Id);
        }

        [Test]
        public void SerializeMetadataType2()
        {
            FaqItem c = new FaqItem()
            {
                FaqId = 1,
                Sections =
                {
                    new FaqSection()
                }
            };

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

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
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

        [Test]
        public void SerializeObservableCollection()
        {
            ObservableCollection<string> c1 = new ObservableCollection<string> { "1", "2" };

            string output = JsonConvert.SerializeObject(c1);
            Assert.AreEqual("[\"1\",\"2\"]", output);

            ObservableCollection<string> c2 = JsonConvert.DeserializeObject<ObservableCollection<string>>(output);
            Assert.AreEqual(2, c2.Count);
            Assert.AreEqual("1", c2[0]);
            Assert.AreEqual("2", c2[1]);
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

        [Test]
        public void NewProperty()
        {
            Assert.AreEqual(@"{""IsTransient"":true}", JsonConvert.SerializeObject(new ChildClass { IsTransient = true }));

            var childClass = JsonConvert.DeserializeObject<ChildClass>(@"{""IsTransient"":true}");
            Assert.AreEqual(true, childClass.IsTransient);
        }

        [Test]
        public void NewPropertyVirtual()
        {
            Assert.AreEqual(@"{""IsTransient"":true}", JsonConvert.SerializeObject(new ChildClassVirtual { IsTransient = true }));

            var childClass = JsonConvert.DeserializeObject<ChildClassVirtual>(@"{""IsTransient"":true}");
            Assert.AreEqual(true, childClass.IsTransient);
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

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsNotNull(serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsNotNull(serializer.SerializationBinder);

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
#pragma warning disable CS0618 // Type or member is obsolete
            serializer.Binder = customBinder;
            Assert.AreEqual(customBinder, serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsInstanceOf(typeof(DefaultSerializationBinder), serializer.SerializationBinder);

            serializer.SerializationBinder = customBinder;
            Assert.AreEqual(customBinder, serializer.SerializationBinder);

#pragma warning disable CS0618 // Type or member is obsolete
            // can still fetch because DefaultSerializationBinder inherits from SerializationBinder
            Assert.AreEqual(customBinder, serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

            serializer.CheckAdditionalContent = true;
            Assert.AreEqual(true, serializer.CheckAdditionalContent);

            serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, serializer.ConstructorHandling);

#if !(DNXCORE50) || NETSTANDARD2_0
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

#if !(PORTABLE || PORTABLE40 || NET20 || DNXCORE50) || NETSTANDARD2_0
#pragma warning disable 618
            serializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, serializer.TypeNameAssemblyFormat);
#pragma warning restore 618

            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, serializer.TypeNameAssemblyFormatHandling);

            serializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
#pragma warning disable 618
            Assert.AreEqual(FormatterAssemblyStyle.Simple, serializer.TypeNameAssemblyFormat);
#pragma warning restore 618
#endif

            serializer.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, serializer.TypeNameAssemblyFormatHandling);

            serializer.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, serializer.TypeNameHandling);
        }

        [Test]
        public void JsonSerializerSettingsProperties()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsNull(settings.Binder);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsNull(settings.SerializationBinder);

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
#pragma warning disable CS0618 // Type or member is obsolete
            settings.Binder = customBinder;
            Assert.AreEqual(customBinder, settings.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

            settings.CheckAdditionalContent = true;
            Assert.AreEqual(true, settings.CheckAdditionalContent);

            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, settings.ConstructorHandling);

#if !(DNXCORE50) || NETSTANDARD2_0
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

#if !(PORTABLE || PORTABLE40 || NET20 || DNXCORE50) || NETSTANDARD2_0
#pragma warning disable 618
            settings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, settings.TypeNameAssemblyFormat);
#pragma warning restore 618

            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, settings.TypeNameAssemblyFormatHandling);

            settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
#pragma warning disable 618
            Assert.AreEqual(FormatterAssemblyStyle.Simple, settings.TypeNameAssemblyFormat);
#pragma warning restore 618
#endif

            settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, settings.TypeNameAssemblyFormatHandling);

            settings.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, settings.TypeNameHandling);
        }

        [Test]
        public void JsonSerializerProxyProperties()
        {
            JsonSerializerProxy serializerProxy = new JsonSerializerProxy(new JsonSerializerInternalReader(new JsonSerializer()));

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsNotNull(serializerProxy.Binder);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsNotNull(serializerProxy.SerializationBinder);

            DefaultSerializationBinder customBinder = new DefaultSerializationBinder();
#pragma warning disable CS0618 // Type or member is obsolete
            serializerProxy.Binder = customBinder;
            Assert.AreEqual(customBinder, serializerProxy.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsInstanceOf(typeof(DefaultSerializationBinder), serializerProxy.SerializationBinder);

            serializerProxy.SerializationBinder = customBinder;
            Assert.AreEqual(customBinder, serializerProxy.SerializationBinder);

#pragma warning disable CS0618 // Type or member is obsolete
            // can still fetch because DefaultSerializationBinder inherits from SerializationBinder
            Assert.AreEqual(customBinder, serializerProxy.Binder);
#pragma warning restore CS0618 // Type or member is obsolete


            serializerProxy.CheckAdditionalContent = true;
            Assert.AreEqual(true, serializerProxy.CheckAdditionalContent);

            serializerProxy.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            Assert.AreEqual(ConstructorHandling.AllowNonPublicDefaultConstructor, serializerProxy.ConstructorHandling);

#if !(DNXCORE50) || NETSTANDARD2_0
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

#if !(PORTABLE || PORTABLE40 || NET20 || DNXCORE50) || NETSTANDARD2_0
#pragma warning disable 618
            serializerProxy.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
            Assert.AreEqual(FormatterAssemblyStyle.Full, serializerProxy.TypeNameAssemblyFormat);
#pragma warning restore 618

            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, serializerProxy.TypeNameAssemblyFormatHandling);

            serializerProxy.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
#pragma warning disable 618
            Assert.AreEqual(FormatterAssemblyStyle.Simple, serializerProxy.TypeNameAssemblyFormat);
#pragma warning restore 618
#endif

            serializerProxy.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
            Assert.AreEqual(TypeNameAssemblyFormatHandling.Full, serializerProxy.TypeNameAssemblyFormatHandling);

            serializerProxy.TypeNameHandling = TypeNameHandling.All;
            Assert.AreEqual(TypeNameHandling.All, serializerProxy.TypeNameHandling);
        }

#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(NET20 || NET35)
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

#if !(DNXCORE50) || NETSTANDARD2_0
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

#if !(DNXCORE50)
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<string> javaScriptSerializerResult = javaScriptSerializer.Deserialize<List<string>>(json);
#endif

            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
            List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

            List<string> jsonNetResult = JsonConvert.DeserializeObject<List<string>>(json);

            Assert.AreEqual(1, jsonNetResult.Count);
            Assert.AreEqual(dataContractResult[0], jsonNetResult[0]);
#if !(DNXCORE50)
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
#if (NETSTANDARD2_0)
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
  ""Float"": 3.4028235E+38,
  ""DBNull"": null,
  ""Bool"": true,
  ""Char"": ""\u0000""
}";
#elif NETSTANDARD1_3
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
  ""Float"": 3.4028235E+38,
  ""Bool"": true,
  ""Char"": ""\u0000""
}";
#elif !(PORTABLE || DNXCORE50) || NETSTANDARD1_3
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
#if !(PORTABLE || DNXCORE50 || PORTABLE40)
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

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                InterfacePropertyTestClass testFromDe = (InterfacePropertyTestClass)JsonConvert.DeserializeObject(strFromTest, typeof(InterfacePropertyTestClass));
            }, @"Could not create an instance of type Newtonsoft.Json.Tests.TestObjects.ICo. Type is an interface or abstract class and cannot be instantiated. Path 'co.Name', line 1, position 14.");
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

        [Test]
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

            StringAssert.AreEqual(@"{
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

#if !(NET20 || DNXCORE50)
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

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Event>(json); }, @"Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.Events.Event. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'sublocation', line 1, position 15.");
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

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Person>(json); }, @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person' because the type requires a JSON object (e.g. {""name"":""value""}) to deserialize correctly.
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

#if !(PORTABLE || DNXCORE50) || NETSTANDARD2_0
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

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeObject<JObject>(json); },
                "Deserialized JSON type 'Newtonsoft.Json.Linq.JArray' is not compatible with expected type 'Newtonsoft.Json.Linq.JObject'. Path '', line 1, position 2.");
        }

        [Test]
        public void CannotDeserializeConstructorIntoObject()
        {
            string json = @"new Constructor(123)";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<Person>(json); }, @"Error converting value ""Constructor"" to type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person'. Path '', line 1, position 16.");
        }

        [Test]
        public void CannotDeserializeConstructorIntoObjectNested()
        {
            string json = @"[new Constructor(123)]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<List<Person>>(json); }, @"Error converting value ""Constructor"" to type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person'. Path '[0]', line 1, position 17.");
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
                Assert.IsTrue(ex.Message.StartsWith(@"Cannot deserialize the current JSON object (e.g. {""name"":""value""}) into type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Organization.Person]' because the type requires a JSON array (e.g. [1,2,3]) to deserialize correctly." + Environment.NewLine +
                                                    @"To fix this error either change the JSON to a JSON array (e.g. [1,2,3]) or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine +
                                                    @"Path ''"));
            }
        }

        [Test]
        public void CannotPopulateArrayIntoObject()
        {
            string json = @"[]";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject(json, new Person()); }, @"Cannot populate JSON array onto type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person'. Path '', line 1, position 1.");
        }

        [Test]
        public void CannotPopulateObjectIntoArray()
        {
            string json = @"{}";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.PopulateObject(json, new List<Person>()); }, @"Cannot populate JSON object onto type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Organization.Person]'. Path '', line 1, position 2.");
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
#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
  ""Newtonsoft.Json.Tests.TestObjects.Organization.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Organization.Person"": 2
}", json);
        }

        [Test]
        public void DeserializePersonKeyedDictionary()
        {
            try
            {
                string json =
                    @"{
  ""Newtonsoft.Json.Tests.TestObjects.Organization.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Organization.Person"": 2
}";

                JsonConvert.DeserializeObject<Dictionary<Person, int>>(json);
                Assert.Fail();
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Could not convert string 'Newtonsoft.Json.Tests.TestObjects.Organization.Person' to dictionary key type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person'. Create a TypeConverter to convert from the string to the key type object. Path '['Newtonsoft.Json.Tests.TestObjects.Organization.Person']'"));
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
            }), "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: Newtonsoft.Json.Tests.TestObjects.DictionaryWithNoDefaultConstructor. Path 'key1', line 1, position 16.");
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

        [Test]
        public void DeserializeToObjectProperty()
        {
            var json = "{ Key: 'abc', Value: 123 }";
            var item = JsonConvert.DeserializeObject<KeyValueTestClass>(json);

            Assert.AreEqual(123L, item.Value);
        }

#if !(NET20 || NET35)
        [Test]
        public void DataContractJsonSerializerTest()
        {
            DataContractJsonSerializerTestClass c = new DataContractJsonSerializerTestClass()
            {
                TimeSpanProperty = new TimeSpan(200, 20, 59, 30, 900),
                GuidProperty = new Guid("66143115-BE2A-4a59-AF0A-348E1EA15B1E"),
                AnimalProperty = new Human() { Ethnicity = "European" }
            };
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

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
                JsonTypeReflector.SetFullyTrusted(null);
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
                }, @"Type 'Newtonsoft.Json.Tests.TestObjects.ISerializableTestObject' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                   @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine +
                   @"Path 'booleanValue', line 1, position 14.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(null);
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
                }, @"Type 'Newtonsoft.Json.Tests.TestObjects.ISerializableTestObject' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                   @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine +
                   @"Path ''.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(null);
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
            AATestClass myA = new AATestClass(2);
            string json = JsonConvert.SerializeObject(myA, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""AA_field1"": 2,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2
}", json);

            BBTestClass myB = new BBTestClass(3, 4);
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

#if !(PORTABLE) || NETSTANDARD2_0
        [Test]
        public void DeserializeClassWithInheritedProtectedMember()
        {
            AATestClass myA = JsonConvert.DeserializeObject<AATestClass>(
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

            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myA));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myA));

            BBTestClass myB = JsonConvert.DeserializeObject<BBTestClass>(
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

            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AATestClass).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myB));

            Assert.AreEqual(4, myB.BB_field1);
            Assert.AreEqual(4, myB.BB_field2);
            Assert.AreEqual(3, myB.BB_property1);
            Assert.AreEqual(3, myB.BB_property2);
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BBTestClass).GetProperty("BB_property3", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BBTestClass).GetProperty("BB_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
            Assert.AreEqual(0, myB.BB_property5);
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BBTestClass).GetProperty("BB_property6", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BBTestClass).GetProperty("BB_property7", BindingFlags.Instance | BindingFlags.Public), myB));
            Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BBTestClass).GetProperty("BB_property8", BindingFlags.Instance | BindingFlags.Public), myB));
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

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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

        [Test]
        public void DeserializeIgnoredPropertyInConstructor()
        {
            string json = @"{""First"":""First"",""Second"":2,""Ignored"":{""Name"":""James""},""AdditionalContent"":{""LOL"":true}}";

            var cc = JsonConvert.DeserializeObject<ConstructorCompexIgnoredProperty>(json);
            Assert.AreEqual("First", cc.First);
            Assert.AreEqual(2, cc.Second);
            Assert.AreEqual(null, cc.Ignored);
        }
        
        [Test]
        public void DeserializeIgnoredPropertyInConstructorWithoutThrowingMissingMemberError()
        {
            string json = @"{""First"":""First"",""Second"":2,""Ignored"":{""Name"":""James""}}";

            var cc = JsonConvert.DeserializeObject<ConstructorCompexIgnoredProperty>(
                json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
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

        [Test]
        public void DeserializeByteArray()
        {
            JsonSerializer serializer1 = new JsonSerializer();
            serializer1.Converters.Add(new IsoDateTimeConverter());
            serializer1.NullValueHandling = NullValueHandling.Ignore;

            string json = @"[{""Prop1"":""""},{""Prop1"":""""}]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            ByteArrayTestClass[] z = (ByteArrayTestClass[])serializer1.Deserialize(reader, typeof(ByteArrayTestClass[]));
            Assert.AreEqual(2, z.Length);
            Assert.AreEqual(0, z[0].Prop1.Length);
            Assert.AreEqual(0, z[1].Prop1.Length);
        }

#if !(NET20 || DNXCORE50) || NETSTANDARD2_0
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

#if !NET20
        [Test]
        public void ReadWriteTimeZoneOffsetIso()
        {
            var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
            {
                Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
            });

            Assert.AreEqual("{\"Offset\":\"2000-01-01T00:00:00+06:00\"}", serializeObject);

            JsonTextReader reader = new JsonTextReader(new StringReader(serializeObject))
            {
                DateParseHandling = DateParseHandling.None
            };
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

        [Test]
        public void DeserializeStructProperty()
        {
            VectorParent obj = new VectorParent();
            obj.Position = new TestObjects.Vector { X = 1, Y = 2, Z = 3 };

            string str = JsonConvert.SerializeObject(obj);

            obj = JsonConvert.DeserializeObject<VectorParent>(str);

            Assert.AreEqual(1, obj.Position.X);
            Assert.AreEqual(2, obj.Position.Y);
            Assert.AreEqual(3, obj.Position.Z);
        }

        [Test]
        public void PrivateSetterOnBaseClassProperty()
        {
            var derived = new PrivateSetterDerived("meh", "woo");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            string json = JsonConvert.SerializeObject(derived, Formatting.Indented, settings);

            var meh = JsonConvert.DeserializeObject<PrivateSetterBase>(json, settings);

            Assert.AreEqual(((PrivateSetterDerived)meh).IDoWork, "woo");
            Assert.AreEqual(meh.IDontWork, "meh");
        }

#if !(NET20 || DNXCORE50) || NETSTANDARD2_0
        [Test]
        public void DeserializeNullableStruct()
        {
            NullableStructPropertyClass nullableStructPropertyClass = new NullableStructPropertyClass()
            {
                Foo1 = new StructISerializable() { Name = "foo 1" },
                Foo2 = new StructISerializable() { Name = "foo 2" }
            };
            NullableStructPropertyClass barWithNull = new NullableStructPropertyClass()
            {
                Foo1 = new StructISerializable() { Name = "foo 1" },
                Foo2 = null
            };

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

        [Test]
        public void DeserializeJToken()
        {
            JTokenTestClass c = new JTokenTestClass
            {
                Name = "Success",
                Data = new JObject(new JProperty("First", "Value1"), new JProperty("Second", "Value2"))
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented);

            JTokenTestClass deserializedResponse = JsonConvert.DeserializeObject<JTokenTestClass>(json);

            Assert.AreEqual("Success", deserializedResponse.Name);
            Assert.IsTrue(deserializedResponse.Data.DeepEquals(c.Data));
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

        [Test]
        public void PopulationBehaviourForOmittedPropertiesIsTheSameForParameterisedConstructorAsForDefaultConstructor()
        {
            string json = @"{A:""Test""}";

            var withoutParameterisedConstructor = JsonConvert.DeserializeObject<DTOWithoutParameterisedConstructor>(json);
            var withParameterisedConstructor = JsonConvert.DeserializeObject<DTOWithParameterisedConstructor>(json);
            Assert.AreEqual(withoutParameterisedConstructor.B, withParameterisedConstructor.B);
        }

        [Test]
        public void SkipPopulatingArrayPropertyClass()
        {
            string json = JsonConvert.SerializeObject(new EnumerableArrayPropertyClass());
            JsonConvert.DeserializeObject<EnumerableArrayPropertyClass>(json);
        }

#if !(NET20)
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

            Assert.AreEqual(@"<ChildDataContract xmlns=""http://schemas.datacontract.org/2004/07/Newtonsoft.Json.Tests.TestObjects"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><nonVirtualMember>NonVirtualMember!</nonVirtualMember><virtualMember>VirtualMember!</virtualMember><NewMember i:nil=""true""/></ChildDataContract>", xml);
        }
#endif

        [Test]
        public void ChildObjectTest()
        {
            VirtualOverrideNewChildObject cc = new VirtualOverrideNewChildObject
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
            VirtualOverrideNewChildWithDifferentOverrideObject cc = new VirtualOverrideNewChildWithDifferentOverrideObject
            {
                VirtualMember = "VirtualMember!",
                NonVirtualMember = "NonVirtualMember!"
            };

            string result = JsonConvert.SerializeObject(cc);
            Assert.AreEqual(@"{""differentVirtualMember"":""VirtualMember!"",""nonVirtualMember"":""NonVirtualMember!""}", result);
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

        public class CustomClass
        {
#if !(NET20 || PORTABLE)
            [Required]
#endif
            public System.Guid? clientId { get; set; }
        }

        [Test]
        public void DeserializeStringIntoNullableGuid()
        {
            string json = @"{ 'clientId': 'bb2f3da7-bf79-4d14-9d54-0a1f7ff5f902' }";

            CustomClass c = JsonConvert.DeserializeObject<CustomClass>(json);

            Assert.AreEqual(new Guid("bb2f3da7-bf79-4d14-9d54-0a1f7ff5f902"), c.clientId);
        }

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD2_0
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

#if !(NET20 || DNXCORE50) || NETSTANDARD2_0
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

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                List<int> numbers = JsonConvert.DeserializeObject<List<int>>(json);
            }, "Error converting value {null} to type 'System.Int32'. Path '[3]', line 5, position 6.");
        }

#if !(PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
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

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ConvertableIntTestClass>(json), "Error converting value 1 to type 'Newtonsoft.Json.Tests.TestObjects.ConvertibleInt'. Path 'Integer', line 2, position 14.");
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
        public void SerializeNullableGuidCustomWriterOverridesNullableGuid()
        {
            NullableGuid ng = new NullableGuid {Id = Guid.Empty};
            NullableGuidCountingJsonTextWriter writer = new NullableGuidCountingJsonTextWriter(new StreamWriter(Stream.Null));
            JsonSerializer serializer = JsonSerializer.Create();
            serializer.Serialize(writer, ng);
            Assert.AreEqual(1, writer.NullableGuidCount);
            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            serializer.TraceWriter = traceWriter;
            serializer.Serialize(writer, ng);
            Assert.AreEqual(2, writer.NullableGuidCount);
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
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(byte[]), 0, DefaultSerializationBinder.Instance) + @""",
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
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: value", // mono
                    "Value cannot be null. (Parameter 'value')"
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
                    "Argument cannot be null." + Environment.NewLine + "Parameter name: value", // mono
                    "Value cannot be null. (Parameter 'value')"
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

#if !(DNXCORE50)
        [Test]
        public void MetroBlogPost()
        {
            Product product = new Product()
            {
                Name = "Apple",
                ExpiryDate = new DateTime(2012, 4, 1),
                Price = 3.99M,
                Sizes = new[] { "Small", "Medium", "Large" }
            };

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
#endif

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

#pragma warning disable 618
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
#pragma warning restore 618

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
                { new DateTime(2020, 12, 12, 12, 12, 12, DateTimeKind.Utc), 1 },
                { new DateTime(2023, 12, 12, 12, 12, 12, DateTimeKind.Utc), 2 }
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
            Assert.AreEqual(1, dic2[new DateTime(2020, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
            Assert.AreEqual(2, dic2[new DateTime(2023, 12, 12, 12, 12, 12, DateTimeKind.Utc)]);
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

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void SerializeAndDeserializeWithAttributes()
        {
            var testObj = new PersonSerializable() { Name = "John Doe", Age = 28 };

            var json = Serialize(testObj);
            var objDeserialized = Deserialize<PersonSerializable>(json);

            Assert.AreEqual(testObj.Name, objDeserialized.Name);
            Assert.AreEqual(0, objDeserialized.Age);
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

        [Test]
        public void CheckAdditionalContentJustComment()
        {
            string json = "{one:1} // This is just a comment";

            JsonSerializerSettings settings = new JsonSerializerSettings {CheckAdditionalContent = true};
            JsonSerializer s = JsonSerializer.Create(settings);
            IDictionary<string, int> o = s.Deserialize<Dictionary<string, int>>(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(o);
            Assert.AreEqual(1, o["one"]);
        }

        [Test]
        public void CheckAdditionalContentJustMultipleComments()
        {
            string json = @"{one:1} // This is just a comment
/* This is just a comment
over multiple
lines.*/

// This is just another comment.";

            JsonSerializerSettings settings = new JsonSerializerSettings {CheckAdditionalContent = true};
            JsonSerializer s = JsonSerializer.Create(settings);
            IDictionary<string, int> o = s.Deserialize<Dictionary<string, int>>(new JsonTextReader(new StringReader(json)));

            Assert.IsNotNull(o);
            Assert.AreEqual(1, o["one"]);
        }

        [Test]
        public void CheckAdditionalContentCommentsThenAnotherObject()
        {
            string json = @"{one:1} // This is just a comment
/* This is just a comment
over multiple
lines.*/

// This is just another comment. But here comes an empty object.
{}";

            JsonSerializerSettings settings = new JsonSerializerSettings { CheckAdditionalContent = true };
            JsonSerializer s = JsonSerializer.Create(settings);
            ExceptionAssert.Throws<JsonReaderException>(() => { s.Deserialize<Dictionary<string, int>>(new JsonTextReader(new StringReader(json))); }, "Additional text encountered after finished reading JSON content: {. Path '', line 7, position 0.");
        }

#if !(PORTABLE || DNXCORE50 || PORTABLE40)
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

                serializer.Deserialize(reader, typeof(ItemConverterTestClass));
            }, "Additional text found in JSON string after finishing deserializing object. Path '[1]', line 1, position 5.");
        }

        [Test]
        public void AdditionalContentAfterFinishCheckNotRequested()
        {
            string json = @"{ ""MyProperty"":{""Key"":""Value""}} A bunch of junk at the end of the json";

            JsonSerializer serializer = new JsonSerializer();

            var reader = new JsonTextReader(new StringReader(json));

            ItemConverterTestClass mt = (ItemConverterTestClass)serializer.Deserialize(reader, typeof(ItemConverterTestClass));
            Assert.AreEqual(1, mt.MyProperty.Count);
        }

        [Test]
        public void AdditionalContentAfterCommentsCheckNotRequested()
        {
            string json = @"{ ""MyProperty"":{""Key"":""Value""}} /*this is a comment */
// this is also a comment
This is just junk, though.";

            JsonSerializer serializer = new JsonSerializer();

            var reader = new JsonTextReader(new StringReader(json));

            ItemConverterTestClass mt = (ItemConverterTestClass)serializer.Deserialize(reader, typeof(ItemConverterTestClass));
            Assert.AreEqual(1, mt.MyProperty.Count);
        }

        [Test]
        public void AdditionalContentAfterComments()
        {
            string json = @"[{ ""MyProperty"":{""Key"":""Value""}} /*this is a comment */
// this is also a comment
,{}";

            JsonSerializer serializer = new JsonSerializer();
            serializer.CheckAdditionalContent = true;
            var reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            reader.Read();

            ExceptionAssert.Throws<JsonSerializationException>(() => serializer.Deserialize(reader, typeof(ItemConverterTestClass)),
                "Additional text found in JSON string after finishing deserializing object. Path '[1]', line 3, position 2.");
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

        [Test]
        public void DeserializeDictionaryItemConverter()
        {
            var actual = JsonConvert.DeserializeObject<ItemConverterTestClass>(@"{ ""MyProperty"":{""Key"":""Y""}}");
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
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD2_0
            doStuff();
            Assert.AreEqual(500, obj.Item1);
#else
            ExceptionAssert.Throws<JsonSerializationException>(
                doStuff,
                "Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.MyTuple`1[System.Int32]. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'm_Item1', line 1, position 11.");
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

                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<MyTuplePartial<int>>(json), "Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.MyTuplePartial`1[System.Int32]. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Path 'm_Item1', line 1, position 11.");
            }
            finally
            {
                JsonTypeReflector.SetFullyTrusted(true);
            }
        }
#endif

#if !(PORTABLE || NET35 || NET20 || PORTABLE40 || DNXCORE50) || NETSTANDARD2_0
        [Test]
        public void SerializeTupleWithSerializableAttribute()
        {
            var tuple = Tuple.Create(500);

            SerializableContractResolver contractResolver = new SerializableContractResolver();

            var json = JsonConvert.SerializeObject(tuple, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });
            Assert.AreEqual(@"{""m_Item1"":500}", json);

            var obj = JsonConvert.DeserializeObject<Tuple<int>>(json, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });
            Assert.AreEqual(500, obj.Item1);
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

        [Test]
        public void NoConstructorReadOnlyCollectionTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<NoConstructorReadOnlyCollection<int>>("[1]"), "Cannot deserialize readonly or fixed size list: Newtonsoft.Json.Tests.TestObjects.NoConstructorReadOnlyCollection`1[System.Int32]. Path '', line 1, position 1.");
        }

#if !(NET40 || NET35 || NET20 || PORTABLE40)
        [Test]
        public void NoConstructorReadOnlyDictionaryTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<NoConstructorReadOnlyDictionary<int, int>>("{'1':1}"), "Cannot deserialize readonly or fixed size dictionary: Newtonsoft.Json.Tests.TestObjects.NoConstructorReadOnlyDictionary`2[System.Int32,System.Int32]. Path '1', line 1, position 5.");
        }
#endif

#if !(PORTABLE || NET35 || NET20 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void ReadTooLargeInteger()
        {
            string json = @"[999999999999999999999999999999999999999999999999]";

            IList<BigInteger> l = JsonConvert.DeserializeObject<IList<BigInteger>>(json);

            Assert.AreEqual(BigInteger.Parse("999999999999999999999999999999999999999999999999"), l[0]);

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<IList<long>>(json), "Error converting value 999999999999999999999999999999999999999999999999 to type 'System.Int64'. Path '[0]', line 1, position 49.");
        }
#endif

        [Test]
        public void SerializeStructWithSerializableAndDataContract()
        {
            Pair<string, int> p = new Pair<string, int>("One", 2);

            string json = JsonConvert.SerializeObject(p);

            Assert.AreEqual(@"{""First"":""One"",""Second"":2}", json);

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(PORTABLE || PORTABLE40 || DNXCORE50)
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

#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void SerializeDictionaryWithStructKey_Custom()
        {
            string json = JsonConvert.SerializeObject(
                new Dictionary<TypeConverterSize, TypeConverterSize> { { new TypeConverterSize(1, 2), new TypeConverterSize(3, 4) } }
            );

            Assert.AreEqual(@"{""1, 2"":""3, 4""}", json);

            Dictionary<TypeConverterSize, TypeConverterSize> d = JsonConvert.DeserializeObject<Dictionary<TypeConverterSize, TypeConverterSize>>(json);

            Assert.AreEqual(new TypeConverterSize(1, 2), d.Keys.First());
            Assert.AreEqual(new TypeConverterSize(3, 4), d.Values.First());
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

#if !(NET35 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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

#if !(PORTABLE) || NETSTANDARD1_3 || NETSTANDARD2_0
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

        [Test]
        public void ErrorCreatingJsonConverter()
        {
            ExceptionAssert.Throws<JsonException>(() => JsonConvert.SerializeObject(new ErroringTestClass()), "Error creating 'Newtonsoft.Json.Tests.TestObjects.ErroringJsonConverter'.");
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

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
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
#endif

#if !(DNXCORE50) || NETSTANDARD2_0
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
#endif

        [Test]
        public void ParametrizedConstructor_IncompleteJson()
        {
            string s = @"{""text"":""s"",""cursorPosition"":189,""dataSource"":""json_northwind"",";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<CompletionDataRequest>(s), "Unexpected end when deserializing object. Path 'dataSource', line 1, position 63.");
        }

        [Test]
        public void ChildClassWithProtectedOverridePlusJsonProperty_Serialize()
        {
            JsonObjectContract c = (JsonObjectContract)DefaultContractResolver.Instance.ResolveContract(typeof(ChildClassWithProtectedOverridePlusJsonProperty));
            Assert.AreEqual(1, c.Properties.Count);

            var propertyValue = "test";
            var testJson = @"{ 'MyProperty' : '" + propertyValue + "' }";

            var testObject = JsonConvert.DeserializeObject<ChildClassWithProtectedOverridePlusJsonProperty>(testJson);

            Assert.AreEqual(propertyValue, testObject.GetPropertyValue(), "MyProperty should be populated");
        }

        [Test]
        public void JsonPropertyConverter()
        {
            DateTime dt = new DateTime(2000, 12, 20, 0, 0, 0, DateTimeKind.Utc);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new JsonPropertyConverterContractResolver(),
                Formatting = Formatting.Indented
            };

            JsonPropertyConverterTestClass c1 = new JsonPropertyConverterTestClass
            {
                NormalDate = dt,
                JavaScriptDate = dt
            };

            string json = JsonConvert.SerializeObject(c1, settings);

            StringAssert.AreEqual(@"{
  ""NormalDate"": ""2000-12-20T00:00:00Z"",
  ""JavaScriptDate"": new Date(
    977270400000
  )
}", json);

            JsonPropertyConverterTestClass c2 = JsonConvert.DeserializeObject<JsonPropertyConverterTestClass>(json, settings);

            Assert.AreEqual(dt, c2.NormalDate);
            Assert.AreEqual(dt, c2.JavaScriptDate);
        }

        [Test]
        public void StringEmptyValue()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<EmptyJsonValueTestClass>("{ A: , B: 1, C: 123, D: 1.23, E: 3.45, F: null }"),
                "Unexpected character encountered while parsing value: ,. Path 'A', line 1, position 6.");
        }

        [Test]
        public void NullableIntEmptyValue()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<EmptyJsonValueTestClass>("{ A: \"\", B: , C: 123, D: 1.23, E: 3.45, F: null }"),
                "Unexpected character encountered while parsing value: ,. Path 'B', line 1, position 13.");
        }

        [Test]
        public void NullableLongEmptyValue()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<EmptyJsonValueTestClass>("{ A: \"\", B: 1, C: , D: 1.23, E: 3.45, F: null }"),
                "An undefined token is not a valid System.Nullable`1[System.Int64]. Path 'C', line 1, position 18.");
        }

        [Test]
        public void NullableDecimalEmptyValue()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<EmptyJsonValueTestClass>("{ A: \"\", B: 1, C: 123, D: , E: 3.45, F: null }"),
                "Unexpected character encountered while parsing value: ,. Path 'D', line 1, position 27.");
        }

        [Test]
        public void NullableDoubleEmptyValue()
        {
            ExceptionAssert.Throws<JsonReaderException>(
                () => JsonConvert.DeserializeObject<EmptyJsonValueTestClass>("{ A: \"\", B: 1, C: 123, D: 1.23, E: , F: null }"),
                "Unexpected character encountered while parsing value: ,. Path 'E', line 1, position 36.");
        }

        [Test]
        public void SetMaxDepth_DepthExceeded()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("[[['text']]]"));
            Assert.AreEqual(64, reader.MaxDepth);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            Assert.AreEqual(64, settings.MaxDepth);
            Assert.AreEqual(false, settings._maxDepthSet);

            // Default should be the same
            Assert.AreEqual(reader.MaxDepth, settings.MaxDepth);

            settings.MaxDepth = 2;
            Assert.AreEqual(2, settings.MaxDepth);
            Assert.AreEqual(true, settings._maxDepthSet);

            JsonSerializer serializer = JsonSerializer.Create(settings);
            Assert.AreEqual(2, serializer.MaxDepth);

            ExceptionAssert.Throws<JsonReaderException>(
                () => serializer.Deserialize(reader),
                "The reader's MaxDepth of 2 has been exceeded. Path '[0][0]', line 1, position 3.");
        }

        [Test]
        public void SetMaxDepth_DepthNotExceeded()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader("['text']"));
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.MaxDepth = 2;

            JsonSerializer serializer = JsonSerializer.Create(settings);
            Assert.AreEqual(2, serializer.MaxDepth);

            serializer.Deserialize(reader);

            Assert.AreEqual(64, reader.MaxDepth);
        }
    }
}
