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
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.Linq
{
  [TestFixture]
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

#if !(NETFX_CORE || PORTABLE)
      v.Value = DBNull.Value;
      Assert.AreEqual(DBNull.Value, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);
#endif

      byte[] data = new byte[0];
      v.Value = data;

      Assert.AreEqual(data, v.Value);
      Assert.AreEqual(JTokenType.Bytes, v.Type);

      v.Value = StringComparison.OrdinalIgnoreCase;
      Assert.AreEqual(StringComparison.OrdinalIgnoreCase, v.Value);
      Assert.AreEqual(JTokenType.Integer, v.Type);

      v.Value = new Uri("http://json.codeplex.com/");
      Assert.AreEqual(new Uri("http://json.codeplex.com/"), v.Value);
      Assert.AreEqual(JTokenType.Uri, v.Type);

      v.Value = TimeSpan.FromDays(1);
      Assert.AreEqual(TimeSpan.FromDays(1), v.Value);
      Assert.AreEqual(JTokenType.TimeSpan, v.Type);

      Guid g = Guid.NewGuid();
      v.Value = g;
      Assert.AreEqual(g, v.Value);
      Assert.AreEqual(JTokenType.Guid, v.Type);
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
    public void ToString()
    {
      JValue v;

      v = new JValue(true);
      Assert.AreEqual("True", v.ToString());

      v = new JValue(Encoding.UTF8.GetBytes("Blah"));
      Assert.AreEqual("System.Byte[]", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue("I am a string!");
      Assert.AreEqual("I am a string!", v.ToString());

      v = new JValue(null, JTokenType.Null);
      Assert.AreEqual("", v.ToString());

      v = new JValue(null, JTokenType.Null);
      Assert.AreEqual("", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new DateTime(2000, 12, 12, 20, 59, 59, DateTimeKind.Utc), JTokenType.Date);
      Assert.AreEqual("12/12/2000 20:59:59", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new Uri("http://json.codeplex.com/"));
      Assert.AreEqual("http://json.codeplex.com/", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(TimeSpan.FromDays(1));
      Assert.AreEqual("1.00:00:00", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new Guid("B282ADE7-C520-496C-A448-4084F6803DE5"));
      Assert.AreEqual("b282ade7-c520-496c-a448-4084f6803de5", v.ToString(null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Last()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
      () =>
      {
        JValue v = new JValue(true);
        JToken last = v.Last;
      });
    }

    [Test]
    public void Children()
    {
      JValue v = new JValue(true);
      var c = v.Children();
      Assert.AreEqual(JEnumerable<JToken>.Empty, c);
    }

    [Test]
    public void First()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
      () =>
      {
        JValue v = new JValue(true);
        JToken first = v.First;
      });
    }

    [Test]
    public void Item()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
      () =>
      {
        JValue v = new JValue(true);
        JToken first = v[0];
      });
    }

    [Test]
    public void Values()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
      () =>
      {
        JValue v = new JValue(true);
        v.Values<int>();
      });
    }

    [Test]
    public void RemoveParentNull()
    {
      ExceptionAssert.Throws<InvalidOperationException>("The parent is missing.",
      () =>
      {
        JValue v = new JValue(true);
        v.Remove();
      });
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
    public void SetValue()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot set child value on Newtonsoft.Json.Linq.JValue.",
      () =>
      {
        JToken t = new JValue(5L);
        t[0] = new JValue(3);
      });
    }

    [Test]
    public void CastNullValueToNonNullable()
    {
      ExceptionAssert.Throws<ArgumentException>("Can not convert Null to Int32.",
      () =>
      {
        JValue v = new JValue((object)null);
        int i = (int)v;
      });
    }

    [Test]
    public void ConvertValueToCompatibleType()
    {
      IComparable c = (new JValue(1).Value<IComparable>());
      Assert.AreEqual(1L, c);
    }

    [Test]
    public void ConvertValueToFormattableType()
    {
      IFormattable f = (new JValue(1).Value<IFormattable>());
      Assert.AreEqual(1L, f);

      Assert.AreEqual("01", f.ToString("00", CultureInfo.InvariantCulture));
    }

    [Test]
    public void Ordering()
    {
      JObject o = new JObject(
        new JProperty("Integer", new JValue(1)),
        new JProperty("Float", new JValue(1.2d)),
        new JProperty("Decimal", new JValue(1.1m))
        );

      IList<object> orderedValues = o.Values().Cast<JValue>().OrderBy(v => v).Select(v => v.Value).ToList();

      Assert.AreEqual(1L, orderedValues[0]);
      Assert.AreEqual(1.1m, orderedValues[1]);
      Assert.AreEqual(1.2d, orderedValues[2]);
    }
  }
}