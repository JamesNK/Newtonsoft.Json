using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace Newtonsoft.Json.Tests
{
  public class JTokenTests : TestFixtureBase
  {
    [Test]
    public void Parent()
    {
      JValue v = new JValue(new DateTime(2000, 12, 20));

      Assert.AreEqual(null, v.Parent);

      JObject o =
        new JObject(
          new JProperty("Test1", v),
          new JProperty("Test2", "Test2Value"),
          new JProperty("Test3", "Test3Value"),
          new JProperty("Test4", null)
        );

      Assert.AreEqual(o.Property("Test1"), v.Parent);

      JProperty p = new JProperty("NewProperty", v);

      // existing value should still have same parent
      Assert.AreEqual(o.Property("Test1"), v.Parent);

      // new value should be cloned
      Assert.AreNotEqual(p.Value, v);

      Assert.AreEqual((DateTime)p.Value, (DateTime)v.Value);

      Assert.AreEqual(v, o["Test1"]);

      XText t = new XText("XText");
      Assert.AreEqual(null, t.Parent);

      Assert.AreEqual(null, o.Parent);
      JProperty o1 = new JProperty("O1", o);
      Assert.AreEqual(o, o1.Value);

      Assert.AreNotEqual(null, o.Parent);
      JProperty o2 = new JProperty("O2", o);

      Assert.AreNotEqual(o1.Value, o2.Value);
      Assert.AreEqual(o1.Value.Children().Count(), o2.Value.Children().Count());
      Assert.AreEqual(false, JToken.DeepEquals(o1, o2));
      Assert.AreEqual(true, JToken.DeepEquals(o1.Value, o2.Value));
    }

    [Test]
    public void Children()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1, 2, 3),
          new JArray(1, 2, 3),
          new JArray(1, 2, 3)
        );

      Assert.AreEqual(4, a.Count());
      Assert.AreEqual(3, a.Children<JArray>().Count());
    }

    [Test]
    public void Casting()
    {
      //JToken t = new JValue(new DateTime(2000, 12, 20));
      //DateTime d = (DateTime)t;

      Assert.AreEqual(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
      Assert.AreEqual(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
      Assert.AreEqual(true, (bool)new JValue(true));
      Assert.AreEqual(true, (bool?)new JValue(true));
      Assert.AreEqual(null, (bool?)((JValue)null));
      Assert.AreEqual(null, (bool?)JValue.Null);
      Assert.AreEqual(null, (bool?)JValue.Undefined);
    }
  }
}