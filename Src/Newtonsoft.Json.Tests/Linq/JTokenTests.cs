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
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JTokenTests : TestFixtureBase
  {
    [Test]
    public void ReadFrom()
    {
      JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(new StringReader("{'pie':true}")));
      Assert.AreEqual(true, (bool)o["pie"]);

      JArray a = (JArray)JToken.ReadFrom(new JsonTextReader(new StringReader("[1,2,3]")));
      Assert.AreEqual(1, (int)a[0]);
      Assert.AreEqual(2, (int)a[1]);
      Assert.AreEqual(3, (int)a[2]);

      JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
      reader.Read();
      reader.Read();

      JProperty p = (JProperty)JToken.ReadFrom(reader);
      Assert.AreEqual("pie", p.Name);
      Assert.AreEqual(true, (bool)p.Value);

      JConstructor c = (JConstructor)JToken.ReadFrom(new JsonTextReader(new StringReader("new Date(1)")));
      Assert.AreEqual("Date", c.Name);
      Assert.IsTrue(JToken.DeepEquals(new JValue(1), c.Values().ElementAt(0)));

      JValue v;

      v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""stringvalue""")));
      Assert.AreEqual("stringvalue", (string)v);

      v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1")));
      Assert.AreEqual(1, (int)v);

      v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1.1")));
      Assert.AreEqual(1.1, (double)v);
    }

    [Test]
    public void Load()
    {
      JObject o = (JObject)JToken.Load(new JsonTextReader(new StringReader("{'pie':true}")));
      Assert.AreEqual(true, (bool)o["pie"]);
    }

    [Test]
    public void Parse()
    {
      JObject o = (JObject)JToken.Parse("{'pie':true}");
      Assert.AreEqual(true, (bool)o["pie"]);
    }

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
      Assert.AreNotSame(p.Value, v);

      Assert.AreEqual((DateTime)((JValue)p.Value[1]).Value, (DateTime)((JValue)v[1]).Value);

      Assert.AreEqual(v, o["Test1"]);

      Assert.AreEqual(null, o.Parent);
      JProperty o1 = new JProperty("O1", o);
      Assert.AreEqual(o, o1.Value);

      Assert.AreNotEqual(null, o.Parent);
      JProperty o2 = new JProperty("O2", o);

      Assert.AreNotSame(o1.Value, o2.Value);
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
      Assert.AreEqual(1L, (long)(new JValue(1)));
      Assert.AreEqual(2L, (long) new JArray(1, 2, 3)[1]);

      Assert.AreEqual(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
#if !PocketPC && !NET20
      Assert.AreEqual(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
      Assert.AreEqual(null, (DateTimeOffset?)new JValue((DateTimeOffset?)null));
      Assert.AreEqual(null, (DateTimeOffset?)(JValue)null);
#endif
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
      Assert.AreEqual(null, (DateTime?)(JValue)null);
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
      Assert.AreEqual(5m, (decimal)(new JValue(5L)));
      Assert.AreEqual(5m, (decimal?)(new JValue(5L)));
      Assert.AreEqual(5f, (float)(new JValue(5L)));
      Assert.AreEqual(5f, (float)(new JValue(5m)));
      Assert.AreEqual(5f, (float?)(new JValue(5m)));

      byte[] data = new byte[0];
      Assert.AreEqual(data, (byte[])(new JValue(data)));

      Assert.AreEqual(5, (int)(new JValue(StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void ImplicitCastingTo()
    {
      Assert.IsTrue(JToken.DeepEquals(new JValue(new DateTime(2000, 12, 20)), (JValue)new DateTime(2000, 12, 20)));
#if !PocketPC && !NET20
      Assert.IsTrue(JToken.DeepEquals(new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)), (JValue)new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
      Assert.IsTrue(JToken.DeepEquals(new JValue((DateTimeOffset?)null), (JValue)(DateTimeOffset?)null));
#endif

      Assert.IsTrue(JToken.DeepEquals(new JValue(true), (JValue)true));
      Assert.IsTrue(JToken.DeepEquals(new JValue(true), (JValue)(bool?)true));
      Assert.IsTrue(JToken.DeepEquals(new JValue((bool?)null), (JValue)(bool?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue(10), (JValue)10));
      Assert.IsTrue(JToken.DeepEquals(new JValue((long?)null), (JValue)(long?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue(long.MaxValue), (JValue)long.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue((int?)null), (JValue)(int?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((short?)null), (JValue)(short?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((double?)null), (JValue)(double?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((uint?)null), (JValue)(uint?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((decimal?)null), (JValue)(decimal?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((ulong?)null), (JValue)(ulong?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((sbyte?)null), (JValue)(sbyte?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((ushort?)null), (JValue)(ushort?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue(ushort.MaxValue), (JValue)ushort.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(11.1f), (JValue)11.1f));
      Assert.IsTrue(JToken.DeepEquals(new JValue(float.MinValue), (JValue)float.MinValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(double.MinValue), (JValue)double.MinValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(uint.MaxValue), (JValue)uint.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(ulong.MaxValue), (JValue)ulong.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(ulong.MinValue), (JValue)ulong.MinValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue((string)null), (JValue)(string)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
      Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)decimal.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)(decimal?)decimal.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MinValue), (JValue)decimal.MinValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(float.MaxValue), (JValue)(float?)float.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue(double.MaxValue), (JValue)(double?)double.MaxValue));
      Assert.IsTrue(JToken.DeepEquals(new JValue((object)null), (JValue)(double?)null));

      Assert.IsFalse(JToken.DeepEquals(new JValue(true), (JValue)(bool?)null));
      Assert.IsFalse(JToken.DeepEquals(new JValue((object)null), (JValue)(object)null));

      byte[] emptyData = new byte[0];
      Assert.IsTrue(JToken.DeepEquals(new JValue(emptyData), (JValue)emptyData));
      Assert.IsFalse(JToken.DeepEquals(new JValue(emptyData), (JValue)new byte[1]));
      Assert.IsTrue(JToken.DeepEquals(new JValue(Encoding.UTF8.GetBytes("Hi")), (JValue)Encoding.UTF8.GetBytes("Hi")));
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
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JProperty to Newtonsoft.Json.Linq.JArray.")]
    public void AddPropertyToArray()
    {
      JArray a = new JArray();
      a.Add(new JProperty("PropertyName"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.")]
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

    [Test]
    public void ToStringWithConverters()
    {
      JArray a =
        new JArray(
          new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
        );

      string json = a.ToString(Formatting.Indented, new IsoDateTimeConverter());

      Assert.AreEqual(@"[
  ""2009-02-15T00:00:00Z""
]", json);

      json = JsonConvert.SerializeObject(a, new IsoDateTimeConverter());

      Assert.AreEqual(@"[""2009-02-15T00:00:00Z""]", json);
    }

    [Test]
    public void ToStringWithNoIndenting()
    {
      JArray a =
        new JArray(
          new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
        );

      string json = a.ToString(Formatting.None, new IsoDateTimeConverter());

      Assert.AreEqual(@"[""2009-02-15T00:00:00Z""]", json);
    }

    [Test]
    public void AddAfterSelf()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      a[1].AddAfterSelf("pie");

      Assert.AreEqual(5, (int)a[0]);
      Assert.AreEqual(1, a[1].Count());
      Assert.AreEqual("pie", (string)a[2]);
      Assert.AreEqual(5, a.Count());

      a[4].AddAfterSelf("lastpie");

      Assert.AreEqual("lastpie", (string)a[5]);
      Assert.AreEqual("lastpie", (string)a.Last);
    }

    [Test]
    public void AddBeforeSelf()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3)
        );

      a[1].AddBeforeSelf("pie");

      Assert.AreEqual(5, (int)a[0]);
      Assert.AreEqual("pie", (string)a[1]);
      Assert.AreEqual(a, a[1].Parent);
      Assert.AreEqual(a[2], a[1].Next);
      Assert.AreEqual(5, a.Count());

      a[0].AddBeforeSelf("firstpie");

      Assert.AreEqual("firstpie", (string)a[0]);
      Assert.AreEqual(5, (int)a[1]);
      Assert.AreEqual("pie", (string)a[2]);
      Assert.AreEqual(a, a[0].Parent);
      Assert.AreEqual(a[1], a[0].Next);
      Assert.AreEqual(6, a.Count());

      a.Last.AddBeforeSelf("secondlastpie");

      Assert.AreEqual("secondlastpie", (string)a[5]);
      Assert.AreEqual(7, a.Count());
    }

    [Test]
    public void DeepClone()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3),
          new JObject(
            new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
            new JProperty("Second", 1),
            new JProperty("Third", null),
            new JProperty("Fourth", new JConstructor("Date", 12345)),
            new JProperty("Fifth", double.PositiveInfinity),
            new JProperty("Sixth", double.NaN)
            )
        );

      JArray a2 = (JArray)a.DeepClone();

      Console.WriteLine(a2.ToString(Formatting.Indented));

      Assert.IsTrue(a.DeepEquals(a2));
    }

#if !SILVERLIGHT
    [Test]
    public void Clone()
    {
      JArray a =
        new JArray(
          5,
          new JArray(1),
          new JArray(1, 2),
          new JArray(1, 2, 3),
          new JObject(
            new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
            new JProperty("Second", 1),
            new JProperty("Third", null),
            new JProperty("Fourth", new JConstructor("Date", 12345)),
            new JProperty("Fifth", double.PositiveInfinity),
            new JProperty("Sixth", double.NaN)
            )
        );

      ICloneable c = a;

      JArray a2 = (JArray) c.Clone();

      Assert.IsTrue(a.DeepEquals(a2));
    }
#endif

    [Test]
    public void DoubleDeepEquals()
    {
      JArray a =
        new JArray(
          double.NaN,
          double.PositiveInfinity,
          double.NegativeInfinity
        );

      JArray a2 = (JArray)a.DeepClone();

      Assert.IsTrue(a.DeepEquals(a2));

      double d = 1 + 0.1 + 0.1 + 0.1;

      JValue v1 = new JValue(d);
      JValue v2 = new JValue(1.3);

      Assert.IsTrue(v1.DeepEquals(v2));
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Additional text encountered after finished reading JSON content: ,. Line 5, position 2.")]
    public void ParseAdditionalContent()
    {
      string json = @"[
""Small"",
""Medium"",
""Large""
],";

      JToken.Parse(json);
    }
  }
}