using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JObjectTests : TestFixtureBase
  {
    [Test]
    public void TryGetValue()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      JToken t;
      Assert.AreEqual(false, o.TryGetValue("sdf", out t));
      Assert.AreEqual(null, t);

      Assert.AreEqual(false, o.TryGetValue(null, out t));
      Assert.AreEqual(null, t);

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));
    }

    [Test]
    public void DictionaryItemShouldSet()
    {
      JObject o = new JObject();
      o["PropertyNameValue"] = new JValue(1);
      Assert.AreEqual(1, o.Children().Count());

      JToken t;
      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));

      o["PropertyNameValue"] = new JValue(2);
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(2), t));

      o["PropertyNameValue"] = null;
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue((object)null), t));
    }

    [Test]
    public void Remove()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(false, o.Remove("sdf"));
      Assert.AreEqual(false, o.Remove(null));
      Assert.AreEqual(true, o.Remove("PropertyNameValue"));

      Assert.AreEqual(0, o.Children().Count());
    }

    [Test]
    public void GenericCollectionRemove()
    {
      JValue v = new JValue(1);
      JObject o = new JObject();
      o.Add("PropertyNameValue", v);
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1))));
      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2))));
      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1))));
      Assert.AreEqual(true, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", v)));

      Assert.AreEqual(0, o.Children().Count());
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Can not add property PropertyNameValue to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void DuplicatePropertyNameShouldThrow()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", null);
      o.Add("PropertyNameValue", null);
    }

    [Test]
    public void GenericDictionaryAdd()
    {
      JObject o = new JObject();

      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, (int)o["PropertyNameValue"]);

      o.Add("PropertyNameValue1", null);
      Assert.AreEqual(null, ((JValue)o["PropertyNameValue1"]).Value);

      Assert.AreEqual(2, o.Children().Count());
    }

    [Test]
    public void GenericCollectionAdd()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string,JToken>>)o).Add(new KeyValuePair<string,JToken>("PropertyNameValue", new JValue(1)));

      Assert.AreEqual(1, (int)o["PropertyNameValue"]);
      Assert.AreEqual(1, o.Children().Count());
    }

    [Test]
    public void GenericCollectionClear()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      JProperty p = (JProperty)o.Children().ElementAt(0);

      ((ICollection<KeyValuePair<string, JToken>>)o).Clear();
      Assert.AreEqual(0, o.Children().Count());

      Assert.AreEqual(null, p.Parent);
    }

    [Test]
    public void GenericCollectionContains()
    {
      JValue v = new JValue(1);
      JObject o = new JObject();
      o.Add("PropertyNameValue", v);
      Assert.AreEqual(1, o.Children().Count());

      bool contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", v));
      Assert.AreEqual(true, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(default(KeyValuePair<string, JToken>));
      Assert.AreEqual(false, contains);
    }

    [Test]
    public void GenericDictionaryContains()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      bool contains = ((IDictionary<string, JToken>)o).ContainsKey("PropertyNameValue");
      Assert.AreEqual(true, contains);
    }

    [Test]
    public void GenericCollectionCopyTo()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));
      o.Add("PropertyNameValue3", new JValue(3));
      Assert.AreEqual(3, o.Children().Count());

      KeyValuePair<string, JToken>[] a = new KeyValuePair<string,JToken>[5];

      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(a, 1);

      Assert.AreEqual(default(KeyValuePair<string,JToken>), a[0]);
      
      Assert.AreEqual("PropertyNameValue", a[1].Key);
      Assert.AreEqual(1, (int)a[1].Value);

      Assert.AreEqual("PropertyNameValue2", a[2].Key);
      Assert.AreEqual(2, (int)a[2].Value);

      Assert.AreEqual("PropertyNameValue3", a[3].Key);
      Assert.AreEqual(3, (int)a[3].Value);

      Assert.AreEqual(default(KeyValuePair<string, JToken>), a[4]);

    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = @"Value cannot be null.
Parameter name: array")]
    public void GenericCollectionCopyToNullArrayShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(null, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"arrayIndex is less than 0.
Parameter name: arrayIndex")]
    public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], -1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"arrayIndex is equal to or greater than the length of array.")]
    public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.")]
    public void GenericCollectionCopyToInsufficientArrayCapacity()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));
      o.Add("PropertyNameValue3", new JValue(3));

      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[3], 1);
    }
  }
}
