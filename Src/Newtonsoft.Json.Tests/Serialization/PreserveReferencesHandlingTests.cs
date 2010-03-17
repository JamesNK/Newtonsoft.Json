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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class PreserveReferencesHandlingTests : TestFixtureBase
  {
    [Test]
    public void SerializeDictionarysWithPreserveObjectReferences()
    {
      CircularDictionary circularDictionary = new CircularDictionary();
      circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
      circularDictionary.Add("self", circularDictionary);

      string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
        new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

      Assert.AreEqual(@"{
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
      Assert.AreEqual(circularDictionary, circularDictionary["self"]);
    }

    public class CircularList : List<CircularList>
    {
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Self referencing loop")]
    public void SerializeCircularListsError()
    {
      CircularList circularList = new CircularList();
      circularList.Add(null);
      circularList.Add(new CircularList { null });
      circularList.Add(new CircularList { new CircularList { circularList } });

      JsonConvert.SerializeObject(circularList, Formatting.Indented);
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

      Assert.AreEqual(@"[
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

      WriteEscapedJson(json);
      Assert.AreEqual(@"{
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
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Cannot preserve reference to array or readonly list: System.String[][]")]
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

      JsonConvert.DeserializeObject<string[][]>(json,
        new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
    }

    public class CircularDictionary : Dictionary<string, CircularDictionary>
    {
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Self referencing loop")]
    public void SerializeCircularDictionarysError()
    {
      CircularDictionary circularDictionary = new CircularDictionary();
      circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
      circularDictionary.Add("self", circularDictionary);

      JsonConvert.SerializeObject(circularDictionary, Formatting.Indented);
    }

    [Test]
    public void SerializeCircularDictionarysIgnore()
    {
      CircularDictionary circularDictionary = new CircularDictionary();
      circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
      circularDictionary.Add("self", circularDictionary);

      string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

      Assert.AreEqual(@"{
  ""other"": {
    ""blah"": null
  }
}", json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Unexpected end when deserializing object.")]
    public void UnexpectedEnd()
    {
      string json = @"{
  ""$id"":";

      JsonConvert.DeserializeObject<string[][]>(json,
        new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
    }

    public class CircularReferenceClassConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        CircularReferenceClass circularReferenceClass = (CircularReferenceClass)value;

        string reference = serializer.ReferenceResolver.GetReference(circularReferenceClass);

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
          return serializer.ReferenceResolver.ResolveReference(reference);
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

      Assert.AreEqual(@"{
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
      Assert.AreEqual(@"[
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

      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"[
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
          {"One", e1},
          {"Two", e2},
          {"Three", e1},
          {"Four", e2}
        };

      string json = JsonConvert.SerializeObject(employees, Formatting.Indented);

      Assert.AreEqual(@"{
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

      Console.WriteLine(deserializedPeople.Count);
      // 2

      Person p1 = deserializedPeople[0];
      Person p2 = deserializedPeople[1];

      Console.WriteLine(p1.Name);
      // James
      Console.WriteLine(p2.Name);
      // James

      bool equal = Object.ReferenceEquals(p1, p2);
      // true
    }

  }
}
