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
using System.Diagnostics;
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
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
            SerializationEventTestObject[] objs = new[] { new SerializationEventTestObject(), new DerivedSerializationEventTestObject() };

            foreach (SerializationEventTestObject current in objs)
            {
                SerializationEventTestObject obj = current;

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

                string expectedError = String.Format("Error message for member Member6 = Error getting value from 'Member6' on '{0}'.", obj.GetType().FullName);
                Assert.AreEqual(expectedError, obj.Member5);

                JObject o = JObject.Parse(@"{
  ""Member1"": 11,
  ""Member2"": ""This value went into the data file during serialization."",
  ""Member4"": null
}");
                o["Member6"] = "Dummy text for error";

                obj = (SerializationEventTestObject)JsonConvert.DeserializeObject(o.ToString(), obj.GetType());

                Assert.AreEqual(11, obj.Member1);
                Assert.AreEqual("This value went into the data file during serialization.", obj.Member2);
                Assert.AreEqual("This value was set during deserialization", obj.Member3);
                Assert.AreEqual("This value was set after deserialization.", obj.Member4);

                expectedError = String.Format("Error message for member Member6 = Error setting value to 'Member6' on '{0}'.", obj.GetType());
                Assert.AreEqual(expectedError, obj.Member5);

                DerivedSerializationEventTestObject derivedObj = obj as DerivedSerializationEventTestObject;
                if (derivedObj != null) Assert.AreEqual("This value was set after deserialization.", derivedObj.Member7);
            }
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

#if !(NETFX_CORE || PORTABLE)
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

#if !PORTABLE
        public void WhenSerializationErrorDetectedBySerializer_ThenCallbackIsCalled()
        {
            // Verify contract is properly finding our callback
            var resolver = new DefaultContractResolver().ResolveContract(typeof(FooEvent));

#pragma warning disable 612,618
            Debug.Assert(resolver.OnError != null);
            Debug.Assert(resolver.OnError == typeof(FooEvent).GetMethod("OnError", BindingFlags.Instance | BindingFlags.NonPublic));
#pragma warning restore 612,618

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
#endif

        public class FooEvent
        {
            public int Identifier { get; set; }

            [OnError]
            private void OnError(StreamingContext context, ErrorContext error)
            {
                Identifier = 25;

                // Here we could for example manually copy the
                // persisted "Id" value into the renamed "Identifier"
                // property, etc.
                error.Handled = true;

                // We never get here :(
                Console.WriteLine("Error has been fixed");
            }
        }

        [Test]
        public void DerivedSerializationEvents()
        {
            var c = JsonConvert.DeserializeObject<DerivedSerializationEventOrderTestObject>("{}");

            JsonConvert.SerializeObject(c, Formatting.Indented);

            IList<string> e = c.GetEvents();

            Assert.AreEqual(@"OnDeserializing
OnDeserializing_Derived
OnDeserialized
OnDeserialized_Derived
OnSerializing
OnSerializing_Derived
OnSerialized
OnSerialized_Derived", string.Join(Environment.NewLine, e.ToArray()));
        }

        [Test]
        public void DerivedDerivedSerializationEvents()
        {
            var c = JsonConvert.DeserializeObject<DerivedDerivedSerializationEventOrderTestObject>("{}");

            JsonConvert.SerializeObject(c, Formatting.Indented);

            IList<string> e = c.GetEvents();

            Assert.AreEqual(@"OnDeserializing
OnDeserializing_Derived
OnDeserializing_Derived_Derived
OnDeserialized
OnDeserialized_Derived
OnDeserialized_Derived_Derived
OnSerializing
OnSerializing_Derived
OnSerializing_Derived_Derived
OnSerialized
OnSerialized_Derived
OnSerialized_Derived_Derived", string.Join(Environment.NewLine, e.ToArray()));
        }

#if !(NET20)
        [Test]
        public void DerivedDerivedSerializationEvents_DataContractSerializer()
        {
            string xml = @"<DerivedDerivedSerializationEventOrderTestObject xmlns=""http://schemas.datacontract.org/2004/07/Newtonsoft.Json.Tests.Serialization"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""/>";

            DataContractSerializer ss = new DataContractSerializer(typeof(DerivedDerivedSerializationEventOrderTestObject));

            DerivedDerivedSerializationEventOrderTestObject c = (DerivedDerivedSerializationEventOrderTestObject)ss.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

            MemoryStream ms = new MemoryStream();
            ss.WriteObject(ms, c);

            IList<string> e = c.GetEvents();

            Assert.AreEqual(@"OnDeserializing
OnDeserializing_Derived
OnDeserializing_Derived_Derived
OnDeserialized
OnDeserialized_Derived
OnDeserialized_Derived_Derived
OnSerializing
OnSerializing_Derived
OnSerializing_Derived_Derived
OnSerialized
OnSerialized_Derived
OnSerialized_Derived_Derived", string.Join(Environment.NewLine, e.ToArray()));
        }
#endif

        [Test]
        public void NoStreamingContextParameter()
        {
            ExportPostData d = new ExportPostData
            {
                user = "user!",
                contract = new Contract
                {
                    contractName = "name!"
                }
            };

            ExceptionAssert.Throws<JsonException>(
                "Serialization Callback 'Void Deserialized()' in type 'Newtonsoft.Json.Tests.Serialization.Contract' must have a single parameter of type 'System.Runtime.Serialization.StreamingContext'.",
                () => JsonConvert.SerializeObject(d, Formatting.Indented));
        }
    }

    public class SerializationEventOrderTestObject
    {
        protected IList<string> Events { get; private set; }

        public SerializationEventOrderTestObject()
        {
            Events = new List<string>();
        }

        public IList<string> GetEvents()
        {
            return Events;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing");
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized");
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing");
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized");
        }
    }

    public class DerivedSerializationEventOrderTestObject : SerializationEventOrderTestObject
    {
        [OnSerializing]
        internal new void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing_Derived");
        }

        [OnSerialized]
        internal new void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized_Derived");
        }

        [OnDeserializing]
        internal new void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing_Derived");
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized_Derived");
        }
    }

    public class DerivedDerivedSerializationEventOrderTestObject : DerivedSerializationEventOrderTestObject
    {
        [OnSerializing]
        internal new void OnSerializingMethod(StreamingContext context)
        {
            Events.Add("OnSerializing_Derived_Derived");
        }

        [OnSerialized]
        internal new void OnSerializedMethod(StreamingContext context)
        {
            Events.Add("OnSerialized_Derived_Derived");
        }

        [OnDeserializing]
        internal new void OnDeserializingMethod(StreamingContext context)
        {
            Events.Add("OnDeserializing_Derived_Derived");
        }

        [OnDeserialized]
        internal new void OnDeserializedMethod(StreamingContext context)
        {
            Events.Add("OnDeserialized_Derived_Derived");
        }
    }

    public class ExportPostData
    {
        public Contract contract { get; set; }
        public bool includeSubItems { get; set; }
        public string user { get; set; }
        public string[] projects { get; set; }
    }

    public class Contract
    {
        public string _id { get; set; }
        public string contractName { get; set; }
        public string contractNumber { get; set; }
        public string updatedBy { get; set; }
        public DateTime updated_at { get; set; }

        private bool _onDeserializedCalled;

        public bool GetOnDeserializedCalled()
        {
            return _onDeserializedCalled;
        }

        [OnDeserialized]
        internal void Deserialized()
        {
            _onDeserializedCalled = true;
        }
    }
}