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

#if !(NET20 || NET35 || NET40 || PORTABLE40 || PORTABLE)

using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
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
using System.IO;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class TypeNameHandlingAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task NestedValueObjectsAsync()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                sb.Append(@"{""$value"":");
            }

            await ExceptionAssert.ThrowsAsync<JsonSerializationException>(async () =>
            {
                var reader = new JsonTextReader(new StringReader(sb.ToString()));
                var ser = new JsonSerializer();
                ser.MetadataPropertyHandling = MetadataPropertyHandling.Default;
                await ser.DeserializeAsync<sbyte>(reader);
            }, "Unexpected token when deserializing primitive value: StartObject. Path '$value', line 1, position 11.");
        }

        [Test]
        public async Task SerializeRootTypeNameIfDerivedWithAutoAsync()
        {
            var serializer = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var sw = new StringWriter();
            await serializer.SerializeAsync(new JsonTextWriter(sw) { Formatting = Formatting.Indented }, new WagePerson(), typeof(Person));
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
                var person = await serializer.DeserializeAsync<Person>(rd);

                CustomAssert.IsInstanceOfType(typeof(WagePerson), person);
            }
        }

        public class Wrapper
        {
            public IList<EmployeeReference> Array { get; set; }
            public IDictionary<string, EmployeeReference> Dictionary { get; set; }
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

        public class TypeNameProperty
        {
            public string Name { get; set; }

            [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
            public object Value { get; set; }
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
        public async Task CollectionWithAbstractItemsAsync()
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
                await serializingTester.SerializeAsync(jsonWriter, testObject);
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

                anotherTestObject = await deserializingTester.DeserializeAsync<HolderClass>(jsonReader);
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

        public class UrlStatus
        {
            public int Status { get; set; }
            public string Url { get; set; }
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

#if !DNXCORE50
        [Test]
        public async Task ISerializableTypeNameHandlingTestAsync()
        {
            IExample e = new Example("Rob");

            SerializableWrapper w = new SerializableWrapper
            {
                Content = e
            };

            await TestJsonSerializationRoundTripAsync(w, TypeNameHandling.All);
            await TestJsonSerializationRoundTripAsync(w, TypeNameHandling.Auto);
            await TestJsonSerializationRoundTripAsync(w, TypeNameHandling.Objects);
        }

        private async Task TestJsonSerializationRoundTripAsync(SerializableWrapper e, TypeNameHandling flag)
        {
            StringWriter writer = new StringWriter();

            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = flag;

            await serializer.SerializeAsync(new JsonTextWriter(writer), e);

            SerializableWrapper f = await serializer.DeserializeAsync<SerializableWrapper>(new JsonTextReader(new StringReader(writer.ToString())));

            Assert.AreEqual(e, f, "Objects should be equal after round trip json serialization");
        }
#endif
    }
}

#endif
