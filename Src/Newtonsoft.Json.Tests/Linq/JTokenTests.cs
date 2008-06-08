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
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JTokenTests : TestFixtureBase
  {
    [Test]
    public void Parent()
    {
      JArray v = new JArray(new JConstructor("TestConstructor"), new JValue(new DateTime(2000, 12, 20)));

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

      Assert.AreEqual((DateTime)((JValue)p.Value[1]).Value, (DateTime)((JValue)v[1]).Value);

      Assert.AreEqual(v, o["Test1"]);

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
    public void Next()
    {
      JArray a =
        new JArray(
          5,
          6,
          new JArray(7, 8),
          new JArray(9, 10)
        );

      JToken next = a[0].Next;
      Assert.AreEqual(6, (int)next);

      next = next.Next;
      Assert.IsTrue(JToken.DeepEquals(new JArray(7, 8), next));
 
      next = next.Next;
      Assert.IsTrue(JToken.DeepEquals(new JArray(9, 10), next));

      next = next.Next;
      Assert.IsNull(next);
    }

    [Test]
    public void Previous()
    {
      JArray a =
        new JArray(
          5,
          6,
          new JArray(7, 8),
          new JArray(9, 10)
        );

      JToken previous = a[3].Previous;
      Assert.IsTrue(JToken.DeepEquals(new JArray(7, 8), previous));

      previous = previous.Previous;
      Assert.AreEqual(6, (int)previous);

      previous = previous.Previous;
      Assert.AreEqual(5, (int)previous);

      previous = previous.Previous;
      Assert.IsNull(previous);
    }

    [Test]
    public void Children()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      Assert.AreEqual(4, a.Count());
      Assert.AreEqual(3, a.Children<JArray>().Count());
    }

    [Test]
    public void BeforeAfter()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1, 2, 3),
          new JArray(1, 2, 3),
          new JArray(1, 2, 3)
        );

      Assert.AreEqual(5, (int)a[1].Previous);
      Assert.AreEqual(2, a[2].BeforeSelf().Count());
      //Assert.AreEqual(2, a[2].AfterSelf().Count());
    }

    [Test]
    public void Casting()
    {
      Assert.AreEqual(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
      Assert.AreEqual(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
      Assert.AreEqual(true, (bool)new JValue(true));
      Assert.AreEqual(true, (bool?)new JValue(true));
      Assert.AreEqual(null, (bool?)((JValue)null));
      Assert.AreEqual(null, (bool?)new JValue((object)null));
      Assert.AreEqual(10, (long)new JValue(10));
      Assert.AreEqual(null, (long?)new JValue((long?)null));
      Assert.AreEqual(null, (long?)(JValue)null);
      Assert.AreEqual(null, (int?)new JValue((int?)null));
      Assert.AreEqual(null, (int?)(JValue)null);
      Assert.AreEqual(null, (DateTime?)new JValue((DateTime?)null));
      Assert.AreEqual(null, (DateTimeOffset?)new JValue((DateTimeOffset?)null));
      Assert.AreEqual(null, (DateTime?)(JValue)null);
      Assert.AreEqual(null, (DateTimeOffset?)(JValue)null);
      Assert.AreEqual(null, (short?)new JValue((short?)null));
      Assert.AreEqual(null, (short?)(JValue)null);
      Assert.AreEqual(null, (float?)new JValue((float?)null));
      Assert.AreEqual(null, (float?)(JValue)null);
      Assert.AreEqual(null, (double?)new JValue((double?)null));
      Assert.AreEqual(null, (double?)(JValue)null);
      Assert.AreEqual(null, (decimal?)new JValue((decimal?)null));
      Assert.AreEqual(null, (decimal?)(JValue)null);
      Assert.AreEqual(null, (uint?)new JValue((uint?)null));
      Assert.AreEqual(null, (uint?)(JValue)null);
      Assert.AreEqual(null, (sbyte?)new JValue((sbyte?)null));
      Assert.AreEqual(null, (sbyte?)(JValue)null);
      Assert.AreEqual(null, (ulong?)new JValue((ulong?)null));
      Assert.AreEqual(null, (ulong?)(JValue)null);
      Assert.AreEqual(null, (uint?)new JValue((uint?)null));
      Assert.AreEqual(null, (uint?)(JValue)null);
      Assert.AreEqual(11.1f, (float)new JValue(11.1));
      Assert.AreEqual(float.MinValue, (float)new JValue(float.MinValue));
      Assert.AreEqual(1.1, (double)new JValue(1.1));
      Assert.AreEqual(uint.MaxValue, (uint)new JValue(uint.MaxValue));
      Assert.AreEqual(ulong.MaxValue, (ulong)new JValue(ulong.MaxValue));
      Assert.AreEqual(ulong.MaxValue, (ulong)new JProperty("Test", new JValue(ulong.MaxValue)));
      Assert.AreEqual(null, (string)new JValue((string)null));
    }

    [Test]
    public void Root()
    {
      JArray a =
        new JArray(
          5,
          6,
          new JArray(7, 8),
          new JArray(9, 10)
        );

      Assert.AreEqual(a, a.Root);
      Assert.AreEqual(a, a[0].Root);
      Assert.AreEqual(a, ((JArray)a[2])[0].Root);
    }

    [Test]
    public void Remove()
    {
      JToken t;
      JArray a =
        new JArray(
          5,
          6,
          new JArray(7, 8),
          new JArray(9, 10)
        );

      a[0].Remove();

      Assert.AreEqual(6, (int)a[0]);

      a[1].Remove();

      Assert.AreEqual(6, (int)a[0]);
      Assert.IsTrue(JToken.DeepEquals(new JArray(9, 10), a[1]));
      Assert.AreEqual(2, a.Count());

      t = a[1];
      t.Remove();
      Assert.AreEqual(6, (int)a[0]);
      Assert.IsNull(t.Next);
      Assert.IsNull(t.Previous);
      Assert.IsNull(t.Parent);

      t = a[0];
      t.Remove();
      Assert.AreEqual(0, a.Count());

      Assert.IsNull(t.Next);
      Assert.IsNull(t.Previous);
      Assert.IsNull(t.Parent);
    }

    [Test]
    public void AfterSelf()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      JToken t = a[1];
      List<JToken> afterTokens = t.AfterSelf().ToList();

      Assert.AreEqual(2, afterTokens.Count);
      Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2), afterTokens[0]));
      Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), afterTokens[1]));
    }

    [Test]
    public void BeforeSelf()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      JToken t = a[2];
      List<JToken> beforeTokens = t.BeforeSelf().ToList();

      Assert.AreEqual(2, beforeTokens.Count);
      Assert.IsTrue(JToken.DeepEquals(new JValue(5), beforeTokens[0]));
      Assert.IsTrue(JToken.DeepEquals(new JArray(1), beforeTokens[1]));
    }

    [Test]
    public void HasValues()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      Assert.IsTrue(a.HasValues);
    }

    [Test]
    public void Ancestors()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      JToken t = a[1][0];
      List<JToken> ancestors = t.Ancestors().ToList();
      Assert.AreEqual(2, ancestors.Count());
      Assert.AreEqual(a[1], ancestors[0]);
      Assert.AreEqual(a, ancestors[1]);
    }

    [Test]
    public void Descendants()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      List<JToken> descendants = a.Descendants().ToList();
      Assert.AreEqual(10, descendants.Count());
      Assert.AreEqual(5, (int)descendants[0]);
      Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 4]));
      Assert.AreEqual(1, (int)descendants[descendants.Count - 3]);
      Assert.AreEqual(2, (int)descendants[descendants.Count - 2]);
      Assert.AreEqual(3, (int)descendants[descendants.Count - 1]);
    }

    [Test]
    public void CreateWriter()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      JsonWriter writer = a.CreateWriter();
      Assert.IsNotNull(writer);
      Assert.AreEqual(4, a.Count());

      writer.WriteValue("String");
      Assert.AreEqual(5, a.Count());
      Assert.AreEqual("String", (string)a[4]);

      writer.WriteStartObject();
      writer.WritePropertyName("Property");
      writer.WriteValue("PropertyValue");
      writer.WriteEnd();

      Assert.AreEqual(6, a.Count());
      Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
    }

    [Test]
    public void AddFirst()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      a.AddFirst("First");

      Assert.AreEqual("First", (string)a[0]);
      Assert.AreEqual(a, a[0].Parent);
      Assert.AreEqual(a[1], a[0].Next);
      Assert.AreEqual(5, a.Count());

      a.AddFirst("NewFirst");
      Assert.AreEqual("NewFirst", (string)a[0]);
      Assert.AreEqual(a, a[0].Parent);
      Assert.AreEqual(a[1], a[0].Next);
      Assert.AreEqual(6, a.Count());

      Assert.AreEqual(a[0], a[0].Next.Previous);
    }

    [Test]
    public void RemoveAll()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      JToken first = a.First;
      Assert.AreEqual(5, (int)first);

      a.RemoveAll();
      Assert.AreEqual(0, a.Count());

      Assert.IsNull(first.Parent);
      Assert.IsNull(first.Next);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JProperty to Newtonsoft.Json.Linq.JArray")]
    public void AddPropertyToArray()
    {
      JArray a = new JArray();
      a.Add(new JProperty("PropertyName"));
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject")]
    public void AddValueToObject()
    {
      JObject o = new JObject();
      o.Add(5);
    }

    [Test]
    public void Replace()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      a[0].Replace(new JValue(int.MaxValue));
      Assert.AreEqual(int.MaxValue, (int)a[0]);
      Assert.AreEqual(4, a.Count());

      a[1][0].Replace(new JValue("Test"));
      Assert.AreEqual("Test", (string)a[1][0]);

      a[2].Replace(new JValue(int.MaxValue));
      Assert.AreEqual(int.MaxValue, (int)a[2]);
      Assert.AreEqual(4, a.Count());

      Assert.IsTrue(JToken.DeepEquals(new JArray(int.MaxValue, new JArray("Test"), int.MaxValue, new JArray(1, 2, 3)), a));
    }
  }
}