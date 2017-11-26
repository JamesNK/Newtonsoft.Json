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
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class PreserveReferencesHandlingTests : TestFixtureBase
    {
        public class ContentB
        {
            public bool SomeValue { get; set; }
        }

        [JsonConverter(typeof(ListConverter))]
        public class ContentA : List<object>
        {
            public ContentB B { get; set; }

            public ContentA()
            {
                B = new ContentB();
            }
        }

        public class ListConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new ContentA() { B = serializer.Deserialize<ContentB>(reader) }; // Construct my data back.
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                ContentB b = ((ContentA)value).B;
                serializer.Serialize(writer, b); // My Content.B contains all useful data.
            }
        }

        public class Container
        {
            public List<ContentA> ListA { get; set; }
            public List<ContentA> ListB { get; set; }

            public Container()
            {
                ListA = new List<ContentA>();
                ListB = new List<ContentA>();
            }
        }

        [Test]
        public void SerializeReferenceInConvert()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
            settings.TypeNameHandling = TypeNameHandling.All;
            settings.Formatting = Formatting.Indented;

            Container c1 = new Container();
            ContentA content = new ContentA();
            content.B.SomeValue = true;
            c1.ListA.Add(content);
            c1.ListB.Add(content);

            string s = JsonConvert.SerializeObject(c1, settings);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$type"": ""Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+Container, Newtonsoft.Json.Tests"",
  ""ListA"": {
    ""$id"": ""2"",
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(List<ContentA>), 0, DefaultSerializationBinder.Instance) + @""",
    ""$values"": [
      {
        ""$id"": ""3"",
        ""$type"": ""Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+ContentB, Newtonsoft.Json.Tests"",
        ""SomeValue"": true
      }
    ]
  },
  ""ListB"": {
    ""$id"": ""4"",
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(List<ContentA>), 0, DefaultSerializationBinder.Instance) + @""",
    ""$values"": [
      {
        ""$ref"": ""3""
      }
    ]
  }
}", s);

            Container c2 = JsonConvert.DeserializeObject<Container>(s, settings);

            Assert.AreEqual(c2.ListA[0], c2.ListB[0]);
            Assert.AreEqual(true, c2.ListA[0].B.SomeValue);
        }

        public class Parent
        {
            public Child ReadOnlyChild
            {
                get { return Child1; }
            }

            public Child Child1 { get; set; }
            public Child Child2 { get; set; }

            public IList<string> ReadOnlyList
            {
                get { return List1; }
            }

            public IList<string> List1 { get; set; }
            public IList<string> List2 { get; set; }
        }

        public class Child
        {
            public string PropertyName { get; set; }
        }

        [Test]
        public void SerializeReadOnlyProperty()
        {
            Child c = new Child
            {
                PropertyName = "value?"
            };
            IList<string> l = new List<string>
            {
                "value!"
            };
            Parent p = new Parent
            {
                Child1 = c,
                Child2 = c,
                List1 = l,
                List2 = l
            };

            string json = JsonConvert.SerializeObject(p, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""ReadOnlyChild"": {
    ""PropertyName"": ""value?""
  },
  ""Child1"": {
    ""$id"": ""2"",
    ""PropertyName"": ""value?""
  },
  ""Child2"": {
    ""$ref"": ""2""
  },
  ""ReadOnlyList"": [
    ""value!""
  ],
  ""List1"": {
    ""$id"": ""3"",
    ""$values"": [
      ""value!""
    ]
  },
  ""List2"": {
    ""$ref"": ""3""
  }
}", json);

            Parent newP = JsonConvert.DeserializeObject<Parent>(json, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

            Assert.AreEqual("value?", newP.Child1.PropertyName);
            Assert.AreEqual(newP.Child1, newP.Child2);
            Assert.AreEqual(newP.Child1, newP.ReadOnlyChild);

            Assert.AreEqual("value!", newP.List1[0]);
            Assert.AreEqual(newP.List1, newP.List2);
            Assert.AreEqual(newP.List1, newP.ReadOnlyList);
        }

        [Test]
        public void SerializeDictionarysWithPreserveObjectReferences()
        {
            CircularDictionary circularDictionary = new CircularDictionary();
            circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}", json);
        }

        [Test]
        public void DeserializeDictionarysWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}";

            CircularDictionary circularDictionary = JsonConvert.DeserializeObject<CircularDictionary>(json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All
                });

            Assert.AreEqual(2, circularDictionary.Count);
            Assert.AreEqual(1, circularDictionary["other"].Count);
            Assert.IsTrue(ReferenceEquals(circularDictionary, circularDictionary["self"]));
        }

        public class CircularList : List<CircularList>
        {
        }

        [Test]
        public void SerializeCircularListsError()
        {
            string classRef = typeof(CircularList).FullName;

            CircularList circularList = new CircularList();
            circularList.Add(null);
            circularList.Add(new CircularList { null });
            circularList.Add(new CircularList { new CircularList { circularList } });

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(circularList, Formatting.Indented); }, "Self referencing loop detected with type '" + classRef + "'. Path '[2][0]'.");
        }

        [Test]
        public void SerializeCircularListsIgnore()
        {
            CircularList circularList = new CircularList();
            circularList.Add(null);
            circularList.Add(new CircularList { null });
            circularList.Add(new CircularList { new CircularList { circularList } });

            string json = JsonConvert.SerializeObject(circularList,
                Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            StringAssert.AreEqual(@"[
  null,
  [
    null
  ],
  [
    []
  ]
]", json);
        }

        [Test]
        public void SerializeListsWithPreserveObjectReferences()
        {
            CircularList circularList = new CircularList();
            circularList.Add(null);
            circularList.Add(new CircularList { null });
            circularList.Add(new CircularList { new CircularList { circularList } });

            string json = JsonConvert.SerializeObject(circularList, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}", json);
        }

        [Test]
        public void DeserializeListsWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

            CircularList circularList = JsonConvert.DeserializeObject<CircularList>(json,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

            Assert.AreEqual(3, circularList.Count);
            Assert.AreEqual(null, circularList[0]);
            Assert.AreEqual(1, circularList[1].Count);
            Assert.AreEqual(1, circularList[2].Count);
            Assert.AreEqual(1, circularList[2][0].Count);
            Assert.IsTrue(ReferenceEquals(circularList, circularList[2][0][0]));
        }

        [Test]
        public void DeserializeArraysWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<string[][]>(json,
                    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
            }, @"Cannot preserve reference to array or readonly list, or list created from a non-default constructor: System.String[][]. Path '$values', line 3, position 14.");
        }

        public class CircularDictionary : Dictionary<string, CircularDictionary>
        {
        }

        [Test]
        public void SerializeCircularDictionarysError()
        {
            string classRef = typeof(CircularDictionary).FullName;

            CircularDictionary circularDictionary = new CircularDictionary();
            circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(circularDictionary, Formatting.Indented); }, @"Self referencing loop detected with type '" + classRef + "'. Path ''.");
        }

        [Test]
        public void SerializeCircularDictionarysIgnore()
        {
            CircularDictionary circularDictionary = new CircularDictionary();
            circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            StringAssert.AreEqual(@"{
  ""other"": {
    ""blah"": null
  }
}", json);
        }

        [Test]
        public void UnexpectedEnd()
        {
            string json = @"{
  ""$id"":";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<string[][]>(json,
                    new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        MetadataPropertyHandling = MetadataPropertyHandling.Default
                    });
            }, @"Unexpected end when reading JSON. Path '$id', line 2, position 8.");
        }

        public class CircularReferenceClassConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                CircularReferenceClass circularReferenceClass = (CircularReferenceClass)value;

                string reference = serializer.ReferenceResolver.GetReference(serializer, circularReferenceClass);

                JObject me = new JObject();
                me["$id"] = new JValue(reference);
                me["$type"] = new JValue(value.GetType().Name);
                me["Name"] = new JValue(circularReferenceClass.Name);

                JObject o = JObject.FromObject(circularReferenceClass.Child, serializer);
                me["Child"] = o;

                me.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject o = JObject.Load(reader);
                string id = (string)o["$id"];
                if (id != null)
                {
                    CircularReferenceClass circularReferenceClass = new CircularReferenceClass();
                    serializer.Populate(o.CreateReader(), circularReferenceClass);
                    return circularReferenceClass;
                }
                else
                {
                    string reference = (string)o["$ref"];
                    return serializer.ReferenceResolver.ResolveReference(serializer, reference);
                }
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(CircularReferenceClass));
            }
        }

        [Test]
        public void SerializeCircularReferencesWithConverter()
        {
            CircularReferenceClass c1 = new CircularReferenceClass { Name = "c1" };
            CircularReferenceClass c2 = new CircularReferenceClass { Name = "c2" };
            CircularReferenceClass c3 = new CircularReferenceClass { Name = "c3" };

            c1.Child = c2;
            c2.Child = c3;
            c3.Child = c1;

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter> { new CircularReferenceClassConverter() }
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$type"": ""CircularReferenceClass"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""$type"": ""CircularReferenceClass"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""$type"": ""CircularReferenceClass"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}", json);
        }

        [Test]
        public void DeserializeCircularReferencesWithConverter()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$type"": ""CircularReferenceClass"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""$type"": ""CircularReferenceClass"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""$type"": ""CircularReferenceClass"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}";

            CircularReferenceClass c1 = JsonConvert.DeserializeObject<CircularReferenceClass>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter> { new CircularReferenceClassConverter() }
            });

            Assert.AreEqual("c1", c1.Name);
            Assert.AreEqual("c2", c1.Child.Name);
            Assert.AreEqual("c3", c1.Child.Child.Name);
            Assert.AreEqual("c1", c1.Child.Child.Child.Name);
        }

        [Test]
        public void SerializeEmployeeReference()
        {
            EmployeeReference mikeManager = new EmployeeReference
            {
                Name = "Mike Manager"
            };
            EmployeeReference joeUser = new EmployeeReference
            {
                Name = "Joe User",
                Manager = mikeManager
            };

            List<EmployeeReference> employees = new List<EmployeeReference>
            {
                mikeManager,
                joeUser
            };

            string json = JsonConvert.SerializeObject(employees, Formatting.Indented);
            StringAssert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""Name"": ""Mike Manager"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""Joe User"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]", json);
        }

        [Test]
        public void DeserializeEmployeeReference()
        {
            string json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""Mike Manager"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""Joe User"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]";

            List<EmployeeReference> employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json);

            Assert.AreEqual(2, employees.Count);
            Assert.AreEqual("Mike Manager", employees[0].Name);
            Assert.AreEqual("Joe User", employees[1].Name);
            Assert.AreEqual(employees[0], employees[1].Manager);
        }

        [JsonObject(IsReference = true)]
        private class Condition
        {
            public int Value { get; }

            public Condition(int value)
            {
                Value = value;
            }
        }

        private class ClassWithConditions
        {
            public Condition Condition1 { get; }

            public Condition Condition2 { get; }

            public ClassWithConditions(Condition condition1, Condition condition2)
            {
                Condition1 = condition1;
                Condition2 = condition2;
            }
        }

        [Test]
        public void SerializeIsReferenceReadonlyProperty()
        {
            Condition condition = new Condition(1);
            ClassWithConditions value = new ClassWithConditions(condition, condition);

            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Condition1"": {
    ""$id"": ""1"",
    ""Value"": 1
  },
  ""Condition2"": {
    ""$ref"": ""1""
  }
}", json);
        }

        [Test]
        public void DeserializeIsReferenceReadonlyProperty()
        {
            string json = @"{
  ""Condition1"": {
    ""$id"": ""1"",
    ""Value"": 1
  },
  ""Condition2"": {
    ""$ref"": ""1""
  }
}";

            ClassWithConditions value = JsonConvert.DeserializeObject<ClassWithConditions>(json);
            Assert.AreEqual(value.Condition1.Value, 1);
            Assert.AreEqual(value.Condition1, value.Condition2);
        }

        [Test]
        public void SerializeCircularReference()
        {
            CircularReferenceClass c1 = new CircularReferenceClass { Name = "c1" };
            CircularReferenceClass c2 = new CircularReferenceClass { Name = "c2" };
            CircularReferenceClass c3 = new CircularReferenceClass { Name = "c3" };

            c1.Child = c2;
            c2.Child = c3;
            c3.Child = c1;

            string json = JsonConvert.SerializeObject(c1, Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}", json);
        }

        [Test]
        public void DeserializeCircularReference()
        {
            string json = @"{
  ""$id"": ""1"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}";

            CircularReferenceClass c1 =
                JsonConvert.DeserializeObject<CircularReferenceClass>(json, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });

            Assert.AreEqual("c1", c1.Name);
            Assert.AreEqual("c2", c1.Child.Name);
            Assert.AreEqual("c3", c1.Child.Child.Name);
            Assert.AreEqual("c1", c1.Child.Child.Child.Name);
        }

        [Test]
        public void SerializeReferenceInList()
        {
            EmployeeReference e1 = new EmployeeReference { Name = "e1" };
            EmployeeReference e2 = new EmployeeReference { Name = "e2" };

            List<EmployeeReference> employees = new List<EmployeeReference> { e1, e2, e1, e2 };

            string json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            StringAssert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]", json);
        }

        [Test]
        public void DeserializeReferenceInList()
        {
            string json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]";

            List<EmployeeReference> employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json);
            Assert.AreEqual(4, employees.Count);

            Assert.AreEqual("e1", employees[0].Name);
            Assert.AreEqual("e2", employees[1].Name);
            Assert.AreEqual("e1", employees[2].Name);
            Assert.AreEqual("e2", employees[3].Name);

            Assert.AreEqual(employees[0], employees[2]);
            Assert.AreEqual(employees[1], employees[3]);
        }

        [Test]
        public void SerializeReferenceInDictionary()
        {
            EmployeeReference e1 = new EmployeeReference { Name = "e1" };
            EmployeeReference e2 = new EmployeeReference { Name = "e2" };

            Dictionary<string, EmployeeReference> employees = new Dictionary<string, EmployeeReference>
            {
                { "One", e1 },
                { "Two", e2 },
                { "Three", e1 },
                { "Four", e2 }
            };

            string json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""One"": {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  ""Two"": {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  ""Three"": {
    ""$ref"": ""1""
  },
  ""Four"": {
    ""$ref"": ""2""
  }
}", json);
        }

        [Test]
        public void DeserializeReferenceInDictionary()
        {
            string json = @"{
  ""One"": {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  ""Two"": {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  ""Three"": {
    ""$ref"": ""1""
  },
  ""Four"": {
    ""$ref"": ""2""
  }
}";

            Dictionary<string, EmployeeReference> employees = JsonConvert.DeserializeObject<Dictionary<string, EmployeeReference>>(json);
            Assert.AreEqual(4, employees.Count);

            EmployeeReference e1 = employees["One"];
            EmployeeReference e2 = employees["Two"];

            Assert.AreEqual("e1", e1.Name);
            Assert.AreEqual("e2", e2.Name);

            Assert.AreEqual(e1, employees["Three"]);
            Assert.AreEqual(e2, employees["Four"]);
        }

        [Test]
        public void ExampleWithout()
        {
            Person p = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Department = "IT",
                Name = "James"
            };

            List<Person> people = new List<Person>();
            people.Add(p);
            people.Add(p);

            string json = JsonConvert.SerializeObject(people, Formatting.Indented);
            //[
            //  {
            //    "Name": "James",
            //    "BirthDate": "\/Date(346377600000)\/",
            //    "LastModified": "\/Date(1235134761000)\/"
            //  },
            //  {
            //    "Name": "James",
            //    "BirthDate": "\/Date(346377600000)\/",
            //    "LastModified": "\/Date(1235134761000)\/"
            //  }
            //]
        }

        [Test]
        public void ExampleWith()
        {
            Person p = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Department = "IT",
                Name = "James"
            };

            List<Person> people = new List<Person>();
            people.Add(p);
            people.Add(p);

            string json = JsonConvert.SerializeObject(people, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
            //[
            //  {
            //    "$id": "1",
            //    "Name": "James",
            //    "BirthDate": "\/Date(346377600000)\/",
            //    "LastModified": "\/Date(1235134761000)\/"
            //  },
            //  {
            //    "$ref": "1"
            //  }
            //]

            List<Person> deserializedPeople = JsonConvert.DeserializeObject<List<Person>>(json,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            Assert.AreEqual(2, deserializedPeople.Count);

            Person p1 = deserializedPeople[0];
            Person p2 = deserializedPeople[1];

            Assert.AreEqual("James", p1.Name);
            Assert.AreEqual("James", p2.Name);

            bool equal = Object.ReferenceEquals(p1, p2);
            Assert.AreEqual(true, equal);
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class User
        {
            #region properties
            [JsonProperty(Required = Required.Always, PropertyName = "SecretType")]
            private string secretType;

            [JsonProperty(Required = Required.Always)]
            public string Login { get; set; }

            public Type SecretType
            {
                get { return Type.GetType(secretType); }
                set { secretType = value.AssemblyQualifiedName; }
            }

            [JsonProperty]
            public User Friend { get; set; }
            #endregion

            #region constructors
            public User()
            {
            }

            public User(string login, Type secretType)
                : this()
            {
                Login = login;
                SecretType = secretType;
            }
            #endregion

            #region methods
            public override int GetHashCode()
            {
                return SecretType.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("SecretType: {0}, Login: {1}", secretType, Login);
            }
            #endregion
        }

        [Test]
        public void DeserializeTypeWithDubiousGetHashcode()
        {
            User user1 = new User("Peter", typeof(Version));
            User user2 = new User("Michael", typeof(Version));

            user1.Friend = user2;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            string json = JsonConvert.SerializeObject(user1, Formatting.Indented, serializerSettings);

            User deserializedUser = JsonConvert.DeserializeObject<User>(json, serializerSettings);
            Assert.IsNotNull(deserializedUser);
        }

        [Test]
        public void PreserveReferencesHandlingWithReusedJsonSerializer()
        {
            MyClass c = new MyClass();

            IList<MyClass> myClasses1 = new List<MyClass>
            {
                c,
                c
            };

            var ser = new JsonSerializer()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };

            MemoryStream ms = new MemoryStream();

            using (var sw = new StreamWriter(ms))
            using (var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                ser.Serialize(writer, myClasses1);
            }

            byte[] data = ms.ToArray();
            string json = Encoding.UTF8.GetString(data, 0, data.Length);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""$id"": ""2"",
      ""PreProperty"": 0,
      ""PostProperty"": 0
    },
    {
      ""$ref"": ""2""
    }
  ]
}", json);

            ms = new MemoryStream(data);
            IList<MyClass> myClasses2;

            using (var sr = new StreamReader(ms))
            using (var reader = new JsonTextReader(sr))
            {
                myClasses2 = ser.Deserialize<IList<MyClass>>(reader);
            }

            Assert.AreEqual(2, myClasses2.Count);
            Assert.AreEqual(myClasses2[0], myClasses2[1]);

            Assert.AreNotEqual(myClasses1[0], myClasses2[0]);
        }

        [Test]
        public void ReferencedIntList()
        {
            ReferencedList<int> l = new ReferencedList<int>();
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
        public void ReferencedComponentList()
        {
            var c1 = new TestComponentSimple();

            ReferencedList<TestComponentSimple> l = new ReferencedList<TestComponentSimple>();
            l.Add(c1);
            l.Add(new TestComponentSimple());
            l.Add(c1);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"[
  {
    ""$id"": ""1"",
    ""MyProperty"": 0
  },
  {
    ""$id"": ""2"",
    ""MyProperty"": 0
  },
  {
    ""$ref"": ""1""
  }
]", json);
        }

        [Test]
        public void ReferencedIntDictionary()
        {
            ReferencedDictionary<int> l = new ReferencedDictionary<int>();
            l.Add("First", 1);
            l.Add("Second", 2);
            l.Add("Third", 3);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""First"": 1,
  ""Second"": 2,
  ""Third"": 3
}", json);
        }

        [Test]
        public void ReferencedComponentDictionary()
        {
            var c1 = new TestComponentSimple();

            ReferencedDictionary<TestComponentSimple> l = new ReferencedDictionary<TestComponentSimple>();
            l.Add("First", c1);
            l.Add("Second", new TestComponentSimple());
            l.Add("Third", c1);

            string json = JsonConvert.SerializeObject(l, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""First"": {
    ""$id"": ""1"",
    ""MyProperty"": 0
  },
  ""Second"": {
    ""$id"": ""2"",
    ""MyProperty"": 0
  },
  ""Third"": {
    ""$ref"": ""1""
  }
}", json);

            ReferencedDictionary<TestComponentSimple> d = JsonConvert.DeserializeObject<ReferencedDictionary<TestComponentSimple>>(json);
            Assert.AreEqual(3, d.Count);
            Assert.IsTrue(ReferenceEquals(d["First"], d["Third"]));
        }

        [Test]
        public void ReferencedObjectItems()
        {
            ReferenceObject o1 = new ReferenceObject();

            o1.Component1 = new TestComponentSimple { MyProperty = 1 };
            o1.Component2 = o1.Component1;
            o1.ComponentNotReference = new TestComponentSimple();
            o1.String = "String!";
            o1.Integer = int.MaxValue;

            string json = JsonConvert.SerializeObject(o1, Formatting.Indented);
            string expected = @"{
  ""Component1"": {
    ""$id"": ""1"",
    ""MyProperty"": 1
  },
  ""Component2"": {
    ""$ref"": ""1""
  },
  ""ComponentNotReference"": {
    ""MyProperty"": 0
  },
  ""String"": ""String!"",
  ""Integer"": 2147483647
}";
            StringAssert.AreEqual(expected, json);

            ReferenceObject referenceObject = JsonConvert.DeserializeObject<ReferenceObject>(json);
            Assert.IsNotNull(referenceObject);

            Assert.IsTrue(ReferenceEquals(referenceObject.Component1, referenceObject.Component2));
        }

        [Test]
        public void PropertyItemIsReferenceObject()
        {
            TestComponentSimple c1 = new TestComponentSimple();

            PropertyItemIsReferenceObject o1 = new PropertyItemIsReferenceObject
            {
                Data = new PropertyItemIsReferenceBody
                {
                    Prop1 = c1,
                    Prop2 = c1,
                    Data = new List<TestComponentSimple>
                    {
                        c1
                    }
                }
            };

            string json = JsonConvert.SerializeObject(o1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Data"": {
    ""Prop1"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    },
    ""Prop2"": {
      ""$ref"": ""1""
    },
    ""Data"": {
      ""$id"": ""2"",
      ""$values"": [
        {
          ""MyProperty"": 0
        }
      ]
    }
  }
}", json);

            PropertyItemIsReferenceObject o2 = JsonConvert.DeserializeObject<PropertyItemIsReferenceObject>(json);

            TestComponentSimple c2 = o2.Data.Prop1;
            TestComponentSimple c3 = o2.Data.Prop2;
            TestComponentSimple c4 = o2.Data.Data[0];

            Assert.IsTrue(ReferenceEquals(c2, c3));
            Assert.IsFalse(ReferenceEquals(c2, c4));
        }

        [Test]
        public void DuplicateId()
        {
            string json = @"{
  ""Data"": {
    ""Prop1"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    },
    ""Prop2"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    }
  }
}";

            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<PropertyItemIsReferenceObject>(json, new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            }), "Error reading object reference '1'. Path 'Data.Prop2.MyProperty', line 9, position 19.");
        }
    }

    public class PropertyItemIsReferenceBody
    {
        public TestComponentSimple Prop1 { get; set; }
        public TestComponentSimple Prop2 { get; set; }
        public IList<TestComponentSimple> Data { get; set; }
    }

    public class PropertyItemIsReferenceObject
    {
        [JsonProperty(ItemIsReference = true)]
        public PropertyItemIsReferenceBody Data { get; set; }
    }

    public class PropertyItemIsReferenceList
    {
        [JsonProperty(ItemIsReference = true)]
        public IList<IList<object>> Data { get; set; }
    }

    [JsonArray(ItemIsReference = true)]
    public class ReferencedList<T> : List<T>
    {
    }

    [JsonDictionary(ItemIsReference = true)]
    public class ReferencedDictionary<T> : Dictionary<string, T>
    {
    }

    [JsonObject(ItemIsReference = true)]
    public class ReferenceObject
    {
        public TestComponentSimple Component1 { get; set; }
        public TestComponentSimple Component2 { get; set; }

        [JsonProperty(IsReference = false)]
        public TestComponentSimple ComponentNotReference { get; set; }

        public string String { get; set; }
        public int Integer { get; set; }
    }
}