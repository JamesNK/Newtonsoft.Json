#if !(NET35 || NET20)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Utilities;
using System.Globalization;

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
    public void JValueEquals()
    {
      JObject o = new JObject(
        new JProperty("Null", new JValue(null, JTokenType.Null)),
        new JProperty("Integer", new JValue(1)),
        new JProperty("Float", new JValue(1.1d)),
        new JProperty("Decimal", new JValue(1.1m)),
        new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
        new JProperty("Boolean", new JValue(true)),
        new JProperty("String", new JValue("A string lol!")),
        new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!")))
        );

      dynamic d = o;

      Assert.IsTrue(d.Null == d.Null);
      Assert.IsTrue(d.Null == null);
      Assert.IsTrue(d.Null == new JValue(null, JTokenType.Null));
      Assert.IsFalse(d.Null == 1);

      Assert.IsTrue(d.Integer == d.Integer);
      Assert.IsTrue(d.Integer > 0);
      Assert.IsTrue(d.Integer > 0.0m);
      Assert.IsTrue(d.Integer > 0.0f);
      Assert.IsTrue(d.Integer > null);
      Assert.IsTrue(d.Integer >= null);
      Assert.IsTrue(d.Integer == 1);
      Assert.IsTrue(d.Integer == 1m);
      Assert.IsTrue(d.Integer != 1.1f);
      Assert.IsTrue(d.Integer != 1.1d);

      Assert.IsTrue(d.Decimal == d.Decimal);
      Assert.IsTrue(d.Decimal > 0);
      Assert.IsTrue(d.Decimal > 0.0m);
      Assert.IsTrue(d.Decimal > 0.0f);
      Assert.IsTrue(d.Decimal > null);
      Assert.IsTrue(d.Decimal >= null);
      Assert.IsTrue(d.Decimal == 1.1);
      Assert.IsTrue(d.Decimal == 1.1m);
      Assert.IsTrue(d.Decimal != 1.0f);
      Assert.IsTrue(d.Decimal != 1.0d);

      Assert.IsTrue(d.Float == d.Float);
      Assert.IsTrue(d.Float > 0);
      Assert.IsTrue(d.Float > 0.0m);
      Assert.IsTrue(d.Float > 0.0f);
      Assert.IsTrue(d.Float > null);
      Assert.IsTrue(d.Float >= null);
      Assert.IsTrue(d.Float < 2);
      Assert.IsTrue(d.Float <= 1.1);
      Assert.IsTrue(d.Float == 1.1);
      Assert.IsTrue(d.Float == 1.1m);
      Assert.IsTrue(d.Float != 1.0f);
      Assert.IsTrue(d.Float != 1.0d);

      Assert.IsTrue(d.Bytes == d.Bytes);
      Assert.IsTrue(d.Bytes == Encoding.UTF8.GetBytes("A string lol!"));
      Assert.IsTrue(d.Bytes == new JValue(Encoding.UTF8.GetBytes("A string lol!")));
    }

    [Test]
    public void JValueToString()
    {
      JObject o = new JObject(
        new JProperty("Null", new JValue(null, JTokenType.Null)),
        new JProperty("Integer", new JValue(1)),
        new JProperty("Float", new JValue(1.1)),
        new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
        new JProperty("Boolean", new JValue(true)),
        new JProperty("String", new JValue("A string lol!")),
        new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!")))
        );

      dynamic d = o;

      Assert.AreEqual("", d.Null.ToString());
      Assert.AreEqual("1", d.Integer.ToString());
      Assert.AreEqual("1.1", d.Float.ToString(CultureInfo.InvariantCulture));
      Assert.AreEqual("12/29/2000 23:51:10", d.DateTime.ToString(null, CultureInfo.InvariantCulture));
      Assert.AreEqual("True", d.Boolean.ToString());
      Assert.AreEqual("A string lol!", d.String.ToString());
      Assert.AreEqual("System.Byte[]", d.Bytes.ToString());
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
      AssertValueConverted<bool?>(null);
      AssertValueConverted<bool?>("true", true);
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
      AssertValueConverted<decimal>(1);
      AssertValueConverted<decimal>(1.1f, 1.1m);
      AssertValueConverted<decimal>("1.1", 1.1m);
      AssertValueConverted<double>(99.9);
      AssertValueConverted<double>(99.9m);
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
      AssertValueConverted<string>(null);
      AssertValueConverted<string>(1, "1");
      AssertValueConverted<uint>(uint.MinValue);
      AssertValueConverted<uint?>(uint.MinValue);
      AssertValueConverted<uint?>("1", 1);
      AssertValueConverted<ulong>(ulong.MaxValue);
      AssertValueConverted<ulong?>(ulong.MaxValue);
      AssertValueConverted<ushort>(ushort.MinValue);
      AssertValueConverted<ushort?>(ushort.MinValue);
      AssertValueConverted<ushort?>(null);
    }

    private static void AssertValueConverted<T>(object value)
    {
      AssertValueConverted<T>(value, value);
    }

    private static void AssertValueConverted<T>(object value, object expected)
    {
      JValue v = new JValue(value);
      dynamic d = v;

      T t = d;
      Assert.AreEqual(expected, t);
    }

    [Test]
    public void DynamicSerializerExample()
    {
      dynamic value = new DynamicDictionary();

      value.Name = "Arine Admin";
      value.Enabled = true;
      value.Roles = new[] {"Admin", "User"};

      string json = JsonConvert.SerializeObject(value, Formatting.Indented);
      // {
      //   "Name": "Arine Admin",
      //   "Enabled": true,
      //   "Roles": [
      //     "Admin",
      //     "User"
      //   ]
      // }

      dynamic newValue = JsonConvert.DeserializeObject<DynamicDictionary>(json);

      string role = newValue.Roles[0];
      // Admin
    }

    [Test]
    public void DynamicLinqExample()
    {
      JObject oldAndBusted = new JObject();
      oldAndBusted["Name"] = "Arnie Admin";
      oldAndBusted["Enabled"] = true;
      oldAndBusted["Roles"] = new JArray(new[] { "Admin", "User" });

      string oldRole = (string) oldAndBusted["Roles"][0];
      // Admin


      dynamic newHotness = new JObject();
      newHotness.Name = "Arnie Admin";
      newHotness.Enabled = true;
      newHotness.Roles = new JArray(new[] { "Admin", "User" });

      string newRole = newHotness.Roles[0];
      // Admin
    }

    public class DynamicDictionary : DynamicObject
    {
      private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

      public override IEnumerable<string> GetDynamicMemberNames()
      {
        return _values.Keys;
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
        result = _values[binder.Name];
        return true;
      }

      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
        _values[binder.Name] = value;
        return true;
      }
    }
  }
}
#endif