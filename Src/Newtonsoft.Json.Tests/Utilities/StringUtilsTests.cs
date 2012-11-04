using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Utilities
{
  [TestFixture]
  public class StringUtilsTests : TestFixtureBase
  {
    [Test]
    public void ToCamelCaseTest()
    {
      Assert.AreEqual("urlValue", StringUtils.ToCamelCase("URLValue"));
      Assert.AreEqual("url", StringUtils.ToCamelCase("URL"));
      Assert.AreEqual("id", StringUtils.ToCamelCase("ID"));
      Assert.AreEqual("i", StringUtils.ToCamelCase("I"));
      Assert.AreEqual("", StringUtils.ToCamelCase(""));
      Assert.AreEqual(null, StringUtils.ToCamelCase(null));
      Assert.AreEqual("iPhone", StringUtils.ToCamelCase("iPhone"));
      Assert.AreEqual("person", StringUtils.ToCamelCase("Person"));
      Assert.AreEqual("iPhone", StringUtils.ToCamelCase("IPhone"));
      Assert.AreEqual("i Phone", StringUtils.ToCamelCase("I Phone"));
      Assert.AreEqual(" IPhone", StringUtils.ToCamelCase(" IPhone"));
    }
  }
}