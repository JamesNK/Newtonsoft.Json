using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
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
  }
}
