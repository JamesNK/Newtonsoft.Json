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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Utilities;
using System.Net;
using System.Runtime.Serialization;
using System.IO;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class TypeNameHandlingTests : TestFixtureBase
  {
    [Test]
    public void WriteTypeNameForObjects()
    {
      string employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), FormatterAssemblyStyle.Simple);

      EmployeeReference employee = new EmployeeReference();

      string json = JsonConvert.SerializeObject(employee, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });

      Assert.AreEqual(@"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": null,
  ""Manager"": null
}", json);
    }

    [Test]
    public void DeserializeTypeName()
    {
      string employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), FormatterAssemblyStyle.Simple);

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

      Assert.IsInstanceOfType(typeof(EmployeeReference), employee);
      Assert.AreEqual("Name!", ((EmployeeReference)employee).Name);
    }

#if !SILVERLIGHT && !PocketPC
    [Test]
    public void DeserializeTypeNameFromGacAssembly()
    {
      string cookieRef = ReflectionUtils.GetTypeName(typeof(Cookie), FormatterAssemblyStyle.Simple);

      string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + cookieRef + @"""
}";

      object cookie = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });

      Assert.IsInstanceOfType(typeof(Cookie), cookie);
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
              Manager = new EmployeeReference {Name = "Frank"}
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
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
      });

      Assert.AreEqual(@"[
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
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
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
      Assert.AreEqual(int.MinValue, values[3]);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException))]
    public void DeserializeWithBadTypeName()
    {
      string employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

      string json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      JsonConvert.DeserializeObject(json, typeof(Person), new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects,
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
      });
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

      Assert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Manager"": null
}", o.ToString());
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Type specified in JSON 'Newtonsoft.Json.Tests.TestObjects.Employee' was not resolved.")]
    public void DeserializeTypeNameOnly()
    {
      string json = @"{
  ""$id"": ""1"",
  ""$type"": ""Newtonsoft.Json.Tests.TestObjects.Employee"",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

      JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.Objects
      });
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
          TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
        });

      Assert.IsInstanceOfType(typeof(SendHttpRequest), message);

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
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
              });

      Assert.AreEqual(@"{
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
      string typeNamePropertyRef = ReflectionUtils.GetTypeName(typeof(TypeNameProperty), FormatterAssemblyStyle.Simple);

      TypeNameProperty typeNameProperty = new TypeNameProperty
                                            {
                                              Name = "Name!",
                                              Value = new TypeNameProperty
                                                        {
                                                          Name = "Nested!"
                                                        }
                                            };

      string json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + typeNamePropertyRef + @""",
    ""Name"": ""Nested!"",
    ""Value"": null
  }
}", json);

      TypeNameProperty deserialized = JsonConvert.DeserializeObject<TypeNameProperty>(json);
      Assert.AreEqual("Name!", deserialized.Name);
      Assert.IsInstanceOfType(typeof(TypeNameProperty), deserialized.Value);

      TypeNameProperty nested = (TypeNameProperty)deserialized.Value;
      Assert.AreEqual("Nested!", nested.Name);
      Assert.AreEqual(null, nested.Value);
    }

    [Test]
    public void WriteListTypeNameForProperty()
    {
      string listRef = ReflectionUtils.GetTypeName(typeof(List<int>), FormatterAssemblyStyle.Simple);

      TypeNameProperty typeNameProperty = new TypeNameProperty
      {
        Name = "Name!",
        Value = new List<int> { 1, 2, 3, 4, 5 }
      };

      string json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

      Assert.AreEqual(@"{
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
      Assert.IsInstanceOfType(typeof(List<int>), deserialized.Value);

      List<int> nested = (List<int>)deserialized.Value;
      Assert.AreEqual(5, nested.Count);
      Assert.AreEqual(1, nested[0]);
      Assert.AreEqual(2, nested[1]);
      Assert.AreEqual(3, nested[2]);
      Assert.AreEqual(4, nested[3]);
      Assert.AreEqual(5, nested[4]);
    }

#if !SILVERLIGHT && !PocketPC
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
        Binder = new CustomSerializationBinder()
      });

      Assert.IsInstanceOfType(typeof(Person), p);

      Person person = (Person)p;

      Assert.AreEqual("Name!", person.Name);
    }

    public class CustomSerializationBinder : SerializationBinder
    {
      public override Type BindToType(string assemblyName, string typeName)
      {
        return typeof (Person);
      }
    }
#endif

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

      string contentSubClassRef = ReflectionUtils.GetTypeName(typeof(ContentSubClass), FormatterAssemblyStyle.Simple);
      string dictionaryRef = ReflectionUtils.GetTypeName(typeof(Dictionary<int, IList<ContentBaseClass>>), FormatterAssemblyStyle.Simple);
      string listRef = ReflectionUtils.GetTypeName(typeof(List<ContentBaseClass>), FormatterAssemblyStyle.Simple);


      Assert.AreEqual(@"{
  ""TestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""First One""
  },
  ""AnotherTestMember"": {
    ""$type"": """ + dictionaryRef + @""",
    ""1"": {
      ""$type"": """ + listRef + @""",
      ""$values"": [
        {
          ""$type"": """ + contentSubClassRef + @""",
          ""SomeString"": ""Second One""
        }
      ]
    }
  },
  ""AThirdTestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""Third One""
  }
}", json);
      Console.WriteLine(json);

      StringReader sr = new StringReader(json);

      JsonSerializer deserializingTester = new JsonSerializer();

      HolderClass anotherTestObject;

      using (JsonTextReader jsonReader = new JsonTextReader(sr))
      {
        deserializingTester.TypeNameHandling = TypeNameHandling.Auto;

        anotherTestObject = deserializingTester.Deserialize<HolderClass>(jsonReader);
      }

      Assert.IsNotNull(anotherTestObject);
      Assert.IsInstanceOfType(typeof(ContentSubClass), anotherTestObject.TestMember);
      Assert.IsInstanceOfType(typeof(Dictionary<int, IList<ContentBaseClass>>), anotherTestObject.AnotherTestMember);
      Assert.AreEqual(1, anotherTestObject.AnotherTestMember.Count);

      IList<ContentBaseClass> list = anotherTestObject.AnotherTestMember[1];

      Assert.IsInstanceOfType(typeof(List<ContentBaseClass>), list);
      Assert.AreEqual(1, list.Count);
      Assert.IsInstanceOfType(typeof(ContentSubClass), list[0]);
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

      SearchDetails searchDetails = (SearchDetails) deserialized.Body;
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
          {"First", new UrlStatus{ Status = 404, Url = @"http://www.bing.com"}},
          {"Second", new UrlStatus{Status = 400, Url = @"http://www.google.com"}},
          {"List", new List<UrlStatus>
            {
              new UrlStatus {Status = 300, Url = @"http://www.yahoo.com"},
              new UrlStatus {Status = 200, Url = @"http://www.askjeeves.com"}
            }
          }
        };

      string json = JsonConvert.SerializeObject(collection, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.All,
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
      });

      string urlStatusTypeName = ReflectionUtils.GetTypeName(typeof (UrlStatus), FormatterAssemblyStyle.Simple);

      Assert.AreEqual(@"{
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
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
      });

      Assert.IsInstanceOfType(typeof(Dictionary<string, object>), c);

      Dictionary<string, object> newCollection = (Dictionary<string, object>)c;
      Assert.AreEqual(3, newCollection.Count);
      Assert.AreEqual(@"http://www.bing.com", ((UrlStatus)newCollection["First"]).Url);

      List<UrlStatus> statues = (List<UrlStatus>) newCollection["List"];
      Assert.AreEqual(2, statues.Count);
    }


    [Test]
    public void SerializingIEnumerableOfTShouldRetainGenericTypeInfo()
    {
      string productClassRef = ReflectionUtils.GetTypeName(typeof(Product[]), FormatterAssemblyStyle.Simple);

      CustomEnumerable<Product> products = new CustomEnumerable<Product>();

      string json = JsonConvert.SerializeObject(products, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

      Assert.AreEqual(@"{
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
          yield break;
        yield return value;

        var nextInLine = next;
        while (nextInLine != null)
        {
          if (nextInLine.count != 0)
            yield return nextInLine.value;
          nextInLine = nextInLine.next;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }

  }

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
}