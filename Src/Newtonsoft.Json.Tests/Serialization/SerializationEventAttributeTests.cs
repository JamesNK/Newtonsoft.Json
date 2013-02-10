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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
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
      Assert.AreEqual("Error message for member Member6 = Error getting value from 'Member6' on 'Newtonsoft.Json.Tests.TestObjects.SerializationEventTestObject'.", obj.Member5);

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
      Assert.AreEqual("Error message for member Member6 = Error setting value to 'Member6' on 'Newtonsoft.Json.Tests.TestObjects.SerializationEventTestObject'.", obj.Member5);
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

    public class SerializationEventBaseTestObject
    {
      public string TestMember { get; set; }

      [OnSerializing]
      internal void OnSerializingMethod(StreamingContext context)
      {
        TestMember = "Set!";
      }
    }

    public class SerializationEventContextSubClassTestObject : SerializationEventBaseTestObject
    {
    }

    [Test]
    public void SerializationEventContextTestObjectSubClassTest()
    {
      SerializationEventContextSubClassTestObject obj = new SerializationEventContextSubClassTestObject();

      string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
      Assert.AreEqual(@"{
  ""TestMember"": ""Set!""
}", json);
    }

#if !SILVERLIGHT && !NETFX_CORE
    public class SerializationEventContextTestObject
    {
      public string TestMember { get; set; }

      [OnSerializing]
      internal void OnSerializingMethod(StreamingContext context)
      {
        TestMember = context.State + " " + context.Context;
      }
    }

    [Test]
    public void SerializationEventContextTest()
    {
      SerializationEventContextTestObject value = new SerializationEventContextTestObject();

      string json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
                                                                              {
                                                                                Context =
                                                                                  new StreamingContext(
                                                                                  StreamingContextStates.Remoting,
                                                                                  "ContextValue")
                                                                              });

      Assert.AreEqual(@"{
  ""TestMember"": ""Remoting ContextValue""
}", json);
    }
#endif

    public void WhenSerializationErrorDetectedBySerializer_ThenCallbackIsCalled()
    {
      // Verify contract is properly finding our callback
      var resolver = new DefaultContractResolver().ResolveContract(typeof(FooEvent));

      Debug.Assert(resolver.OnError != null);
      Debug.Assert(resolver.OnError == typeof(FooEvent).GetMethod("OnError", BindingFlags.Instance | BindingFlags.NonPublic));

      var serializer = JsonSerializer.Create(new JsonSerializerSettings
      {
        // If I don't specify Error here, the callback isn't called
        // either, but no exception is thrown.
        MissingMemberHandling = MissingMemberHandling.Error,
      });

      // This throws with missing member exception, rather than calling my callback.
      var foo = serializer.Deserialize<FooEvent>(new JsonTextReader(new StringReader("{ Id: 25 }")));

      // When fixed, this would pass.
      Debug.Assert(foo.Identifier == 25);
    }

    public class FooEvent
    {
      public int Identifier { get; set; }

      [OnError]
      private void OnError(StreamingContext context, ErrorContext error)
      {
        this.Identifier = 25;

        // Here we could for example manually copy the
        // persisted "Id" value into the renamed "Identifier"
        // property, etc.
        error.Handled = true;

        // We never get here :(
        Console.WriteLine("Error has been fixed");
      }
    }
  }
}