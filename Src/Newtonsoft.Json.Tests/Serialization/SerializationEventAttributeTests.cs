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

#if !PocketPC && !SILVERLIGHT && !NET20
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Collections;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class SerializationEventAttributeTests : TestFixtureBase
  {
    [Test]
    public void ObjectEvents()
    {
      SerializationEventTestObject obj = new SerializationEventTestObject();

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);
      Assert.AreEqual(null, obj.Member5);

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}", json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value was reset after serialization.", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);
      Assert.AreEqual("Error message for member Member6 = Exception has been thrown by the target of an invocation.", obj.Member5);

      JObject o = JObject.Parse(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}");
      o["Member6"] = "Dummy text for error";

      obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(o.ToString());

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value went into the data file during serialization.", obj.Member2);
      Assert.AreEqual("This value was set during deserialization", obj.Member3);
      Assert.AreEqual("This value was set after deserialization.", obj.Member4);
      Assert.AreEqual("Error message for member Member6 = Exception has been thrown by the target of an invocation.", obj.Member5);
    }

    [Test]
    public void ObjectWithConstructorEvents()
    {
      SerializationEventTestObjectWithConstructor obj = new SerializationEventTestObjectWithConstructor(11, "Hello World!", null);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}", json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value was reset after serialization.", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      obj = JsonConvert.DeserializeObject<SerializationEventTestObjectWithConstructor>(json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value went into the data file during serialization.", obj.Member2);
      Assert.AreEqual("This value was set during deserialization", obj.Member3);
      Assert.AreEqual("This value was set after deserialization.", obj.Member4);
    }

    public class ListErrorObject
    {
      public string Member { get; set; }

      private string _throwError;
      public string ThrowError
      {
        get
        {
          if (_throwError != null)
            return _throwError;

          throw new Exception("ListErrorObject.ThrowError get error!");
        }
        set
        {
          if (value != null && value.StartsWith("Handle"))
          {
            _throwError = value;
            return;
          }

          throw new Exception("ListErrorObject.ThrowError set error!");
        }
      }

      public string Member2 { get; set; }
    }

    public class ListErrorObjectCollection : Collection<ListErrorObject>
    {
      [OnError]
      internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
      {
        errorContext.Handled = true;
      }
    }

    public class DateTimeErrorObjectCollection : Collection<DateTime>
    {
      [OnError]
      internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
      {
        errorContext.Handled = true;
      }
    }

    public class VersionKeyedCollection : KeyedCollection<string, Person>, IList
    {
      public List<string> Messages { get; set; }

      public VersionKeyedCollection()
      {
        Messages = new List<string>();
      }

      protected override string GetKeyForItem(Person item)
      {
        return item.Name;
      }

      [OnError]
      internal void OnErrorMethod(StreamingContext context, ErrorContext errorContext)
      {
        Messages.Add("Error message for member " + errorContext.Member + " = " + errorContext.Error.Message);
        errorContext.Handled = true;
      }

      object IList.this[int index]
      {
        get
        {
          if (index % 2 == 0)
            throw new Exception("Index even: " + index);

          return this[index];
        }
        set { this[index] = (Person)value; }
      }
    }

    [Test]
    public void ErrorDeserializingListHandled()
    {
      string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

      VersionKeyedCollection c = JsonConvert.DeserializeObject<VersionKeyedCollection>(json);
      Assert.AreEqual(1, c.Count);
      Assert.AreEqual(1, c.Messages.Count);
      Assert.AreEqual("Error message for member 1 = An item with the same key has already been added.", c.Messages[0]);
    }

    [Test]
    public void ErrorSerializingListHandled()
    {
      VersionKeyedCollection c = new VersionKeyedCollection();
      c.Add(new Person
      {
        Name = "Jim",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jimbo",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jimmy",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jim Bean",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"[
  {
    ""Name"": ""Jimbo"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim Bean"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]", json);

      Assert.AreEqual(2, c.Messages.Count);
      Assert.AreEqual("Error message for member 0 = Index even: 0", c.Messages[0]);
      Assert.AreEqual("Error message for member 2 = Index even: 2", c.Messages[1]);
    }

    [Test]
    public void DeserializingErrorInChildObject()
    {
      ListErrorObjectCollection c = JsonConvert.DeserializeObject<ListErrorObjectCollection>(@"[
  {
    ""Member"": ""Value1"",
    ""Member2"": null
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""ThrowError"": ""Value"",
    ""Object"": {
      ""Array"": [
        1,
        2
      ]
    }
  },
  {
    ""ThrowError"": ""Handle this!"",
    ""Member"": ""Value3""
  }
]");

      Assert.AreEqual(3, c.Count);
      Assert.AreEqual("Value1", c[0].Member);
      Assert.AreEqual("Value2", c[1].Member);
      Assert.AreEqual("Value3", c[2].Member);
      Assert.AreEqual("Handle this!", c[2].ThrowError);
    }

    [Test]
    public void SerializingErrorInChildObject()
    {
      ListErrorObjectCollection c = new ListErrorObjectCollection
        {
          new ListErrorObject
            {
              Member = "Value1",
              ThrowError = "Handle this!",
              Member2 = "Member1"
            },
          new ListErrorObject
            {
              Member = "Value2",
              Member2 = "Member2"
            },
          new ListErrorObject
            {
              Member = "Value3",
              ThrowError = "Handle that!",
              Member2 = "Member3"
            }
        };

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"[
  {
    ""Member"": ""Value1"",
    ""ThrowError"": ""Handle this!"",
    ""Member2"": ""Member1""
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""Member"": ""Value3"",
    ""ThrowError"": ""Handle that!"",
    ""Member2"": ""Member3""
  }
]", json);
    }

    [Test]
    public void DeserializingErrorInDateTimeCollection()
    {
      DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(@"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]", new IsoDateTimeConverter());

      Assert.AreEqual(3, c.Count);
      Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
      Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
      Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);
    }

    [Test]
    public void ListEvents()
    {
      SerializationEventTestList obj = new SerializationEventTestList
        {
          1.1m,
          2.222222222m,
          int.MaxValue,
          Convert.ToDecimal(Math.PI)
        };

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      Assert.AreEqual(@"[
  -1.0,
  1.1,
  2.222222222,
  2147483647.0,
  3.14159265358979
]", json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value was reset after serialization.", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      obj = JsonConvert.DeserializeObject<SerializationEventTestList>(json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This value was set during deserialization", obj.Member3);
      Assert.AreEqual("This value was set after deserialization.", obj.Member4);
    }

    [Test]
    public void DictionaryEvents()
    {
      SerializationEventTestDictionary obj = new SerializationEventTestDictionary
        {
          { 1.1m, "first" },
          { 2.222222222m, "second" },
          { int.MaxValue, "third" },
          { Convert.ToDecimal(Math.PI), "fourth" }
        };

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      Assert.AreEqual(@"{
  ""1.1"": ""first"",
  ""2.222222222"": ""second"",
  ""2147483647"": ""third"",
  ""3.14159265358979"": ""fourth"",
  ""79228162514264337593543950335"": ""Inserted on serializing""
}", json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("This value was reset after serialization.", obj.Member2);
      Assert.AreEqual("This is a nonserialized value", obj.Member3);
      Assert.AreEqual(null, obj.Member4);

      obj = JsonConvert.DeserializeObject<SerializationEventTestDictionary>(json);

      Assert.AreEqual(11, obj.Member1);
      Assert.AreEqual("Hello World!", obj.Member2);
      Assert.AreEqual("This value was set during deserialization", obj.Member3);
      Assert.AreEqual("This value was set after deserialization.", obj.Member4);
    }

    [Test]
    public void ObjectEventsDocumentationExample()
    {
      SerializationEventTestObject obj = new SerializationEventTestObject();

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // Hello World!
      Console.WriteLine(obj.Member3);
      // This is a nonserialized value
      Console.WriteLine(obj.Member4);
      // null
      Console.WriteLine(obj.Member5);
      // null

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      // {
      //   "Member1": 11,
      //   "Member2": "This value went into the data file during serialization.",
      //   "Member4": null
      // }

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // This value was reset after serialization.
      Console.WriteLine(obj.Member3);
      // This is a nonserialized value
      Console.WriteLine(obj.Member4);
      // null
      Console.WriteLine(obj.Member5);
      // Error message for member Member6 = Exception has been thrown by the target of an invocation.

      obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

      Console.WriteLine(obj.Member1);
      // 11
      Console.WriteLine(obj.Member2);
      // This value went into the data file during serialization.
      Console.WriteLine(obj.Member3);
      // This value was set during deserialization
      Console.WriteLine(obj.Member4);
      // This value was set after deserialization.
    }
  }
}
#endif