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
using Newtonsoft.Json.Serialization;
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
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class CamelCasePropertyNamesContractResolverTests : TestFixtureBase
    {
        [Test]
        public void JsonConvertSerializerSettings()
        {
            Person person = new Person();
            person.BirthDate = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.LastModified = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.Name = "Name!";

            string json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            StringAssert.AreEqual(@"{
  ""name"": ""Name!"",
  ""birthDate"": ""2000-11-20T23:55:44Z"",
  ""lastModified"": ""2000-11-20T23:55:44Z""
}", json);

            Person deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            Assert.AreEqual(person.BirthDate, deserializedPerson.BirthDate);
            Assert.AreEqual(person.LastModified, deserializedPerson.LastModified);
            Assert.AreEqual(person.Name, deserializedPerson.Name);

            json = JsonConvert.SerializeObject(person, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
        }

        [Test]
        public void JTokenWriter()
        {
            JsonIgnoreAttributeOnClassTestClass ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass();
            ignoreAttributeOnClassTestClass.Field = int.MinValue;

            JsonSerializer serializer = new JsonSerializer();
            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

            JTokenWriter writer = new JTokenWriter();

            serializer.Serialize(writer, ignoreAttributeOnClassTestClass);

            JObject o = (JObject)writer.Token;
            JProperty p = o.Property("theField");

            Assert.IsNotNull(p);
            Assert.AreEqual(int.MinValue, (int)p.Value);

            string json = o.ToString();
        }

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
#pragma warning disable 618
        [Test]
        public void MemberSearchFlags()
        {
            PrivateMembersClass privateMembersClass = new PrivateMembersClass("PrivateString!", "InternalString!");

            string json = JsonConvert.SerializeObject(privateMembersClass, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver { DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance }
            });

            StringAssert.AreEqual(@"{
  ""_privateString"": ""PrivateString!"",
  ""i"": 0,
  ""_internalString"": ""InternalString!""
}", json);

            PrivateMembersClass deserializedPrivateMembersClass = JsonConvert.DeserializeObject<PrivateMembersClass>(@"{
  ""_privateString"": ""Private!"",
  ""i"": -2,
  ""_internalString"": ""Internal!""
}", new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver { DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance }
            });

            Assert.AreEqual("Private!", ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("_privateString", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));
            Assert.AreEqual("Internal!", ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("_internalString", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));

            // readonly
            Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("i", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));
        }
#pragma warning restore 618
#endif

        [Test]
        public void BlogPostExample()
        {
            Product product = new Product
            {
                ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                Name = "Widget",
                Price = 9.99m,
                Sizes = new[] { "Small", "Medium", "Large" }
            };

            string json =
                JsonConvert.SerializeObject(
                    product,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                    );

            //{
            //  "name": "Widget",
            //  "expiryDate": "\/Date(1292868060000)\/",
            //  "price": 9.99,
            //  "sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            StringAssert.AreEqual(@"{
  ""name"": ""Widget"",
  ""expiryDate"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
        }

#if !(NET35 || NET20 || PORTABLE40)
        [Test]
        public void DynamicCamelCasePropertyNames()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Integer = int.MaxValue;

            string json = JsonConvert.SerializeObject(o, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            StringAssert.AreEqual(@"{
  ""explicit"": false,
  ""text"": ""Text!"",
  ""integer"": 2147483647,
  ""int"": 0,
  ""childObject"": null
}", json);
        }
#endif

        [Test]
        public void DictionaryCamelCasePropertyNames()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "First", "Value1!" },
                { "Second", "Value2!" }
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            StringAssert.AreEqual(@"{
  ""first"": ""Value1!"",
  ""second"": ""Value2!""
}", json);
        }
    }
}