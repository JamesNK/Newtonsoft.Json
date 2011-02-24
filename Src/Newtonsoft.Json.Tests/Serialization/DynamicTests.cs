#if !(NET35 || NET20)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class DynamicTests : TestFixtureBase
  {
    public class DynamicChildObject
    {
      public string Text { get; set; }
      public int Integer { get; set; }
    }

    public class TestDynamicObject : DynamicObject
    {
      private readonly Dictionary<string, object> _members;

      public int Int;
      public DynamicChildObject ChildObject { get; set; }

      internal Dictionary<string, object> Members
      {
        get { return _members; }
      }

      public TestDynamicObject()
      {
        _members = new Dictionary<string, object>();
      }

      public override IEnumerable<string> GetDynamicMemberNames()
      {
        return _members.Keys.Union(new[] { "Int", "ChildObject" });
      }

      public override bool TryConvert(ConvertBinder binder, out object result)
      {
        Type targetType = binder.Type;

        if (targetType == typeof(IDictionary<string, object>) ||
            targetType == typeof(IDictionary))
        {
          result = new Dictionary<string, object>(_members);
          return true;
        }
        else
        {
          return base.TryConvert(binder, out result);
        }
      }

      public override bool TryDeleteMember(DeleteMemberBinder binder)
      {
        return _members.Remove(binder.Name);
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
        return _members.TryGetValue(binder.Name, out result);
      }

      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
        _members[binder.Name] = value;
        return true;
      }
    }

    public class ErrorSettingDynamicObject : DynamicObject
    {
      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
        return false;
      }
    }

    [Test]
    public void SerializeDynamicObject()
    {
      TestDynamicObject dynamicObject = new TestDynamicObject();

      dynamic d = dynamicObject;
      d.Int = 1;
      d.Decimal = 99.9d;
      d.ChildObject = new DynamicChildObject();

      Dictionary<string, object> values = new Dictionary<string, object>();

      foreach (string memberName in dynamicObject.GetDynamicMemberNames())
      {
        object value;
        dynamicObject.TryGetMember(memberName, out value);

        values.Add(memberName, value);
      }

      Assert.AreEqual(d.Int, values["Int"]);
      Assert.AreEqual(d.Decimal, values["Decimal"]);
      Assert.AreEqual(d.ChildObject, values["ChildObject"]);

      string json = JsonConvert.SerializeObject(dynamicObject, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Decimal"": 99.9,
  ""Int"": 1,
  ""ChildObject"": {
    ""Text"": null,
    ""Integer"": 0
  }
}", json);

      TestDynamicObject newDynamicObject = JsonConvert.DeserializeObject<TestDynamicObject>(json);
      d = newDynamicObject;

      Assert.AreEqual(99.9, d.Decimal);
      Assert.AreEqual(1, d.Int);
      Assert.AreEqual(dynamicObject.ChildObject.Integer, d.ChildObject.Integer);
      Assert.AreEqual(dynamicObject.ChildObject.Text, d.ChildObject.Text);
    }

    [Test]
    public void sdfsdf()
    {
      ErrorSettingDynamicObject d = JsonConvert.DeserializeObject<ErrorSettingDynamicObject>("{'hi':5}");
    }

    [Test]
    public void SerializeDynamicObjectWithObjectTracking()
    {
      dynamic o = new ExpandoObject();
      o.Text = "Text!";
      o.Integer = int.MaxValue;
      o.DynamicChildObject = new DynamicChildObject
        {
          Integer = int.MinValue,
          Text = "Child text!"
        };

      string json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All,
          TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
        });

      Console.WriteLine(json);

      string dynamicChildObjectTypeName = ReflectionUtils.GetTypeName(typeof(DynamicChildObject), FormatterAssemblyStyle.Full);
      string expandoObjectTypeName = ReflectionUtils.GetTypeName(typeof(ExpandoObject), FormatterAssemblyStyle.Full);

      Assert.AreEqual(@"{
  ""$type"": """ + expandoObjectTypeName + @""",
  ""Text"": ""Text!"",
  ""Integer"": 2147483647,
  ""DynamicChildObject"": {
    ""$type"": """ + dynamicChildObjectTypeName + @""",
    ""Text"": ""Child text!"",
    ""Integer"": -2147483648
  }
}", json);

      dynamic n = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All,
          TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
        });

      Assert.IsInstanceOfType(typeof(ExpandoObject), n);
      Assert.AreEqual("Text!", n.Text);
      Assert.AreEqual(int.MaxValue, n.Integer);

      Assert.IsInstanceOfType(typeof(DynamicChildObject), n.DynamicChildObject);
      Assert.AreEqual("Child text!", n.DynamicChildObject.Text);
      Assert.AreEqual(int.MinValue, n.DynamicChildObject.Integer);
    }
  }
}
#endif