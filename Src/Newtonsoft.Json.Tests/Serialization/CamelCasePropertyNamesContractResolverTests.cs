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
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
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

      Assert.AreEqual(@"{
  ""name"": ""Name!"",
  ""birthDate"": ""\/Date(974764544000)\/"",
  ""lastModified"": ""\/Date(974764544000)\/""
}", json);

      Person deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
                                                                        {
                                                                          ContractResolver = new CamelCasePropertyNamesContractResolver()
                                                                        });

      Assert.AreEqual(person.BirthDate, deserializedPerson.BirthDate);
      Assert.AreEqual(person.LastModified, deserializedPerson.LastModified);
      Assert.AreEqual(person.Name, deserializedPerson.Name);

      json = JsonConvert.SerializeObject(person, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""\/Date(974764544000)\/"",
  ""LastModified"": ""\/Date(974764544000)\/""
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

      JObject o = (JObject) writer.Token;
      JProperty p = o.Property("theField");

      Assert.IsNotNull(p);
      Assert.AreEqual(int.MinValue, (int)p.Value);

      string json = o.ToString();
    }

    [Test]
    public void MemberSearchFlags()
    {
      PrivateMembersClass privateMembersClass = new PrivateMembersClass("PrivateString!", "InternalString!");

      string json = JsonConvert.SerializeObject(privateMembersClass, Formatting.Indented, new JsonSerializerSettings
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver { DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance }
      });

      Assert.AreEqual(@"{
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

    [Test]
    public void BlogPostExample()
    {
      Product product = new Product
                          {
                            ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                            Name = "Widget",
                            Price = 9.99m,
                            Sizes = new[] {"Small", "Medium", "Large"}
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

      Assert.AreEqual(@"{
  ""name"": ""Widget"",
  ""expiryDate"": ""\/Date(1292868060000)\/"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
    }
  }
}