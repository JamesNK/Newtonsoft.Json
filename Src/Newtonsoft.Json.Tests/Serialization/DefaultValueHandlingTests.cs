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
using System.ComponentModel;
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
  public class DefaultValueHandlingTests : TestFixtureBase
  {
    [Test]
    public void SerializeInvoice()
    {
      Invoice invoice = new Invoice
      {
        Company = "Acme Ltd.",
        Amount = 50.0m,
        Paid = false,
        FollowUpDays = 30,
        FollowUpEmailAddress = string.Empty,
        PaidDate = null
      };

      string included = JsonConvert.SerializeObject(invoice,
        Formatting.Indented,
        new JsonSerializerSettings { });

      // {
      //   "Company": "Acme Ltd.",
      //   "Amount": 50.0,
      //   "Paid": false,
      //   "PaidDate": null,
      //   "FollowUpDays": 30,
      //   "FollowUpEmailAddress": ""
      // }

      string ignored = JsonConvert.SerializeObject(invoice,
        Formatting.Indented,
        new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

      // {
      //   "Company": "Acme Ltd.",
      //   "Amount": 50.0
      // }

      Console.WriteLine(included);
      Console.WriteLine(ignored);
    }

    [Test]
    public void SerializeDefaultValueAttributeTest()
    {
      string json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass(),
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestField1"":0,""TestProperty1"":null}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = int.MinValue, TestProperty1 = "NotDefault" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestField1"":-2147483648,""TestProperty1"":""NotDefault""}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "NotDefault" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestProperty1"":""NotDefault""}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "TestProperty1Value" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{}", json);
    }

    [Test]
    public void DeserializeDefaultValueAttributeTest()
    {
      string json = "{}";

      DefaultValueAttributeTestClass c = JsonConvert.DeserializeObject<DefaultValueAttributeTestClass>(json, new JsonSerializerSettings
        {
          DefaultValueHandling = DefaultValueHandling.Populate
        });
      Assert.AreEqual("TestProperty1Value", c.TestProperty1);

      c = JsonConvert.DeserializeObject<DefaultValueAttributeTestClass>(json, new JsonSerializerSettings
      {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
      });
      Assert.AreEqual("TestProperty1Value", c.TestProperty1);
    }

    [JsonObject]
    public class NetworkUser
    {
      [JsonProperty(PropertyName = "userId")]
      [DefaultValue(-1)]
      public long GlobalId { get; set; }

      [JsonProperty(PropertyName = "age")]
      [DefaultValue(0)]
      public int Age { get; set; }

      [JsonProperty(PropertyName = "amount")]
      [DefaultValue(0.0)]
      public decimal Amount { get; set; }

      [JsonProperty(PropertyName = "floatUserId")]
      [DefaultValue(-1.0d)]
      public float FloatGlobalId { get; set; }

      [JsonProperty(PropertyName = "firstName")]
      public string Firstname { get; set; }
      [JsonProperty(PropertyName = "lastName")]
      public string Lastname { get; set; }

      public NetworkUser()
      {
        GlobalId = -1;
        FloatGlobalId = -1.0f;
        Amount = 0.0m;
        Age = 0;
      }
    }

    [Test]
    public void IgnoreNumberTypeDifferencesWithDefaultValue()
    {
      NetworkUser user = new NetworkUser
      {
        Firstname = "blub"
      };

      string json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

      Assert.AreEqual(@"{""firstName"":""blub""}", json);
    }

    [Test]
    public void ApproxEquals()
    {
      Assert.IsTrue(MathUtils.ApproxEquals(0.0, 0.0));
      Assert.IsTrue(MathUtils.ApproxEquals(1000.0, 1000.0000000000001));

      Assert.IsFalse(MathUtils.ApproxEquals(1000.0, 1000.000000000001));
      Assert.IsFalse(MathUtils.ApproxEquals(0.0, 0.00001));
    }
  }
}