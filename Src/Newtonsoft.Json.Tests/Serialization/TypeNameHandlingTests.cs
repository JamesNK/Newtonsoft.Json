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

#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if !(PORTABLE || PORTABLE40)
using System.Collections.ObjectModel;
#if !(NET35 || NET20)
using System.Dynamic;
#endif
using System.Text;
using Newtonsoft.Json.Tests.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Utilities;
using System.Net;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class TypeNameHandlingTests : TestFixtureBase
    {
#if !(NET20 || NET35)
        [Test]
        public void SerializeValueTupleWithTypeName()
        {
            string tupleRef = ReflectionUtils.GetTypeName(typeof(ValueTuple<int, int, string>), TypeNameAssemblyFormatHandling.Simple, null);

            ValueTuple<int, int, string> t = ValueTuple.Create(1, 2, "string");

            string json = JsonConvert.SerializeObject(t, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            StringAssert.AreEqual(@"{
  ""$type"": """ + tupleRef + @""",
  ""Item1"": 1,
  ""Item2"": 2,
  ""Item3"": ""string""
}", json);

            ValueTuple<int, int, string> t2 = (ValueTuple<int, int, string>)JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            Assert.AreEqual("string", t2.Item3);
        }
#endif

#if !(NET20 || NET35 || NET40)
        public class KnownAutoTypes
        {
            public ICollection<string> Collection { get; set; }
            public IList<string> List { get; set; }
            public IDictionary<string, string> Dictionary { get; set; }
            public ISet<string> Set { get; set; }
            public IReadOnlyCollection<string> ReadOnlyCollection { get; set; }
            public IReadOnlyList<string> ReadOnlyList { get; set; }
            public IReadOnlyDictionary<string, string> ReadOnlyDictionary { get; set; }
        }

        [Test]
        public void KnownAutoTypesTest()
        {
            KnownAutoTypes c = new KnownAutoTypes
            {
                Collection = new List<string> { "Collection value!" },
                List = new List<string> { "List value!" },
                Dictionary = new Dictionary<string, string>
                {
                    { "Dictionary key!", "Dictionary value!" }
                },
                ReadOnlyCollection = new ReadOnlyCollection<string>(new[] { "Read Only Collection value!" }),
                ReadOnlyList = new ReadOnlyCollection<string>(new[] { "Read Only List value!" }),
                Set = new HashSet<string> { "Set value!" },
                ReadOnlyDictionary = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
                {
                    { "Read Only Dictionary key!", "Read Only Dictionary value!" }
                })
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{
  ""Collection"": [
    ""Collection value!""
  ],
  ""List"": [
    ""List value!""
  ],
  ""Dictionary"": {
    ""Dictionary key!"": ""Dictionary value!""
  },
  ""Set"": [
    ""Set value!""
  ],
  ""ReadOnlyCollection"": [
    ""Read Only Collection value!""
  ],
  ""ReadOnlyList"": [
    ""Read Only List value!""
  ],
  ""ReadOnlyDictionary"": {
    ""Read Only Dictionary key!"": ""Read Only Dictionary value!""
  }
}", json);
        }
#endif

        [Test]
        public void DictionaryAuto()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>
            {
                { "movie", new Movie { Name = "Die Hard" } }
            };

            string json = JsonConvert.SerializeObject(dic, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{
  ""movie"": {
    ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Movie, Newtonsoft.Json.Tests"",
    ""Name"": ""Die Hard"",
    ""Description"": null,
    ""Classification"": null,
    ""Studio"": null,
    ""ReleaseDate"": null,
    ""ReleaseCountries"": null
  }
}", json);
        }

        [Test]
        public void KeyValuePairAuto()
        {
            IList<KeyValuePair<string, object>> dic = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("movie", new Movie { Name = "Die Hard" })
            };

            string json = JsonConvert.SerializeObject(dic, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"[
  {
    ""Key"": ""movie"",
    ""Value"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Movie, Newtonsoft.Json.Tests"",
      ""Name"": ""Die Hard"",
      ""Description"": null,
      ""Classification"": null,
      ""Studio"": null,
      ""ReleaseDate"": null,
      ""ReleaseCountries"": null
    }
  }
]", json);
        }

        [Test]
        public void NestedValueObjects()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                sb.Append(@"{""$value"":");
            }

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                var reader = new JsonTextReader(new StringReader(sb.ToString()));
                var ser = new JsonSerializer();
                ser.MetadataPropertyHandling = MetadataPropertyHandling.Default;
                ser.Deserialize<sbyte>(reader);
            }, "Unexpected token when deserializing primitive value: StartObject. Path '$value', line 1, position 11.");
        }

        [Test]
        public void SerializeRootTypeNameIfDerivedWithAuto()
        {
            var serializer = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var sw = new StringWriter();
            serializer.Serialize(new JsonTextWriter(sw) { Formatting = Formatting.Indented }, new WagePerson(), typeof(Person));
            var result = sw.ToString();

            StringAssert.AreEqual(@"{
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Organization.WagePerson, Newtonsoft.Json.Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", result);

            Assert.IsTrue(result.Contains("WagePerson"));
            using (var rd = new JsonTextReader(new StringReader(result)))
            {
                var person = serializer.Deserialize<Person>(rd);

                CustomAssert.IsInstanceOfType(typeof(WagePerson), person);
            }
        }

        [Test]
        public void SerializeRootTypeNameAutoWithJsonConvert()
        {
            string json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Organization.WagePerson, Newtonsoft.Json.Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", json);
        }

        [Test]
        public void SerializeRootTypeNameAutoWithJsonConvert_Generic()
        {
            string json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Organization.WagePerson, Newtonsoft.Json.Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", json);
        }

        [Test]
        public void SerializeRootTypeNameAutoWithJsonConvert_Generic2()
        {
            string json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{""$type"":""Newtonsoft.Json.Tests.TestObjects.Organization.WagePerson, Newtonsoft.Json.Tests"",""HourlyWage"":0.0,""Name"":null,""BirthDate"":""0001-01-01T00:00:00"",""LastModified"":""0001-01-01T00:00:00""}", json);
        }

        public class Wrapper
        {
            public IList<EmployeeReference> Array { get; set; }
            public IDictionary<string, EmployeeReference> Dictionary { get; set; }
        }

        [Test]
        public void SerializeWrapper()
        {
            Wrapper wrapper = new Wrapper();
            wrapper.Array = new List<EmployeeReference>
            {
                new EmployeeReference()
            };
            wrapper.Dictionary = new Dictionary<string, EmployeeReference>
            {
                { "First", new EmployeeReference() }
            };

            string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            StringAssert.AreEqual(@"{
  ""Array"": [
    {
      ""$id"": ""1"",
      ""Name"": null,
      ""Manager"": null
    }
  ],
  ""Dictionary"": {
    ""First"": {
      ""$id"": ""2"",
      ""Name"": null,
      ""Manager"": null
    }
  }
}", json);

            Wrapper w2 = JsonConvert.DeserializeObject<Wrapper>(json);
            CustomAssert.IsInstanceOfType(typeof(List<EmployeeReference>), w2.Array);
            CustomAssert.IsInstanceOfType(typeof(Dictionary<string, EmployeeReference>), w2.Dictionary);
        }

        [Test]
        public void WriteTypeNameForObjects()
        {
            string employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), TypeNameAssemblyFormatHandling.Simple, null);

            EmployeeReference employee = new EmployeeReference();

            string json = JsonConvert.SerializeObject(employee, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": null,
  ""Manager"": null
}", json);
        }

        [Test]
        public void DeserializeTypeName()
        {
            string employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), TypeNameAssemblyFormatHandling.Simple, null);

            string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

            object employee = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            CustomAssert.IsInstanceOfType(typeof(EmployeeReference), employee);
            Assert.AreEqual("Name!", ((EmployeeReference)employee).Name);
        }

#if !(PORTABLE || DNXCORE50)
        [Test]
        public void DeserializeTypeNameFromGacAssembly()
        {
            string cookieRef = ReflectionUtils.GetTypeName(typeof(Cookie), TypeNameAssemblyFormatHandling.Simple, null);

            string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + cookieRef + @"""
}";

            object cookie = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            CustomAssert.IsInstanceOfType(typeof(Cookie), cookie);
        }
#endif

        [Test]
        public void SerializeGenericObjectListWithTypeName()
        {
            string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
            string personRef = typeof(Person).AssemblyQualifiedName;

            List<object> values = new List<object>
            {
                new EmployeeReference
                {
                    Name = "Bob",
                    Manager = new EmployeeReference { Name = "Frank" }
                },
                new Person
                {
                    Department = "Department",
                    BirthDate = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                    LastModified = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc)
                },
                "String!",
                int.MinValue
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            });

            StringAssert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
    ""BirthDate"": ""2000-12-30T00:00:00Z"",
    ""LastModified"": ""2000-12-30T00:00:00Z""
  },
  ""String!"",
  -2147483648
]", json);
        }

        [Test]
        public void DeserializeGenericObjectListWithTypeName()
        {
            string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
            string personRef = typeof(Person).AssemblyQualifiedName;

            string json = @"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
    ""BirthDate"": ""\/Date(978134400000)\/"",
    ""LastModified"": ""\/Date(978134400000)\/""
  },
  ""String!"",
  -2147483648
]";

            List<object> values = (List<object>)JsonConvert.DeserializeObject(json, typeof(List<object>), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            });

            Assert.AreEqual(4, values.Count);

            EmployeeReference e = (EmployeeReference)values[0];
            Person p = (Person)values[1];

            Assert.AreEqual("Bob", e.Name);
            Assert.AreEqual("Frank", e.Manager.Name);

            Assert.AreEqual(null, p.Name);
            Assert.AreEqual(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.BirthDate);
            Assert.AreEqual(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.LastModified);

            Assert.AreEqual("String!", values[2]);
            Assert.AreEqual((long)int.MinValue, values[3]);
        }

        [Test]
        public void DeserializeWithBadTypeName()
        {
            string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
            string personRef = typeof(Person).AssemblyQualifiedName;

            string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

            try
            {
                JsonConvert.DeserializeObject(json, typeof(Person), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
                });
            }
            catch (JsonSerializationException ex)
            {
                Assert.IsTrue(ex.Message.StartsWith(@"Type specified in JSON '" + employeeRef + @"' is not compatible with '" + personRef + @"'."));
            }
        }

        [Test]
        public void DeserializeTypeNameWithNoTypeNameHandling()
        {
            string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

            string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

            JObject o = (JObject)JsonConvert.DeserializeObject(json);

            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Manager"": null
}", o.ToString());
        }

        [Test]
        public void DeserializeTypeNameOnly()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Employee"",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
            }, "Type specified in JSON 'Newtonsoft.Json.Tests.TestObjects.Employee' was not resolved. Path '$type', line 3, position 55.");
        }

        public interface ICorrelatedMessage
        {
            string CorrelationId { get; set; }
        }

        public class SendHttpRequest : ICorrelatedMessage
        {
            public SendHttpRequest()
            {
                RequestEncoding = "UTF-8";
                Method = "GET";
            }

            public string Method { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public string Url { get; set; }
            public Dictionary<string, string> RequestData;
            public string RequestBodyText { get; set; }
            public string User { get; set; }
            public string Passwd { get; set; }
            public string RequestEncoding { get; set; }
            public string CorrelationId { get; set; }
        }

        [Test]
        public void DeserializeGenericTypeName()
        {
            string typeName = typeof(SendHttpRequest).AssemblyQualifiedName;

            string json = @"{
""$type"": """ + typeName + @""",
""RequestData"": {
""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"",
""Id"": ""siedemnaście"",
""X"": ""323""
},
""Method"": ""GET"",
""Url"": ""http://www.onet.pl"",
""RequestEncoding"": ""UTF-8"",
""CorrelationId"": ""xyz""
}";

            ICorrelatedMessage message = JsonConvert.DeserializeObject<ICorrelatedMessage>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            });

            CustomAssert.IsInstanceOfType(typeof(SendHttpRequest), message);

            SendHttpRequest request = (SendHttpRequest)message;
            Assert.AreEqual("xyz", request.CorrelationId);
            Assert.AreEqual(2, request.RequestData.Count);
            Assert.AreEqual("siedemnaście", request.RequestData["Id"]);
        }

        [Test]
        public void SerializeObjectWithMultipleGenericLists()
        {
            string containerTypeName = typeof(Container).AssemblyQualifiedName;
            string productListTypeName = typeof(List<Product>).AssemblyQualifiedName;

            Container container = new Container
            {
                In = new List<Product>(),
                Out = new List<Product>()
            };

            string json = JsonConvert.SerializeObject(container, Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
                });

            StringAssert.AreEqual(@"{
  ""$type"": """ + containerTypeName + @""",
  ""In"": {
    ""$type"": """ + productListTypeName + @""",
    ""$values"": []
  },
  ""Out"": {
    ""$type"": """ + productListTypeName + @""",
    ""$values"": []
  }
}", json);
        }

        public class TypeNameProperty
        {
            public string Name { get; set; }

            [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
            public object Value { get; set; }
        }

        [Test]
        public void WriteObjectTypeNameForProperty()
        {
            string typeNamePropertyRef = ReflectionUtils.GetTypeName(typeof(TypeNameProperty), TypeNameAssemblyFormatHandling.Simple, null);

            TypeNameProperty typeNameProperty = new TypeNameProperty
            {
                Name = "Name!",
                Value = new TypeNameProperty
                {
                    Name = "Nested!"
                }
            };

            string json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + typeNamePropertyRef + @""",
    ""Name"": ""Nested!"",
    ""Value"": null
  }
}", json);

            TypeNameProperty deserialized = JsonConvert.DeserializeObject<TypeNameProperty>(json);
            Assert.AreEqual("Name!", deserialized.Name);
            CustomAssert.IsInstanceOfType(typeof(TypeNameProperty), deserialized.Value);

            TypeNameProperty nested = (TypeNameProperty)deserialized.Value;
            Assert.AreEqual("Nested!", nested.Name);
            Assert.AreEqual(null, nested.Value);
        }

        [Test]
        public void WriteListTypeNameForProperty()
        {
            string listRef = ReflectionUtils.GetTypeName(typeof(List<int>), TypeNameAssemblyFormatHandling.Simple, null);

            TypeNameProperty typeNameProperty = new TypeNameProperty
            {
                Name = "Name!",
                Value = new List<int> { 1, 2, 3, 4, 5 }
            };

            string json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + listRef + @""",
    ""$values"": [
      1,
      2,
      3,
      4,
      5
    ]
  }
}", json);

            TypeNameProperty deserialized = JsonConvert.DeserializeObject<TypeNameProperty>(json);
            Assert.AreEqual("Name!", deserialized.Name);
            CustomAssert.IsInstanceOfType(typeof(List<int>), deserialized.Value);

            List<int> nested = (List<int>)deserialized.Value;
            Assert.AreEqual(5, nested.Count);
            Assert.AreEqual(1, nested[0]);
            Assert.AreEqual(2, nested[1]);
            Assert.AreEqual(3, nested[2]);
            Assert.AreEqual(4, nested[3]);
            Assert.AreEqual(5, nested[4]);
        }

        [Test]
        public void DeserializeUsingCustomBinder()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Employee"",
  ""Name"": ""Name!""
}";

            object p = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable CS0618 // Type or member is obsolete
                Binder = new CustomSerializationBinder()
#pragma warning restore CS0618 // Type or member is obsolete
            });

            CustomAssert.IsInstanceOfType(typeof(Person), p);

            Person person = (Person)p;

            Assert.AreEqual("Name!", person.Name);
        }

        public class CustomSerializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return typeof(Person);
            }
        }

#if !(NET20 || NET35)
        [Test]
        public void SerializeUsingCustomBinder()
        {
            TypeNameSerializationBinder binder = new TypeNameSerializationBinder("Newtonsoft.Json.Tests.Serialization.{0}, Newtonsoft.Json.Tests");

            IList<object> values = new List<object>
            {
                new Customer
                {
                    Name = "Caroline Customer"
                },
                new Purchase
                {
                    ProductName = "Elbow Grease",
                    Price = 5.99m,
                    Quantity = 1
                }
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
#pragma warning disable CS0618 // Type or member is obsolete
                Binder = binder
#pragma warning restore CS0618 // Type or member is obsolete
            });

            //[
            //  {
            //    "$type": "Customer",
            //    "Name": "Caroline Customer"
            //  },
            //  {
            //    "$type": "Purchase",
            //    "ProductName": "Elbow Grease",
            //    "Price": 5.99,
            //    "Quantity": 1
            //  }
            //]

            StringAssert.AreEqual(@"[
  {
    ""$type"": ""Customer"",
    ""Name"": ""Caroline Customer""
  },
  {
    ""$type"": ""Purchase"",
    ""ProductName"": ""Elbow Grease"",
    ""Price"": 5.99,
    ""Quantity"": 1
  }
]", json);

            IList<object> newValues = JsonConvert.DeserializeObject<IList<object>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
#pragma warning disable CS0618 // Type or member is obsolete
                Binder = new TypeNameSerializationBinder("Newtonsoft.Json.Tests.Serialization.{0}, Newtonsoft.Json.Tests")
#pragma warning restore CS0618 // Type or member is obsolete
            });

            CustomAssert.IsInstanceOfType(typeof(Customer), newValues[0]);
            Customer customer = (Customer)newValues[0];
            Assert.AreEqual("Caroline Customer", customer.Name);

            CustomAssert.IsInstanceOfType(typeof(Purchase), newValues[1]);
            Purchase purchase = (Purchase)newValues[1];
            Assert.AreEqual("Elbow Grease", purchase.ProductName);
        }

        public class TypeNameSerializationBinder : SerializationBinder
        {
            public string TypeFormat { get; private set; }

            public TypeNameSerializationBinder(string typeFormat)
            {
                TypeFormat = typeFormat;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                string resolvedTypeName = string.Format(TypeFormat, typeName);

                return Type.GetType(resolvedTypeName, true);
            }
        }
#endif

        [Test]
        public void NewSerializeUsingCustomBinder()
        {
            NewTypeNameSerializationBinder binder = new NewTypeNameSerializationBinder("Newtonsoft.Json.Tests.Serialization.{0}, Newtonsoft.Json.Tests");

            IList<object> values = new List<object>
            {
                new Customer
                {
                    Name = "Caroline Customer"
                },
                new Purchase
                {
                    ProductName = "Elbow Grease",
                    Price = 5.99m,
                    Quantity = 1
                }
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = binder
            });

            //[
            //  {
            //    "$type": "Customer",
            //    "Name": "Caroline Customer"
            //  },
            //  {
            //    "$type": "Purchase",
            //    "ProductName": "Elbow Grease",
            //    "Price": 5.99,
            //    "Quantity": 1
            //  }
            //]

            StringAssert.AreEqual(@"[
  {
    ""$type"": ""Customer"",
    ""Name"": ""Caroline Customer""
  },
  {
    ""$type"": ""Purchase"",
    ""ProductName"": ""Elbow Grease"",
    ""Price"": 5.99,
    ""Quantity"": 1
  }
]", json);

            IList<object> newValues = JsonConvert.DeserializeObject<IList<object>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new NewTypeNameSerializationBinder("Newtonsoft.Json.Tests.Serialization.{0}, Newtonsoft.Json.Tests")
            });

            CustomAssert.IsInstanceOfType(typeof(Customer), newValues[0]);
            Customer customer = (Customer)newValues[0];
            Assert.AreEqual("Caroline Customer", customer.Name);

            CustomAssert.IsInstanceOfType(typeof(Purchase), newValues[1]);
            Purchase purchase = (Purchase)newValues[1];
            Assert.AreEqual("Elbow Grease", purchase.ProductName);
        }

        public class NewTypeNameSerializationBinder : ISerializationBinder
        {
            public string TypeFormat { get; private set; }

            public NewTypeNameSerializationBinder(string typeFormat)
            {
                TypeFormat = typeFormat;
            }

            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.Name;
            }

            public Type BindToType(string assemblyName, string typeName)
            {
                string resolvedTypeName = string.Format(TypeFormat, typeName);

                return Type.GetType(resolvedTypeName, true);
            }
        }

        [Test]
        public void CollectionWithAbstractItems()
        {
            HolderClass testObject = new HolderClass();
            testObject.TestMember = new ContentSubClass("First One");
            testObject.AnotherTestMember = new Dictionary<int, IList<ContentBaseClass>>();
            testObject.AnotherTestMember.Add(1, new List<ContentBaseClass>());
            testObject.AnotherTestMember[1].Add(new ContentSubClass("Second One"));
            testObject.AThirdTestMember = new ContentSubClass("Third One");

            JsonSerializer serializingTester = new JsonSerializer();
            serializingTester.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            StringWriter sw = new StringWriter();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;
                serializingTester.TypeNameHandling = TypeNameHandling.Auto;
                serializingTester.Serialize(jsonWriter, testObject);
            }

            string json = sw.ToString();

            string contentSubClassRef = ReflectionUtils.GetTypeName(typeof(ContentSubClass), TypeNameAssemblyFormatHandling.Simple, null);
            string dictionaryRef = ReflectionUtils.GetTypeName(typeof(Dictionary<int, IList<ContentBaseClass>>), TypeNameAssemblyFormatHandling.Simple, null);
            string listRef = ReflectionUtils.GetTypeName(typeof(List<ContentBaseClass>), TypeNameAssemblyFormatHandling.Simple, null);

            string expected = @"{
  ""TestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""First One""
  },
  ""AnotherTestMember"": {
    ""$type"": """ + dictionaryRef + @""",
    ""1"": [
      {
        ""$type"": """ + contentSubClassRef + @""",
        ""SomeString"": ""Second One""
      }
    ]
  },
  ""AThirdTestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""Third One""
  }
}";

            StringAssert.AreEqual(expected, json);

            StringReader sr = new StringReader(json);

            JsonSerializer deserializingTester = new JsonSerializer();

            HolderClass anotherTestObject;

            using (JsonTextReader jsonReader = new JsonTextReader(sr))
            {
                deserializingTester.TypeNameHandling = TypeNameHandling.Auto;

                anotherTestObject = deserializingTester.Deserialize<HolderClass>(jsonReader);
            }

            Assert.IsNotNull(anotherTestObject);
            CustomAssert.IsInstanceOfType(typeof(ContentSubClass), anotherTestObject.TestMember);
            CustomAssert.IsInstanceOfType(typeof(Dictionary<int, IList<ContentBaseClass>>), anotherTestObject.AnotherTestMember);
            Assert.AreEqual(1, anotherTestObject.AnotherTestMember.Count);

            IList<ContentBaseClass> list = anotherTestObject.AnotherTestMember[1];

            CustomAssert.IsInstanceOfType(typeof(List<ContentBaseClass>), list);
            Assert.AreEqual(1, list.Count);
            CustomAssert.IsInstanceOfType(typeof(ContentSubClass), list[0]);
        }

        [Test]
        public void WriteObjectTypeNameForPropertyDemo()
        {
            Message message = new Message();
            message.Address = "http://www.google.com";
            message.Body = new SearchDetails
            {
                Query = "Json.NET",
                Language = "en-us"
            };

            string json = JsonConvert.SerializeObject(message, Formatting.Indented);
            // {
            //   "Address": "http://www.google.com",
            //   "Body": {
            //     "$type": "Newtonsoft.Json.Tests.Serialization.SearchDetails, Newtonsoft.Json.Tests",
            //     "Query": "Json.NET",
            //     "Language": "en-us"
            //   }
            // }

            Message deserialized = JsonConvert.DeserializeObject<Message>(json);

            SearchDetails searchDetails = (SearchDetails)deserialized.Body;
            // Json.NET
        }

        public class UrlStatus
        {
            public int Status { get; set; }
            public string Url { get; set; }
        }

        [Test]
        public void GenericDictionaryObject()
        {
            Dictionary<string, object> collection = new Dictionary<string, object>()
            {
                { "First", new UrlStatus { Status = 404, Url = @"http://www.bing.com" } },
                { "Second", new UrlStatus { Status = 400, Url = @"http://www.google.com" } },
                {
                    "List", new List<UrlStatus>
                    {
                        new UrlStatus { Status = 300, Url = @"http://www.yahoo.com" },
                        new UrlStatus { Status = 200, Url = @"http://www.askjeeves.com" }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(collection, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
            });

            string urlStatusTypeName = ReflectionUtils.GetTypeName(typeof(UrlStatus), TypeNameAssemblyFormatHandling.Simple, null);

            StringAssert.AreEqual(@"{
  ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib"",
  ""First"": {
    ""$type"": """ + urlStatusTypeName + @""",
    ""Status"": 404,
    ""Url"": ""http://www.bing.com""
  },
  ""Second"": {
    ""$type"": """ + urlStatusTypeName + @""",
    ""Status"": 400,
    ""Url"": ""http://www.google.com""
  },
  ""List"": {
    ""$type"": ""System.Collections.Generic.List`1[[" + urlStatusTypeName + @"]], mscorlib"",
    ""$values"": [
      {
        ""$type"": """ + urlStatusTypeName + @""",
        ""Status"": 300,
        ""Url"": ""http://www.yahoo.com""
      },
      {
        ""$type"": """ + urlStatusTypeName + @""",
        ""Status"": 200,
        ""Url"": ""http://www.askjeeves.com""
      }
    ]
  }
}", json);

            object c = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
            });

            CustomAssert.IsInstanceOfType(typeof(Dictionary<string, object>), c);

            Dictionary<string, object> newCollection = (Dictionary<string, object>)c;
            Assert.AreEqual(3, newCollection.Count);
            Assert.AreEqual(@"http://www.bing.com", ((UrlStatus)newCollection["First"]).Url);

            List<UrlStatus> statues = (List<UrlStatus>)newCollection["List"];
            Assert.AreEqual(2, statues.Count);
        }

        [Test]
        public void SerializingIEnumerableOfTShouldRetainGenericTypeInfo()
        {
            string productClassRef = ReflectionUtils.GetTypeName(typeof(CustomEnumerable<Product>), TypeNameAssemblyFormatHandling.Simple, null);

            CustomEnumerable<Product> products = new CustomEnumerable<Product>();

            string json = JsonConvert.SerializeObject(products, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            StringAssert.AreEqual(@"{
  ""$type"": """ + productClassRef + @""",
  ""$values"": []
}", json);
        }

        public class CustomEnumerable<T> : IEnumerable<T>
        {
            //NOTE: a simple linked list
            private readonly T value;
            private readonly CustomEnumerable<T> next;
            private readonly int count;

            private CustomEnumerable(T value, CustomEnumerable<T> next)
            {
                this.value = value;
                this.next = next;
                count = this.next.count + 1;
            }

            public CustomEnumerable()
            {
                count = 0;
            }

            public CustomEnumerable<T> AddFirst(T newVal)
            {
                return new CustomEnumerable<T>(newVal, this);
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (count == 0) // last node
                {
                    yield break;
                }
                yield return value;

                var nextInLine = next;
                while (nextInLine != null)
                {
                    if (nextInLine.count != 0)
                    {
                        yield return nextInLine.value;
                    }
                    nextInLine = nextInLine.next;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class Car
        {
            // included in JSON
            public string Model { get; set; }
            public DateTime Year { get; set; }
            public List<string> Features { get; set; }
            public object[] Objects { get; set; }

            // ignored
            [JsonIgnore]
            public DateTime LastModified { get; set; }
        }

        [Test]
        public void ByteArrays()
        {
            Car testerObject = new Car();
            testerObject.Year = new DateTime(2000, 10, 5, 1, 1, 1, DateTimeKind.Utc);
            byte[] data = new byte[] { 75, 65, 82, 73, 82, 65 };
            testerObject.Objects = new object[] { data, "prueba" };

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.TypeNameHandling = TypeNameHandling.All;

            string output = JsonConvert.SerializeObject(testerObject, Formatting.Indented, jsonSettings);

            string carClassRef = ReflectionUtils.GetTypeName(typeof(Car), TypeNameAssemblyFormatHandling.Simple, null);

            StringAssert.AreEqual(output, @"{
  ""$type"": """ + carClassRef + @""",
  ""Year"": ""2000-10-05T01:01:01Z"",
  ""Objects"": {
    ""$type"": ""System.Object[], mscorlib"",
    ""$values"": [
      {
        ""$type"": ""System.Byte[], mscorlib"",
        ""$value"": ""S0FSSVJB""
      },
      ""prueba""
    ]
  }
}");
            Car obj = JsonConvert.DeserializeObject<Car>(output, jsonSettings);

            Assert.IsNotNull(obj);

            Assert.IsTrue(obj.Objects[0] is byte[]);

            byte[] d = (byte[])obj.Objects[0];
            CollectionAssert.AreEquivalent(data, d);
        }

#if !(DNXCORE50)
        [Test]
        public void ISerializableTypeNameHandlingTest()
        {
            //Create an instance of our example type
            IExample e = new Example("Rob");

            SerializableWrapper w = new SerializableWrapper
            {
                Content = e
            };

            //Test Binary Serialization Round Trip
            //This will work find because the Binary Formatter serializes type names
            //this.TestBinarySerializationRoundTrip(e);

            //Test Json Serialization
            //This fails because the JsonSerializer doesn't serialize type names correctly for ISerializable objects
            //Type Names should be serialized for All, Auto and Object modes
            TestJsonSerializationRoundTrip(w, TypeNameHandling.All);
            TestJsonSerializationRoundTrip(w, TypeNameHandling.Auto);
            TestJsonSerializationRoundTrip(w, TypeNameHandling.Objects);
        }

        private void TestJsonSerializationRoundTrip(SerializableWrapper e, TypeNameHandling flag)
        {
            StringWriter writer = new StringWriter();

            //Create our serializer and set Type Name Handling appropriately
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = flag;

            //Do the actual serialization and dump to Console for inspection
            serializer.Serialize(new JsonTextWriter(writer), e);

            //Now try to deserialize
            //Json.Net will cause an error here as it will try and instantiate
            //the interface directly because it failed to respect the
            //TypeNameHandling property on serialization
            SerializableWrapper f = serializer.Deserialize<SerializableWrapper>(new JsonTextReader(new StringReader(writer.ToString())));

            //Check Round Trip
            Assert.AreEqual(e, f, "Objects should be equal after round trip json serialization");
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void SerializationBinderWithFullName()
        {
            Message message = new Message
            {
                Address = "jamesnk@testtown.com",
                Body = new Version(1, 2, 3, 4)
            };

            string json = JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable CS0618 // Type or member is obsolete
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
                Binder = new MetroBinder(),
#pragma warning restore CS0618 // Type or member is obsolete
                ContractResolver = new DefaultContractResolver
                {
#if !(PORTABLE || DNXCORE50)
                    IgnoreSerializableAttribute = true
#endif
                }
            });

            StringAssert.AreEqual(@"{
  ""$type"": "":::MESSAGE:::, AssemblyName"",
  ""Address"": ""jamesnk@testtown.com"",
  ""Body"": {
    ""$type"": "":::VERSION:::, AssemblyName"",
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  }
}", json);
        }

        public class MetroBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return null;
            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = "AssemblyName";
#if !(DNXCORE50)
                typeName = ":::" + serializedType.Name.ToUpper(CultureInfo.InvariantCulture) + ":::";
#else
                typeName = ":::" + serializedType.Name.ToUpper() + ":::";
#endif
            }
        }
#endif

        [Test]
        public void TypeNameIntList()
        {
            TypeNameList<int> l = new TypeNameList<int>();
            l.Add(1);
            l.Add(2);
            l.Add(3);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"[
  1,
  2,
  3
]", json);
        }

        [Test]
        public void TypeNameComponentList()
        {
            var c1 = new TestComponentSimple();

            TypeNameList<object> l = new TypeNameList<object>();
            l.Add(c1);
            l.Add(new Employee
            {
                BirthDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                Department = "Department!"
            });
            l.Add("String!");
            l.Add(long.MaxValue);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"[
  {
    ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
    ""MyProperty"": 0
  },
  {
    ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Organization.Employee, Newtonsoft.Json.Tests"",
    ""FirstName"": null,
    ""LastName"": null,
    ""BirthDate"": ""2000-12-12T12:12:12Z"",
    ""Department"": ""Department!"",
    ""JobTitle"": null
  },
  ""String!"",
  9223372036854775807
]", json);

            TypeNameList<object> l2 = JsonConvert.DeserializeObject<TypeNameList<object>>(json);
            Assert.AreEqual(4, l2.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), l2[0]);
            CustomAssert.IsInstanceOfType(typeof(Employee), l2[1]);
            CustomAssert.IsInstanceOfType(typeof(string), l2[2]);
            CustomAssert.IsInstanceOfType(typeof(long), l2[3]);
        }

        [Test]
        public void TypeNameDictionary()
        {
            TypeNameDictionary<object> l = new TypeNameDictionary<object>();
            l.Add("First", new TestComponentSimple { MyProperty = 1 });
            l.Add("Second", "String!");
            l.Add("Third", long.MaxValue);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""First"": {
    ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
    ""MyProperty"": 1
  },
  ""Second"": ""String!"",
  ""Third"": 9223372036854775807
}", json);

            TypeNameDictionary<object> l2 = JsonConvert.DeserializeObject<TypeNameDictionary<object>>(json);
            Assert.AreEqual(3, l2.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), l2["First"]);
            Assert.AreEqual(1, ((TestComponentSimple)l2["First"]).MyProperty);
            CustomAssert.IsInstanceOfType(typeof(string), l2["Second"]);
            CustomAssert.IsInstanceOfType(typeof(long), l2["Third"]);
        }

        [Test]
        public void TypeNameObjectItems()
        {
            TypeNameObject o1 = new TypeNameObject();

            o1.Object1 = new TestComponentSimple { MyProperty = 1 };
            o1.Object2 = 123;
            o1.ObjectNotHandled = new TestComponentSimple { MyProperty = int.MaxValue };
            o1.String = "String!";
            o1.Integer = int.MaxValue;

            string json = JsonConvert.SerializeObject(o1, Formatting.Indented);
            string expected = @"{
  ""Object1"": {
    ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
    ""MyProperty"": 1
  },
  ""Object2"": 123,
  ""ObjectNotHandled"": {
    ""MyProperty"": 2147483647
  },
  ""String"": ""String!"",
  ""Integer"": 2147483647
}";
            StringAssert.AreEqual(expected, json);

            TypeNameObject o2 = JsonConvert.DeserializeObject<TypeNameObject>(json);
            Assert.IsNotNull(o2);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), o2.Object1);
            Assert.AreEqual(1, ((TestComponentSimple)o2.Object1).MyProperty);
            CustomAssert.IsInstanceOfType(typeof(long), o2.Object2);
            CustomAssert.IsInstanceOfType(typeof(JObject), o2.ObjectNotHandled);
            StringAssert.AreEqual(@"{
  ""MyProperty"": 2147483647
}", o2.ObjectNotHandled.ToString());
        }

        [Test]
        public void PropertyItemTypeNameHandling()
        {
            PropertyItemTypeNameHandling c1 = new PropertyItemTypeNameHandling();
            c1.Data = new List<object>
            {
                1,
                "two",
                new TestComponentSimple { MyProperty = 1 }
            };

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": [
    1,
    ""two"",
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    }
  ]
}", json);

            PropertyItemTypeNameHandling c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
            Assert.AreEqual(3, c2.Data.Count);

            CustomAssert.IsInstanceOfType(typeof(long), c2.Data[0]);
            CustomAssert.IsInstanceOfType(typeof(string), c2.Data[1]);
            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), c2.Data[2]);
            TestComponentSimple c = (TestComponentSimple)c2.Data[2];
            Assert.AreEqual(1, c.MyProperty);
        }

        [Test]
        public void PropertyItemTypeNameHandlingNestedCollections()
        {
            PropertyItemTypeNameHandling c1 = new PropertyItemTypeNameHandling
            {
                Data = new List<object>
                {
                    new TestComponentSimple { MyProperty = 1 },
                    new List<object>
                    {
                        new List<object>
                        {
                            new List<object>()
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": [
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    {
      ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
      ""$values"": [
        [
          []
        ]
      ]
    }
  ]
}", json);

            PropertyItemTypeNameHandling c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
            Assert.AreEqual(2, c2.Data.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), c2.Data[0]);
            CustomAssert.IsInstanceOfType(typeof(List<object>), c2.Data[1]);
            List<object> c = (List<object>)c2.Data[1];
            CustomAssert.IsInstanceOfType(typeof(JArray), c[0]);

            json = @"{
  ""Data"": [
    {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    {
      ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
      ""$values"": [
        {
          ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
          ""MyProperty"": 1
        }
      ]
    }
  ]
}";

            c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
            Assert.AreEqual(2, c2.Data.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), c2.Data[0]);
            CustomAssert.IsInstanceOfType(typeof(List<object>), c2.Data[1]);
            c = (List<object>)c2.Data[1];
            CustomAssert.IsInstanceOfType(typeof(JObject), c[0]);
            JObject o = (JObject)c[0];
            Assert.AreEqual(1, (int)o["MyProperty"]);
        }

        [Test]
        public void PropertyItemTypeNameHandlingNestedDictionaries()
        {
            PropertyItemTypeNameHandlingDictionary c1 = new PropertyItemTypeNameHandlingDictionary()
            {
                Data = new Dictionary<string, object>
                {
                    {
                        "one", new TestComponentSimple { MyProperty = 1 }
                    },
                    {
                        "two", new Dictionary<string, object>
                        {
                            {
                                "one", new Dictionary<string, object>
                                {
                                    { "one", 1 }
                                }
                            }
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib"",
      ""one"": {
        ""one"": 1
      }
    }
  }
}", json);

            PropertyItemTypeNameHandlingDictionary c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDictionary>(json);
            Assert.AreEqual(2, c2.Data.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), c2.Data["one"]);
            CustomAssert.IsInstanceOfType(typeof(Dictionary<string, object>), c2.Data["two"]);
            Dictionary<string, object> c = (Dictionary<string, object>)c2.Data["two"];
            CustomAssert.IsInstanceOfType(typeof(JObject), c["one"]);

            json = @"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib"",
      ""one"": {
        ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
        ""MyProperty"": 1
      }
    }
  }
}";

            c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDictionary>(json);
            Assert.AreEqual(2, c2.Data.Count);

            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), c2.Data["one"]);
            CustomAssert.IsInstanceOfType(typeof(Dictionary<string, object>), c2.Data["two"]);
            c = (Dictionary<string, object>)c2.Data["two"];
            CustomAssert.IsInstanceOfType(typeof(JObject), c["one"]);

            JObject o = (JObject)c["one"];
            Assert.AreEqual(1, (int)o["MyProperty"]);
        }

        [Test]
        public void PropertyItemTypeNameHandlingObject()
        {
            PropertyItemTypeNameHandlingObject o1 = new PropertyItemTypeNameHandlingObject
            {
                Data = new TypeNameHandlingTestObject
                {
                    Prop1 = new List<object>
                    {
                        new TestComponentSimple
                        {
                            MyProperty = 1
                        }
                    },
                    Prop2 = new TestComponentSimple
                    {
                        MyProperty = 1
                    },
                    Prop3 = 3,
                    Prop4 = new JObject()
                }
            };

            string json = JsonConvert.SerializeObject(o1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": {
    ""Prop1"": {
      ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
      ""$values"": [
        {
          ""MyProperty"": 1
        }
      ]
    },
    ""Prop2"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    ""Prop3"": 3,
    ""Prop4"": {}
  }
}", json);

            PropertyItemTypeNameHandlingObject o2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingObject>(json);
            Assert.IsNotNull(o2);
            Assert.IsNotNull(o2.Data);

            CustomAssert.IsInstanceOfType(typeof(List<object>), o2.Data.Prop1);
            CustomAssert.IsInstanceOfType(typeof(TestComponentSimple), o2.Data.Prop2);
            CustomAssert.IsInstanceOfType(typeof(long), o2.Data.Prop3);
            CustomAssert.IsInstanceOfType(typeof(JObject), o2.Data.Prop4);

            List<object> o = (List<object>)o2.Data.Prop1;
            JObject j = (JObject)o[0];
            Assert.AreEqual(1, (int)j["MyProperty"]);
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void PropertyItemTypeNameHandlingDynamic()
        {
            PropertyItemTypeNameHandlingDynamic d1 = new PropertyItemTypeNameHandlingDynamic();

            dynamic data = new DynamicDictionary();
            data.one = new TestComponentSimple
            {
                MyProperty = 1
            };

            dynamic data2 = new DynamicDictionary();
            data2.one = new TestComponentSimple
            {
                MyProperty = 2
            };

            data.two = data2;

            d1.Data = (DynamicDictionary)data;

            string json = JsonConvert.SerializeObject(d1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""Newtonsoft.Json.Tests.Linq.DynamicDictionary, Newtonsoft.Json.Tests"",
      ""one"": {
        ""MyProperty"": 2
      }
    }
  }
}", json);

            PropertyItemTypeNameHandlingDynamic d2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDynamic>(json);
            Assert.IsNotNull(d2);
            Assert.IsNotNull(d2.Data);

            dynamic data3 = d2.Data;
            TestComponentSimple c = (TestComponentSimple)data3.one;
            Assert.AreEqual(1, c.MyProperty);

            dynamic data4 = data3.two;
            JObject o = (JObject)data4.one;
            Assert.AreEqual(2, (int)o["MyProperty"]);

            json = @"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""Newtonsoft.Json.Tests.Linq.DynamicDictionary, Newtonsoft.Json.Tests"",
      ""one"": {
        ""$type"": ""Newtonsoft.Json.Tests.TestObjects.TestComponentSimple, Newtonsoft.Json.Tests"",
        ""MyProperty"": 2
      }
    }
  }
}";

            d2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDynamic>(json);
            data3 = d2.Data;
            data4 = data3.two;
            o = (JObject)data4.one;
            Assert.AreEqual(2, (int)o["MyProperty"]);
        }
#endif

#if !(DNXCORE50)
        [Test]
        public void SerializeDeserialize_DictionaryContextContainsGuid_DeserializesItemAsGuid()
        {
            const string contextKey = "k1";
            var someValue = new Guid("a6e986df-fc2c-4906-a1ef-9492388f7833");

            Dictionary<string, Guid> inputContext = new Dictionary<string, Guid>();
            inputContext.Add(contextKey, someValue);

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All
            };
            string serializedString = JsonConvert.SerializeObject(inputContext, jsonSerializerSettings);

            StringAssert.AreEqual(@"{
  ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Guid, mscorlib]], mscorlib"",
  ""k1"": ""a6e986df-fc2c-4906-a1ef-9492388f7833""
}", serializedString);

            var deserializedObject = (Dictionary<string, Guid>)JsonConvert.DeserializeObject(serializedString, jsonSerializerSettings);

            Assert.AreEqual(someValue, deserializedObject[contextKey]);
        }

        [Test]
        public void TypeNameHandlingWithISerializableValues()
        {
            MyParent p = new MyParent
            {
                Child = new MyChild
                {
                    MyProperty = "string!"
                }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(p, settings);

            StringAssert.AreEqual(@"{
  ""c"": {
    ""$type"": ""Newtonsoft.Json.Tests.Serialization.MyChild, Newtonsoft.Json.Tests"",
    ""p"": ""string!""
  }
}", json);

            MyParent p2 = JsonConvert.DeserializeObject<MyParent>(json, settings);
            CustomAssert.IsInstanceOfType(typeof(MyChild), p2.Child);
            Assert.AreEqual("string!", ((MyChild)p2.Child).MyProperty);
        }

        [Test]
        public void TypeNameHandlingWithISerializableValuesAndArray()
        {
            MyParent p = new MyParent
            {
                Child = new MyChildList
                {
                    "string!"
                }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(p, settings);

            StringAssert.AreEqual(@"{
  ""c"": {
    ""$type"": ""Newtonsoft.Json.Tests.Serialization.MyChildList, Newtonsoft.Json.Tests"",
    ""$values"": [
      ""string!""
    ]
  }
}", json);

            MyParent p2 = JsonConvert.DeserializeObject<MyParent>(json, settings);
            CustomAssert.IsInstanceOfType(typeof(MyChildList), p2.Child);
            Assert.AreEqual(1, ((MyChildList)p2.Child).Count);
            Assert.AreEqual("string!", ((MyChildList)p2.Child)[0]);
        }

        [Test]
        public void ParentTypeNameHandlingWithISerializableValues()
        {
            ParentParent pp = new ParentParent();

            pp.ParentProp = new MyParent
            {
                Child = new MyChild
                {
                    MyProperty = "string!"
                }
            };

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(pp, settings);

            StringAssert.AreEqual(@"{
  ""ParentProp"": {
    ""c"": {
      ""$type"": ""Newtonsoft.Json.Tests.Serialization.MyChild, Newtonsoft.Json.Tests"",
      ""p"": ""string!""
    }
  }
}", json);

            ParentParent pp2 = JsonConvert.DeserializeObject<ParentParent>(json, settings);
            MyParent p2 = pp2.ParentProp;
            CustomAssert.IsInstanceOfType(typeof(MyChild), p2.Child);
            Assert.AreEqual("string!", ((MyChild)p2.Child).MyProperty);
        }
#endif

        [Test]
        public void ListOfStackWithFullAssemblyName()
        {
            var input = new List<Stack<string>>();

            input.Add(new Stack<string>(new List<string> { "One", "Two", "Three" }));
            input.Add(new Stack<string>(new List<string> { "Four", "Five", "Six" }));
            input.Add(new Stack<string>(new List<string> { "Seven", "Eight", "Nine" }));

            string serialized = JsonConvert.SerializeObject(input,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Full // TypeNameHandling.Auto will work
#pragma warning restore 618
                });

            var output = JsonConvert.DeserializeObject<List<Stack<string>>>(serialized,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
                );

            List<string> strings = output.SelectMany(s => s).ToList();

            Assert.AreEqual(9, strings.Count);
            Assert.AreEqual("One", strings[0]);
            Assert.AreEqual("Nine", strings[8]);
        }

#if !NET20
        [Test]
        public void ExistingBaseValue()
        {
            string json = @"{
    ""itemIdentifier"": {
        ""$type"": ""Newtonsoft.Json.Tests.Serialization.ReportItemKeys, Newtonsoft.Json.Tests"",
        ""dataType"": 0,
        ""wantedUnitID"": 1,
        ""application"": 3,
        ""id"": 101,
        ""name"": ""Machine""
    },
    ""isBusinessEntity"": false,
    ""isKeyItem"": true,
    ""summarizeOnThisItem"": false
}";

            GroupingInfo g = JsonConvert.DeserializeObject<GroupingInfo>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            ReportItemKeys item = (ReportItemKeys)g.ItemIdentifier;
            Assert.AreEqual(1UL, item.WantedUnitID);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void GenericItemTypeCollection()
        {
            DataType data = new DataType();
            data.Rows.Add("key", new List<MyInterfaceImplementationType> { new MyInterfaceImplementationType() { SomeProperty = "property" } });
            string serialized = JsonConvert.SerializeObject(data, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Rows"": {
    ""key"": {
      ""$type"": ""System.Collections.Generic.List`1[[Newtonsoft.Json.Tests.Serialization.MyInterfaceImplementationType, Newtonsoft.Json.Tests]], mscorlib"",
      ""$values"": [
        {
          ""SomeProperty"": ""property""
        }
      ]
    }
  }
}", serialized);

            DataType deserialized = JsonConvert.DeserializeObject<DataType>(serialized);

            Assert.AreEqual("property", deserialized.Rows["key"].First().SomeProperty);
        }
#endif

#if !(NET20 || PORTABLE || PORTABLE40)
        [Test]
        public void DeserializeComplexGenericDictionary_Simple()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
            };

            Dictionary<int, HashSet<string>> dictionary = new Dictionary<int, HashSet<string>>
            {
                { 1, new HashSet<string>(new[] { "test" }) },
            };

            string obtainedJson = JsonConvert.SerializeObject(dictionary, serializerSettings);

            Dictionary<int, HashSet<string>> obtainedDictionary = (Dictionary<int, HashSet<string>>)JsonConvert.DeserializeObject(obtainedJson, serializerSettings);

            Assert.IsNotNull(obtainedDictionary);
        }

        [Test]
        public void DeserializeComplexGenericDictionary_Full()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            };

            Dictionary<int, HashSet<string>> dictionary = new Dictionary<int, HashSet<string>>
            {
                { 1, new HashSet<string>(new[] { "test" }) },
            };

            string obtainedJson = JsonConvert.SerializeObject(dictionary, serializerSettings);

            Dictionary<int, HashSet<string>> obtainedDictionary = (Dictionary<int, HashSet<string>>)JsonConvert.DeserializeObject(obtainedJson, serializerSettings);

            Assert.IsNotNull(obtainedDictionary);
        }

        [Test]
        public void SerializeNullableStructProperty_Auto()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            ObjectWithOptionalMessage objWithMessage = new ObjectWithOptionalMessage(new Message2("Hello!"));

            string json = JsonConvert.SerializeObject(objWithMessage, serializerSettings);

            StringAssert.AreEqual(@"{
  ""Message"": {
    ""Value"": ""Hello!""
  }
}", json);
        }

        [Test]
        public void DeserializeNullableStructProperty_Auto()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            string json = @"{
  ""Message"": {
    ""Value"": ""Hello!""
  }
}";
            ObjectWithOptionalMessage objWithMessage = JsonConvert.DeserializeObject<ObjectWithOptionalMessage>(json, serializerSettings);

            StringAssert.AreEqual("Hello!", objWithMessage.Message.Value.Value);
        }
#endif

#if !(NET20 || NET35)
        [Test]
        public void SerializerWithDefaultBinder()
        {
            var serializer = JsonSerializer.Create();
#pragma warning disable CS0618
            Assert.NotNull(serializer.Binder);
            Assert.IsInstanceOf(typeof(DefaultSerializationBinder), serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsInstanceOf(typeof(DefaultSerializationBinder), serializer.SerializationBinder);
        }

        [Test]
        public void ObsoleteBinderThrowsIfISerializationBinderSet()
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings() { SerializationBinder = new FancyBinder() });
            ExceptionAssert.Throws<InvalidOperationException>(() =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var serializationBinder = serializer.Binder;
#pragma warning restore CS0618 // Type or member is obsolete
                serializationBinder.ToString();
            }, "Cannot get SerializationBinder because an ISerializationBinder was previously set.");

            Assert.IsInstanceOf(typeof(FancyBinder), serializer.SerializationBinder);
        }

        [Test]
        public void SetOldBinderAndSerializationBinderReturnsWrapper()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var serializer = JsonSerializer.Create(new JsonSerializerSettings() { Binder = new OldBinder() });
            Assert.IsInstanceOf(typeof(OldBinder), serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

            var binder = serializer.SerializationBinder;

            Assert.IsInstanceOf(typeof(SerializationBinderAdapter), binder);
            Assert.AreEqual(typeof(string), binder.BindToType(null, null));
        }

        public class FancyBinder : ISerializationBinder
        {
            private static readonly string Annotate = new string(':', 3);

            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = string.Format("FancyAssemblyName=>{0}", Assembly.GetAssembly(serializedType)?.GetName().Name);
                typeName = string.Format("{0}{1}{0}", Annotate, serializedType.Name);
            }

            public Type BindToType(string assemblyName, string typeName)
            {
                return null;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public class OldBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return typeof(string);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
#endif
    }

    public struct Message2
    {
        public string Value { get; }

        [JsonConstructor]
        public Message2(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Value = value;
        }
    }

    public class ObjectWithOptionalMessage
    {
        public Message2? Message { get; }

        public ObjectWithOptionalMessage(Message2? message)
        {
            Message = message;
        }
    }

    public class DataType
    {
        public DataType()
        {
            Rows = new Dictionary<string, IEnumerable<IMyInterfaceType>>();
        }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto, TypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<string, IEnumerable<IMyInterfaceType>> Rows { get; private set; }
    }

    public interface IMyInterfaceType
    {
        string SomeProperty { get; set; }
    }

    public class MyInterfaceImplementationType : IMyInterfaceType
    {
        public string SomeProperty { get; set; }
    }

#if !(DNXCORE50)
    public class ParentParent
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public MyParent ParentProp { get; set; }
    }

    [Serializable]
    public class MyParent : ISerializable
    {
        public ISomeBase Child { get; internal set; }

        public MyParent(SerializationInfo info, StreamingContext context)
        {
            Child = (ISomeBase)info.GetValue("c", typeof(ISomeBase));
        }

        public MyParent()
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("c", Child);
        }
    }

    public class MyChild : ISomeBase
    {
        [JsonProperty("p")]
        public String MyProperty { get; internal set; }
    }

    public class MyChildList : List<string>, ISomeBase
    {
    }

    public interface ISomeBase
    {
    }
#endif

    public class Message
    {
        public string Address { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Body { get; set; }
    }

    public class SearchDetails
    {
        public string Query { get; set; }
        public string Language { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
    }

    public class Purchase
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

#if !(DNXCORE50)
    public class SerializableWrapper
    {
        public object Content { get; set; }

        public override bool Equals(object obj)
        {
            SerializableWrapper w = obj as SerializableWrapper;

            if (w == null)
            {
                return false;
            }

            return Equals(w.Content, Content);
        }

        public override int GetHashCode()
        {
            if (Content == null)
            {
                return 0;
            }

            return Content.GetHashCode();
        }
    }

    public interface IExample
        : ISerializable
    {
        String Name { get; }
    }

    [Serializable]
    public class Example
        : IExample
    {
        public Example(String name)
        {
            Name = name;
        }

        protected Example(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("name");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", Name);
        }

        public String Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is IExample)
            {
                return Name.Equals(((IExample)obj).Name);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (Name == null)
            {
                return 0;
            }

            return Name.GetHashCode();
        }
    }
#endif

    public class PropertyItemTypeNameHandlingObject
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public TypeNameHandlingTestObject Data { get; set; }
    }

#if !(NET35 || NET20 || PORTABLE40)
    public class PropertyItemTypeNameHandlingDynamic
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public DynamicDictionary Data { get; set; }
    }
#endif

    public class TypeNameHandlingTestObject
    {
        public object Prop1 { get; set; }
        public object Prop2 { get; set; }
        public object Prop3 { get; set; }
        public object Prop4 { get; set; }
    }

    public class PropertyItemTypeNameHandlingDictionary
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public IDictionary<string, object> Data { get; set; }
    }

    public class PropertyItemTypeNameHandling
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public IList<object> Data { get; set; }
    }

    [JsonArray(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TypeNameList<T> : List<T>
    {
    }

    [JsonDictionary(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TypeNameDictionary<T> : Dictionary<string, T>
    {
    }

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class TypeNameObject
    {
        public object Object1 { get; set; }
        public object Object2 { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public object ObjectNotHandled { get; set; }

        public string String { get; set; }
        public int Integer { get; set; }
    }

#if !NET20
    [DataContract]
    public class GroupingInfo
    {
        [DataMember]
        public ApplicationItemKeys ItemIdentifier { get; set; }

        public GroupingInfo()
        {
            ItemIdentifier = new ApplicationItemKeys();
        }
    }

    [DataContract]
    public class ApplicationItemKeys
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class ReportItemKeys : ApplicationItemKeys
    {
        protected ulong _wantedUnit;

        [DataMember]
        public ulong WantedUnitID
        {
            get { return _wantedUnit; }
            set { _wantedUnit = value; }
        }
    }
#endif
}
#endif