using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
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
    public void DefaultValueAttributeTest()
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
  }
}