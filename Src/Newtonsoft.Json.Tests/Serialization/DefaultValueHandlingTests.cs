using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

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
  }
}