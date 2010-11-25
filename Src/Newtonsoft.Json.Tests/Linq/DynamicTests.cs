#if !(NET35 || NET20 || SILVERLIGHT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Linq
{
  public class DynamicTests : TestFixtureBase
  {
    [Test]
    public void JObjectPropertyNames()
    {
      JObject o = new JObject(
        new JProperty("ChildValue", "blah blah"));

      dynamic d = o;

      d.First = "A value!";

      Assert.AreEqual(new JValue("A value!"), d.First);
      Assert.AreEqual("A value!", (string)d.First);

      d.First = null;
      Assert.AreEqual(JTokenType.Null, d.First.Type);

      Assert.IsTrue(d.Remove("First"));
      Assert.IsNull(d.First);

      JValue v1 = d.ChildValue;
      JValue v2 = d["ChildValue"];
      Assert.AreEqual(v1, v2);

      JValue newValue1 = new JValue("Blah blah");
      d.NewValue = newValue1;
      JValue newValue2 = d.NewValue;

      Assert.IsTrue(ReferenceEquals(newValue1, newValue2));
    }

    [Test]
    public void JObjectEnumerator()
    {
      JObject o = new JObject(
        new JProperty("ChildValue", "blah blah"));

      dynamic d = o;

      foreach (JProperty value in d)
      {
        Assert.AreEqual("ChildValue", value.Name);
        Assert.AreEqual("blah blah", (string)value.Value);
      }

      foreach (dynamic value in d)
      {
        Assert.AreEqual("ChildValue", value.Name);
        Assert.AreEqual("blah blah", (string)value.Value);
      }
    }

    [Test]
    public void JObjectPropertyNameWithJArray()
    {
      JObject o = new JObject(
        new JProperty("ChildValue", "blah blah"));

      dynamic d = o;

      d.First = new JArray();
      d.First.Add("Hi");

      Assert.AreEqual(1, d.First.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Could not determine JSON object type for type System.String[].")]
    public void JObjectPropertyNameWithNonToken()
    {
      dynamic d = new JObject();

      d.First = new [] {"One", "II", "3"};
    }

    [Test]
    public void JObjectMethods()
    {
      JObject o = new JObject(
        new JProperty("ChildValue", "blah blah"));

      dynamic d = o;

      d.Add("NewValue", 1);

      object count = d.Count;

      Assert.IsNull(count);
      Assert.IsNull(d["Count"]);

      JToken v;
      Assert.IsTrue(d.TryGetValue("ChildValue", out v));
      Assert.AreEqual("blah blah", (string)v);
    }

    [Test]
    public void JObjectGetDynamicPropertyNames()
    {
      JObject o = new JObject(
        new JProperty("ChildValue", "blah blah"),
        new JProperty("Hello Joe", null));

      dynamic d = o;

      List<string> memberNames = o.GetDynamicMemberNames().ToList();

      Assert.AreEqual(2, memberNames.Count);
      Assert.AreEqual("ChildValue", memberNames[0]);
      Assert.AreEqual("Hello Joe", memberNames[1]);

      o = new JObject(
        new JProperty("ChildValue1", "blah blah"),
        new JProperty("Hello Joe1", null));

      d = o;

      memberNames = o.GetDynamicMemberNames().ToList();

      Assert.AreEqual(2, memberNames.Count);
      Assert.AreEqual("ChildValue1", memberNames[0]);
      Assert.AreEqual("Hello Joe1", memberNames[1]);
    }

    [Test]
    public void JValueConvert()
    {
      AssertValueConverted<bool>(true);
      AssertValueConverted<bool?>(true);
      AssertValueConverted<bool?>(false);
      AssertValueConverted<byte[]>(null);
      AssertValueConverted<byte[]>(Encoding.UTF8.GetBytes("blah"));
      AssertValueConverted<DateTime>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
      AssertValueConverted<DateTime?>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
      AssertValueConverted<DateTime?>(null);
      AssertValueConverted<DateTimeOffset>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
      AssertValueConverted<DateTimeOffset?>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
      AssertValueConverted<DateTimeOffset?>(null);
      AssertValueConverted<decimal>(99.9m);
      AssertValueConverted<decimal?>(99.9m);
      AssertValueConverted<double>(99.9);
      AssertValueConverted<double?>(99.9);
      AssertValueConverted<float>(99.9f);
      AssertValueConverted<float?>(99.9f);
      AssertValueConverted<int>(int.MinValue);
      AssertValueConverted<int?>(int.MinValue);
      AssertValueConverted<long>(long.MaxValue);
      AssertValueConverted<long?>(long.MaxValue);
      AssertValueConverted<short>(short.MaxValue);
      AssertValueConverted<short?>(short.MaxValue);
      AssertValueConverted<string>("blah");
      AssertValueConverted<uint>(uint.MinValue);
      AssertValueConverted<uint?>(uint.MinValue);
      AssertValueConverted<ulong>(ulong.MaxValue);
      AssertValueConverted<ulong?>(ulong.MaxValue);
      AssertValueConverted<ushort>(ushort.MinValue);
      AssertValueConverted<ushort?>(ushort.MinValue);
      AssertValueConverted<ushort?>(null);
    }

    private static void AssertValueConverted<T>(T value)
    {
      JValue v = new JValue(value);
      dynamic d = v;

      T t = d;
      Assert.AreEqual(value, t);
    }
  }
}
#endif