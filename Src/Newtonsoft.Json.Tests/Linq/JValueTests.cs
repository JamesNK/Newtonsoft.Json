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
  public class JValueTests : TestFixtureBase
  {
    [Test]
    public void ChangeValue()
    {
      JValue v = new JValue(true);
      Assert.AreEqual(true, v.Value);
      Assert.AreEqual(JTokenType.Boolean, v.Type);

      v.Value = "Pie";
      Assert.AreEqual("Pie", v.Value);
      Assert.AreEqual(JTokenType.String, v.Type);

      v.Value = null;
      Assert.AreEqual(null, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      v.Value = (int?)null;
      Assert.AreEqual(null, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      v.Value = "Pie";
      Assert.AreEqual("Pie", v.Value);
      Assert.AreEqual(JTokenType.String, v.Type);

      v.Value = DBNull.Value;
      Assert.AreEqual(DBNull.Value, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      byte[] data = new byte[0];
      v.Value = data;

      Assert.AreEqual(data, v.Value);
      Assert.AreEqual(JTokenType.Bytes, v.Type);

      v.Value = StringComparison.OrdinalIgnoreCase;
      Assert.AreEqual(StringComparison.OrdinalIgnoreCase, v.Value);
      Assert.AreEqual(JTokenType.Integer, v.Type);
    }

    [Test]
    public void CreateComment()
    {
      JValue commentValue = JValue.CreateComment(null);
      Assert.AreEqual(null, commentValue.Value);
      Assert.AreEqual(JTokenType.Comment, commentValue.Type);

      commentValue.Value = "Comment";
      Assert.AreEqual("Comment", commentValue.Value);
      Assert.AreEqual(JTokenType.Comment, commentValue.Type);
    }

    [Test]
    public void CreateString()
    {
      JValue stringValue = JValue.CreateString(null);
      Assert.AreEqual(null, stringValue.Value);
      Assert.AreEqual(JTokenType.String, stringValue.Type);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access child value on Newtonsoft.Json.Linq.JValue.")]
    public void Last()
    {
      JValue v = new JValue(true);
      JToken last = v.Last;
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access child value on Newtonsoft.Json.Linq.JValue.")]
    public void Children()
    {
      JValue v = new JValue(true);
      var c = v.Children();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access child value on Newtonsoft.Json.Linq.JValue.")]
    public void First()
    {
      JValue v = new JValue(true);
      JToken first = v.First;
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access child value on Newtonsoft.Json.Linq.JValue.")]
    public void Item()
    {
      JValue v = new JValue(true);
      JToken first = v[0];
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot access child value on Newtonsoft.Json.Linq.JValue.")]
    public void Values()
    {
      JValue v = new JValue(true);
      v.Values<int>();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "The parent is missing.")]
    public void RemoveParentNull()
    {
      JValue v = new JValue(true);
      v.Remove();
    }

    [Test]
    public void Root()
    {
      JValue v = new JValue(true);
      Assert.AreEqual(v, v.Root);
    }

    [Test]
    public void Previous()
    {
      JValue v = new JValue(true);
      Assert.IsNull(v.Previous);
    }

    [Test]
    public void Next()
    {
      JValue v = new JValue(true);
      Assert.IsNull(v.Next);
    }

    [Test]
    public void DeepEquals()
    {
      Assert.IsTrue(JToken.DeepEquals(new JValue(5L), new JValue(5)));
      Assert.IsFalse(JToken.DeepEquals(new JValue(5M), new JValue(5)));
      Assert.IsTrue(JToken.DeepEquals(new JValue((ulong)long.MaxValue), new JValue(long.MaxValue)));
    }

    [Test]
    public void HasValues()
    {
      Assert.IsFalse((new JValue(5L)).HasValues);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot set child value on Newtonsoft.Json.Linq.JValue.")]
    public void SetValue()
    {
      JToken t = new JValue(5L);
      t[0] = new JValue(3);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not convert Null to Int32.")]
    public void CastNullValueToNonNullable()
    {
      JValue v = new JValue((object)null);
      int i = (int) v;
    }
  }
}