using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
  [TestFixture]
  public class JPropertyTests : TestFixtureBase
  {
    [Test]
    public void NullValue()
    {
      JProperty p = new JProperty("TestProperty", null);
      Assert.IsNotNull(p.Value);
      Assert.AreEqual(JTokenType.Null, p.Value.Type);
      Assert.AreEqual(p, p.Value.Parent);

      p.Value = null;
      Assert.IsNotNull(p.Value);
      Assert.AreEqual(JTokenType.Null, p.Value.Type);
      Assert.AreEqual(p, p.Value.Parent);
    }

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
    [Test]
    public void ListChanged()
    {
      JProperty p = new JProperty("TestProperty", null);
      IBindingList l = p;

      ListChangedType? listChangedType = null;
      int? index = null;

      l.ListChanged += (sender, args) =>
        {
          listChangedType = args.ListChangedType;
          index = args.NewIndex;
        };

      p.Value = 1;

      Assert.AreEqual(ListChangedType.ItemChanged, listChangedType.Value);
      Assert.AreEqual(0, index.Value);
    }
#endif

    [Test]
    public void IListCount()
    {
      JProperty p = new JProperty("TestProperty", null);
      IList l = p;

      Assert.AreEqual(1, l.Count);
    }

    [Test]
    public void IListClear()
    {
      JProperty p = new JProperty("TestProperty", null);
      IList l = p;

      ExceptionAssert.Throws<JsonException>(
        "Cannot add or remove items from Newtonsoft.Json.Linq.JProperty.",
        () =>
        {
          l.Clear();
        });
    }

    [Test]
    public void IListAdd()
    {
      JProperty p = new JProperty("TestProperty", null);
      IList l = p;

      ExceptionAssert.Throws<JsonException>(
        "Newtonsoft.Json.Linq.JProperty cannot have multiple values.",
        () =>
          {
            l.Add(null);
          });
    }

    [Test]
    public void IListRemove()
    {
      JProperty p = new JProperty("TestProperty", null);
      IList l = p;

      ExceptionAssert.Throws<JsonException>(
        "Cannot add or remove items from Newtonsoft.Json.Linq.JProperty.",
        () =>
          {
            l.Remove(p.Value);
          });
    }

    [Test]
    public void Load()
    {
      JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
      reader.Read();

      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      reader.Read();

      JProperty property = JProperty.Load(reader);
      Assert.AreEqual("propertyname", property.Name);
      Assert.IsTrue(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
      reader.Read();

      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      reader.Read();

      property = JProperty.Load(reader);
      Assert.AreEqual("propertyname", property.Name);
      Assert.IsTrue(JToken.DeepEquals(new JValue(null, JTokenType.Null), property.Value));

      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void MultiContentConstructor()
    {
      JProperty p = new JProperty("error", new List<string> {"one", "two"});
      JArray a = (JArray) p.Value;

      Assert.AreEqual(a.Count, 2);
      Assert.AreEqual("one", (string) a[0]);
      Assert.AreEqual("two", (string) a[1]);
    }

    [Test]
    public void IListGenericAdd()
    {
      IList<JToken> t = new JProperty("error", new List<string> {"one", "two"});

      ExceptionAssert.Throws<JsonException>(
        "Newtonsoft.Json.Linq.JProperty cannot have multiple values.",
        () =>
          {
            t.Add(1);
          });
    }
  }
}