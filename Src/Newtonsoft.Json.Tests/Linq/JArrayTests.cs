using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JArrayTests : TestFixtureBase
  {
    [Test]
    public void Clear()
    {
      JArray a = new JArray { 1 };
      Assert.AreEqual(1, a.Count);

      a.Clear();
      Assert.AreEqual(0, a.Count);
    }

    [Test]
    public void Contains()
    {
      JValue v = new JValue(1);

      JArray a = new JArray { v };

      Assert.AreEqual(false, a.Contains(new JValue(2)));
      Assert.AreEqual(false, a.Contains(new JValue(1)));
      Assert.AreEqual(false, a.Contains(null));
      Assert.AreEqual(true, a.Contains(v));
    }

    [Test]
    public void GenericCollectionCopyTo()
    {
      JArray j = new JArray();
      j.Add(new JValue(1));
      j.Add(new JValue(2));
      j.Add(new JValue(3));
      Assert.AreEqual(3, j.Count);

      JToken[] a = new JToken[5];

      ((ICollection<JToken>)j).CopyTo(a, 1);

      Assert.AreEqual(null, a[0]);

      Assert.AreEqual(1, (int)a[1]);

      Assert.AreEqual(2, (int)a[2]);

      Assert.AreEqual(3, (int)a[3]);

      Assert.AreEqual(null, a[4]);

    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = @"Value cannot be null.
Parameter name: array")]
    public void GenericCollectionCopyToNullArrayShouldThrow()
    {
      JArray j = new JArray();
      ((ICollection<JToken>)j).CopyTo(null, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"arrayIndex is less than 0.
Parameter name: arrayIndex")]
    public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
    {
      JArray j = new JArray();
      ((ICollection<JToken>)j).CopyTo(new JToken[1], -1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"arrayIndex is equal to or greater than the length of array.")]
    public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
    {
      JArray j = new JArray();
      ((ICollection<JToken>)j).CopyTo(new JToken[1], 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.")]
    public void GenericCollectionCopyToInsufficientArrayCapacity()
    {
      JArray j = new JArray();
      j.Add(new JValue(1));
      j.Add(new JValue(2));
      j.Add(new JValue(3));

      ((ICollection<JToken>)j).CopyTo(new JToken[3], 1);
    }

    [Test]
    public void Remove()
    {
      JValue v = new JValue(1);
      JArray j = new JArray();
      j.Add(v);

      Assert.AreEqual(1, j.Count);

      Assert.AreEqual(false, j.Remove(new JValue(1)));
      Assert.AreEqual(false, j.Remove(null));
      Assert.AreEqual(true, j.Remove(v));
      Assert.AreEqual(false, j.Remove(v));

      Assert.AreEqual(0, j.Count);
    }

    [Test]
    public void IndexOf()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(1);
      JValue v3 = new JValue(1);

      JArray j = new JArray();

      j.Add(v1);
      Assert.AreEqual(0, j.IndexOf(v1));

      j.Add(v2);
      Assert.AreEqual(0, j.IndexOf(v1));
      Assert.AreEqual(1, j.IndexOf(v2));

      j.AddFirst(v3);
      Assert.AreEqual(1, j.IndexOf(v1));
      Assert.AreEqual(2, j.IndexOf(v2));
      Assert.AreEqual(0, j.IndexOf(v3));

      v3.Remove();
      Assert.AreEqual(0, j.IndexOf(v1));
      Assert.AreEqual(1, j.IndexOf(v2));
      Assert.AreEqual(-1, j.IndexOf(v3));
    }

    [Test]
    public void RemoveAt()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(1);
      JValue v3 = new JValue(1);

      JArray j = new JArray();

      j.Add(v1);
      j.Add(v2);
      j.Add(v3);

      Assert.AreEqual(true, j.Contains(v1));
      j.RemoveAt(0);
      Assert.AreEqual(false, j.Contains(v1));

      Assert.AreEqual(true, j.Contains(v3));
      j.RemoveAt(1);
      Assert.AreEqual(false, j.Contains(v3));

      Assert.AreEqual(1, j.Count);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"index is equal to or greater than Count.
Parameter name: index")]
    public void RemoveAtOutOfRangeIndexShouldError()
    {
      JArray j = new JArray();
      j.RemoveAt(0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"index is less than 0.
Parameter name: index")]
    public void RemoveAtNegativeIndexShouldError()
    {
      JArray j = new JArray();
      j.RemoveAt(-1);
    }

    [Test]
    public void Insert()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(2);
      JValue v3 = new JValue(3);
      JValue v4 = new JValue(4);

      JArray j = new JArray();

      j.Add(v1);
      j.Add(v2);
      j.Add(v3);
      j.Insert(1, v4);

      Assert.AreEqual(0, j.IndexOf(v1));
      Assert.AreEqual(1, j.IndexOf(v4));
      Assert.AreEqual(2, j.IndexOf(v2));
      Assert.AreEqual(3, j.IndexOf(v3));
    }

    [Test]
    public void AddFirstAddedTokenShouldBeFirst()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(2);
      JValue v3 = new JValue(3);

      JArray j = new JArray();
      Assert.AreEqual(null, j.First);
      Assert.AreEqual(null, j.Last);

      j.AddFirst(v1);
      Assert.AreEqual(v1, j.First);
      Assert.AreEqual(v1, j.Last);

      j.AddFirst(v2);
      Assert.AreEqual(v2, j.First);
      Assert.AreEqual(v1, j.Last);

      j.AddFirst(v3);
      Assert.AreEqual(v3, j.First);
      Assert.AreEqual(v1, j.Last);
    }

    [Test]
    public void InsertShouldInsertAtZeroIndex()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(2);

      JArray j = new JArray();

      j.Insert(0, v1);
      Assert.AreEqual(0, j.IndexOf(v1));

      j.Insert(0, v2);
      Assert.AreEqual(1, j.IndexOf(v1));
      Assert.AreEqual(0, j.IndexOf(v2));
    }

    [Test]
    public void InsertNull()
    {
      JArray j = new JArray();
      j.Insert(0, null);

      Assert.AreEqual(null, ((JValue)j[0]).Value);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"Specified argument was out of the range of valid values.
Parameter name: index")]
    public void InsertNegativeIndexShouldThrow()
    {
      JArray j = new JArray();
      j.Insert(-1, new JValue(1));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"Specified argument was out of the range of valid values.
Parameter name: index")]
    public void InsertOutOfRangeIndexShouldThrow()
    {
      JArray j = new JArray();
      j.Insert(2, new JValue(1));
    }

    [Test]
    public void Item()
    {
      JValue v1 = new JValue(1);
      JValue v2 = new JValue(2);
      JValue v3 = new JValue(3);
      JValue v4 = new JValue(4);

      JArray j = new JArray();

      j.Add(v1);
      j.Add(v2);
      j.Add(v3);

      j[1] = v4;

      Assert.AreEqual(null, v2.Parent);
      Assert.AreEqual(-1, j.IndexOf(v2));
      Assert.AreEqual(j, v4.Parent);
      Assert.AreEqual(1, j.IndexOf(v4));
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Error reading JArray from JsonReader. Current JsonReader item is not an array: StartObject")]
    public void Parse_ShouldThrowOnUnexpectedToken()
    {
      string json = @"{""prop"":""value""}";
      JArray.Parse(json);
    }
  }
}