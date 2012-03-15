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
using Newtonsoft.Json.Tests.TestObjects;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Converters
{
  [TestFixture]
  public class JavaScriptDateTimeConverterTests : TestFixtureBase
  {
    [Test]
    public void SerializeDateTime()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTime d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);
      string result;

      result = JsonConvert.SerializeObject(d, converter);
      Assert.AreEqual("new Date(976918263055)", result);
    }

#if !PocketPC && !NET20
    [Test]
    public void SerializeDateTimeOffset()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTimeOffset now = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);
      string result;

      result = JsonConvert.SerializeObject(now, converter);
      Assert.AreEqual("new Date(976918263055)", result);
    }

    [Test]
    public void sdfs()
    {
      int i = Convert.ToInt32("+1");
      Console.WriteLine(i);
    }

    [Test]
    public void SerializeNullableDateTimeClass()
    {
      NullableDateTimeTestClass t = new NullableDateTimeTestClass()
      {
        DateTimeField = null,
        DateTimeOffsetField = null
      };

      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      string result;

      result = JsonConvert.SerializeObject(t, converter);
      Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

      t = new NullableDateTimeTestClass()
      {
        DateTimeField = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc),
        DateTimeOffsetField = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero)
      };

      result = JsonConvert.SerializeObject(t, converter);
      Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":new Date(976918263055),""DateTimeOffsetField"":new Date(976918263055),""PostField"":null}", result);
    }

    [Test]
    public void DeserializeNullToNonNullable()
    {
      ExceptionAssert.Throws<Exception>("Cannot convert null value to System.DateTime.",
      () =>
      {
        DateTimeTestClass c2 =
         JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new JavaScriptDateTimeConverter());
      });
    }

    [Test]
    public void DeserializeDateTimeOffset()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();
      DateTimeOffset start = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

      string json = JsonConvert.SerializeObject(start, converter);

      DateTimeOffset result = JsonConvert.DeserializeObject<DateTimeOffset>(json, converter);
      Assert.AreEqual(new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero), result);
    }
#endif

    [Test]
    public void DeserializeDateTime()
    {
      JavaScriptDateTimeConverter converter = new JavaScriptDateTimeConverter();

      DateTime result = JsonConvert.DeserializeObject<DateTime>("new Date(976918263055)", converter);
      Assert.AreEqual(new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc), result);
    }
  }
}