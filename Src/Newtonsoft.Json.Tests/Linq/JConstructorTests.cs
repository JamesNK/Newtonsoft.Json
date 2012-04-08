using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.Linq
{
  [TestFixture]
  public class JConstructorTests : TestFixtureBase
  {
    [Test]
    public void Load()
    {
      JsonReader reader = new JsonTextReader(new StringReader("new Date(123)"));
      reader.Read();

      JConstructor constructor = JConstructor.Load(reader);
      Assert.AreEqual("Date", constructor.Name);
      Assert.IsTrue(JToken.DeepEquals(new JValue(123), constructor.Values().ElementAt(0)));
    }

    [Test]
    public void CreateWithMultiValue()
    {
      JConstructor constructor = new JConstructor("Test", new List<int> { 1, 2, 3 });
      Assert.AreEqual("Test", constructor.Name);
      Assert.AreEqual(3, constructor.Children().Count());
      Assert.AreEqual(1, (int)constructor.Children().ElementAt(0));
      Assert.AreEqual(2, (int)constructor.Children().ElementAt(1));
      Assert.AreEqual(3, (int)constructor.Children().ElementAt(2));
    }

    [Test]
    public void Iterate()
    {
      JConstructor c = new JConstructor("MrConstructor", 1, 2, 3, 4, 5);

      int i = 1;
      foreach (JToken token in c)
      {
        Assert.AreEqual(i, (int)token);
        i++;
      }
    }

    [Test]
    public void SetValueWithInvalidIndex()
    {
      ExceptionAssert.Throws<ArgumentException>(@"Set JConstructor values with invalid key value: ""badvalue"". Argument position index expected.",
      () =>
      {
        JConstructor c = new JConstructor();
        c["badvalue"] = new JValue(3);
      }); 
    }

    [Test]
    public void SetValue()
    {
      object key = 0;

      JConstructor c = new JConstructor();
      c.Name = "con";
      c.Add(null);
      c[key] = new JValue(3);

      Assert.AreEqual(3, (int)c[key]);
    }
  }
}
